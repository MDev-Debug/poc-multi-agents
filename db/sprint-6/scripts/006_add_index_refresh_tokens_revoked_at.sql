-- =============================================================================
-- Objetivo  : Criar índice filtrado em RefreshTokens (UserId, RevokedAt)
--             restrito apenas a tokens revogados (RevokedAt IS NOT NULL).
--             Suporta queries de auditoria de segurança:
--               - Listar tokens revogados por usuário
--               - Detectar tentativa de reutilização de token revogado (token reuse detection)
--             O filtro WHERE RevokedAt IS NOT NULL exclui tokens ativos,
--             mantendo o índice focado no subconjunto de interesse da auditoria.
-- Sprint    : DBA - Análise Global (2026-04-21)
-- Data      : 2026-04-21
-- Impacto   : Melhora performance de queries de auditoria de segurança.
--             Índice filtrado (partial index) — footprint reduzido.
--             Sem risco de perda de dados. Operação online.
-- Severidade: Média (problema 5.5 da análise)
-- =============================================================================

USE [CHAT];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.RefreshTokens')
      AND name      = N'IX_RefreshTokens_UserId_RevokedAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId_RevokedAt]
        ON [dbo].[RefreshTokens] ([UserId], [RevokedAt])
        WHERE [RevokedAt] IS NOT NULL;

    PRINT 'Índice IX_RefreshTokens_UserId_RevokedAt criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_RefreshTokens_UserId_RevokedAt já existe — nenhuma ação necessária.';
END
GO
