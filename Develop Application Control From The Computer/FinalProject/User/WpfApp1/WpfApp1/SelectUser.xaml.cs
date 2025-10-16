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
        static string url_user = "https://user-play-game-default-rtdb.firebaseio.com/User.json";
        static string url_question = "https://ailatrieuphu-34a98-default-rtdb.firebaseio.com/.json";
        private static readonly HttpClient httpClient = new HttpClient();
        private List<DataQuestion.CauHoi>? danhSachCauHoi;
        private DispatcherTimer countdownTimer = new DispatcherTimer();
        private int seconds = 30;
        private string correctAnswer = string.Empty;
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
        private async void LoadAllQuestionFromFireBase()
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
            danhSachCauHoi = danhSachCauHoi.OrderBy(x => Guid.NewGuid()).ToList();
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
                /* Start timer */
                countdownTimer.Start();
            }
        }
        public SelectUser()
        {
            InitializeComponent();
            StartCountdown();
            LoadAllQuestionFromFireBase();
            StartQuestionToPickUser();
        }
        public void StartCountdown()
        {
            // Đặt khoảng thời gian lặp lại (ví dụ: mỗi 1 giây)
            countdownTimer.Interval = TimeSpan.FromSeconds(1);

            // Đăng ký phương thức sẽ được gọi mỗi khi timer tick
            countdownTimer.Tick += CountdownTimer_Tick;

        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            // Giảm số giây và cập nhật TextBlock (giả sử có một TextBlock tên là txtTimer)
            seconds--;
            txtTimer.Text = seconds.ToString();

            if (seconds <= 0)
            {
                countdownTimer.Stop();
                txtTimer.Text = countdownTimer.ToString();
            }
            /*Get user data to read all timer */
            /*Check if user have the minium time to answer */
        }
        private void HandleAnswer(string answer)
        {
            if (answer == null) { }
            else
            {
                if (answer == correctAnswer)
                {

                }
                else
                {

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
    }
}
