# Backend — Presence via SignalR (usuários online reais) (Sprint 4)

## Objetivo/escopo
Disponibilizar uma API de presença para listar usuários online (logados com sessão ativa no site).

## Definição de “online”
- Um usuário é considerado online se tiver feito `heartbeat` autenticado nos últimos 30s.

## Hub
- `GET /hubs/presence` (SignalR, JWT obrigatório)
  - JWT deve ser aceito via query string `access_token` (padrão SignalR no browser).

## Contrato de mensagens
- Server -> Clients: `OnlineUsers` (payload: lista de usuários online)
- Client -> Server: `GetOnlineUsers` (retorna lista atual)

## Critérios de aceitação (Given/When/Then)
- Given um usuário autenticado, When ele conecta no hub, Then ele aparece na lista enviada via `OnlineUsers`.
- Given 2 usuários autenticados e com hub conectado, When ambos estiverem com sessão ativa, Then ambos aparecem como online.
- Given um usuário que fecha a aba (desconecta), When o servidor processa o disconnect, Then ele deixa de aparecer na lista.

## Definição de pronto (DoD)
- Build ok
- OpenAPI expõe os endpoints
