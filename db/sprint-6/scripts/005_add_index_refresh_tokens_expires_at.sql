-- =============================================================================
-- Objetivo  : Criar índice filtrado em RefreshTokens.ExpiresAt restrito a tokens
--             ainda ativos (RevokedAt IS NULL). Otimiza a operação periódica de
--             limpeza de tokens expirados:
--               DELETE FROM RefreshTokens WHERE ExpiresAt < SYSUTCDATETIME()
--             O filtro WHERE RevokedAt IS NULL reduz o tamanho do índice
--             excluindo tokens já revogados (que não precisam ser varridos
--             para limpeza por expiração).
-- Sprint    : DBA - Análise Global (2026-04-21)
-- Data      : 2026-04-21
-- Impacto   : Melhora performance de manutenção (job de limpeza de tokens).
--             Índice filtrado (partial index) — footprint menor que índice full.
--             Sem risco de perda de dados. Operação online.
-- Severidade: Média (problema 5.4 da análise)
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
GO
USE [CHAT];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.RefreshTokens')
      AND name      = N'IX_RefreshTokens_ExpiresAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_ExpiresAt]
        ON [dbo].[RefreshTokens] ([ExpiresAt])
        WHERE [RevokedAt] IS NULL;

    PRINT 'Índice IX_RefreshTokens_ExpiresAt criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_RefreshTokens_ExpiresAt já existe — nenhuma ação necessária.';
END
GO
