# Relatório de Segurança — Sprint 6
**Data**: 2026-04-21
**Analista**: Especialista em Segurança Sênior
**Escopo**: Código-fonte backend (`Chat.Api`, `Chat.Application`, `Chat.Infrastructure`, `Chat.Domain`) e frontend (`frontend/src/`)
**Sprint atual**: Sprint 6 (Chat privado com SignalR)

---

## Sumário Executivo

A análise identificou **10 vulnerabilidades** distribuídas entre backend e frontend, sendo **1 crítica**, **3 altas**, **4 médias** e **2 baixas**.

A vulnerabilidade mais grave é o armazenamento de mensagens de chat em **texto plano no banco de dados** — qualquer acesso ao banco (DBA, backup, dump por SQL injection) expõe todo o histórico de conversas privadas. Um patch completo com AES-256-GCM foi criado (patch-backend-1).

Em positivo, o projeto já implementa corretamente: hash de senhas (`PasswordHasher` Identity), refresh token rotation com SHA-256, validação JWT com issuer/audience/lifetime, `[Authorize]` nos hubs e controllers, e Angular protege contra XSS via template engine.

---

## Tabela de Vulnerabilidades

| ID | Componente | Severidade | Título | Patch |
|---|---|---|---|---|
| VUL-B1a | Backend | **CRÍTICA** | Mensagens em texto plano no banco | patch-backend-1 |
| VUL-B2a | Backend | **ALTA** | Chave JWT hardcoded em appsettings.json | patch-backend-2 |
| VUL-B2b | Backend | **ALTA** | Connection string com senha no repositório | patch-backend-2 |
| VUL-B2c | Backend | MÉDIA | Sem rate limiting em autenticação e SignalR | patch-backend-2 |
| VUL-B2d | Backend | MÉDIA | JWT com expiração de 120 minutos | patch-backend-2 |
| VUL-B3a | Backend | MÉDIA | DTOs sem MaxLength (risco de DoS no hash) | patch-backend-3 |
| VUL-B3c | Backend | MÉDIA | Ausência de cabeçalhos de segurança HTTP | patch-backend-3 |
| VUL-F1b | Frontend | BAIXA | URLs hardcoded (HTTP, não HTTPS) | patch-frontend-1 |
| VUL-F1c | Frontend | BAIXA | Sem interceptor de renovação automática de token | patch-frontend-1 |
| VUL-F2d | Frontend | MÉDIA | Validação de senha fraca no formulário (minLength 6) | patch-frontend-2 |

---

## Análise Detalhada

### VUL-B1a — CRÍTICA: Mensagens em Texto Plano no Banco

**Arquivo**: `backend/Chat.Domain/Entities/Message.cs` L8, `backend/Chat.Infrastructure/Services/MessageService.cs` L22, Migration `AddMessages`

**Evidência**:
```csharp
// Message.cs
public string Content { get; set; } = string.Empty;

// AddMessages migration
Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)

// MessageService.cs — salva direto, sem cifrar
var message = new Message { ..., Content = content, ... };
```

**Impacto**: Qualquer pessoa com acesso ao banco SQL Server lê todas as conversas. Backups, logs de query, dumps de emergência, e potencial SQL injection (caso surgisse) expõem o conteúdo integral das mensagens privadas. Viola LGPD Art. 46 (medidas técnicas para proteger dados pessoais).

**Solução**: patch-backend-1 — implementação completa de `IEncryptionService` com AES-256-GCM, migration de schema para `nvarchar(max)`, e script de migração de dados existentes. Chave gerida via variável de ambiente `CHAT_ENCRYPTION_KEY`.

---

### VUL-B2a / VUL-B2b — ALTA: Segredos em Repositório Git

**Arquivo**: `backend/Chat.Api/appsettings.json`

**Evidências**:
```json
"SigningKey": "dev-signing-key-change-me-32-bytes-minimum",
"DefaultConnection": "...Password=YourStrong!Passw0rd..."
```

**Impacto da SigningKey exposta**: Qualquer pessoa pode forjar tokens JWT válidos com qualquer `userId` e `email`, ganhando acesso total à API e ao chat de qualquer usuário.

**Impacto da Connection string**: Acesso irrestrito ao banco SQL Server, incluindo leitura de todos os dados, execução de comandos DDL/DML.

**Solução**: patch-backend-2 — strings vazias no `appsettings.json` commitado, valores via `dotnet user-secrets` (dev) ou variáveis de ambiente (produção). Adicionada validação que impede o servidor de iniciar sem `SigningKey` configurada.

