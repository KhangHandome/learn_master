using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for SelectUser.xaml
    /// </summary>
    /// 
    
    public partial class SelectUser : UserControl
    {
        private DispatcherTimer countdownTimer = new DispatcherTimer();
        private int seconds = 60;

        public void StartCountdown()
        {
            // Đặt khoảng thời gian lặp lại (ví dụ: mỗi 1 giây)
            countdownTimer.Interval = TimeSpan.FromSeconds(1);

            // Đăng ký phương thức sẽ được gọi mỗi khi timer tick
            countdownTimer.Tick += CountdownTimer_Tick;

            // Bắt đầu timer
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object ? sender, EventArgs e)
        {
            // Giảm số giây và cập nhật TextBlock (giả sử có một TextBlock tên là txtTimer)
            seconds--;
            txtTimer.Text = seconds.ToString();

            if (seconds <= 0)
            {
                countdownTimer.Stop();
                txtTimer.Text = countdownTimer.ToString();
            }
        }

        public SelectUser()
        {
            InitializeComponent();
            StartCountdown();
        }
        
        private void Register_User_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(true)
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.MainContent.Children.Clear();
                    main.MainContent.Children.Add(new Player());
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.MainContent.Children.Clear();
                    main.MainContent.Children.Add(new Player());
                }            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.MainContent.Children.Clear();
                    main.MainContent.Children.Add(new Player());
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                Window currentWindow = Application.Current.MainWindow;
                MainWindow mainWindow = (MainWindow)currentWindow;
                mainWindow.MainContent.Children.Clear();
                Player newGameScreen = new Player();
                mainWindow.MainContent.Children.Add(newGameScreen);
            }
        }
    }
}
