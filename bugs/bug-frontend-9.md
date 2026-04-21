# Bug Frontend 9 — Mensagem de erro persiste após usuário corrigir campos

## Como reproduzir
1) Abrir `/auth`.
2) Causar um erro (ex.: senha errada no login).
3) Alterar o e-mail ou senha.

## Resultado atual
- `errorMessage` permanece visível até a próxima submissão/troca de modo.

## Resultado esperado
- Ao usuário alterar qualquer campo, a mensagem de erro deve ser limpa para não “grudar” e confundir.

## Critério de correção
- Alterações em `email` ou `password` limpam `errorMessage` automaticamente.
