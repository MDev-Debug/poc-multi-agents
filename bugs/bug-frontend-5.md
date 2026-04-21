# Bug Frontend 5 — Sidebar fechada perde nome acessível (Home/Chat/Logout)

## Como reproduzir
1) Entrar no `/dashboard` autenticado.
2) Fechar a sidebar (hamburger).
3) Tentar identificar os botões por acessibilidade (screen reader) ou simplesmente por “hover”/tooltip.

## Resultado atual
- Quando a sidebar está fechada, os textos (`Home`, `Chat`, `Logout`) não são renderizados.
- Os botões ficam sem nome acessível claro (apenas elementos visuais), piorando navegação por teclado/leitores.

## Resultado esperado
- Mesmo com sidebar fechada, cada item deve ter nome acessível (`aria-label`) e, idealmente, `title` para tooltip.

## Critério de correção
- Com a sidebar fechada, Home/Chat/Logout continuam com nome acessível (ex.: `aria-label="Home"`).
- Com sidebar aberta, comportamento atual permanece.
