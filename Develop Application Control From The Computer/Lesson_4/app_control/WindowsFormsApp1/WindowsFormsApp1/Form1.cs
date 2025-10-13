using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string data_rev = null;
        int angle = 0;
        Bitmap image0 = Properties.Resources._0;
        Bitmap image45 = Properties.Resources._45;
        Bitmap image90 = Properties.Resources._90;
        Bitmap image135 = Properties.Resources._135;
        Bitmap image180 = Properties.Resources._180;
        Bitmap image225 = Properties.Resources._225;
        Bitmap image270 = Properties.Resources._270;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                string data_sent = temp_tb.Text.ToString();
                serialPort1.Write(data_sent);
                serialPort1.Write("\n");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort1.PortName = "COM5";
            serialPort1.BaudRate = 9600; 
            serialPort1.Open();
            timer1.Enabled = true;

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort1.Close();
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            data_rev = serialPort1.ReadLine();
            data_rev = data_rev.Trim();

            if (data_rev.Length > 1)
            {
                char line = data_rev[0];
                string value = data_rev.Substring(1);

                this.BeginInvoke(new Action(() =>
                {

                }));

            }
        }
        private void UpdateImageByAngle(int angle)
        {
            switch (angle)
            {
                case 0:
                    pictureBox1.Image = image0;
                    break;

                case 45:
                    pictureBox1.Image = image45;
                    break;

                case 90:
                    pictureBox1.Image = image90;
                    break;

                case 135:
                    pictureBox1.Image = image135;
                    break;

                case 180:
                    pictureBox1.Image = image180;
                    break;

                case 225:
                    pictureBox1.Image = image225;
                    break;

                case 270:
                    pictureBox1.Image = image270;
                    break;

                default:
                    // nếu không khớp góc nào thì giữ nguyên hoặc gán ảnh mặc định
                    pictureBox1.Image = null;
                    break;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                string data_sent = trackBar1.Value.ToString();
                serialPort1.Write(data_sent);
                serialPort1.Write("\n");
            }
            timer1.Interval = 255 - trackBar1.Value;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            angle += 45;
            UpdateImageByAngle(angle);
            if(angle >= 270)
            {
                angle = 0; 
            }
        }
    }
}
