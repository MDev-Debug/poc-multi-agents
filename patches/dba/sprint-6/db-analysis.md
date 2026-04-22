# Análise Global do Banco de Dados — poc-multi-agentes

**Data da análise:** 2026-04-21
**Analista:** DBA Sênior (Claude Code)
**Stack:** .NET 10 + EF Core 10.0.6 + SQL Server
**Database:** CHAT

---

## 1. Inventário de Artefatos Analisados

| Tipo | Arquivo |
|---|---|
| Entidade | `Chat.Domain/Entities/AppUser.cs` |
| Entidade | `Chat.Domain/Entities/RefreshToken.cs` |
| Entidade | `Chat.Domain/Entities/Message.cs` |
| DbContext | `Chat.Infrastructure/Data/ChatDbContext.cs` |
| Migration | `20260408024425_InitialAuth.cs` |
| Migration | `20260408030711_RefreshTokens.cs` |
| Migration | `20260421040106_AddMessages.cs` |
| Snapshot | `ChatDbContextModelSnapshot.cs` |

---

## 2. Schema Completo Mapeado

### 2.1 Tabela: `Users`

**Migration de origem:** `InitialAuth` (20260408024425)

| Coluna | Tipo SQL | Nullable | Constraint | Observações |
|---|---|---|---|---|
| `Id` | `uniqueidentifier` | NOT NULL | PK (`PK_Users`) | GUID, gerado na inserção via EF |
| `Email` | `nvarchar(320)` | NOT NULL | — | Limite correto para RFC 5321 |
| `PasswordHash` | `nvarchar(max)` | NOT NULL | — | Hash ASP.NET Identity |
| `CreatedAt` | `datetimeoffset` | NOT NULL | — | Timezone-aware |

**Índices:**

| Nome | Colunas | Tipo |
|---|---|---|
| `PK_Users` | `Id` | Clustered (PK) |
| `IX_Users_Email` | `Email` | Unique Non-Clustered |

**Foreign Keys:** nenhuma (tabela raiz)

---

### 2.2 Tabela: `RefreshTokens`

**Migration de origem:** `RefreshTokens` (20260408030711)

| Coluna | Tipo SQL | Nullable | Constraint | Observações |
|---|---|---|---|---|
| `Id` | `uniqueidentifier` | NOT NULL | PK (`PK_RefreshTokens`) | GUID |
| `UserId` | `uniqueidentifier` | NOT NULL | FK → `Users.Id` | Cascade DELETE |
| `TokenHash` | `nvarchar(128)` | NOT NULL | — | SHA-256 em hex (64 chars), max=128 é folga adequada |
| `ExpiresAt` | `datetimeoffset` | NOT NULL | — | Timezone-aware |
| `CreatedAt` | `datetimeoffset` | NOT NULL | — | Timezone-aware |
| `RevokedAt` | `datetimeoffset` | NULL | — | NULL = token ativo |

**Índices:**

| Nome | Colunas | Tipo |
|---|---|---|
| `PK_RefreshTokens` | `Id` | Clustered (PK) |
| `IX_RefreshTokens_TokenHash` | `TokenHash` | Unique Non-Clustered |
| `IX_RefreshTokens_UserId` | `UserId` | Non-Clustered |

**Foreign Keys:**

| Nome | Coluna | Referência | On Delete |
|---|---|---|---|
| `FK_RefreshTokens_Users_UserId` | `UserId` | `Users.Id` | CASCADE |

---

### 2.3 Tabela: `Messages`

**Migration de origem:** `AddMessages` (20260421040106)

| Coluna | Tipo SQL | Nullable | Constraint | Observações |
|---|---|---|---|---|
| `Id` | `uniqueidentifier` | NOT NULL | PK (`PK_Messages`) | GUID |
| `SenderId` | `uniqueidentifier` | NOT NULL | FK → `Users.Id` | RESTRICT on delete |
| `ReceiverId` | `uniqueidentifier` | NOT NULL | FK → `Users.Id` | RESTRICT on delete |
| `Content` | `nvarchar(4000)` | NOT NULL | — | Limite de 4000 chars |
| `SentAt` | `datetimeoffset` | NOT NULL | — | Timezone-aware |

