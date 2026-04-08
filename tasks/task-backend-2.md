# Backend — Endpoints de Cadastro/Login + Token (Sprint 1)

## Contexto
A tela de autenticação (frontend) precisa de endpoints para registrar e autenticar usuários.

## Escopo
- Inclui:
  - Endpoint de cadastro.
  - Endpoint de login.
  - Emissão de token (ex.: JWT) no login (e opcionalmente após cadastro).
- Não inclui:
  - Refresh token.
  - Roles/perfis.

## Dependências
- `task-backend-1.md` concluída.

## Requisitos (funcionais)
- `POST /api/auth/register`:
  - Valida `email` e `password`.
  - Cria usuário com senha hasheada.
  - Se e-mail já existir, retorna erro apropriado.
- `POST /api/auth/login`:
  - Valida credenciais.
  - Se ok, retorna token e dados mínimos do usuário (ex.: id/email).
  - Se inválido, retorna erro apropriado.

## Requisitos UX/UI
- N/A

## Critérios de aceitação (Given/When/Then)
- Given um e-mail novo e senha válida, When eu chamo `POST /api/auth/register`, Then recebo sucesso e o usuário é persistido.
- Given um usuário existente, When eu chamo `POST /api/auth/login` com senha correta, Then recebo token e id/email.
- Given um usuário existente, When eu chamo `POST /api/auth/login` com senha incorreta, Then recebo erro (401/400) sem vazar detalhes.
- Given payload inválido (campos vazios), When eu chamo os endpoints, Then recebo erro de validação.

## Definição de pronto (DoD)
- Build ok
- Endpoints documentados (Swagger padrão ok)
- Erros tratados e consistentes
