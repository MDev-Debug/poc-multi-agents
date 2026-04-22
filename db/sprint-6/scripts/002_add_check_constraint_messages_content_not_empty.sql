-- =============================================================================
-- Objetivo  : Adicionar CHECK constraint em Messages.Content para impedir
--             inserção de strings vazias ou compostas apenas por espaços.
--             Complementa a validação da camada de aplicação (EF Core MaxLength)
--             com integridade garantida diretamente no banco.
-- Sprint    : DBA - Análise Global (2026-04-21)
-- Data      : 2026-04-21
-- Impacto   : Impede INSERTs/UPDATEs diretos com conteúdo vazio. Se existirem
--             registros com Content em branco antes da execução, o ALTER falhará
--             — verificar com: SELECT COUNT(*) FROM Messages WHERE LEN(LTRIM(RTRIM(Content))) = 0
-- Severidade: Alta (problema 5.6 da análise)
-- =============================================================================

USE [CHAT];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.Messages')
      AND name             = N'CHK_Messages_Content_NotEmpty'
)
BEGIN
    ALTER TABLE [dbo].[Messages]
        ADD CONSTRAINT [CHK_Messages_Content_NotEmpty]
        CHECK (LEN(LTRIM(RTRIM([Content]))) > 0);

    PRINT 'Constraint CHK_Messages_Content_NotEmpty criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Constraint CHK_Messages_Content_NotEmpty já existe — nenhuma ação necessária.';
END
GO
