using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiIdentitySample.Migrations
{
    /// <inheritdoc />
    public partial class Ajoutdesinfospourconnexionviamicrosoft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AzureObjectId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AzureTenantId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AzureObjectId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AzureTenantId",
                table: "AspNetUsers");
        }
    }
}
