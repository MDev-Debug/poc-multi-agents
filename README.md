# Desenvolvimento Assistido por IA (Multi-Agentes)

Este repositório foi desenvolvido **inteiramente com assistência de Inteligência Artificial (IA)**, com **orientação e validação humana (Desenvolvedor)**.

## Como foi feito
- O trabalho é conduzido por um fluxo **multi-agentes** (IA), onde cada agente possui um papel específico.
- Um **Desenvolvedor** orienta o objetivo, define restrições e valida os resultados (build/test/execução local), conduzindo o projeto até a entrega.

## Fluxo de trabalho (multi-agentes)
1) **Product Owner (PO)**
   - Recebe um único prompt inicial de feature.
   - Documenta o planejamento em `planning/sprint-N.md`.
   - Quebra o escopo em tasks em `tasks/task-backend-N.md` e `tasks/task-frontend-N.md`.
   - Define/aprova o contrato de UX/UI (tema cyberpunk: fundo escuro + acentos neon).

2) **Dev Backend**
   - Lê `tasks/task-backend-*.md` e implementa o backend.
   - Stack: **.NET 10** + **SQL Server local**.

3) **Dev Frontend**
   - Lê `tasks/task-frontend-*.md` e implementa o frontend.
   - Stack: **Angular (última versão)**.

4) **QA**
   - Testa backend e frontend contra os critérios das tasks.
   - Registra bugs em `bugs/bug-backend-N.md` ou `bugs/bug-frontend-N.md`.

5) **Encerramento (PO)**
   - Após QA OK e “de acordo” do PO, é feito o **commit final** com a mensagem descrevendo o que foi desenvolvido.

## Autonomia
- Os agentes operam com **autonomia**, executando comandos e alterações no repositório **sem necessidade de aprovação a cada passo**, exceto quando houver bloqueio real (ambiguidade material, alto impacto, segredos/credenciais, ou risco de compliance/segurança).

## Observações
- Apesar do uso extensivo de IA, a responsabilidade por revisão, validação e decisões finais permanece humana.
- O objetivo deste repositório é demonstrar um pipeline de entrega local com **multi-agentes** e governança simples por artefatos (`planning/`, `tasks/`, `bugs/`).
