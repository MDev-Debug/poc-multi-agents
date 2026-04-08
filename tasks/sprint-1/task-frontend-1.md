# Frontend — Scaffold Angular + Tela Inicial de Autenticação (Sprint 1)

## Contexto
Precisamos do app Angular com uma tela inicial de autenticação (cadastro e login) com tema cyberpunk.

## Escopo
- Inclui:
  - Criar projeto Angular.
  - Rota/página `/auth` (ou `/`) com formulário de Login e Cadastro.
  - Estilização cyberpunk (fundo escuro + acentos neon) conforme contrato do PO.
- Não inclui:
  - Dashboard funcional (Sprint 2).

## Dependências
- Node + Angular CLI

## Requisitos (funcionais)
- Exibir dois estados (Login e Cadastro) com alternância clara.
- Validações de formulário:
  - Email obrigatório e formato válido.
  - Senha obrigatória (mínimo razoável, ex.: 6+).
- Estados:
  - Carregando ao enviar.
  - Erro visível quando validação falhar.

## Requisitos UX/UI (quando aplicável)
- Fluxo principal:
  - Usuário entra → escolhe Login/Cadastro → preenche → envia.
- Estados: vazio, carregando, erro, sucesso.
- Acessibilidade:
  - Inputs com labels.
  - Foco visível.
  - Navegação por teclado.
- Tema:
  - Cyberpunk (escuro + neon), consistente.

## Critérios de aceitação (Given/When/Then)
- Given a tela de autenticação, When eu alterno Login/Cadastro, Then vejo os campos correspondentes.
- Given formulário inválido, When eu tento enviar, Then vejo mensagens de validação e nada é enviado.
- Given envio em progresso, When eu clico no botão, Then o botão mostra loading e fica desabilitado.

## Definição de pronto (DoD)
- `npm run build` (ou equivalente) funciona.
- UI aderente ao contrato do PO.
