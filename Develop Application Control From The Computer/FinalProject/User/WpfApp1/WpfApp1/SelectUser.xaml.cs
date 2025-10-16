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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static WpfApp1.DataQuestion;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for SelectUser.xaml
    /// </summary>
    /// 
    
    public partial class SelectUser : System.Windows.Controls.UserControl
    {
        static string url_user = "https://user-play-game-default-rtdb.firebaseio.com";
        static string url_question = "https://ailatrieuphu-34a98-default-rtdb.firebaseio.com/.json";
        private static readonly HttpClient httpClient = new HttpClient();
        private List<DataQuestion.CauHoi>? danhSachCauHoi;
        private List<UserControl>? danhSachUser;
        private DispatcherTimer countdownTimer = new DispatcherTimer();
        private int seconds = 30;
        private string correctAnswer = string.Empty;
        private bool hasChoicePlayer = false;
        private bool isAnswer = false;
        int currentQuestionIndex = 0; 
        public static async Task<List<DataQuestion.CauHoi>> SelectUSERLayTatCaCauHoi()
        {
            try
            {
                string json = await httpClient.GetStringAsync(url_question);
                var danhSach = JsonConvert.DeserializeObject<List<DataQuestion.CauHoi>>(json);
                return danhSach ?? new List<DataQuestion.CauHoi>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DataQuestion.CauHoi>();
            }
        }
        public static async Task<List<UserControl>> GetStatus()
        {
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
        public static async Task<List<UserControl>> LayTatCaUser()
        {
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
        private async Task LoadAllQuestionFromFireBase()
        {
            
            danhSachCauHoi = await SelectUSERLayTatCaCauHoi();
            if(danhSachCauHoi == null)
            {
                return; 
            }
            else
            {

            }
            // Trộn ngẫu nhiên câu hỏi
        }
        private async Task LoadAllUserFromFireBase()
        {
            danhSachUser = await LayTatCaUser();
            if (danhSachUser == null)
            {
                return;
            }
            else
            {

            }
            // Trộn ngẫu nhiên câu hỏi
        }
        // Tạo hàm khởi tạo bất đồng bộ
        private async void InitializeAsync()
        {
            await LoadAllUserFromFireBase();
            // await (Chờ) cho đến khi dữ liệu được tải xong.
            await LoadAllQuestionFromFireBase();

            // CHỈ KHI dữ liệu đã xong, mới tiến hành hiển thị câu hỏi.
            StartQuestionToPickUser();
        }
        private void StartQuestionToPickUser()
        {
            if (danhSachCauHoi != null)
            {
                var question = danhSachCauHoi[currentQuestionIndex];
                btnAnswer1.Content = $"A) {question.LuaChon.A}";
                btnAnswer2.Content = $"B) {question.LuaChon.B}";
                btnAnswer3.Content = $"C) {question.LuaChon.C}";
                btnAnswer4.Content = $"D) {question.LuaChon.D}";
                txtQuestion.Text = question.NoiDung;
                correctAnswer = question.DapAn;
                currentQuestionIndex += 1;
                seconds = 30;
                /* Start timer */
                countdownTimer.Start();
                isAnswer = false;
                ResetAnswerButtons();
            }
        }
        public SelectUser()
        {
            InitializeComponent();
            StartCountdown();
            // DÒNG CHỦ CHỐT: await (Chờ) cho đến khi dữ liệu được tải xong.
            InitializeAsync();
        }
        public void StartCountdown()
        {
            // Đặt khoảng thời gian lặp lại (ví dụ: mỗi 1 giây)
            countdownTimer.Interval = TimeSpan.FromSeconds(1);

            // Đăng ký phương thức sẽ được gọi mỗi khi timer tick
            countdownTimer.Tick +=  CountdownTimer_Tick;

        }

        private async void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            // Giảm số giây và cập nhật TextBlock (giả sử có một TextBlock tên là txtTimer)
            seconds--;
            txtTimer.Text = seconds.ToString();
            bool isWiner;
            if (seconds <= 1)
            {
                countdownTimer.Stop();
                txtTimer.Text = countdownTimer.ToString();
                if(isAnswer == false)
                {
                    HandleAnswer("X");
                }
                isWiner = await DetermineWiner();
                if (hasChoicePlayer == true)
                {
                    if(isWiner == true)
                    {
                        ChangeDisplayToPlayer();
                    }
                    else
                    {
                        txtStatus.Text = "Mất lượt ";
                    }
                }
                else
                {
                    hasChoicePlayer = false;
                    StartQuestionToPickUser();
                }
            }
            /*Get user data to read all timer */
            /*Check if user have the minium time to answer */
        }
        private async Task<bool> DetermineWiner()
        {
            int minumTimeToAnswer = 30;
            bool retVal;
            await LoadAllUserFromFireBase();
            if (danhSachUser == null) { return false; }
            /* Get minium to ansswer */
            for ( int i = 0; i < danhSachUser.Count; i++)
            {
                if(danhSachUser[i].lastAnswerTime < minumTimeToAnswer && danhSachUser[i].lastAnswerTime < 30 && danhSachUser[i].hasPlayed == false )
                {
                    minumTimeToAnswer = (int)danhSachUser[i].lastAnswerTime;
                    hasChoicePlayer = true;
                }
            }
            if (danhSachUser[UserControl.ID_Player].lastAnswerTime <= minumTimeToAnswer && danhSachUser[UserControl.ID_Player].hasPlayed == false)
            {
                retVal = true;
            }
            else 
            {
                retVal = false;
            }
            return retVal;
        }

        private async void HandleAnswer(string answer)
        {
            if (answer == null) { return; }

            // Vô hiệu hóa tất cả nút để tránh click thêm
            DisableAllAnswerButtons();

            // Lấy nút được chọn
            Button selectedButton = GetButtonByAnswer(answer);

            isAnswer = true;
            if (answer == correctAnswer)
            {
                // ✅ Trả lời đúng - Hiển thị màu xanh
                selectedButton.Background = new SolidColorBrush(Colors.Green);
                txtStatus.Text = "Chính xác! 🎉";
                try
                {
                    if (danhSachUser == null) { return; }
                    danhSachUser[UserControl.ID_Player].lastAnswerTime = 30 - seconds;

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
            else
            {
                // ❌ Trả lời sai - Hiển thị màu đỏ cho nút được chọn
                selectedButton.Background = new SolidColorBrush(Colors.Red);
                txtStatus.Text = $"Sai rồi! Đáp án đúng là {correctAnswer}.";
                try
                {
                    if (danhSachUser == null) { return; }
                    danhSachUser[UserControl.ID_Player].lastAnswerTime = 100;

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
        }
        private void ChangeDisplayToPlayer()
        {
            // Chuyển sang màn hình SelectUser
            if (Application.Current.MainWindow is MainWindow main )
            {
                main.MainContent.Children.Clear();
                main.MainContent.Children.Add(new Player());
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HandleAnswer("A");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            HandleAnswer("B");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            HandleAnswer("C");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            HandleAnswer("D");
        }
        private Button GetButtonByAnswer(string answer)
        {
            return answer.ToUpper() switch
            {
                "A" => btnAnswer1,
                "B" => btnAnswer2,
                "C" => btnAnswer3,
                "D" => btnAnswer4,
                _ => btnAnswer1
            };
        }
        // Hàm tắt tất cả nút để tránh click liên tiếp
        private void DisableAllAnswerButtons()
        {
            btnAnswer1.IsEnabled = false;
            btnAnswer2.IsEnabled = false;
            btnAnswer3.IsEnabled = false;
            btnAnswer4.IsEnabled = false;
        }

        // Hàm bật lại tất cả nút
        private void EnableAllAnswerButtons()
        {
            btnAnswer1.IsEnabled = true;
            btnAnswer2.IsEnabled = true;
            btnAnswer3.IsEnabled = true;
            btnAnswer4.IsEnabled = true;
        }

        // Hàm reset màu các nút về trạng thái ban đầu
        private void ResetAnswerButtons()
        {
            btnAnswer1.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));  // #2196F3
            btnAnswer2.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            btnAnswer3.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            btnAnswer4.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            EnableAllAnswerButtons();
        }
    }
}
