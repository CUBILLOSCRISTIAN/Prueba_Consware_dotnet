using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelRequests.Infrastructure.Migrations
{
    public partial class AddPasswordRecoveryFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordRecoveryCode",
                schema: "dbo",
                table: "user",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordRecoveryCodeExpiresAt",
                schema: "dbo",
                table: "user",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordRecoveryCode",
                schema: "dbo",
                table: "user");

            migrationBuilder.DropColumn(
                name: "PasswordRecoveryCodeExpiresAt",
                schema: "dbo",
                table: "user");
        }
    }
}