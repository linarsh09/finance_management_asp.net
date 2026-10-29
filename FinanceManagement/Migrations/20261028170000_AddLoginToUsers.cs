using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Migrations;

public partial class AddLoginToUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Login",
            table: "Users",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.Sql(@"
            WITH candidates AS (
                SELECT ""Id"",
                       CASE
                           WHEN ""Email"" IS NULL OR POSITION('@' IN ""Email"") = 0
                               THEN 'user' || ""Id""::text
                           ELSE LEFT(LOWER(SPLIT_PART(""Email"", '@', 1)), 40)
                       END AS base_login
                FROM ""Users""
            ), ranked AS (
                SELECT ""Id"", base_login,
                       ROW_NUMBER() OVER (PARTITION BY base_login ORDER BY ""Id"") AS row_number
                FROM candidates
            )
            UPDATE ""Users"" AS users
            SET ""Login"" = CASE
                WHEN ranked.row_number = 1 THEN ranked.base_login
                ELSE LEFT(ranked.base_login, 40) || users.""Id""::text
            END
            FROM ranked
            WHERE users.""Id"" = ranked.""Id"";
        ");

        migrationBuilder.AlterColumn<string>(
            name: "Login",
            table: "Users",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Login",
            table: "Users",
            column: "Login",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Users_Login", table: "Users");
        migrationBuilder.DropColumn(name: "Login", table: "Users");
    }
}
