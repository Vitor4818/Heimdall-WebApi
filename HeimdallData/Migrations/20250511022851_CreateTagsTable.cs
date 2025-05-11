using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallData.Migrations
{
    /// <inheritdoc />
    public partial class CreateTagsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagsRfid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MotoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FaixaFrequencia = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Banda = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Aplicacao = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsRfid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagsRfid_Moto_MotoId",
                        column: x => x.MotoId,
                        principalTable: "Moto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagsRfid_MotoId",
                table: "TagsRfid",
                column: "MotoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagsRfid");
        }
    }
}
