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
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
namespace DataSystem.Views
{
    /// <summary>
    /// MonitorView.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorView : Window
    {
        public MonitorView()
        {
            InitializeComponent();
            initPort();
        }

        private SerialPort serial = new SerialPort();

        private void initPort()
        {
            String[] portName = SerialPort.GetPortNames();
            foreach (String s in portName)
            {
                comboBox1.Items.Add(s);
            }
            comboBox1.SelectedItem = portName[0];

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

            if (button2.Content.Equals("打开串口"))
            {
                button2.Content = "关闭串口";
                String selectPort = comboBox1.SelectedItem.ToString();
                try
                {
                    serial.PortName = selectPort;
                    serial.DataBits = 8;
                    serial.BaudRate = 115200;
                    serial.Parity = Parity.None;
                    serial.StopBits = StopBits.One;
                    serial.DataReceived += Serial_DataReceived;
                    serial.DtrEnable = true;
                    serial.RtsEnable = true;
                    serial.Open();
                    if (serial.IsOpen)
                    {
                        richTextBox.AppendText("开启串口成功\n");
                        richTextBox.ScrollToEnd();
                    }
                    else
                    {
                        richTextBox.AppendText("开启失败\n");
                        richTextBox.ScrollToEnd();
                    }
                }
                catch (Exception ex)
                {

                }

            }
            else
            {
                if (serial.IsOpen)
                {
                    serial.Close();
                    richTextBox.AppendText("串口已关闭\n");
                    richTextBox.ScrollToEnd();
                }

                button2.Content = "打开串口";
            }

        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string res = serial.ReadExisting();
            res = res.Trim();
            richTextBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                richTextBox.AppendText(res + "\n");
                richTextBox.ScrollToEnd();

            }));

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Write("6" + loops.Text);
                Thread.Sleep(2);
                //serial.Write(loops.Text);
            }
            else
            {
                MessageBox.Show("串口没打开");
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Write("7" + loops.Text);
                Thread.Sleep(2);
                //serial.Write(loops.Text);
            }
            else
            {
                MessageBox.Show("串口没打开");
            }

        }
        }
    }
