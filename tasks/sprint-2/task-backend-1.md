# Backend — Refresh Token (Sprint 2)

## Contexto
O backend já emite access token no login/cadastro. Agora precisamos de refresh token para renovar o access token sem exigir login a cada expiração.

## Escopo
- Inclui:
  - Persistir refresh tokens no banco (SQL Server).
  - Emitir refresh token no login e no cadastro.
  - Endpoint para trocar refresh token por um novo access token (e rotacionar refresh token).
- Não inclui:
  - Multi-device avançado, gestão de sessões complexa.

## Dependências
- Sprint 1 backend concluída.

## Requisitos (funcionais)
- Modelar entidade/tabela para refresh token com relação ao usuário.
- `POST /api/auth/refresh`:
  - Recebe refresh token.
  - Valida existência, não expirado, não revogado.
  - Retorna novo access token e novo refresh token.
- Rotação:
  - Ao usar um refresh token, ele deve ser invalidado (revogado) e um novo criado.

## Critérios de aceitação (Given/When/Then)
- Given um usuário logado, When eu chamo `POST /api/auth/login`, Then recebo access token e refresh token.
- Given um refresh token válido, When eu chamo `POST /api/auth/refresh`, Then recebo um novo access token e um novo refresh token.
- Given um refresh token já usado/rotacionado, When eu tento reutilizar, Then recebo erro.
- Given um refresh token expirado, When eu chamo refresh, Then recebo erro.

## Definição de pronto (DoD)
- Migration criada/aplicável
- Build ok
- Endpoint documentado (OpenAPI)
- Erros consistentes
