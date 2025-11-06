using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallData.Migrations
{
    /// <inheritdoc />
    public partial class updateMoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "KmRodados",
                table: "Moto",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KmRodados",
                table: "Moto");
        }
    }
}
