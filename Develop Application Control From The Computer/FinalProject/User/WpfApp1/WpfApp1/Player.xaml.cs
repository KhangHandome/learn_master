using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp1
{
    // ===== CLASS XỬ LÝ FIREBASE =====
    public class FirebaseService
    {
        private readonly static HttpClient httpClient = new HttpClient();

        public static async Task<List<DataQuestion.CauHoi>> LayTatCaCauHoi(string url)
        {
            try
            {
                string json = await httpClient.GetStringAsync(url);
                var danhSach = JsonConvert.DeserializeObject<List<DataQuestion.CauHoi>>(json);
                return danhSach ?? new List<DataQuestion.CauHoi>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DataQuestion.CauHoi>();
            }
        }
    }

    // ===== CLASS CHÍNH GAME =====
    public partial class Player : System.Windows.Controls.UserControl
    {
        private List<UserControl>? danhSachUser;
        static string url_user = "https://user-play-game-default-rtdb.firebaseio.com";
        private const string FIREBASE_URL = "https://ailatrieuphu-34a98-default-rtdb.firebaseio.com/.json";
        // Mảng tiền thưởng
        private readonly int[] prizeMilestones = new int[]
        {
            200000, 400000, 600000, 1000000, 2000000, 3000000, 6000000, 10000000,
            14000000, 22000000, 30000000, 40000000, 60000000, 85000000, 150000000
        };
        public static async Task<List<UserControl>> LayTatCaUser()
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

        // Danh sách câu hỏi
        private List<DataQuestion.CauHoi>? danhSachCauHoi;

        // Câu hỏi hiện tại
        private int currentQuestionIndex = 0;

        // Timer
        private DispatcherTimer? timer;
        private int timeLeft = 30;

        // Trợ giúp đã dùng
        private bool used5050 = false;
        private bool usedAudience = false;

        // Câu trả lời đúng
        private string correctAnswer = string.Empty;

        // Các đáp án đã bị loại bởi 50:50
        private readonly List<string> removedAnswers = new List<string>();

        public Player()
        {
            InitializeComponent();
            KhoiTaoGame();
        }
        private async void ExitGame()
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                if (danhSachUser == null) { return; }
                danhSachUser[UserControl.ID_Player].lastAnswerTime = 100;
                danhSachUser[UserControl.ID_Player].hasPlayed = true;
                danhSachUser[UserControl.ID_Player].currentAnswer = correctAnswer;
                danhSachUser[UserControl.ID_Player].score = prizeMilestones[Math.Max(0, currentQuestionIndex - 1)];
                danhSachUser[UserControl.ID_Player].name = string.Empty;
                string playerUrl = $"{url_user}/players/{UserControl.ID_Player}.json";
                string playerJson = JsonConvert.SerializeObject(danhSachUser[UserControl.ID_Player]);
                var content = new StringContent(playerJson, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(playerUrl, content);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật Firebase: {ex.Message}");
            }
        }
        private async void KhoiTaoGame()
        {
            danhSachUser = await LayTatCaUser();
            // Hiển thị loading
            labelQuestion.Text = "Đang tải câu hỏi...";
            DisableAllButtons();

            // Tải câu hỏi từ Firebase
            danhSachCauHoi = await FirebaseService.LayTatCaCauHoi(FIREBASE_URL);

            if (danhSachCauHoi == null || danhSachCauHoi.Count == 0)
            {
                labelQuestion.Text = "Không thể tải câu hỏi. Vui lòng thử lại!";
                return;
            }

            // Trộn ngẫu nhiên câu hỏi
            danhSachCauHoi = danhSachCauHoi.OrderBy(x => Guid.NewGuid()).ToList();

            // Bắt đầu game
            currentQuestionIndex = 0;
            await UpdatePrize();
            LoadQuestion();
            StartTimer();
        }

        private void LoadQuestion()
        {
            if (danhSachCauHoi == null || currentQuestionIndex >= 15 || currentQuestionIndex >= danhSachCauHoi.Count)
            {
                WinGame();
                return;
            }

            var cauHoi = danhSachCauHoi[currentQuestionIndex];

            // Hiển thị câu hỏi
            labelQuestion.Text = cauHoi.NoiDung;

            // Hiển thị đáp án
            btnAnswerA.Content = $"A) {cauHoi.LuaChon.A}";
            btnAnswerB.Content = $"B) {cauHoi.LuaChon.B}";
            btnAnswerC.Content = $"C) {cauHoi.LuaChon.C}";
            btnAnswerD.Content = $"D) {cauHoi.LuaChon.D}";

            // Reset màu các nút
            ResetButtonColors();

            // Lưu đáp án đúng
            correctAnswer = cauHoi.DapAn;

            // Reset timer
            timeLeft = 30;
            txtTimer.Text = timeLeft.ToString();

            // Enable tất cả các nút (bao gồm cả nút bị disable từ 50:50)
            EnableAllButtons();

            // Xóa danh sách đáp án đã loại
            removedAnswers.Clear();
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timeLeft--;
            txtTimer.Text = timeLeft.ToString();

            // Đổi màu cảnh báo khi còn ít thời gian
            if (timeLeft <= 10)
            {
                txtTimer.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                txtTimer.Foreground = new SolidColorBrush(Colors.White);
            }

            // Hết giờ
            if (timeLeft <= 0)
            {
                timer?.Stop();
                GameOver("Hết thời gian!");
            }
        }

        public async Task UpdatePrize()
        {
            if (currentQuestionIndex < prizeMilestones.Length)
            {
                int currentPrize = prizeMilestones[currentQuestionIndex];
                labelPrize.Text = $"{currentPrize:N0} đ";
                txtQuestionNum.Text = (currentQuestionIndex + 1).ToString();
            }

            await Task.CompletedTask;
        }

        // ===== XỬ LÝ CHỌN ĐÁP ÁN =====
        private async void CheckAnswer(string selectedAnswer, Button selectedButton)
        {
            timer?.Stop();
            DisableAllButtons();

            if (selectedAnswer == correctAnswer)
            {
                // Đúng - Đổi màu xanh
                selectedButton.Background = new SolidColorBrush(Colors.Green);
                await Task.Delay(1500);

                currentQuestionIndex++;
                await UpdatePrize();

                if (currentQuestionIndex >= 15)
                {
                    WinGame();
                }
                else
                {
                    LoadQuestion();
                    timer?.Start();
                }
            }
            else
            {
                // Sai - Đổi màu đỏ
                selectedButton.Background = new SolidColorBrush(Colors.Red);

                // Hiện đáp án đúng màu xanh
                GetButtonByAnswer(correctAnswer).Background = new SolidColorBrush(Colors.Green);

                await Task.Delay(2000);
                GameOver($"Sai rồi! Đáp án đúng là {correctAnswer}");
            }
        }

        private Button GetButtonByAnswer(string answer)
        {
            return answer.ToUpper() switch
            {
                "A" => btnAnswerA,
                "B" => btnAnswerB,
                "C" => btnAnswerC,
                "D" => btnAnswerD,
                _ => btnAnswerA
            };
        }

        // ===== TRỢ GIÚP 50:50 =====
        private void Button_Click_5050(object sender, RoutedEventArgs e)
        {
            if (used5050) return;

            used5050 = true;
            btn5050.IsEnabled = false;

            // Lấy danh sách đáp án sai
            List<string> wrongAnswers = new List<string> { "A", "B", "C", "D" };
            wrongAnswers.Remove(correctAnswer);

            // Chọn ngẫu nhiên 2 đáp án sai để đổi màu đỏ
            Random random = new Random();
            for (int i = 0; i < 2; i++)
            {
                int index = random.Next(wrongAnswers.Count);
                string answerToRemove = wrongAnswers[index];

                // Đổi màu đỏ và disable nút
                Button btn = GetButtonByAnswer(answerToRemove);
                btn.Background = new SolidColorBrush(Colors.Red);
                btn.IsEnabled = false;
                removedAnswers.Add(answerToRemove);

                wrongAnswers.RemoveAt(index);
            }
        }

        // ===== TRỢ GIÚP HỎI KHÁN GIẢ =====
        private void Button_Click_Audience(object sender, RoutedEventArgs e)
        {
            if (usedAudience) return;

            usedAudience = true;
            btnAudience.IsEnabled = false;

            // Tạo tỷ lệ % cho đáp án đúng (60-80%)
            Random random = new Random();
            int correctPercent = random.Next(60, 81);

            // Chia phần còn lại cho các đáp án khác
            List<int> otherPercents = new List<int>();
            int remaining = 100 - correctPercent;

            for (int i = 0; i < 2; i++)
            {
                int percent = random.Next(0, remaining / 2);
                otherPercents.Add(percent);
                remaining -= percent;
            }
            otherPercents.Add(remaining);
            otherPercents = otherPercents.OrderByDescending(x => x).ToList();

            // Tạo message
            string message = "Kết quả khán giả:\n\n";
            List<string> answers = new List<string> { "A", "B", "C", "D" };

            int otherIndex = 0;
            foreach (string ans in answers)
            {
                if (ans == correctAnswer)
                {
                    message += $"{ans}: {correctPercent}%\n";
                }
                else
                {
                    message += $"{ans}: {otherPercents[otherIndex]}%\n";
                    otherIndex++;
                }
            }

            MessageBox.Show(message, "Hỏi ý kiến khán giả", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ===== XỬ LÝ NÚT ĐÁP ÁN =====
        private void Button_Click_A(object sender, RoutedEventArgs e) => CheckAnswer("A", btnAnswerA);
        private void Button_Click_B(object sender, RoutedEventArgs e) => CheckAnswer("B", btnAnswerB);
        private void Button_Click_C(object sender, RoutedEventArgs e) => CheckAnswer("C", btnAnswerC);
        private void Button_Click_D(object sender, RoutedEventArgs e) => CheckAnswer("D", btnAnswerD);

        // ===== DỪNG CUỘC CHƠI =====
        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            timer?.Stop();

            int prize = currentQuestionIndex > 0 ? prizeMilestones[currentQuestionIndex - 1] : 0;
            MessageBox.Show($"Bạn dừng cuộc chơi!\nSố tiền nhận được: {prize:N0} đ",
                          "Kết thúc", MessageBoxButton.OK, MessageBoxImage.Information);
            ExitGame();
            BackToMenu();
        }

        // ===== GAME OVER =====
        private void GameOver(string message)
        {
            int prize = currentQuestionIndex > 0 ? prizeMilestones[currentQuestionIndex - 1] : 0;
            MessageBox.Show($"{message}\n\nSố tiền nhận được: {prize:N0} đ",
                          "Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);
            ExitGame();
            BackToMenu();
        }

        // ===== CHIẾN THẮNG =====
        private void WinGame()
        {
            timer?.Stop();
            MessageBox.Show($"🎉 CHÚC MỪNG! 🎉\n\nBạn đã chiến thắng!\nTổng giải thưởng: {prizeMilestones[14]:N0} đ",
                          "Chiến thắng!", MessageBoxButton.OK, MessageBoxImage.Information);
            ExitGame();
            BackToMenu();
        }

        // ===== UTILITY =====
        private void DisableAllButtons()
        {
            btnAnswerA.IsEnabled = false;
            btnAnswerB.IsEnabled = false;
            btnAnswerC.IsEnabled = false;
            btnAnswerD.IsEnabled = false;
        }

        private void EnableAllButtons()
        {
            btnAnswerA.IsEnabled = true;
            btnAnswerB.IsEnabled = true;
            btnAnswerC.IsEnabled = true;
            btnAnswerD.IsEnabled = true;
        }

        private void ResetButtonColors()
        {
            btnAnswerA.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            btnAnswerB.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            btnAnswerC.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            btnAnswerD.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
        }

        private void BackToMenu()
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainContent.Children.Clear();
                main.MainContent.Children.Add(new Register_User());
            }
        }
    }
}