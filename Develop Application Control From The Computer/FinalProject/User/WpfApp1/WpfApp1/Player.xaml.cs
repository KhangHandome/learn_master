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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : UserControl
    {
        // Mảng tiền thưởng theo thứ tự từng câu
        private readonly int[] prizeMilestones = new int[]
        {
            1000, 2000, 3000, 5000, 10000, 20000, 40000, 80000, 160000, 320000, 640000, 1250000, 2500000, 5000000, 10000000
        };

        // Biến lưu câu hiện tại (0 = câu 1)
        private int currentQuestionIndex = 0;
        private void UpdatePrize()
        {
            if (currentQuestionIndex < prizeMilestones.Length)
            {
                int currentPrize = prizeMilestones[currentQuestionIndex];
                labelPrize.Content = $"Mốc thưởng: {currentPrize:N0} VND"; // N0 định dạng số với dấu phẩy
            }
            else
            {
                labelPrize.Content = "Bạn đã thắng toàn bộ giải thưởng!";
            }
        }
        private void CorrectAnswer()
        {
            // Tăng câu hiện tại
            currentQuestionIndex++;

            // Cập nhật mốc thưởng
            UpdatePrize();

            // Có thể load câu hỏi tiếp theo ở đây
            /*LoadNextQuestion();*/
        }
        public Player()
        {
            InitializeComponent();
        }

        private void Button_Click_A(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                
            }
            else
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.MainContent.Children.Clear();
                    main.MainContent.Children.Add(new Register_User());
                }
            }    
        }

        private void Button_Click_B(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                CorrectAnswer();
            }
        }

        private void Button_Click_C(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                CorrectAnswer();
            }
        }

        private void Button_Click_D(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                CorrectAnswer();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Chúc mừng bạn đã chiến thắng với số tiền là : {prizeMilestones[currentQuestionIndex]}");
            Application.Current.Shutdown();
        }
    }
}
