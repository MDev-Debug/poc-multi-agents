# Sprint 2 — Dashboard (UI/Design) + Proteção de Rotas + Refresh Token

## Prompt inicial (original)
"Na sprint 2 será feito a implementação e refatoração do design o design está feio e preciso de um design melhor e mais elegante com cores neon no dashboard quero uma sidebar com movimento com menu hambuguer que abre e fecha com os menus, home, chat, logout, por enquanto esses menus não terão nada implementado isso será feito nas proximas sprints, tambem quero trabalhar com proteção a tela logada atualmente se eu digitar /dashboard mesmo sem autenticação eu consigo entrar preciso que tenha bloqueio e que sedja implementado um guardian no frontend, no backend deve ser feito uma implementação utilizando refresh token"

## Objetivo e valor
- Objetivo: elevar a qualidade visual do dashboard (cyberpunk elegante) e garantir acesso apenas autenticado.
- Para quem: usuários autenticados.
- Valor esperado: base sólida para futuras sprints (Home/Chat) e segurança melhor com refresh token.

## Escopo
- Inclui:
  - Frontend:
    - Refatorar UI do dashboard (mais elegante, neon).
    - Sidebar com animação (hamburger abre/fecha) e itens: Home, Chat, Logout (sem implementar telas/funcionalidades ainda).
    - Guard/Proteção de rota para bloquear `/dashboard` sem autenticação.
  - Backend:
    - Implementar refresh token (persistido) e endpoint para renovar access token.
- Não inclui:
  - Implementação de Home/Chat (próximas sprints).
  - Autorização por roles/perfis.

## Assunções e dependências
- Sprint 1 já entregou cadastro/login e emissão de access token.
- Ambiente local com SQL Server.

## Contrato de UX/UI (aprovado pelo PO)
- Tema: cyberpunk com fundo escuro + neon (mais elegante que Sprint 1).
- Sidebar:
  - Deve ter botão hamburger.
  - Deve animar abrindo/fechando (movimento suave).
  - Menus visíveis: Home, Chat, Logout.
  - Home/Chat não navegam para features reais ainda (placeholder).
  - Logout deve deslogar e redirecionar para `/auth`.
- Proteção:
  - Ao tentar acessar `/dashboard` sem token, redirecionar para `/auth`.

## Plano de execução
- Tasks backend:
  - `tasks/sprint-2/task-backend-1.md`
- Tasks frontend:
  - `tasks/sprint-2/task-frontend-1.md`
  - `tasks/sprint-2/task-frontend-2.md`
- Estratégia de QA:
  - Validar que `/dashboard` está protegido.
  - Validar fluxo login/cadastro → dashboard.
  - Validar refresh token via endpoint (pelo menos smoke test).
