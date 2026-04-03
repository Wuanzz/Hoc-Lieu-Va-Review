using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoc_Lieu_Va_Review.Migrations
{
    /// <inheritdoc />
    public partial class Create8Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    KhoaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.KhoaID);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    NguoiDungID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false),
                    MatKhau = table.Column<string>(type: "varchar(255)", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "varchar(500)", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(50)", nullable: false),
                    VaiTro = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.NguoiDungID);
                });

            migrationBuilder.CreateTable(
                name: "Nganh",
                columns: table => new
                {
                    NganhID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNganh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KhoaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nganh", x => x.NganhID);
                    table.ForeignKey(
                        name: "FK_Nganh_Khoa_KhoaID",
                        column: x => x.KhoaID,
                        principalTable: "Khoa",
                        principalColumn: "KhoaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HocPhan",
                columns: table => new
                {
                    HocPhanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenHocPhan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NganhID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocPhan", x => x.HocPhanID);
                    table.ForeignKey(
                        name: "FK_HocPhan_Nganh_NganhID",
                        column: x => x.NganhID,
                        principalTable: "Nganh",
                        principalColumn: "NganhID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ReviewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiDuyet = table.Column<string>(type: "varchar(50)", nullable: false),
                    NguoiDungID = table.Column<int>(type: "int", nullable: false),
                    HocPhanID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.ReviewID);
                    table.ForeignKey(
                        name: "FK_Review_HocPhan_HocPhanID",
                        column: x => x.HocPhanID,
                        principalTable: "HocPhan",
                        principalColumn: "HocPhanID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Review_NguoiDung_NguoiDungID",
                        column: x => x.NguoiDungID,
                        principalTable: "NguoiDung",
                        principalColumn: "NguoiDungID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu",
                columns: table => new
                {
                    TaiLieuID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTaiLieu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DuongDanFile = table.Column<string>(type: "varchar(500)", nullable: false),
                    LoaiTaiLieu = table.Column<string>(type: "varchar(50)", nullable: false),
                    KichThuoc = table.Column<double>(type: "float", nullable: false),
                    NgayUpload = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiDuyet = table.Column<string>(type: "varchar(50)", nullable: false),
                    LuotTai = table.Column<int>(type: "int", nullable: false),
                    NguoiDungID = table.Column<int>(type: "int", nullable: false),
                    HocPhanID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu", x => x.TaiLieuID);
                    table.ForeignKey(
                        name: "FK_TaiLieu_HocPhan_HocPhanID",
                        column: x => x.HocPhanID,
                        principalTable: "HocPhan",
                        principalColumn: "HocPhanID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaiLieu_NguoiDung_NguoiDungID",
                        column: x => x.NguoiDungID,
                        principalTable: "NguoiDung",
                        principalColumn: "NguoiDungID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BinhLuan",
                columns: table => new
                {
                    BinhLuanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiDuyet = table.Column<string>(type: "varchar(50)", nullable: false),
                    NguoiDungID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: true),
                    ReviewID = table.Column<int>(type: "int", nullable: true),
                    TaiLieuID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuan", x => x.BinhLuanID);
                    table.ForeignKey(
                        name: "FK_BinhLuan_BinhLuan_ParentID",
                        column: x => x.ParentID,
                        principalTable: "BinhLuan",
                        principalColumn: "BinhLuanID");
                    table.ForeignKey(
                        name: "FK_BinhLuan_NguoiDung_NguoiDungID",
                        column: x => x.NguoiDungID,
                        principalTable: "NguoiDung",
                        principalColumn: "NguoiDungID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BinhLuan_Review_ReviewID",
                        column: x => x.ReviewID,
                        principalTable: "Review",
                        principalColumn: "ReviewID");
                    table.ForeignKey(
                        name: "FK_BinhLuan_TaiLieu_TaiLieuID",
                        column: x => x.TaiLieuID,
                        principalTable: "TaiLieu",
                        principalColumn: "TaiLieuID");
                });

            migrationBuilder.CreateTable(
                name: "BaoCao",
                columns: table => new
                {
                    BaoCaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayBaoCao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiXuLy = table.Column<string>(type: "varchar(50)", nullable: false),
                    NguoiDungID = table.Column<int>(type: "int", nullable: false),
                    ReviewID = table.Column<int>(type: "int", nullable: true),
                    TaiLieuID = table.Column<int>(type: "int", nullable: true),
                    BinhLuanID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCao", x => x.BaoCaoID);
                    table.ForeignKey(
                        name: "FK_BaoCao_BinhLuan_BinhLuanID",
                        column: x => x.BinhLuanID,
                        principalTable: "BinhLuan",
                        principalColumn: "BinhLuanID");
                    table.ForeignKey(
                        name: "FK_BaoCao_NguoiDung_NguoiDungID",
                        column: x => x.NguoiDungID,
                        principalTable: "NguoiDung",
                        principalColumn: "NguoiDungID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoCao_Review_ReviewID",
                        column: x => x.ReviewID,
                        principalTable: "Review",
                        principalColumn: "ReviewID");
                    table.ForeignKey(
                        name: "FK_BaoCao_TaiLieu_TaiLieuID",
                        column: x => x.TaiLieuID,
                        principalTable: "TaiLieu",
                        principalColumn: "TaiLieuID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_BinhLuanID",
                table: "BaoCao",
                column: "BinhLuanID");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_NguoiDungID",
                table: "BaoCao",
                column: "NguoiDungID");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_ReviewID",
                table: "BaoCao",
                column: "ReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TaiLieuID",
                table: "BaoCao",
                column: "TaiLieuID");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_NguoiDungID",
                table: "BinhLuan",
                column: "NguoiDungID");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_ParentID",
                table: "BinhLuan",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_ReviewID",
                table: "BinhLuan",
                column: "ReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_TaiLieuID",
                table: "BinhLuan",
                column: "TaiLieuID");

            migrationBuilder.CreateIndex(
                name: "IX_HocPhan_NganhID",
                table: "HocPhan",
                column: "NganhID");

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_KhoaID",
                table: "Nganh",
                column: "KhoaID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_HocPhanID",
                table: "Review",
                column: "HocPhanID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_NguoiDungID",
                table: "Review",
                column: "NguoiDungID");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_HocPhanID",
                table: "TaiLieu",
                column: "HocPhanID");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_NguoiDungID",
                table: "TaiLieu",
                column: "NguoiDungID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCao");

            migrationBuilder.DropTable(
                name: "BinhLuan");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "TaiLieu");

            migrationBuilder.DropTable(
                name: "HocPhan");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "Nganh");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
