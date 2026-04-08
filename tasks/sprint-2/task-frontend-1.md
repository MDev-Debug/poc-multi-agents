# Frontend — Dashboard elegante + Sidebar com hamburger (Sprint 2)

## Contexto
O dashboard na Sprint 1 é placeholder e o design está simples. Precisamos refatorar para um visual mais elegante cyberpunk e adicionar sidebar animada.

## Escopo
- Inclui:
  - Layout do dashboard com sidebar.
  - Hamburger abre/fecha com animação suave.
  - Menus: Home, Chat, Logout (sem features reais ainda).
- Não inclui:
  - Implementação de telas Home/Chat.

## Dependências
- Sprint 1 frontend concluída.

## Requisitos (funcionais)
- Sidebar deve alternar estado (aberta/fechada).
- Itens Home/Chat podem ser placeholders (sem navegação funcional).
- Logout:
  - Limpa tokens locais.
  - Redireciona para `/auth`.

## Requisitos UX/UI
- Tema cyberpunk, fundo escuro e acentos neon.
- Design deve ser mais elegante (melhor espaçamento, tipografia, contraste, estados hover/foco).
- Sidebar com movimento suave (open/close).

## Critérios de aceitação (Given/When/Then)
- Given o dashboard, When eu clico no hamburger, Then a sidebar abre/fecha com animação.
- Given o dashboard, When eu clico em Logout, Then sou redirecionado para `/auth` e não consigo voltar ao dashboard sem logar.

## Definição de pronto (DoD)
- Build ok
- UI aderente ao contrato
