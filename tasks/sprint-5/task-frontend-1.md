# task-frontend-1 — Reorganizar estrutura Angular + migrar para SCSS

## Contexto
O frontend atual tem estrutura plana sem organização de features, usa CSS global sem variáveis e mistura responsabilidades. Refatorar para a estrutura Angular padrão com SCSS.

## Escopo
- Inclui:
  - Reorganizar pastas para core/shared/features
  - Migrar styles.css → styles.scss com design system completo
  - Migrar arquivos de componentes para SCSS individual
  - Renomear componentes para convenção Angular (feature.component.ts ao invés de feature.page.ts)
  - Atualizar angular.json para SCSS
  - Atualizar todas as referências de import
- Não inclui:
  - Redesign visual (feito nas tasks seguintes)
  - Novas features

## Estrutura alvo

```
frontend/src/app/
  core/
    guards/
      auth.guard.ts
    interceptors/
      auth.interceptor.ts
    services/
      auth.service.ts
      presence-hub.service.ts
  shared/
    components/
      (componentes reutilizáveis futuros)
  features/
    auth/
      auth.component.ts
      auth.component.html
      auth.component.scss
    dashboard/
      dashboard.component.ts
      dashboard.component.html
      dashboard.component.scss
      components/
        sidebar/
          sidebar.component.ts
          sidebar.component.html
          sidebar.component.scss
        topbar/
          topbar.component.ts
          topbar.component.html
          topbar.component.scss
        online-users/
          online-users.component.ts
          online-users.component.html
          online-users.component.scss
  app.ts
  app.html
  app.routes.ts
  app.config.ts

frontend/src/
  styles.scss      (migrar de styles.css)
  main.ts
  index.html
```

## Requisitos funcionais

1. Criar pastas: `core/guards/`, `core/interceptors/`, `core/services/`, `shared/components/`, `features/auth/`, `features/dashboard/`, `features/dashboard/components/sidebar/`, `features/dashboard/components/topbar/`, `features/dashboard/components/online-users/`

2. Mover e renomear:
   - `pages/auth/auth.page.ts` → `features/auth/auth.component.ts` (atualizar selector e class name)
   - `pages/auth/auth.page.html` → `features/auth/auth.component.html`
   - `pages/dashboard/dashboard.page.ts` → `features/dashboard/dashboard.component.ts`
   - `pages/dashboard/dashboard.page.html` → `features/dashboard/dashboard.component.html`
   - `guards/auth.guard.ts` → `core/guards/auth.guard.ts`
   - `interceptors/auth.interceptor.ts` → `core/interceptors/auth.interceptor.ts`
   - `services/auth.service.ts` → `core/services/auth.service.ts`
   - `services/presence-hub.service.ts` → `core/services/presence-hub.service.ts`

3. Remover `services/presence.service.ts` (legado HTTP, não usado)

4. Criar componentes filhos do dashboard (standalone, vazios por enquanto — serão preenchidos em task-frontend-3):
   - `SidebarComponent`
   - `TopbarComponent`
   - `OnlineUsersComponent`

5. Atualizar `angular.json`:
   - `"inlineStyleLanguage": "scss"` (se não existir)
   - `"styles": ["src/styles.scss"]`

6. Migrar `src/styles.css` → `src/styles.scss` preservando todo o conteúdo existente e adicionando o design system de tokens (ver task-frontend-2)

7. Atualizar `app.routes.ts` com novos paths de import dos componentes

8. Atualizar `app.config.ts` com interceptor se necessário

9. Garantir `ng build` passa sem erros

## Critérios de aceitação
- Given: estrutura refatorada
- When: `ng build`
- Then: 0 erros de compilação

- Given: app rodando
- When: acesso a /auth
- Then: página de auth renderiza

- Given: app rodando com token válido
- When: acesso a /dashboard
- Then: dashboard renderiza

## Definição de pronto (DoD)
- [ ] Estrutura de pastas conforme alvo
- [ ] Todos os imports atualizados
- [ ] `ng build` passa sem erros
- [ ] Roteamento funcionando (/auth e /dashboard)
