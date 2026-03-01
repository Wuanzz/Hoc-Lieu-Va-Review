using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("Review")]
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        public string NoiDung { get; set; } // EF Core mặc định string không giới hạn là nvarchar(max)

        [Range(1, 5)] // Ràng buộc số sao từ 1 đến 5
        public int SoSao { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.Now;

        [Column(TypeName = "varchar(50)")]
        public string TrangThaiDuyet { get; set; }

        // Khóa ngoại
        public int NguoiDungID { get; set; }
        public int HocPhanID { get; set; }

        // Navigation properties
        [ForeignKey("NguoiDungID")]
        public virtual NguoiDung NguoiDung { get; set; }

        [ForeignKey("HocPhanID")]
        public virtual HocPhan HocPhan { get; set; }
    }
}