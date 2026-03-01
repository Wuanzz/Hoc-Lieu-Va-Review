using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("HocPhan")]
    public class HocPhan
    {
        [Key]
        public int HocPhanID { get; set; }

        [Required]
        [StringLength(255)]
        public string TenHocPhan { get; set; }

        public string MoTa { get; set; }

        [ForeignKey("Nganh")]
        public int NganhID { get; set; }

        // Navigation property
        public virtual Nganh Nganh { get; set; }

        // Sẽ thêm ICollection<TaiLieu> và ICollection<Review> ở các bước sau
    }
}