**Índices:**

| Nome | Colunas | Tipo |
|---|---|---|
| `PK_Messages` | `Id` | Clustered (PK) |
| `IX_Messages_ReceiverId` | `ReceiverId` | Non-Clustered |
| `IX_Messages_SenderId_ReceiverId_SentAt` | `SenderId, ReceiverId, SentAt` | Non-Clustered composto |

**Foreign Keys:**

| Nome | Coluna | Referência | On Delete |
|---|---|---|---|
| `FK_Messages_Users_SenderId` | `SenderId` | `Users.Id` | RESTRICT |
| `FK_Messages_Users_ReceiverId` | `ReceiverId` | `Users.Id` | RESTRICT |

---

## 3. Diagrama ER Textual

```
Users (PK: Id)
  │
  ├─── [1:N] RefreshTokens.UserId  (CASCADE DELETE)
  │
  ├─── [1:N] Messages.SenderId     (RESTRICT DELETE)
  └─── [1:N] Messages.ReceiverId   (RESTRICT DELETE)
```

---

## 4. Avaliação de Nomenclatura

### 4.1 Tabelas

| Tabela | Plural | PascalCase | Sem prefixo | Resultado |
|---|---|---|---|---|
| `Users` | OK | OK | OK | **APROVADO** |
| `RefreshTokens` | OK | OK | OK | **APROVADO** |
| `Messages` | OK | OK | OK | **APROVADO** |

### 4.2 Colunas — Verificação Semântica

#### Tabela `Users`

| Coluna | PK/FK correto | Descritiva | Sem abreviação | Resultado |
|---|---|---|---|---|
| `Id` | OK (PK como `Id`) | OK | OK | **APROVADO** |
| `Email` | N/A | OK | OK | **APROVADO** |
| `PasswordHash` | N/A | OK | OK | **APROVADO** |
| `CreatedAt` | N/A (timestamp) | OK | OK | **APROVADO** |

**Ausências detectadas:**
- `UpdatedAt` — não existe. Usuário não tem campo de atualização de perfil ainda, mas a ausência é aceitável dado o escopo atual (somente auth). Registrada como melhoria futura.
- `DeletedAt` — não existe. Soft-delete não implementado. Registrado como melhoria futura.
- `DisplayName` / `Username` — não existe. A aplicação usa apenas `Email` como identidade de usuário. Limitação funcional, não violação de nomenclatura.

#### Tabela `RefreshTokens`

| Coluna | PK/FK correto | Descritiva | Sem abreviação | Resultado |
|---|---|---|---|---|
| `Id` | OK (PK como `Id`) | OK | OK | **APROVADO** |
| `UserId` | OK (FK como `<Entidade>Id`) | OK | OK | **APROVADO** |
| `TokenHash` | N/A | OK | OK | **APROVADO** |
| `CreatedAt` | N/A | OK | OK | **APROVADO** |
| `ExpiresAt` | N/A | OK | OK | **APROVADO** |
| `RevokedAt` | N/A | OK | OK | **APROVADO** |

#### Tabela `Messages`

| Coluna | PK/FK correto | Descritiva | Sem abreviação | Resultado |
|---|---|---|---|---|
| `Id` | OK (PK como `Id`) | OK | OK | **APROVADO** |
| `SenderId` | OK (FK como `<Entidade>Id`) | OK | OK | **APROVADO** |
| `ReceiverId` | OK (FK como `<Entidade>Id`) | OK | OK | **APROVADO** |
| `Content` | N/A | OK | OK | **APROVADO** |
| `SentAt` | N/A | Discutível — ver item 5.1 | OK | **ATENÇÃO** |

---

## 5. Problemas Identificados

### 5.1 [NOMENCLATURA — BAIXA SEVERIDADE] `Messages.SentAt` fora do padrão de timestamps

**Problema:** A convenção definida para timestamps é `CreatedAt / UpdatedAt / DeletedAt`. A coluna `SentAt` representa o momento de criação do registro de mensagem, mas usa semântica de domínio em vez da nomenclatura padronizada de auditoria.

**Impacto:** Baixo. O nome `SentAt` é semanticamente correto para o domínio de mensageria. Porém causa inconsistência com `CreatedAt` das outras tabelas quando queries cruzam tabelas ou quando se aplica auditoria genérica.

