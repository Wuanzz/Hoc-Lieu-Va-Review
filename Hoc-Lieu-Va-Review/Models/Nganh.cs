using Hoc_Lieu_Va_Review.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("Nganh")]
    public class Nganh
    {
        [Key]
        public int NganhID { get; set; }

        [Required]
        [StringLength(255)]
        public string TenNganh { get; set; }

        public string MoTa { get; set; }

        [ForeignKey("Khoa")]
        public int KhoaID { get; set; }

        // Navigation properties
        public virtual Khoa Khoa { get; set; }
        public virtual ICollection<HocPhan> HocPhans { get; set; } = new List<HocPhan>();
    }
}