# task-frontend-2 — Redesenhar Auth Page

## Contexto
A página de autenticação atual é funcional mas tem design básico. Redesenhar completamente com UX profissional e tema cyberpunk de qualidade.

## Escopo
- Inclui:
  - Redesign completo do HTML + SCSS da auth page
  - Preservar toda lógica TypeScript existente (formulário, validação, chamadas de API)
  - Animações sutis de entrada
  - Design system do styles.scss aplicado
- Não inclui:
  - Alterações no AuthService ou backend

## Requisitos UX/UI

### Layout
- Fundo: `--bg-deep` com partículas/grid sutil via CSS (pseudo-elemento)
- Card centralizado: max-width 420px, padding 40px
- Card: fundo `--bg-panel`, border 1px `--border`, border-radius 16px
- Sombra do card: `0 8px 48px rgba(0, 246, 255, 0.08)`

### Cabeçalho do card
- Logo: texto "CHAT" com gradiente linear cyan → pink, font-size 2.5rem, font-weight 900, letter-spacing 0.3em
- Subtítulo: "Sistema Multi-Agente" em `--text-muted`, font-size 0.8rem

### Tabs Login / Cadastro
- Dois botões lado a lado com fundo transparente
- Tab ativa: texto branco + underline neon cyan com transição 300ms
- Tab inativa: texto `--text-muted`

### Inputs
- Fundo: `--bg-surface`
- Borda: 1px solid `--border`
- Border-radius: 8px
- Padding: 12px 16px
- Cor do texto: `--text`
- Focus: border-color `--neon-cyan`, box-shadow glow cyan
- Placeholder: `--text-muted`
- Label flutuante OU label acima do input (padrão), font-size 0.8rem, `--text-muted`

### Botão de submit
- Background: gradiente linear 135deg, `--neon-cyan` → `--neon-pink`
- Cor do texto: `#05050d` (escuro para contraste)
- Font-weight: 700
- Border-radius: 8px
- Padding: 14px
- Width: 100%
- Hover: box-shadow glow cyan + pink, slight scale(1.01)
- Loading: substituir texto por spinner animado (CSS puro)
- Disabled: opacity 0.6

### Mensagens de feedback
- Erro: texto `--danger`, ícone ⚠ à esquerda, animate fadeIn
- Sucesso: texto `--neon-green`, ícone ✓ à esquerda

### Responsividade
- Mobile: card com margin 16px, sem overflow horizontal

## Estados obrigatórios
- Vazio: formulário limpo, botão habilitado
- Loading: botão com spinner, inputs desabilitados
- Erro: mensagem de erro inline
- Sucesso: feedback visual antes de redirecionar

## Critérios de aceitação
- Given: usuário na /auth
- When: renderização inicial
- Then: card centralizado com tema cyberpunk, nenhum layout quebrado

- Given: usuário clica em "Cadastro"
- When: tab muda
- Then: animação de underline neon, formulário troca de modo

- Given: usuário submete com campos inválidos
- When: validação falha
- Then: mensagem de erro visível sem recarregar

- Given: login com credenciais válidas
- When: submit
- Then: spinner no botão → redireciona para /dashboard

## Definição de pronto (DoD)
- [ ] Design implementado conforme contrato
- [ ] Todos os estados de UI presentes
- [ ] Lógica TypeScript preservada intacta
- [ ] `ng build` passa
- [ ] Responsivo (mobile ok)
