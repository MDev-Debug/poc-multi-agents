# Bug Backend 1 — Request de refresh sem token deveria retornar erro de validação

## Como reproduzir
1) Chamar `POST /api/auth/refresh` com body vazio `{}` ou com `refreshToken` vazio.

## Resultado atual
- A API tende a responder `401 Unauthorized` (refresh inválido) mesmo quando a entrada é ausente/vazia.

## Resultado esperado
- Para entrada ausente/vazia, retornar `400 Bad Request` com erro de validação claro (contrato de validação consistente).

## Evidência
- `RefreshRequest` não possui `[Required]`/validação no `RefreshToken`.

## Critério de correção
- `refreshToken` ausente/vazio retorna `400` com payload de validação.
- `refreshToken` inválido mas presente continua retornando `401`.
