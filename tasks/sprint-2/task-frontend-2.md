# Frontend — Guard de autenticação para /dashboard (Sprint 2)

## Contexto
Hoje é possível acessar `/dashboard` sem autenticação digitando a URL. Precisamos proteger rotas.

## Escopo
- Inclui:
  - Implementar guard (guardian) de autenticação.
  - Proteger rota `/dashboard`.
  - Redirecionar para `/auth` se não houver token.
- Não inclui:
  - Interceptor de refresh token automático (pode ser Sprint futura).

## Dependências
- Sprint 1 frontend.

## Requisitos (funcionais)
- Se não existir token armazenado, bloquear navegação para `/dashboard`.
- Em bloqueio, navegar para `/auth`.

## Critérios de aceitação (Given/When/Then)
- Given que eu não estou logado, When eu navego para `/dashboard`, Then sou redirecionado para `/auth`.
- Given que eu estou logado, When eu navego para `/dashboard`, Then acesso normalmente.

## Definição de pronto (DoD)
- Build ok
- Comportamento reproduzível
