# Bug Backend 2 — Mensagens de erro inconsistentes para inputs inválidos

## Como reproduzir
1) Chamar `POST /api/auth/register` com e-mail inválido.
2) Chamar `POST /api/auth/login` com e-mail inválido.

## Resultado atual
- Respostas podem variar entre `400` com `ValidationProblemDetails` e respostas com `{ message: ... }`.

## Resultado esperado
- Erros de validação de entrada devem seguir um padrão consistente (ex.: sempre `ValidationProblemDetails`), facilitando o tratamento no frontend.

## Critério de correção
- Inputs inválidos (formato de e-mail, required, etc.) retornam formato consistente.
- Erros de credencial/negócio (ex.: login inválido) continuam retornando `401` com mensagem amigável.
