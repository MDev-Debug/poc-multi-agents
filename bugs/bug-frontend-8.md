# Bug Frontend 8 — Estado de carregando permite interação incoerente

## Como reproduzir
1) Abrir `/auth`.
2) Submeter login/cadastro.
3) Enquanto o request está em andamento, tentar:
   - Trocar de aba (Login/Cadastro)
   - Editar campos

## Resultado atual
- Apenas o botão de submit é desabilitado; ainda é possível trocar modo e editar campos durante o request.

## Resultado esperado
- No estado `loading`, a UI deve bloquear interações conflitantes (tabs/inputs) e deixar claro que está processando.

## Critério de correção
- Enquanto `loading=true`, tabs e inputs ficam desabilitados (ou interação equivalente), mantendo feedback visual.
