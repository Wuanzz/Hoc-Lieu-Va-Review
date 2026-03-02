using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hoc_Lieu_Va_Review.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public int NguoiDungID { get; set; }
        [Required]
        [StringLength(255)]
        public string HoTen { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Email { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string MatKhau { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string? AnhDaiDien { get; set; }
        public DateTime NgayDangKy { get; set; } = DateTime.Now;
        [Column(TypeName = "varchar(50)")]
        public string TrangThai { get; set; } // Ví dụ: "HoatDong", "BiKhoa"
        [Column(TypeName = "varchar(50)")]
        public string VaiTro { get; set; } // Ví dụ: "SinhVien", "GiangVien", "Admin"
    }
}