**Mudança proposta (aguardando aprovação):**
```sql
-- RENOMEAR coluna (requer migration):
EXEC sp_rename 'Messages.SentAt', 'CreatedAt', 'COLUMN';
-- + atualizar entidade: Message.SentAt → Message.CreatedAt
-- + atualizar índice: IX_Messages_SenderId_ReceiverId_SentAt → IX_Messages_SenderId_ReceiverId_CreatedAt
```
**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.2 [TIPO — MÉDIA SEVERIDADE] `Users.PasswordHash` usa `nvarchar(max)`

**Problema:** A coluna `PasswordHash` está tipada como `nvarchar(max)` sem limite explícito. O hash gerado pelo ASP.NET Identity (`PasswordHasher<T>`) produz strings Base64 com comprimento máximo de ~84 caracteres para V3 (PBKDF2 + salt). Usar `nvarchar(max)` desperdiça espaço de página e impede que a coluna participe de índices (SQL Server limita index key a 900 bytes; `nvarchar(max)` não indexável diretamente).

**Impacto:** Médio. Não há índice atual sobre `PasswordHash`, então não afeta consultas existentes. Mas é uma bad practice que pode causar problemas se o schema evoluir.

**Mudança proposta (aguardando aprovação):**
```sql
-- ALTERAR tipo (requer migration):
ALTER TABLE Users ALTER COLUMN PasswordHash nvarchar(256) NOT NULL;
-- 256 chars é suficiente para qualquer formato de hash atual (bcrypt, PBKDF2, Argon2)
```
**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.3 [ÍNDICE FALTANTE — ALTA SEVERIDADE] Ausência de índice em `Messages.SentAt` para queries de timeline global

**Problema:** O índice composto `IX_Messages_SenderId_ReceiverId_SentAt` cobre bem a query de histórico entre dois usuários específicos. Porém não existe índice isolado em `SentAt` (ou `CreatedAt` se renomeado). Queries de administração, auditoria e relatórios que filtram por intervalo de datas sem especificar sender/receiver resultarão em full scan na tabela `Messages`.

**Impacto:** Médio no volume atual (POC), mas alto em produção com volume de mensagens crescente.

**Índice proposto (aguardando aprovação):**
```sql
CREATE INDEX IX_Messages_SentAt ON Messages (SentAt DESC) INCLUDE (SenderId, ReceiverId, Content);
```
**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.4 [ÍNDICE FALTANTE — MÉDIA SEVERIDADE] Ausência de índice em `RefreshTokens.ExpiresAt` para limpeza de tokens expirados

**Problema:** Não existe índice em `RefreshTokens.ExpiresAt`. A operação de limpeza periódica de tokens expirados (DELETE WHERE ExpiresAt < GETUTCDATE()) fará full scan na tabela `RefreshTokens`. Com uso intenso, a tabela pode crescer significativamente.

**Impacto:** Médio. Operações de manutenção serão lentas sem este índice.

**Índice proposto (aguardando aprovação):**
```sql
CREATE INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens (ExpiresAt) WHERE RevokedAt IS NULL;
-- Índice filtrado (partial index) apenas para tokens ainda ativos
```
**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.5 [ÍNDICE FALTANTE — MÉDIA SEVERIDADE] Ausência de índice em `RefreshTokens.RevokedAt` para tokens revogados

**Problema:** Queries de auditoria de segurança (listar tokens revogados por usuário, detectar reutilização de tokens revogados) não têm suporte de índice em `RevokedAt`.

**Impacto:** Baixo no escopo atual (POC), mas relevante para auditoria de segurança.

**Índice proposto (aguardando aprovação):**
```sql
CREATE INDEX IX_RefreshTokens_UserId_RevokedAt ON RefreshTokens (UserId, RevokedAt) WHERE RevokedAt IS NOT NULL;
```
**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.6 [CONSTRAINT AUSENTE — ALTA SEVERIDADE] Sem CHECK constraint em `Messages.Content` para conteúdo vazio

