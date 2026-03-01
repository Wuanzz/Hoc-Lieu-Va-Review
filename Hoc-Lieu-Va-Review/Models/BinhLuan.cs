using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("BinhLuan")]
    public class BinhLuan
    {
        [Key]
        public int BinhLuanID { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.Now;

        [Column(TypeName = "varchar(50)")]
        public string TrangThaiDuyet { get; set; }

        // Khóa ngoại bắt buộc
        public int NguoiDungID { get; set; }
        [ForeignKey("NguoiDungID")]
        public virtual NguoiDung NguoiDung { get; set; }

        // Các khóa ngoại có thể Null (dùng int?)
        public int? ParentID { get; set; }
        [ForeignKey("ParentID")]
        public virtual BinhLuan ParentBinhLuan { get; set; } // Để reply bình luận

        public int? ReviewID { get; set; }
        [ForeignKey("ReviewID")]
        public virtual Review Review { get; set; } // Nếu bình luận trong Review

        public int? TaiLieuID { get; set; }
        [ForeignKey("TaiLieuID")]
        public virtual TaiLieu TaiLieu { get; set; } // Nếu bình luận trong Tài liệu
    }
}