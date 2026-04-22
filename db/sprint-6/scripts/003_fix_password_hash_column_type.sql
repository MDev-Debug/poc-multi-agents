-- =============================================================================
-- Objetivo  : Alterar tipo da coluna Users.PasswordHash de nvarchar(max) para
--             nvarchar(256), eliminando uso desnecessário de LOB storage.
--             O hash gerado pelo ASP.NET Identity PasswordHasher v3 (PBKDF2)
--             resulta em string Base64 com no máximo ~84 caracteres; nvarchar(256)
--             cobre com folga qualquer formato atual (bcrypt, PBKDF2, Argon2)
--             e futuros formatos de hash de senha.
-- Sprint    : DBA - Análise Global (2026-04-21)
-- Data      : 2026-04-21
-- Impacto   : Reduz footprint de storage, elimina LOB allocation para a coluna
--             e permite que a coluna passe a participar de índices se necessário.
--             Nenhum dado existente será truncado (hashes atuais << 256 chars).
--             Sem risco de perda de dados. Requer breve lock de schema (SCH-M).
-- Severidade: Média (problema 5.2 da análise)
-- =============================================================================

USE [CHAT];
GO

-- Guarda tamanho atual para verificação idempotente.
-- COL_LENGTH retorna o tamanho em bytes declarado; nvarchar(max) retorna -1.
-- nvarchar(256) retorna 512 (256 chars * 2 bytes).
IF COL_LENGTH(N'dbo.Users', N'PasswordHash') = -1
BEGIN
    ALTER TABLE [dbo].[Users]
        ALTER COLUMN [PasswordHash] NVARCHAR(256) NOT NULL;

    PRINT 'Coluna Users.PasswordHash alterada de nvarchar(max) para nvarchar(256) com sucesso.';
END
ELSE
BEGIN
    PRINT 'Coluna Users.PasswordHash já não é nvarchar(max) — nenhuma ação necessária.';
END
GO
