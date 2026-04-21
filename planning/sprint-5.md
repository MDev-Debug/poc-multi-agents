# Sprint 5 — Refatoração completa: arquitetura + design + presença online

## Prompt inicial (original)
"Quero que siga as regras, o design está ruim. A arquitetura dos projetos está muito bagunçada! Quero que refaça tudo desde arquitetura dos projetos e design de interface, o menu de usuarios online não funciona, quero que ao estar logado já veja os usuarios online"

## Objetivo e valor
- Objetivo: Refatorar completamente backend (Clean Architecture) e frontend (estrutura Angular padrão + SCSS), redesenhar a interface com cyberpunk de qualidade e corrigir o bug dos usuários online.
- Para quem: Desenvolvedor que quer ver o projeto com arquitetura sólida e UI profissional.
- Valor esperado: Código organizado, escalável e uma interface que impressiona visualmente — com usuários online funcionando ao logar.

## Escopo

### Inclui:
- **Backend**: Migrar de projeto único para Clean Architecture (Chat.Domain, Chat.Application, Chat.Infrastructure, Chat.Api)
- **Frontend**: Reorganizar estrutura para core/shared/features + migrar de CSS para SCSS
- **UI Auth**: Redesenhar completamente a página de autenticação (login/cadastro)
- **UI Dashboard**: Redesenhar dashboard com layout profissional: sidebar esquerda (Home, Chat, Logout) + painel de usuários online à direita no Chat
- **Bug fix**: Usuários online devem aparecer imediatamente ao entrar no dashboard após login

### Não inclui:
- Chat funcional (mensagens)
- 2FA / recuperação de senha
- Deploy / CI-CD

## Assunções e dependências
- Backend continua rodando em `http://localhost:5000`
- Frontend em `http://localhost:4200`
- SQL Server local (Database: CHAT)
- Migrations existentes são preservadas
- O SignalR Hub `/hubs/presence` continua no mesmo path

## Contrato de UX/UI (aprovado pelo PO)

### Tema cyberpunk — tokens obrigatórios (SCSS)
```scss
--bg-deep:    #05050d;   // fundo mais escuro
--bg-panel:   #0c0c1e;   // painéis/cards
--bg-surface: #12122a;   // superfícies secundárias
--border:     rgba(0, 246, 255, 0.15);
--text:       #e8e8ff;
--text-muted: rgba(232, 232, 255, 0.5);
--neon-cyan:  #00f6ff;   // cor primária
--neon-pink:  #ff2bd6;   // cor de destaque
--neon-green: #39ff88;   // sucesso/online
--danger:     #ff3b5c;
--glow-cyan:  0 0 12px rgba(0, 246, 255, 0.5);
--glow-pink:  0 0 12px rgba(255, 43, 214, 0.5);
```

### Auth Page
- Layout: página full-height com card centralizado
- Card: fundo `--bg-panel`, borda neon sutil, glow no hover
- Logo/título: fonte grande, gradiente cyan→pink
- Tabs: Login / Cadastro com underline neon animado
- Inputs: fundo `--bg-surface`, borda neon no focus, ícones
- Botão CTA: gradiente cyan→pink, glow no hover, spinner no loading
- Mensagens de erro: inline abaixo do campo

### Dashboard
- Layout: sidebar esquerda fixa (240px) + área principal
- Sidebar:
  - Topo: avatar/inicial do usuário + email truncado
  - Navegação: Home, Chat (ícones + labels)
  - Rodapé: botão Logout com ícone
  - Itens ativos: highlight neon lateral
- Chat View (default ao logar):
  - Área central: espaço reservado "Selecione um usuário para conversar"
  - Painel direito (280px): lista de usuários online
    - Indicador verde pulsante por usuário
    - Email e "há X min" como última atividade
    - Contador total no topo do painel
- Home View: placeholder elegante com ícone
- Topbar: título da seção + indicador de conexão SignalR

## Plano de execução
- task-backend-1: Clean Architecture — reestruturação do projeto
- task-frontend-1: Reorganizar estrutura Angular + migrar para SCSS
- task-frontend-2: Redesenhar Auth Page
- task-frontend-3: Redesenhar Dashboard + corrigir bug usuários online

## Encerramento
Após QA OK: registrar "de acordo" e executar commit final.
