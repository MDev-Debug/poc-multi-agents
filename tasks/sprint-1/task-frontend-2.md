# Frontend — Integração com API de Auth + Redirecionamento Dashboard (Sprint 1)

## Contexto
Após cadastro/login, o usuário deve ser autenticado e redirecionado para `/dashboard` (placeholder na Sprint 1).

## Escopo
- Inclui:
  - Service de autenticação consumindo `POST /api/auth/register` e `POST /api/auth/login`.
  - Armazenar token localmente (ex.: localStorage) de forma simples.
  - Redirecionar para `/dashboard` após sucesso.
  - Criar página `/dashboard` **placeholder** informando que será entregue na Sprint 2.
- Não inclui:
  - Guard avançado, refresh token, permissões.

## Dependências
- `task-frontend-1.md`
- `task-backend-2.md`

## Requisitos (funcionais)
- Tratar erros de API e mostrar mensagem amigável.
- Após sucesso:
  - persistir token
  - navegar para `/dashboard`

## Requisitos UX/UI
- Mostrar feedback de sucesso (pode ser textual simples) antes do redirecionamento.
- Página `/dashboard` deve manter o tema e ser claramente um placeholder (Sprint 2).

## Critérios de aceitação (Given/When/Then)
- Given credenciais válidas, When eu faço login, Then recebo token, salvo, e sou redirecionado para `/dashboard`.
- Given e-mail duplicado, When eu tento cadastrar, Then vejo mensagem de erro amigável.
- Given falha de rede/API, When eu envio, Then vejo erro e posso tentar novamente.

## Definição de pronto (DoD)
- Build ok
- Integração funcional contra backend local
- UX consistente e sem fluxos extras
