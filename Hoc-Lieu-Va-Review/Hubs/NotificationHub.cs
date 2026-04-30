using Microsoft.AspNetCore.SignalR;

namespace Hoc_Lieu_Va_Review.Hubs
{
    // Kế thừa từ class Hub của thư viện SignalR
    public class NotificationHub : Hub
    {
        // Vì tính năng thông báo này chỉ đi một chiều: Server ---> Client 
        // (Máy chủ tự động báo xuống trình duyệt chứ trình duyệt không gửi ngược lên)
        // Nên tạm thời không cần viết thêm hàm nào ở đây cả. 
        // Chỉ cần class này tồn tại là Server đã có chỗ để "đứng phát thanh" rồi.
    }
}