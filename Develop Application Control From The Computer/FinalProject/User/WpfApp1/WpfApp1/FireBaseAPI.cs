using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class FireBaseAPI
    {
        private static HttpClient httpClient = new HttpClient();
        private static string url_user = "https://user-play-game-default-rtdb.firebaseio.com";
        public static async Task<string> getGameStatus()
        {
            try
            {
                string url = $"{url_user}/gameStatus.json";
                string json = await httpClient.GetStringAsync(url);
                return json;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return "error";
            }
        }
        public static async Task<bool> PushGameStatus(string status)
        {
            try
            {
                string url = $"{url_user}/gameStatus.json";

                // Đặt chuỗi trong dấu ngoặc kép cho hợp lệ JSON
                string jsonString = $"\"{status}\"";

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                // PUT = ghi đè giá trị tại node
                var response = await httpClient.PutAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
