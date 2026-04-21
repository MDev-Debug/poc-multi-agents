# Bug Frontend 7 — Falta foco visível em elementos interativos

## Como reproduzir
1) Abrir `/auth`.
2) Navegar usando teclado (tecla Tab).
3) Repetir no `/dashboard`.

## Resultado atual
- Alguns elementos não exibem `:focus-visible` perceptível (ex.: tabs, botões principais, itens do menu).

## Resultado esperado
- Conforme contrato da Sprint 1, foco visível deve existir para navegação por teclado.

## Critério de correção
- Tabs, botões (`Entrar/Cadastrar`), itens do menu e botão hamburger exibem foco visível consistente (sem depender do mouse).
