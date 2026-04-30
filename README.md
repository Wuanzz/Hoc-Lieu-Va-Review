## 🎓 EduShare - Nền tảng Chia sẻ Học liệu & Đánh giá Môn học

**EduShare** là một ứng dụng web được phát triển trên nền tảng **ASP.NET Core MVC**, ra đời với sứ mệnh xây dựng một cộng đồng học tập minh bạch, nơi sinh viên có thể chia sẻ tài liệu, trao đổi kiến thức và để lại những đánh giá (review) chân thực về các môn học.

Đặc biệt, dự án tích hợp **Trí tuệ nhân tạo (Gemini AI)** để tự động kiểm duyệt nội dung và công nghệ **SignalR** mang lại trải nghiệm tương tác thời gian thực (real-time) không độ trễ.

---

## ✨ Tính năng nổi bật

### 👨‍🎓 Khu vực Sinh viên (Public Area)

* **Kho Tài liệu:** Tra cứu, xem trước (PDF) và tải xuống tài liệu theo phân loại Khoa - Ngành - Môn học.
* **Cộng đồng Review:** Chia sẻ kinh nghiệm, đánh giá môn học (chấm sao) và thảo luận đa chiều.
* **Bình luận & Thảo luận:** Giao diện khung chat bong bóng (Chat Bubble) hiện đại, hỗ trợ trả lời (reply) theo luồng.
* **Thông báo Real-time:** Nhận thông báo nảy (Toast Notification) ngay lập tức khi có tài liệu hoặc review mới được duyệt mà không cần tải lại trang.

### 🛡️ Khu vực Quản trị (Admin Dashboard)

Hệ thống sử dụng kiến trúc **Areas** để tách biệt hoàn toàn không gian quản trị, với cơ chế Phân quyền (Role-Based Access Control) chặt chẽ:

* **Admin (Quản trị hệ thống):** Toàn quyền quản lý danh mục (Khoa, Ngành, Học phần) và tài khoản Người dùng.
* **Giảng viên (Học thuật):** Chuyên tâm vào Khu vực Kiểm duyệt, xét duyệt các tài liệu và bài đánh giá do sinh viên tải lên.
* **Giao diện thông minh:** Tự động làm mờ và khóa các chức năng không thuộc thẩm quyền, Sidebar có thể thu gọn tối ưu không gian làm việc.

### 🤖 Trợ lý AI Kiểm duyệt

* Tích hợp Google Gemini AI tự động quét các bài viết, bình luận trước khi đăng.
* Tự động từ chối (chặn) các nội dung chứa từ ngữ thô tục, vi phạm tiêu chuẩn cộng đồng.
* Phân loại các nội dung đáng ngờ vào danh sách "Chờ duyệt" để Giảng viên quyết định.

---

## 🛠️ Công nghệ sử dụng

* **Backend:** C# / ASP.NET Core MVC (.NET 6/7/8)

* **Cơ sở dữ liệu:** Microsoft SQL Server & Entity Framework Core (Code-First)

* **Real-time:** Microsoft SignalR

* **AI Integration:** Google Gemini API

* **Frontend:** \* HTML5, CSS3, JavaScript / jQuery

  * Giao diện Bootstrap 5 (Sử dụng theme Zephyr từ Bootswatch)
  * FontAwesome 6 (Icons)

---

## 🚀 Hướng dẫn Cài đặt & Khởi chạy

Để chạy dự án trên máy cá nhân, vui lòng thực hiện theo các bước sau:

**Bước 1: Clone dự án**

```plaintext
git clone https://github.com/your-username/EduShare.git
cd EduShare
```

**Bước 2: Tạo database** Mở SQL Server để tạo database tên `EduShareDB`

**Bước 3: Cấu hình Cơ sở dữ liệu & API Key** Mở file `appsettings.json` và cập nhật chuỗi kết nối SQL Server cũng như API Key của Gemini:

```plaintext
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=EduShareDB;Trusted_Connection=True;MultipleActiveResultSets=true"
},
"GeminiApi": {
  "ApiKey": "ĐIỀN_API_KEY_CỦA_BẠN_VÀO_ĐÂY"
}
```

**Bước 4:&#x20;**&#x4D;ở **Package Manager Console** trong Visual Studio và chạy các lệnh sau:

```plaintext
Add-Migration InitialCreate
Update-Database
```

**Bước 5: Thêm dữ liệu người dùng mẫu** Mở SQL Server tiến hành chạy script:

```plaintext
USE EduShareDB

INSERT INTO NguoiDung (HoTen, Email, MatKhau, AnhDaiDien, NgayDangKy, TrangThai, VaiTro)
VALUES 
(N'Sinh viên', 'sinhvien@hcmue.edu.vn', '123456', NULL, GETDATE(), 'HoatDong', 'SinhVien'),
(N'Giảng viên', 'giangvien@hcmue.edu.vn', '123456', NULL, GETDATE(), 'HoatDong', 'GiangVien'),
(N'Quản trị viên', 'admin@hcmue.edu.vn', '123456', NULL, GETDATE(), 'HoatDong', 'Admin');
```

**Bước 6: Chạy ứng dụng** Nhấn `F5` hoặc nút **Run** trong Visual Studio để khởi chạy dự án.

---

## 👥 Tài khoản Test mặc định (Gợi ý)

Sau khi đã cài đặt môi trường xong, bạn có thể đăng nhập bằng các tài khoản sau:

* **Quản trị viên:** `admin@hcmue.edu.vn` | Mật khẩu: `123456`
* **Giảng viên:** `giangvien@hcmue.edu.vn` | Mật khẩu: `123456`
* **Sinh viên:** `sinhvien@hcmue.edu.vn` | Mật khẩu: `123456`