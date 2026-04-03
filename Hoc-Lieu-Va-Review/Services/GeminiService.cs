using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hoc_Lieu_Va_Review.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["GeminiApi:ApiKey"];
        }

        public async Task<string> KiemDuyetVanBan(string noiDung)
        {
            // 1. Kiểm tra xem có quên nhập API Key không
            if (string.IsNullOrEmpty(_apiKey) || _apiKey.Contains("DÁN_CÁI_MÃ"))
            {
                Console.WriteLine("\n==== [LỖI]: BẠN CHƯA CẤU HÌNH API KEY TRONG APPSETTINGS.JSON ====\n");
                return "ChoDuyet";
            }

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            string prompt = $@"Bạn là một quản trị viên kiểm duyệt nội dung. 
            Hãy phân loại câu sau. CHỈ TRẢ VỀ ĐÚNG 1 TỪ DUY NHẤT (không giải thích, không dùng dấu chấm, không in đậm):
            - HopLe (Nếu câu bình thường)
            - TuChoi (Nếu câu chửi bậy, xúc phạm, rác)
            - ChoDuyet (Nếu không chắc chắn)
            
            Câu cần duyệt: ""{noiDung}""";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync(); // Đọc toàn bộ kết quả để xem thử

                if (response.IsSuccessStatusCode)
                {
                    var json = JObject.Parse(responseString);
                    var resultText = json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                    // In ra cửa sổ Console xem AI thực sự đã nói cái gì
                    Console.WriteLine($"\n==== [AI TRẢ VỀ GỐC]: '{resultText}' ====\n");

                    if (string.IsNullOrEmpty(resultText)) return "ChoDuyet";

                    // Dọn dẹp mạnh tay: bỏ dấu sao in đậm, dấu chấm, khoảng trắng, xuống dòng
                    resultText = resultText.Replace("*", "").Replace(".", "").Replace("\n", "").Replace("\r", "").Trim();

                    // Dùng Contains thay vì dấu == để bắt chữ chính xác hơn
                    if (resultText.Contains("HopLe", StringComparison.OrdinalIgnoreCase)) return "HopLe";
                    if (resultText.Contains("TuChoi", StringComparison.OrdinalIgnoreCase)) return "TuChoi";

                    return "ChoDuyet";
                }
                else
                {
                    // In ra lỗi nếu Google từ chối kết nối
                    Console.WriteLine($"\n==== [LỖI API GOOGLE]: {responseString} ====\n");
                    return "ChoDuyet";
                }
            }
            catch (Exception ex)
            {
                // In ra lỗi nếu đứt mạng hoặc code chạy sai
                Console.WriteLine($"\n==== [LỖI CODE/MẠNG]: {ex.Message} ====\n");
                return "ChoDuyet";
            }
        }
    }
}