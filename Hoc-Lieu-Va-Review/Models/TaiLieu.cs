using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("TaiLieu")]
    public class TaiLieu
    {
        [Key]
        public int TaiLieuID { get; set; }

        [Required]
        [StringLength(255)]
        public string TenTaiLieu { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string DuongDanFile { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string LoaiTaiLieu { get; set; } // Slide, Đề thi, Tham khảo

        public double KichThuoc { get; set; } // Lưu kích thước file (MB hoặc KB)

        public DateTime NgayUpload { get; set; } = DateTime.Now;

        [Column(TypeName = "varchar(50)")]
        public string TrangThaiDuyet { get; set; } // ChoDuyet, HopLe, TuChoi

        public int LuotTai { get; set; } = 0;

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