using Microsoft.EntityFrameworkCore;

namespace Hoc_Lieu_Va_Review.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // public DbSet<Sach> Saches { get; set; }
    }
}