---

### VUL-B2c — MÉDIA: Ausência de Rate Limiting

**Arquivo**: `backend/Chat.Api/Program.cs`, `backend/Chat.Api/Controllers/AuthController.cs`, `backend/Chat.Api/Hubs/ChatHub.cs`

**Impacto**:
- `/api/auth/login`: brute-force de senhas sem qualquer throttle.
- `ChatHub.SendPrivateMessage`: um cliente malicioso pode enviar milhares de mensagens por segundo, estourando a capacidade do banco e a memória do servidor.

**Solução**: patch-backend-2 — `AddRateLimiter` com `FixedWindowLimiter` (10 req/min para auth, 60 msg/min por conexão SignalR).

---

### VUL-B2d — MÉDIA: JWT com Expiração Excessiva

**Arquivo**: `backend/Chat.Api/appsettings.json` + `JwtOptions.cs`

**Evidência**: `"ExpirationMinutes": 120`

**Impacto**: Token comprometido (ex: via XSS ou sniffing em rede não-TLS) permanece válido por 2 horas sem possibilidade de revogação. O refresh token rotation já está implementado corretamente, o que torna seguro reduzir o access token para 15 minutos.

**Solução**: patch-backend-2 — reduzir para 15 minutos. O token-refresh interceptor (patch-frontend-1) renova automaticamente.

---

### VUL-B3a — MÉDIA: DTOs sem Limite Máximo de Tamanho

**Arquivo**: `backend/Chat.Application/DTOs/Auth/RegisterRequest.cs`, `LoginRequest.cs`

**Evidência**: `[MinLength(6)]` sem `[MaxLength]` na senha.

**Impacto**: `PasswordHasher<T>` do Identity usa PBKDF2 internamente — uma senha de 1 MB causaria consumo de CPU significativo por requisição (DoS lento/Billion Laughs análogo). Risco baixo mas real em APIs públicas.

**Solução**: patch-backend-3 — `[MaxLength(128)]` na senha, `[MaxLength(320)]` no email (RFC 5321 limit).

---

### VUL-B3c — MÉDIA: Ausência de Cabeçalhos de Segurança HTTP

**Arquivo**: `backend/Chat.Api/Program.cs`

**Cabeçalhos ausentes**: `X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security`, `Referrer-Policy`.

**Impacto**: Sem `X-Frame-Options: DENY`, a aplicação pode ser embarcada em um iframe para ataques de clickjacking. Sem `HSTS`, navegadores não forçam HTTPS mesmo quando disponível.

**Solução**: patch-backend-3 — middleware de cabeçalhos + `UseHttpsRedirection` em produção.

---

### VUL-F1b — BAIXA: URLs Hardcoded com HTTP

**Arquivos**: `auth.service.ts`, `message-api.service.ts`, `chat-hub.service.ts`, `presence-hub.service.ts`

**Impacto**: Em produção, se as URLs não forem alteradas manualmente, tokens JWT trafegam sobre HTTP não criptografado.

**Solução**: patch-frontend-1 — arquivos `environment.ts` com `apiUrl` configurável por ambiente.

---

### VUL-F1c — BAIXA: Sem Renovação Automática de Token

**Arquivo**: `frontend/src/app/core/interceptors/auth.interceptor.ts`

**Impacto**: Com `ExpirationMinutes: 15` (após o patch), o usuário receberá erros 401 silenciosos após 15 minutos sem que a UI reaja. O `authGuard` não verifica expiração, apenas presença.

**Solução**: patch-frontend-1 — `token-refresh.interceptor.ts` que detecta 401, chama `/api/auth/refresh`, retry automático da requisição original. `authGuard` atualizado para verificar `exp` do JWT payload.

---

### VUL-F2d — MÉDIA: Validação de Senha Fraca no Frontend

**Arquivo**: `frontend/src/app/features/auth/auth.component.ts`

**Evidência**: `Validators.minLength(6)` — inconsistente com o backend após o patch (minLength 8) e sem validação de complexidade.

**Solução**: patch-frontend-2 — `minLength(8)`, `maxLength(128)`, `passwordStrengthValidator` customizado.

---

## O que está correto (pontos positivos)

