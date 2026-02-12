using Microsoft.EntityFrameworkCore;

namespace Hoc_Lieu_Va_Review.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng (DbSet) của bạn ở đây, ví dụ:
        // public DbSet<Product> Products { get; set; }
    }
}
