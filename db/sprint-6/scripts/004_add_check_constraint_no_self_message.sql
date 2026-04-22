-- =============================================================================
-- Objetivo  : Adicionar CHECK constraint em Messages para impedir que um usuário
--             envie mensagem para si mesmo (SenderId = ReceiverId). Em um sistema
--             de chat privado 1-a-1, esse cenário é semanticamente inválido e
--             poderia causar inconsistências na exibição de conversas.
-- Sprint    : DBA - Análise Global (2026-04-21)
-- Data      : 2026-04-21
-- Impacto   : Impede INSERTs/UPDATEs diretos com SenderId = ReceiverId. Se
--             existirem registros de auto-mensagem antes da execução, o ALTER
--             falhará — verificar com:
--             SELECT COUNT(*) FROM Messages WHERE SenderId = ReceiverId
-- Severidade: Média (problema 5.7 da análise)
-- =============================================================================

USE [CHAT];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.Messages')
      AND name             = N'CHK_Messages_NoSelfMessage'
)
BEGIN
    ALTER TABLE [dbo].[Messages]
        ADD CONSTRAINT [CHK_Messages_NoSelfMessage]
        CHECK ([SenderId] <> [ReceiverId]);

    PRINT 'Constraint CHK_Messages_NoSelfMessage criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Constraint CHK_Messages_NoSelfMessage já existe — nenhuma ação necessária.';
END
GO