**Problema:** A coluna `Content` é `nvarchar(4000) NOT NULL`, mas não há CHECK constraint impedindo string vazia (`''`). O EF Core aplica maxLength no lado da aplicação, mas nada impede INSERT direto no banco com conteúdo vazio.

**Mudança proposta (aguardando aprovação):**
```sql
ALTER TABLE Messages ADD CONSTRAINT CHK_Messages_Content_NotEmpty CHECK (LEN(LTRIM(RTRIM(Content))) > 0);
```
**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.7 [CONSTRAINT AUSENTE — MÉDIA SEVERIDADE] Sem CHECK constraint impedindo mensagem para si mesmo em `Messages`

**Problema:** Não existe CHECK constraint impedindo que `SenderId = ReceiverId`. Uma mensagem de um usuário para si mesmo é semanticamente inválida neste sistema de chat privado.

**Mudança proposta (aguardando aprovação):**
```sql
ALTER TABLE Messages ADD CONSTRAINT CHK_Messages_NoSelfMessage CHECK (SenderId <> ReceiverId);
```
**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.8 [DESIGN — BAIXA SEVERIDADE] `AppUser` sem campo `DisplayName` / `Username`

**Problema:** A entidade `AppUser` usa exclusivamente `Email` como identificador de usuário exibido na UI. Isso é observado em `MessageDto.SenderEmail` — o campo de identidade pública é o e-mail. Em um sistema de chat, expor o e-mail completo em mensagens pode ser indesejável (privacidade).

**Sugestão (aguardando aprovação):**
- Adicionar coluna `DisplayName nvarchar(100) NOT NULL DEFAULT ''` na tabela `Users`
- Atualizar `MessageDto` para usar `SenderDisplayName` em vez de `SenderEmail`

**Status:** AGUARDANDO APROVAÇÃO — não executar. Impacto em frontend também.

---

### 5.9 [DESIGN — BAIXA SEVERIDADE] Ausência de soft-delete (`DeletedAt`) nas entidades

**Problema:** Nenhuma das três tabelas possui coluna `DeletedAt` para soft-delete. Deleção de usuários é permanente (CASCADE para RefreshTokens, RESTRICT para Messages). No contexto atual de POC isso é aceitável, mas em produção seria um problema de LGPD/auditoria.

**Sugestão (aguardando aprovação):**
- Adicionar `DeletedAt datetimeoffset NULL` na tabela `Users`
- Implementar global query filter no EF Core: `.HasQueryFilter(u => u.DeletedAt == null)`

**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

### 5.10 [DESIGN — BAIXA SEVERIDADE] `AppUser` sem campo `UpdatedAt`

**Problema:** A tabela `Users` tem `CreatedAt` mas não `UpdatedAt`. Se o sistema evoluir para permitir troca de e-mail ou senha, não haverá rastreabilidade da última modificação.

**Sugestão (aguardando aprovação):**
- Adicionar `UpdatedAt datetimeoffset NULL` na tabela `Users`
- Popular via `SaveChanges` override no DbContext

**Status:** AGUARDANDO APROVAÇÃO — não executar.

---

## 6. Avaliação de Tipos de Dados

| Tabela | Coluna | Tipo Atual | Avaliação | Recomendação |
|---|---|---|---|---|
| `Users` | `Id` | `uniqueidentifier` | OK | Manter |
| `Users` | `Email` | `nvarchar(320)` | OK — RFC 5321 | Manter |
| `Users` | `PasswordHash` | `nvarchar(max)` | **PROBLEMA** (ver 5.2) | `nvarchar(256)` |
| `Users` | `CreatedAt` | `datetimeoffset` | OK — timezone-aware | Manter |
| `RefreshTokens` | `Id` | `uniqueidentifier` | OK | Manter |
| `RefreshTokens` | `UserId` | `uniqueidentifier` | OK | Manter |
| `RefreshTokens` | `TokenHash` | `nvarchar(128)` | OK — SHA-256 hex = 64 chars | Manter |
| `RefreshTokens` | `ExpiresAt` | `datetimeoffset` | OK | Manter |
| `RefreshTokens` | `CreatedAt` | `datetimeoffset` | OK | Manter |
| `RefreshTokens` | `RevokedAt` | `datetimeoffset NULL` | OK — nullable para soft-revoke | Manter |
| `Messages` | `Id` | `uniqueidentifier` | OK | Manter |
| `Messages` | `SenderId` | `uniqueidentifier` | OK | Manter |
| `Messages` | `ReceiverId` | `uniqueidentifier` | OK | Manter |
| `Messages` | `Content` | `nvarchar(4000)` | OK — limite razoável para chat | Manter |
| `Messages` | `SentAt` | `datetimeoffset` | OK — timezone-aware | Manter (ver 5.1 para rename) |

