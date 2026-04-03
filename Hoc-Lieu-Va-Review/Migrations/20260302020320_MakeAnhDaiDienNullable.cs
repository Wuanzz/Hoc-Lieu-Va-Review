using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoc_Lieu_Va_Review.Migrations
{
    /// <inheritdoc />
    public partial class MakeAnhDaiDienNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AnhDaiDien",
                table: "NguoiDung",
                type: "varchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AnhDaiDien",
                table: "NguoiDung",
                type: "varchar(500)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldNullable: true);
        }
    }
}
