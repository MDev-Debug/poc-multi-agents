# Bug Frontend 6 — Implementação de tabs com ARIA incompleta

## Como reproduzir
1) Abrir `/auth`.
2) Inspecionar estrutura de tabs (Login/Cadastro) e tentar navegação via leitor de tela.

## Resultado atual
- Existem `role="tablist"` e `role="tab"`, mas faltam ligações completas (`aria-controls`/`id`) e não existe `role="tabpanel"`.

## Resultado esperado
- Tabs devem seguir o padrão WAI-ARIA mínimo:
  - Cada tab com `id`.
  - `aria-controls` apontando para um `tabpanel`.
  - Conteúdo do formulário envolvido em `role="tabpanel"` com `id`.

## Critério de correção
- Estrutura ARIA completa para tabs/tabpanel sem regressão visual.
- Troca de modo continua funcionando.
