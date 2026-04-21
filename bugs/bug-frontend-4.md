# Bug Frontend 4 — `/auth` permanece acessível mesmo autenticado

## Como reproduzir
1) Fazer login com sucesso (existindo token em `localStorage`).
2) Navegar manualmente para `/auth`.

## Resultado atual
- A tela de autenticação continua acessível mesmo com usuário “logado” (token presente).

## Resultado esperado
- Se existir token, `/auth` deve redirecionar para `/dashboard` para evitar fluxo confuso.

## Critério de correção
- Com token presente, acessar `/auth` redireciona automaticamente para `/dashboard`.
- Sem token, `/auth` continua acessível.
