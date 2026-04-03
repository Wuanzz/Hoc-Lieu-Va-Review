using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("BaoCao")]
    public class BaoCao
    {
        [Key]
        public int BaoCaoID { get; set; }

        [Required]
        [StringLength(500)]
        public string LyDo { get; set; }

        public DateTime NgayBaoCao { get; set; } = DateTime.Now;

        [Column(TypeName = "varchar(50)")]
        public string TrangThaiXuLy { get; set; } // ChuaXuLy, DaXuLy

        // Khóa ngoại bắt buộc
        public int NguoiDungID { get; set; }
        [ForeignKey("NguoiDungID")]
        public virtual NguoiDung NguoiDung { get; set; }

        // Các khóa ngoại có thể Null (dùng int?)
        public int? ReviewID { get; set; }
        [ForeignKey("ReviewID")]
        public virtual Review Review { get; set; }

        public int? TaiLieuID { get; set; }
        [ForeignKey("TaiLieuID")]
        public virtual TaiLieu TaiLieu { get; set; }

        public int? BinhLuanID { get; set; }
        [ForeignKey("BinhLuanID")]
        public virtual BinhLuan BinhLuan { get; set; }
    }
}