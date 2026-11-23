
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentAssistant.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToDocumentName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convertimos los estados existentes de texto a números.
            migrationBuilder.Sql(
                """
                UPDATE Documents
                SET Status = CASE Status
                    WHEN 'Pending' THEN '1'
                    WHEN 'Processing' THEN '2'
                    WHEN 'Completed' THEN '3'
                    WHEN 'Failed' THEN '4'
                    ELSE Status
                END;
                """);

            // Cambiamos el tipo de columna Status de nvarchar a int.
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Documents",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Limitamos Name a 450 caracteres y configuramos la collation.
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: false,
                collation: "SQL_Latin1_General_CP1_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Creamos el índice único.
            migrationBuilder.CreateIndex(
                name: "IX_Documents_Name",
                table: "Documents",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_Name",
                table: "Documents");

            // Revertimos Status de int a texto.
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(
                """
                UPDATE Documents
                SET Status = CASE Status
                    WHEN '1' THEN 'Pending'
                    WHEN '2' THEN 'Processing'
                    WHEN '3' THEN 'Completed'
                    WHEN '4' THEN 'Failed'
                    ELSE Status
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldCollation: "SQL_Latin1_General_CP1_CI_AS");
        }
    }
}
