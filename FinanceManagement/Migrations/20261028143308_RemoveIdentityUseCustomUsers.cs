using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdentityUseCustomUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve the existing accounts, categories, email addresses, and
            // compatible ASP.NET password hashes while moving users to Users.
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""Email"", ""Password"")
                SELECT LOWER(i.""Email""), i.""PasswordHash""
                FROM ""IdentityUser"" i
                WHERE i.""Email"" IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM ""Users"" u WHERE LOWER(u.""Email"") = LOWER(i.""Email""));

                UPDATE ""Accounts"" a
                SET ""UserId"" = u.""Id""::text
                FROM ""IdentityUser"" i
                JOIN ""Users"" u ON LOWER(u.""Email"") = LOWER(i.""Email"")
                WHERE a.""UserId"" = i.""Id"";

                UPDATE ""Categories"" c
                SET ""UserId"" = u.""Id""::text
                FROM ""IdentityUser"" i
                JOIN ""Users"" u ON LOWER(u.""Email"") = LOWER(i.""Email"")
                WHERE c.""UserId"" = i.""Id"";
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_IdentityUser_UserId",
                table: "Categories");

            migrationBuilder.DropTable(
                name: "IdentityUser");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Categories"" ALTER COLUMN ""UserId"" TYPE integer USING ""UserId""::integer;
                ALTER TABLE ""Accounts"" ALTER COLUMN ""UserId"" TYPE integer USING ""UserId""::integer;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_UserId",
                table: "Accounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_UserId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Password");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Categories"" ALTER COLUMN ""UserId"" TYPE text USING ""UserId""::text;
                ALTER TABLE ""Accounts"" ALTER COLUMN ""UserId"" TYPE text USING ""UserId""::text;
            ");

            migrationBuilder.CreateTable(
                name: "IdentityUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityUser", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_IdentityUser_UserId",
                table: "Categories",
                column: "UserId",
                principalTable: "IdentityUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
