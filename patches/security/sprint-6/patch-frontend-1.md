# patch-frontend-1 — Tokens no sessionStorage e Proteção XSS/CSRF

## Vulnerabilidades

### VUL-F1a — MÉDIA: Token JWT armazenado em sessionStorage (leitura via XSS)
- **Arquivo**: `frontend/src/app/core/services/auth.service.ts`
- **Problema**: `sessionStorage` é acessível via JavaScript. Um ataque XSS bem-sucedido pode ler o token (`sessionStorage.getItem('chat_token')`). Angular protege contra XSS por padrão via template engine, mas XSS via bibliotecas de terceiros ou DOM injection ainda é risco.
- **Impacto**: Comprometimento de sessão.
- **Nota de contexto**: `localStorage` seria pior (persiste entre abas/sessões). `sessionStorage` é razoável para SPA de PoC. A alternativa mais segura seria cookie `HttpOnly + Secure + SameSite=Strict`, mas exige mudança de arquitetura backend.

### VUL-F1b — BAIXA: URL hardcoded do backend no AuthService e MessageApiService
- **Arquivos**: `auth.service.ts` linha 14, `message-api.service.ts` linha 9, `chat-hub.service.ts` linha 28, `presence-hub.service.ts` linha 26
- **Problema**: `http://localhost:5000` hardcoded. Em produção, o HTTP sem TLS expõe tokens em trânsito.

### VUL-F1c — BAIXA: Ausência de renovação automática do access token (token refresh interceptor)
- **Arquivo**: `auth.interceptor.ts`
- **Problema**: Quando o access token expira (15 min após o patch-backend-2), todas as chamadas HTTP passam a retornar 401 sem que o usuário seja notificado ou redirecionado. O refresh token não é usado automaticamente.

### VUL-F1d — BAIXA: Ausência de validação de token expirado no authGuard
- **Arquivo**: `auth.guard.ts`
- **Problema**: O guard verifica apenas se o token existe (`auth.getToken()`), não se está expirado. Um token expirado presente no sessionStorage deixa o usuário "preso" no dashboard sem acesso a nenhuma API.

---

## Solução

### 1. Centralizar a URL base via environment — criar `environments/`

**`frontend/src/environments/environment.ts`** (NOVO):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
};
```

**`frontend/src/environments/environment.prod.ts`** (NOVO):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.suaempresa.com',  // HTTPS obrigatório em produção
};
```

Atualizar cada serviço para usar `environment.apiUrl` em vez do hardcode:

**`auth.service.ts`**:
```typescript
import { environment } from '../../../environments/environment';
// ...
private readonly baseUrl = environment.apiUrl;
```

**`message-api.service.ts`**:
```typescript
import { environment } from '../../../environments/environment';
// ...
private readonly baseUrl = environment.apiUrl;
```

**`chat-hub.service.ts`**:
```typescript
import { environment } from '../../../environments/environment';
// ...
.withUrl(`${environment.apiUrl}/hubs/chat`, { ... })
```

**`presence-hub.service.ts`**:
```typescript
import { environment } from '../../../environments/environment';
// ...
.withUrl(`${environment.apiUrl}/hubs/presence`, { ... })
```

---

### 2. Adicionar Token Refresh Interceptor

**`frontend/src/app/core/interceptors/token-refresh.interceptor.ts`** (NOVO):

```typescript
import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const tokenRefreshInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Ignorar erros 401 no próprio endpoint de refresh (evita loop infinito)
      if (error.status !== 401 || req.url.includes('/api/auth/refresh')) {
        return throwError(() => error);
      }

      if (isRefreshing) {
        // Aguardar o refresh em curso
        return refreshTokenSubject.pipe(
          filter(token => token !== null),
          take(1),
          switchMap(token => next(addAuthHeader(req, token!)))
        );
      }

      isRefreshing = true;
      refreshTokenSubject.next(null);

      const refreshToken = auth.getRefreshToken();
      if (!refreshToken) {
        isRefreshing = false;
        auth.clearTokens();
        router.navigate(['/auth']);
        return throwError(() => error);
      }

      return auth.refresh(refreshToken).pipe(
        switchMap(res => {
          isRefreshing = false;
          auth.saveTokens(res.token, res.refreshToken);
          auth.saveEmail(res.email);
          auth.saveUserId(res.userId);
          refreshTokenSubject.next(res.token);
          return next(addAuthHeader(req, res.token));
        }),
        catchError(refreshError => {
          isRefreshing = false;
          auth.clearTokens();
          router.navigate(['/auth']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};

function addAuthHeader(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
```

---

### 3. Adicionar métodos `getRefreshToken()` e `refresh()` no AuthService

```typescript
// Adicionar em auth.service.ts:

getRefreshToken(): string | null {
  return sessionStorage.getItem(this.refreshTokenKey);
}

refresh(refreshToken: string): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(`${this.baseUrl}/api/auth/refresh`, { refreshToken });
}
```

---

### 4. Registrar o interceptor em `app.config.ts`

```typescript
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { tokenRefreshInterceptor } from './core/interceptors/token-refresh.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, tokenRefreshInterceptor])),
  ]
};
```

---

### 5. Melhorar `authGuard` para detectar token expirado

```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getToken();
  if (!token) {
    router.navigateByUrl('/auth');
    return false;
  }

  // Verificar se o token JWT está expirado (parse do payload Base64)
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expMs = payload.exp * 1000;
    if (Date.now() >= expMs) {
      // Token expirado — limpar e redirecionar
      auth.clearTokens();
      router.navigateByUrl('/auth');
      return false;
    }
  } catch {
    // Token malformado
    auth.clearTokens();
    router.navigateByUrl('/auth');
    return false;
  }

  return true;
};
```

---

### 6. Nota sobre Content Security Policy (Angular)

Angular usa `innerHTML` escaping por padrão — qualquer interpolação `{{ valor }}` é escapada automaticamente.
Não foram encontradas instâncias de `[innerHTML]`, `bypassSecurityTrustHtml`, `DomSanitizer.bypassSecurityTrust*` no código.
O risco de XSS via template Angular é BAIXO neste projeto.

Para adicionar CSP via meta tag no `index.html` (defense in depth):

```html
<!-- Adicionar em frontend/src/index.html dentro de <head> -->
<meta http-equiv="Content-Security-Policy"
  content="default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; connect-src 'self' http://localhost:5000 ws://localhost:5000;">
```

Em produção, trocar `http://localhost:5000 ws://localhost:5000` pelo domínio real com HTTPS/WSS.

---

## Checklist de aplicação

- [ ] Criar `frontend/src/environments/environment.ts` e `environment.prod.ts`
- [ ] Atualizar `auth.service.ts`, `message-api.service.ts`, `chat-hub.service.ts`, `presence-hub.service.ts` para usar `environment.apiUrl`
- [ ] Adicionar `getRefreshToken()` e `refresh()` em `AuthService`
- [ ] Criar `token-refresh.interceptor.ts`
- [ ] Registrar `tokenRefreshInterceptor` em `app.config.ts`
- [ ] Atualizar `auth.guard.ts` para verificar expiração do JWT
- [ ] Adicionar CSP meta tag em `index.html`
- [ ] `ng build` — 0 erros
