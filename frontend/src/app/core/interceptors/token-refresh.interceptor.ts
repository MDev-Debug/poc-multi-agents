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
