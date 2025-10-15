using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WpfApp1
{
    public partial class Register_User : UserControl
    {
        // ===================== CLASS NGƯỜI CHƠI =====================
        public class PlayerData
        {
            [JsonProperty("name")]
            public string name { get; set; } = string.Empty;
            [JsonProperty("score")]
            public int score { get; set; } = 0;
            [JsonProperty("hasPlayed")]
            public bool hasPlayed { get; set; } = false;
            [JsonProperty("currentAnswer")]
            public string currentAnswer { get; set; } = string.Empty;   
            [JsonProperty("lastAnswerTime")]
            public long lastAnswerTime { get; set; } = 0;
        }

        private readonly string firebaseUrl = "https://user-play-game-default-rtdb.firebaseio.com";
        private readonly int maxPlayers = 6;
        private readonly HttpClient httpClient = new HttpClient();
        private DispatcherTimer? timer;

        public Register_User()
        {
            InitializeComponent();
            StartMonitoringPlayers();
        }

        // ===================== Handle event when click on button register  =====================
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string playerName = txtPlayerName.Text.Trim();

            // Kiểm tra tên hợp lệ
            if (string.IsNullOrEmpty(playerName))
            {
                MessageBox.Show("Vui lòng nhập tên người chơi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (playerName.Length < 2)
            {
                MessageBox.Show("Tên phải có ít nhất 2 ký tự!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disable nút và hiển thị trạng thái
            btnPlay.IsEnabled = false;
            btnPlay.Content = "ĐANG KẾT NỐI... ⏳";

            try
            {
                /* Gửi yêu cầu đăng ký người chơi */
                bool success = await RegisterPlayer(playerName);

                if (success)
                {
                    MessageBox.Show($"Đăng ký thành công! Đang chờ {maxPlayers} người chơi...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtPlayerName.IsEnabled = false;
                    btnPlay.Content = "ĐÃ ĐĂNG KÝ ✓";
                }
                else
                {
                    MessageBox.Show("Phòng đã đầy! Vui lòng thử lại sau.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    btnPlay.IsEnabled = true;
                    btnPlay.Content = "CHƠI NGAY! 🚀";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                btnPlay.IsEnabled = true;
                btnPlay.Content = "CHƠI NGAY! 🚀";
            }
        }

        // ===================== ĐĂNG KÝ NGƯỜI CHƠI =====================
        private async Task<bool> RegisterPlayer(string playerName)
        {
            try
            {
                /* Lấy danh sách người chơi hiện tại với node dữ liệu là players */
                string url = $"{firebaseUrl}/players.json";
                /* Lấy dữ liệu JSON từ trên node players */
                string json = await httpClient.GetStringAsync(url);

                List<PlayerData> players;

                // Kiểm tra xem chuỗi json có đang bị trống hoặc "null"
                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    // Chưa có dữ liệu -> tạo danh sách mới
                    players = new List<PlayerData>();
                    for (int i = 0; i < maxPlayers; i++)
                    {
                        players.Add(new PlayerData());
                    }
                }
                else
                {
                    // Deserialize JSON
                    var deserialized = JsonConvert.DeserializeObject<List<PlayerData>>(json);
                    players = deserialized ?? new List<PlayerData>();

                    // Nếu JSON có ít hơn maxPlayers, thêm các slot trống
                    while (players.Count < maxPlayers)
                    {
                        players.Add(new PlayerData());
                    }
                }

                // Tìm slot trống
                for (int i = 0; i < players.Count; i++)
                {
                    if (string.IsNullOrEmpty(players[i].name))
                    {
                        // Cập nhật thông tin người chơi
                        players[i].name = playerName;
                        players[i].score = 0;
                        players[i].hasPlayed = false;

                        // Gửi lên Firebase
                        string playerUrl = $"{firebaseUrl}/players/{i}.json";
                        string playerJson = JsonConvert.SerializeObject(players[i]);
                        var content = new StringContent(playerJson, Encoding.UTF8, "application/json");

                        var response = await httpClient.PutAsync(playerUrl, content);
                        return response.IsSuccessStatusCode;
                    }
                }

                return false; // Không còn slot trống
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể đăng ký người chơi: {ex.Message}");
            }
        }

        // ===================== THEO DÕI NGƯỜI CHƠI =====================
        private void StartMonitoringPlayers()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += async (s, e) => await CheckPlayers();
            timer.Start();

            // Check ngay lần đầu
            _ = CheckPlayers();
        }

        private async Task CheckPlayers()
        {
            try
            {
                string url = $"{firebaseUrl}/players.json";
                string json = await httpClient.GetStringAsync(url);

                List<PlayerData> players;

                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    players = new List<PlayerData>();
                }
                else
                {
                    var deserialized = JsonConvert.DeserializeObject<List<PlayerData>>(json);
                    players = deserialized ?? new List<PlayerData>();
                }

                // Đếm số người đã đăng ký
                int registered = 0;
                foreach (var player in players)
                {
                    if (!string.IsNullOrEmpty(player.name))
                        registered++;
                }

                // Cập nhật UI
                txtPlayerCount.Text = $"{registered}/{maxPlayers}";

                // Nếu đủ người chơi
                if (registered >= maxPlayers)
                {
                    timer?.Stop();

                    // Cập nhật trạng thái game
                    string statusUrl = $"{firebaseUrl}/gameStatus.json";
                    var content = new StringContent("\"playing\"", Encoding.UTF8, "application/json");
                    await httpClient.PutAsync(statusUrl, content);

                    // Chuyển sang màn hình SelectUser
                    if (Application.Current.MainWindow is MainWindow main)
                    {
                        main.MainContent.Children.Clear();
                        main.MainContent.Children.Add(new SelectUser());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi kiểm tra người chơi: " + ex.Message);
            }
        }
    }
}
