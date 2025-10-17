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
    public partial class Register_User : System.Windows.Controls.UserControl
    {
        private readonly string firebaseUrl = "https://user-play-game-default-rtdb.firebaseio.com";
        private readonly int maxPlayers = 6;
        private readonly HttpClient httpClient = new HttpClient();
        private DispatcherTimer timer = new DispatcherTimer();
        bool isInputData = false;
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

            // Disable nút và hiển thị trạng thái
            btnPlay.IsEnabled = false;
            btnPlay.Content = "ĐANG KẾT NỐI... ⏳";

            try
            {
                /* Gửi yêu cầu đăng ký người chơi */
                bool success = await RegisterPlayer(playerName);

                if (success)
                {
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
            List<UserControl> players;
            string url = string.Empty;
            string json = string.Empty;
            try
            {
                /* Lấy danh sách người chơi hiện tại với node dữ liệu là players */
                players = await FireBaseAPI.getUserPlay();

                // Nếu JSON có ít hơn maxPlayers, thêm các slot trống
                while (players.Count < maxPlayers)
                {
                    /* Add new slot */
                    players.Add(new UserControl());
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
                        isInputData = true;
                        UserControl.ID_Player = i;
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
            /* Register a timer to check players every 1 seconds */
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += async (s, e) => await CheckPlayers();
            timer.Start();
        }

        private async Task CheckPlayers()
        {
            /* Clear current player list */
            List<UserControl> players;
            // Đếm số người đã đăng ký
            int user_registered = 0;
            try
            {
                /* URL to get data from firebase */
                players = await FireBaseAPI.getUserPlay();
                foreach (var player in players)
                {
                    if (!string.IsNullOrEmpty(player.name))
                    {
                        user_registered++;
                    }
                }

                // Cập nhật UI
                txtPlayerCount.Text = $"{user_registered}/{maxPlayers}";

                // Nếu đủ người chơi
                if (user_registered >= maxPlayers)
                {
                    await FireBaseAPI.PushGameStatus("select_user");
                    // Chuyển sang màn hình SelectUser
                    if (Application.Current.MainWindow is MainWindow main && isInputData == true)
                    {
                        timer?.Stop();
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
