# Bug 2 — Sintaxe de template mista (*ngIf/*ngFor e @if/@for) em OnlineUsersComponent

## Task relacionada
- `tasks/sprint-6/task-frontend-3.md`
- `tasks/sprint-6/task-frontend-2.md`

## Como reproduzir
1. Abrir `frontend/src/app/features/dashboard/components/online-users/online-users.component.html`.
2. Observar que o template usa `*ngIf` e `*ngFor` (sintaxe de diretivas estruturais legadas) para renderizar a lista e o estado vazio.
3. Ao mesmo tempo, usa a sintaxe de bloco `@if` (Angular 17+ built-in control flow) para o badge de mensagens não lidas dentro de cada item.

## Resultado esperado
- O template deve usar consistentemente a nova sintaxe de bloco (`@if`, `@for`, `@empty`) do Angular 17+, que é o padrão adotado no restante do projeto (ex.: `dashboard.component.html`, `private-chat.component.html` usam exclusivamente `@if`/`@for`).
- `*ngIf` e `*ngFor` estão **deprecated** no Angular 17+ e não devem ser usados em componentes novos desta sprint.

## Resultado atual
- `online-users.component.html` usa:
  - `*ngIf="users.length > 0"` na div da lista (linha 7)
  - `*ngFor="let user of users"` nos itens (linha 10)
  - `*ngIf="users.length === 0"` no estado vazio (linha 32)
  - `@if (...)` para o badge de não lidas (linha 21)

## Evidência

```html
<!-- online-users.component.html — linhas 7, 10, 32 -->
<div class="user-list" *ngIf="users.length > 0">
  <div class="user-item" *ngFor="let user of users" ...>
    ...
    @if ((unreadCounts.get(user.userId) ?? 0) > 0) { ... }
  </div>
</div>
<div class="empty-state" *ngIf="users.length === 0">...</div>
```

Comparação com `dashboard.component.html` (consistente — usa apenas nova sintaxe):
```html
@if (currentView() === 'home') { ... }
@if (currentView() === 'chat') { ... }
```

E `private-chat.component.html` (consistente — usa apenas nova sintaxe):
```html
@if (loading()) { ... }
@for (msg of messages(); track msg.messageId) { ... }
```

## Severidade
- [ ] Crítico (bloqueia uso)
- [ ] Alto (funcionalidade principal quebrada)
- [x] Médio (funcionalidade secundária ou visual)
- [ ] Baixo (cosmético / melhoria)

## Critério de correção (Given/When/Then)
- Given o template `online-users.component.html` é aberto
- When as diretivas de controle de fluxo são inspecionadas
- Then todo o template usa exclusivamente a sintaxe de bloco `@if` / `@for` / `@empty`, sem nenhuma ocorrência de `*ngIf` ou `*ngFor`

**Sugestão de correção**:
```html
@if (users.length > 0) {
  <div class="user-list">
    @for (user of users; track user.userId) {
      <div class="user-item" role="button" tabindex="0"
        [attr.aria-label]="'Abrir conversa com ' + user.email"
        (click)="userSelected.emit(user)"
        (keydown.enter)="userSelected.emit(user)">
        ...
        @if ((unreadCounts.get(user.userId) ?? 0) > 0) { ... }
      </div>
    }
  </div>
} @else {
  <div class="empty-state"><p>Nenhum usuário online</p></div>
}
```

Após a correção, `CommonModule` pode ser removido dos imports do componente (já que `NgIf`/`NgFor` não serão mais necessários).

## Status
- [ ] Aberto
- [ ] Em correção
- [ ] Corrigido — aguardando re-teste
- [x] Fechado

## Re-teste (2026-04-21)
- Template `online-users.component.html` usa exclusivamente `@if`, `@for` e `@else` — nenhuma ocorrência de `*ngIf` ou `*ngFor`.
- `online-users.component.ts` tem `imports: []` — `CommonModule` removido.
- `ng build` passa sem erros ou warnings.
- Critério Given/When/Then satisfeito.