**Observação:** Nenhuma coluna usa `datetime` (deprecated) ou `varchar` sem `n`. Todos os tipos de data são `datetimeoffset` (timezone-aware). Uso de `bit` não aplicável pois não há flags booleanas. O schema está alinhado com best practices de SQL Server.

---

## 7. Avaliação de Constraints e Integridade Referencial

| Constraint | Tabela | Status | Severidade |
|---|---|---|---|
| PK em todas as tabelas | Users, RefreshTokens, Messages | OK | — |
| FK RefreshTokens → Users | `UserId` CASCADE | OK | — |
| FK Messages → Users (Sender) | `SenderId` RESTRICT | OK | — |
| FK Messages → Users (Receiver) | `ReceiverId` RESTRICT | OK | — |
| UNIQUE em `Users.Email` | `IX_Users_Email` | OK | — |
| UNIQUE em `RefreshTokens.TokenHash` | `IX_RefreshTokens_TokenHash` | OK | — |
| CHECK Content não vazio | Ausente | **PROBLEMA** | Média-Alta |
| CHECK SenderId ≠ ReceiverId | Ausente | **PROBLEMA** | Média |
| DEFAULT em timestamps | Ausente (gerenciado pelo app) | Aceitável para EF Core | Baixa |

**Nota sobre CASCADE vs RESTRICT:**
- `RefreshTokens` usa CASCADE: correto — tokens não fazem sentido sem o usuário.
- `Messages` usa RESTRICT: correto — evita deleção acidental de usuários com mensagens (dados históricos são preservados por design).

---

## 8. Resumo Executivo

### Pontos Positivos

1. **Nomenclatura geral excelente:** Tabelas em plural PascalCase sem prefixos, PKs como `Id`, FKs como `<Entidade>Id` — totalmente aderente aos critérios.
2. **Tipos de data timezone-aware:** Uso consistente de `datetimeoffset` em todas as colunas temporais.
3. **Índices funcionais para o caso de uso principal:** O índice composto `IX_Messages_SenderId_ReceiverId_SentAt` cobre a query de histórico de chat eficientemente.
4. **Integridade referencial bem modelada:** FKs com comportamento DELETE correto (CASCADE para tokens, RESTRICT para mensagens).
5. **Sem tipos deprecated:** Nenhum `datetime`, `ntext`, `image` ou `varchar` sem `n`.
6. **Token rotation seguro:** Hash SHA-256 armazenado, nunca o token raw — boa prática de segurança.

### Problemas por Prioridade

| Prioridade | Item | Tipo |
|---|---|---|
| Alta | 5.6 — CHECK Content não vazio | Constraint ausente |
| Alta | 5.3 — Índice em `SentAt` para range queries | Índice faltante |
| Média | 5.2 — `PasswordHash nvarchar(max)` | Tipo de dado |
| Média | 5.7 — CHECK SenderId ≠ ReceiverId | Constraint ausente |
| Média | 5.4 — Índice em `ExpiresAt` para limpeza | Índice faltante |
| Média | 5.5 — Índice em `RevokedAt` para auditoria | Índice faltante |
| Baixa | 5.1 — Rename `SentAt` → `CreatedAt` | Nomenclatura |
| Baixa | 5.8 — Campo `DisplayName` em Users | Design |
| Baixa | 5.9 — Soft-delete (`DeletedAt`) | Design |
| Baixa | 5.10 — Campo `UpdatedAt` em Users | Design |

### Decisão

**Nenhuma migration foi executada.** Todas as mudanças propostas estão documentadas acima e aguardam aprovação antes de qualquer implementação.

---

*Gerado por análise estática de código-fonte — entidades de domínio, DbContext e migrations EF Core.*
