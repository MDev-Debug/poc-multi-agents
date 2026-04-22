# Bug 1 — Segredos sensíveis commitados em appsettings.Development.json

## Task relacionada
- `patches/security/sprint-6/patch-backend-2.md`

## Como reproduzir
1. Clonar ou ter acesso ao repositório git.
2. Abrir o arquivo `backend/Chat.Api/appsettings.Development.json`.
3. Observar os valores das chaves `ConnectionStrings:DefaultConnection`, `Jwt:SigningKey` e `Encryption:Key`.

## Resultado esperado
- `appsettings.Development.json` **não deve estar rastreado pelo git** (deve constar no `.gitignore`)
  ou **não deve conter segredos** — apenas placeholders vazios.
- Segredos reais devem existir somente em `dotnet user-secrets` local ou em variáveis de ambiente,
  conforme especificado no patch-backend-2 (seção 2 e 7).

## Resultado atual
- O arquivo `backend/Chat.Api/appsettings.Development.json` está rastreado pelo git (não consta no `.gitignore`)
  e contém os seguintes segredos em texto claro:

  ```json
  {
    "Encryption": {
      "Key": "zpBp7QmmktrAm+L5n+ajzFUl+v0FbQulYdNnfxqfsZs="
    },
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
    },
    "Jwt": {
      "SigningKey": "dev-signing-key-change-me-32-bytes-min!"
    }
  }
  ```

- O `.gitignore` exclui `**/appsettings.Local.json` mas **não** exclui `appsettings.Development.json`.

## Evidência
- Arquivo: `M:/Dev/poc-agentes/poc-multi-agentes/backend/Chat.Api/appsettings.Development.json` linhas 8–16
- `.gitignore` linha 150: exclui apenas `**/appsettings.Local.json`
- patch-backend-2, seção 2: "Definir segredos via variáveis de ambiente" e "alternativa para dev: `dotnet user-secrets` (não comita em git)"
- patch-backend-2, seção 7: instruções explícitas de `.gitignore` para `**/secrets.json`, `**/.env`, etc.

## Severidade
- [x] Alto (funcionalidade principal quebrada)

> Justificativa: A chave de assinatura JWT (`SigningKey`) exposta permite que qualquer pessoa com acesso ao
> repositório forje tokens válidos, ganhando acesso autenticado à API com qualquer `userId`. A chave de
> criptografia (`Encryption:Key`) exposta anula a proteção das mensagens cifradas. A connection string
> expõe credenciais do banco. Embora o ambiente seja de desenvolvimento local, qualquer repositório privado
> comprometido ou colaborador não autorizado tem acesso imediato a todos os segredos.

## Critério de correção (Given/When/Then)
- Given que `appsettings.Development.json` existe no repositório
- When o arquivo é inspecionado
- Then ele NÃO deve conter nenhum segredo — `SigningKey`, `DefaultConnection` e `Encryption:Key` devem ser
  strings vazias ou o arquivo deve ser excluído e adicionado ao `.gitignore`
- AND os segredos reais devem ser configurados via `dotnet user-secrets` (dev) ou variáveis de ambiente
- AND o `.gitignore` deve incluir o padrão `**/appsettings.Development.json` ou equivalente que cubra
  esse arquivo quando contiver segredos

## Status
- [x] Aberto
- [ ] Em correção
- [ ] Corrigido — aguardando re-teste
- [ ] Fechado
