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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace code_led_on_offd
{
    public partial class Form1 : Form
    {
        string data_rev = null;
        Bitmap bmpLedOn = Properties.Resources.image_led_on;
        Bitmap bmpLedOff = Properties.Resources.image_led_off;
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void on_btn_Click(object sender, EventArgs e)
        {
            image_led.Image = bmpLedOn;
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Write("on\n");
            }
        }

        private void off_btn_Click(object sender, EventArgs e)
        {
            image_led.Image = bmpLedOff;
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Write("off\n");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] port = SerialPort.GetPortNames();
            comboBox1.DataSource = port;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == false)
            {
                serialPort1.PortName = comboBox1.SelectedItem.ToString();
                serialPort1.BaudRate = 9600; 
                serialPort1.Open();
                button1.Text = "Disconnect";
            }
            else
            {
                serialPort1.Close();
                button1.Text = "Connect";
            }

        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            data_rev = serialPort1.ReadLine();
            string data_tpm = data_rev.ToString() + '\n';
            data_tb.BeginInvoke(new Action(() => { data_tb.Text = data_tpm; }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Close();
            }
            this.Close();
        }
    }
}
