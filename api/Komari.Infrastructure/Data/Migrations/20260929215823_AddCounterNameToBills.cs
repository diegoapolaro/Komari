using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Komari.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCounterNameToBills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CounterName",
                table: "bills",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CounterName",
                table: "bills");
        }
    }
}
