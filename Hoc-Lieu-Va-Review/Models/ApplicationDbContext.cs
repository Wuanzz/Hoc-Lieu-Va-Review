using Microsoft.EntityFrameworkCore;

namespace Hoc_Lieu_Va_Review.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng sẽ được tạo trong SQL Server
        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<Nganh> Nganhs { get; set; }
        public DbSet<HocPhan> HocPhans { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<TaiLieu> TaiLieus { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<BinhLuan> BinhLuans { get; set; }
        public DbSet<BaoCao> BaoCaos { get; set; }
    }
}
