# Bug Frontend 1 — Mensagem de sucesso não aparece (redirect imediato)

## Como reproduzir
1) Abrir `/auth`.
2) Fazer login ou cadastro com credenciais válidas.
3) Observar o comportamento após sucesso.

## Resultado atual
- A tela redireciona imediatamente para `/dashboard`.
- A mensagem de sucesso (`Login realizado.` / `Cadastro realizado.`) não chega a ser percebida.

## Resultado esperado
- Deve haver uma confirmação visual **breve** de sucesso antes do redirecionamento (conforme contrato de UX/UI da Sprint 1), ou alternativamente remover a mensagem e não exibir estado de “sucesso” que não é visível.

## Evidência
- Implementação atual navega para `/dashboard` imediatamente após setar `okMessage`.

## Critério de correção
- A confirmação de sucesso torna-se perceptível (ex.: atraso curto antes do redirect), ou a UI não exibe uma mensagem que nunca é vista.
- Fluxo continua redirecionando para `/dashboard` em sucesso.
