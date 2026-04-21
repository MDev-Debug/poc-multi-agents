# Bug Frontend 2 — Autocomplete da senha incorreto no Cadastro

## Como reproduzir
1) Abrir `/auth`.
2) Trocar para o modo `Cadastro`.
3) Inspecionar o campo de senha.

## Resultado atual
- O campo senha usa `autocomplete="current-password"` mesmo no modo `Cadastro`.

## Resultado esperado
- No modo `Cadastro`, o campo deve usar `autocomplete="new-password"`.
- No modo `Login`, manter `autocomplete="current-password"`.

## Evidência
- O atributo `autocomplete` está fixo na view.

## Critério de correção
- `Cadastro` usa `new-password` e `Login` usa `current-password`.
