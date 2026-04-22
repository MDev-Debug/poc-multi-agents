# patch-frontend-2 — Sanitização de Input e Validação no Frontend

## Vulnerabilidades

### VUL-F2a — BAIXA: Campo de mensagem sem limite de caracteres no HTML
- **Arquivo**: `frontend/src/app/features/dashboard/components/private-chat/private-chat.component.html` linha 71
- **Problema**: O `<textarea>` não possui atributo `maxlength`. O backend limita a 4000 chars, mas sem validação no frontend o usuário pode colar textos arbitrariamente longos antes de receber o erro do servidor.

### VUL-F2b — BAIXA: Ausência de sanitização de conteúdo de mensagem recebida via SignalR antes de exibição
- **Arquivo**: `private-chat.component.html` linhas 56
- **Análise**: O template usa `{{ msg.content }}` (interpolação Angular), que é automaticamente escaped — não há XSS aqui. Porém, a propriedade `content` poderia conter sequências de controle Unicode (ex: RTLO U+202E) usadas para mascarar URLs maliciosas.

### VUL-F2c — BAIXA: Ausência de confirmação antes de logout (vaza sessão SignalR)
- **Arquivo**: `dashboard.component.ts` linha 80
- **Observação**: Não é vulnerabilidade crítica, mas o logout não aguarda confirmação do disconnect SignalR antes de redirecionar.

### VUL-F2d — MÉDIA: Validação de senha fraca no formulário de registro
- **Arquivo**: `frontend/src/app/features/auth/auth.component.ts` linha 31
- **Problema**: `Validators.minLength(6)` — muito fraco. Com o patch-backend-3 aumentando para 8 chars mínimo no backend, o frontend deve acompanhar. Além disso, não há validação de complexidade mínima.

---

## Solução

### 1. Adicionar `maxlength` no textarea da conversa

**`private-chat.component.html`** — adicionar `maxlength="4000"`:

```html
<textarea
  class="message-input"
  [(ngModel)]="newMessage"
  name="messageContent"
  rows="1"
  maxlength="4000"
  [attr.aria-label]="'Mensagem para ' + otherUser.email"
  placeholder="Digite uma mensagem..."
  [disabled]="sending()"
  (keydown)="onKeydown($event)">
</textarea>
```

Exibir contador de caracteres abaixo do textarea:

```html
<!-- Adicionar após o </textarea>: -->
<span class="char-count" [class.warn]="newMessage.length > 3800">
  {{ newMessage.length }}/4000
</span>
```

---

### 2. Adicionar pipe de sanitização de caracteres de controle Unicode

**`frontend/src/app/core/pipes/safe-text.pipe.ts`** (NOVO):

```typescript
import { Pipe, PipeTransform } from '@angular/core';

/**
 * Remove caracteres de controle Unicode perigosos do texto:
 * - RTLO (U+202E): inverte a direção do texto — usado para mascarar extensões de arquivo
 * - ZWNJ, ZWJ, outros bidirectional markers
 * - Caracteres de controle C0 (exceto newline e tab)
 */
@Pipe({
  name: 'safeText',
  standalone: true,
})
export class SafeTextPipe implements PipeTransform {
  // Regex para caracteres de controle perigosos
  private static readonly DANGEROUS_CHARS = /[\u202E\u200B\u200C\u200D\u2066\u2067\u2068\u2069\u202A-\u202D\u2028\u2029\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]/g;

  transform(value: string | null | undefined): string {
    if (!value) return '';
    return value.replace(SafeTextPipe.DANGEROUS_CHARS, '');
  }
}
```

Usar no template `private-chat.component.html`:

```html
<!-- Importar SafeTextPipe no componente e usar: -->
<p class="message-content">{{ msg.content | safeText }}</p>
```

Importar no componente:

```typescript
import { SafeTextPipe } from '../../../../core/pipes/safe-text.pipe';

@Component({
  // ...
  imports: [CommonModule, FormsModule, SafeTextPipe],  // adicionar SafeTextPipe
})
```

---

### 3. Aumentar validação de senha no formulário de registro

**`auth.component.ts`** — atualizar validadores:

```typescript
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';

// Validator customizado para complexidade mínima de senha
function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
  const value: string = control.value ?? '';
  if (value.length < 8) return null; // defer to minLength

  const hasUpper   = /[A-Z]/.test(value);
  const hasLower   = /[a-z]/.test(value);
  const hasNumber  = /[0-9]/.test(value);

  if (!hasUpper || !hasLower || !hasNumber) {
    return { passwordStrength: 'A senha deve conter letras maiúsculas, minúsculas e números.' };
  }
  return null;
}

// Dentro do constructor AuthComponent, atualizar o form:
this.form = this.fb.nonNullable.group({
  email: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
  password: [
    '',
    [
      Validators.required,
      Validators.minLength(8),       // aumentado de 6 para 8
      Validators.maxLength(128),     // novo — prevenir DoS no hash
      passwordStrengthValidator,     // novo — complexidade mínima
    ]
  ],
});
```

Atualizar a mensagem de erro no template `auth.component.html`:

```html
<!-- Substituir a mensagem de erro da senha -->
<span class="field-error" *ngIf="form.get('password')?.errors?.['minlength'] && form.get('password')?.touched">
  Mínimo 8 caracteres
</span>
<span class="field-error" *ngIf="form.get('password')?.errors?.['passwordStrength'] && form.get('password')?.touched">
  {{ form.get('password')?.errors?.['passwordStrength'] }}
</span>
```

---

### 4. Garantir await completo no logout antes de redirecionar

**`dashboard.component.ts`** — o `logout()` já usa `await`, mas deve esperar os disconnects:

```typescript
async logout(): Promise<void> {
  try {
    // Desconectar SignalR antes de limpar tokens
    await Promise.allSettled([
      this.presenceHub.disconnect(),
      this.chatHub.disconnect(),
    ]);
  } finally {
    this.auth.clearTokens();
    this.router.navigate(['/auth']);
  }
}
```

---

### 5. Adicionar SCSS para o contador de caracteres

**`private-chat.component.scss`** — adicionar:

```scss
.char-count {
  font-size: 0.7rem;
  color: var(--text-muted);
  text-align: right;
  margin-top: 2px;

  &.warn {
    color: var(--neon-pink);
  }
}
```

---

## Checklist de aplicação

- [ ] Adicionar `maxlength="4000"` e contador de chars no `private-chat.component.html`
- [ ] Criar `SafeTextPipe` em `core/pipes/safe-text.pipe.ts`
- [ ] Importar `SafeTextPipe` no `PrivateChatComponent` e aplicar no template
- [ ] Atualizar `minLength(6)` para `minLength(8)` e adicionar `maxLength(128)` e `passwordStrengthValidator` em `auth.component.ts`
- [ ] Atualizar mensagens de erro de senha no `auth.component.html`
- [ ] Atualizar `logout()` com `Promise.allSettled`
- [ ] Adicionar SCSS do contador de chars
- [ ] `ng build` — 0 erros
