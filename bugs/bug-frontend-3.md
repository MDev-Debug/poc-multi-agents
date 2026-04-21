# Bug Frontend 3 — Erros de validação da API não são exibidos de forma útil

## Como reproduzir
1) Abrir `/auth`.
2) Forçar um request inválido (ex.: e-mail inválido via devtools/console ou desabilitando validação do browser).
3) Submeter login/cadastro.

## Resultado atual
- Para respostas 400 (validation problem details), a UI costuma cair no fallback: `Não foi possível concluir. Tente novamente.`
- O usuário não recebe feedback específico do que corrigir (além do genérico).

## Resultado esperado
- A UI deve exibir mensagens úteis e amigáveis vindas da API quando houver validação (ex.: primeiro erro encontrado), mantendo o padrão de mensagens claras do contrato da Sprint 1.

## Evidência
- O handler de erro lê apenas `err?.error?.message`, mas respostas 400 de validação normalmente não vêm com `message`.

## Critério de correção
- Em erros 400 de validação, a UI extrai e exibe ao menos uma mensagem específica (por campo ou primeira mensagem), sem quebrar o fluxo atual.
