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
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string data_to_send = "";
            if (textBox1.Text != "")
            {
                data_to_send = "1"+ textBox1.Text + '\n';
            }
            if (textBox2.Text != "")
            {
                data_to_send += "2"+ textBox2.Text + '\n';
            }
            serialPort1.Write(data_to_send);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort1.PortName = "COM5";
            serialPort1.BaudRate = 9600; 
            serialPort1.Open();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort1.Close();
        }
    }
}
