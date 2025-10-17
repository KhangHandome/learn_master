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
        public static string gameStatus = "waiting"; // waiting, playing, ended
        public static int currentQuestion = 0;
        private static HttpClient httpClient = new HttpClient();
        private static string url_user = "https://user-play-game-default-rtdb.firebaseio.com";
        private static string url_questions = "https://ailatrieuphu-34a98-default-rtdb.firebaseio.com/.json";
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
        public static async Task<int> GetQuestionNumber()
        {
            try
            {
                string url = $"{url_user}/currentQuestion.json";
                string json = await httpClient.GetStringAsync(url);

                // Loại bỏ dấu ngoặc kép nếu Firebase lưu dưới dạng string
                json = json.Trim('"');

                if (int.TryParse(json, out int questionNumber))
                {
                    return questionNumber;
                }
                else
                {
                    MessageBox.Show("Dữ liệu questionNumber không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        public static async Task<bool> PushQuestionNumber(int questionNumber)
        {
            try
            {
                string url = $"{url_user}/currentQuestion.json";

                // Vì questionNumber là int, không cần dấu ngoặc kép
                string jsonString = questionNumber.ToString();

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
        public static async Task<int> getRoundTimeSeconds()
        {
            try
            {
                string url = $"{url_user}/roundTimeSeconds.json";
                string json = await httpClient.GetStringAsync(url);

                // Loại bỏ dấu ngoặc kép nếu Firebase lưu dưới dạng string
                json = json.Trim('"');

                if (int.TryParse(json, out int questionNumber))
                {
                    return questionNumber;
                }
                else
                {
                    MessageBox.Show("Dữ liệu  không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        public static async Task<bool> pushRoundTimeSeconds(int RoundTimeSecond)
        {
            try
            {
                string url = $"{url_user}/currentQuestion.json";

                // Vì questionNumber là int, không cần dấu ngoặc kép
                string jsonString = RoundTimeSecond.ToString();

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
        public static async Task<List<DataQuestion.CauHoi>> getQuestions()
        {
            try
            {
                string json = await httpClient.GetStringAsync(url_questions);
                var danhSach = JsonConvert.DeserializeObject<List<DataQuestion.CauHoi>>(json);
                return danhSach ?? new List<DataQuestion.CauHoi>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DataQuestion.CauHoi>();
            }
        }
        public static async Task<List<UserControl>> getUserPlay()
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                string url = $"{url_user}/players.json";
                string json = await httpClient.GetStringAsync(url);
                var danhSach = JsonConvert.DeserializeObject<List<UserControl>>(json);
                return danhSach ?? new List<UserControl>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<UserControl>();
            }
        }
        public static async Task<bool> pushUserPlay(UserControl user)
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                string playerUrl = $"{url_user}/players/{UserControl.ID_Player}.json";
                string playerJson = JsonConvert.SerializeObject(user);
                var content = new StringContent(playerJson, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync(playerUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi push dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
