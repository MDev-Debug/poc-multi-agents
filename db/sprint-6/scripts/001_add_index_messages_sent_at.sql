-- =============================================================================
-- Objetivo  : Criar índice em Messages.SentAt DESC com colunas INCLUDE para
--             cobrir range queries de timeline global (auditoria, relatórios)
--             sem exigir especificação de SenderId/ReceiverId.
-- Sprint    : DBA - Análise Global (2026-04-21)
-- Data      : 2026-04-21
-- Impacto   : Melhora performance de queries com filtro por intervalo de datas
--             em Messages. Sem risco de perda de dados. Operação online.
-- Severidade: Alta (problema 5.3 da análise)
-- =============================================================================

USE [CHAT];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Messages')
      AND name      = N'IX_Messages_SentAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Messages_SentAt]
        ON [dbo].[Messages] ([SentAt] DESC)
        INCLUDE ([SenderId], [ReceiverId], [Content]);

    PRINT 'Índice IX_Messages_SentAt criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Messages_SentAt já existe — nenhuma ação necessária.';
END
GO
