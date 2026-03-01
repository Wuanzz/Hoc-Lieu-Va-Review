using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("Khoa")]
    public class Khoa
    {
        [Key]
        public int KhoaID { get; set; }

        [Required]
        [StringLength(255)]
        public string TenKhoa { get; set; }

        public string MoTa { get; set; }

        // Navigation property: Một khoa có nhiều ngành
        public virtual ICollection<Nganh> Nganhs { get; set; } = new List<Nganh>();
    }
}