| Aspecto | Status | Detalhe |
|---|---|---|
| Hash de senhas | OK | `PasswordHasher<AppUser>` (PBKDF2+SHA512) do Identity |
| Refresh token rotation | OK | SHA-256 do token armazenado, revogação por `RevokedAt` |
| Refresh token expiration | OK | 14 dias com verificação de `ExpiresAt` e `RevokedAt` |
| JWT — validação completa | OK | issuer, audience, lifetime, signing key todos validados |
| `[Authorize]` nos Hubs | OK | `PresenceHub` e `ChatHub` decorados |
| `[Authorize]` no Controller | OK | `MessagesController` decorado |
| JWT via query string SignalR | OK | Pattern correto no `OnMessageReceived` |
| EF Core parameterizado | OK | Sem SQL raw — sem risco de SQL injection |
| Angular template escaping | OK | Sem `[innerHTML]` nem `bypassSecurityTrust*` |
| CORS restrito | OK | Apenas `http://localhost:4200` permitido (dev) |
| Refresh token em hash | OK | Não armazena o token raw, apenas `SHA-256(token)` |
| Validação de tamanho no Hub | OK | `content.Length > 4000` verificado em `SendPrivateMessage` |
| DTOs de Auth com `[Required]` | OK | Email e Password com anotações básicas |
| ConcurrentDictionary no tracker | OK | Thread-safe para múltiplas conexões |

---

## Análise de Dependências

### Backend — NuGet

| Pacote | Versão | Observação |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | Atual para .NET 10 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.5 | Atual |
| `Microsoft.IdentityModel.Tokens` | 8.9.0 | Verificar CVEs — série 8.x |
| `System.IdentityModel.Tokens.Jwt` | 8.9.0 | Verificar CVEs — série 8.x |
| `Microsoft.Extensions.Identity.Core` | 10.0.5 | Atual |

**Ação recomendada**: Executar `dotnet list package --vulnerable` para verificar CVEs conhecidos nos pacotes instalados. A versão 8.9.0 do `Microsoft.IdentityModel.Tokens` é recente e não possui CVEs conhecidos na data desta análise.

### Frontend — npm

| Pacote | Versão | Observação |
|---|---|---|
| `@angular/core` | ^21.2.0 | Atual |
| `@microsoft/signalr` | ^10.0.0 | Atual para SignalR 10 |
| `rxjs` | ~7.8.0 | Estável |

**Ação recomendada**: Executar `npm audit` na pasta `frontend/` para verificar vulnerabilidades conhecidas nos pacotes transitivos.

---

## Priorização de Correção

| Prioridade | ID | Ação | Esforço |
|---|---|---|---|
| 1 | VUL-B1a | Criptografar mensagens (AES-256-GCM) | Alto (patch-backend-1) |
| 2 | VUL-B2a + VUL-B2b | Remover segredos do git | Baixo (patch-backend-2) |
| 3 | VUL-B2c | Adicionar rate limiting | Médio (patch-backend-2) |
| 4 | VUL-B2d | Reduzir expiração JWT para 15 min | Baixo (patch-backend-2) |
| 5 | VUL-F1c | Token refresh interceptor | Médio (patch-frontend-1) |
| 6 | VUL-B3a + VUL-F2d | MaxLength nos DTOs e formulários | Baixo (patch-backend-3, patch-frontend-2) |
| 7 | VUL-B3c | Cabeçalhos de segurança HTTP | Baixo (patch-backend-3) |
| 8 | VUL-F1b | URLs via environment | Baixo (patch-frontend-1) |

---

## Patches Gerados

| Arquivo | Conteúdo |
|---|---|
| `patches/sprint-6/patch-backend-1.md` | `IEncryptionService` + AES-256-GCM + Migration + DI |
| `patches/sprint-6/patch-backend-2.md` | Segredos via env vars + Rate limiting + JWT 15 min |
| `patches/sprint-6/patch-backend-3.md` | Validação DTOs + Cabeçalhos HTTP + HTTPS + CORS |
| `patches/sprint-6/patch-frontend-1.md` | Environment files + Token refresh interceptor + authGuard |
| `patches/sprint-6/patch-frontend-2.md` | SafeTextPipe + maxlength textarea + senha forte |

---

## Conformidade LGPD

Com a aplicação dos patches, o projeto atende aos principais requisitos da LGPD para proteção de dados pessoais:

- **Art. 46** (segurança): Mensagens cifradas em repouso (AES-256-GCM) e em trânsito (HTTPS + WSS em produção)
- **Art. 46** (acesso minimizado): Rate limiting e autenticação obrigatória em todos os endpoints
- **Art. 46** (integridade): GCM tag garante integridade dos dados cifrados

Sem os patches, o projeto viola o Art. 46 pela exposição de mensagens privadas em texto plano.
