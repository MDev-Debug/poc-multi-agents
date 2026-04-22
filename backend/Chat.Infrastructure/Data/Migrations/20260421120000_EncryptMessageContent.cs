using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Infrastructure.Data.Migrations
{
    public partial class EncryptMessageContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dropar dependências da coluna Content antes de alterar o tipo
            migrationBuilder.Sql("ALTER TABLE [Messages] DROP CONSTRAINT IF EXISTS [CHK_Messages_Content_NotEmpty];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_Messages_SentAt] ON [Messages];");

            // Alterar coluna para nvarchar(max) para comportar o payload cifrado (AES-256-GCM Base64)
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            // Recriar índice e constraint após alter column
            migrationBuilder.Sql(@"
                SET QUOTED_IDENTIFIER ON;
                CREATE NONCLUSTERED INDEX [IX_Messages_SentAt]
                    ON [dbo].[Messages] ([SentAt] DESC)
                    INCLUDE ([SenderId], [ReceiverId], [Content]);");
            migrationBuilder.Sql(@"
                ALTER TABLE [Messages]
                    ADD CONSTRAINT [CHK_Messages_Content_NotEmpty]
                    CHECK (LEN(LTRIM(RTRIM([Content]))) > 0);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dropar dependências antes de reverter o tipo
            migrationBuilder.Sql("ALTER TABLE [Messages] DROP CONSTRAINT IF EXISTS [CHK_Messages_Content_NotEmpty];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_Messages_SentAt] ON [Messages];");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Recriar índice e constraint originais
            migrationBuilder.Sql(@"
                SET QUOTED_IDENTIFIER ON;
                CREATE NONCLUSTERED INDEX [IX_Messages_SentAt]
                    ON [dbo].[Messages] ([SentAt] DESC)
                    INCLUDE ([SenderId], [ReceiverId], [Content]);");
            migrationBuilder.Sql(@"
                ALTER TABLE [Messages]
                    ADD CONSTRAINT [CHK_Messages_Content_NotEmpty]
                    CHECK (LEN(LTRIM(RTRIM([Content]))) > 0);");
        }
    }
}
