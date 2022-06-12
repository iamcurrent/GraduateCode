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
    /// ChromeDevice.xaml 的交互逻辑
    /// </summary>
    public partial class ChromeDevice : Window
    {

        private SerialPort serial = new SerialPort();
        private double [] TwoMi = {3,4,8,16,32,64 };
        public ChromeDevice()
        {
            InitializeComponent();
            InitPort();
            command_box.AppendText("CURR:MAX:DC ?设置最大电流\n");
            command_box.AppendText("RES:DC ?设置电阻值\n");
            command_box.AppendText("MODE ?设置DC下的模式\n");
        }

        private void InitPort()
        {
            
            String[] portName = SerialPort.GetPortNames();
            foreach (String s in portName)
            {
                port_list.Items.Add(s);
            }
            port_list.SelectedItem = portName[0];
        }

        private void re_btn_Click(object sender, RoutedEventArgs e)
        {
            InitPort();
        }

        private void oc_btn_Click(object sender, RoutedEventArgs e)
        {
            if (oc_btn.Content.Equals("打开串口"))
            {
                String selectPort = port_list.SelectedItem.ToString();
                try
                {
                    serial.PortName = selectPort;
                    serial.DataBits = 8;
                    serial.BaudRate = 57600;
                    serial.Parity = Parity.None;
                    serial.StopBits = StopBits.One;
                    //serial.DataReceived += Serial_DataReceived;
                    serial.ErrorReceived += Serial_ErrorReceived;

                    //serial.DtrEnable = true;
                    //serial.RtsEnable = true;

                    serial.Open();

                    if (serial.IsOpen)
                    {
                        receive_box.AppendText("开启串口成功\n");
                        oc_btn.Content = "关闭串口";
                    }
                }
                catch (Exception)
                {
                    receive_box.AppendText("开启串口失败\n");
                }
            }
            else
            {
                if (serial.IsOpen)
                {
                    serial.Close();
                    receive_box.AppendText("开启关闭成功\n");
                }

                oc_btn.Content = "打开串口";
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            String msg = serial.ReadExisting();
            receive_box.Dispatcher.BeginInvoke(new Action(() =>
            {
                receive_box.AppendText(msg);
            }));
        }

        private void Serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            receive_box.AppendText(serial.PortName + "串口出现异常!\n");
        }

        private void load_btn_Click(object sender, RoutedEventArgs e)
        {
            if (load_btn.Content.Equals("加载负载"))
            {
                if (serial != null && serial.IsOpen)
                {
                    try
                    {
                        serial.Write("LOAD ON\n");
                        receive_box.AppendText("加载负载成功!\n");
                        load_btn.Content = "关闭负载";
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                    }

                }
            }
            else
            {
                if (serial != null && serial.IsOpen)
                {
                    try
                    {
                        serial.Write("LOAD OFF\n");
                        receive_box.AppendText("关闭负载成功!\n");
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                    }

                }

                load_btn.Content = "加载负载";
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (serial != null && serial.IsOpen)
                {
                    serial.Write("SYSTem:SETup:MODE AC\n");

                }

            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (serial != null && serial.IsOpen)
                {
                    serial.Write("SYSTem:SETup:MODE DC\n");

                }

            }
            catch (Exception) { }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (serial != null && serial.IsOpen)
            {
                try
                {
                    serial.Write("SYSTem:SETup:MODE?\nMODE?\n");
                    String s = serial.ReadLine();
                    receive_box.AppendText(s + "\n");
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }

            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            getPanel();
        }

        private void getPanel()
        {
            if (serial != null && serial.IsOpen)
            {
                try
                {
                    //电压
                    serial.Write("MEAS:POW?\nMEAS:CURR?\n");
                    Thread.Sleep(10);
                    String s1 = serial.ReadLine();

                    serial.Write("RES:DC?\nMEAS:VOLT?\n");
                    Thread.Sleep(10);
                    String s2 = serial.ReadLine();


                    vol.Content = s2.Split(';')[1];
                    curr.Content = s1.Split(';')[1];
                    power.Content = s1.Split(';')[0];
                    res.Content = s2.Split(';')[0];

                }
                catch (Exception) { }


            }
        }


        private void button8_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(send_box.Document.ContentStart, send_box.Document.ContentEnd);
            String data = textRange.Text.ToString();
            String [] strs = data.Split(' ');
            res_val.Text = strs[strs.Length - 1];
            data = data + "\n";
            Console.WriteLine(data);
            if (serial!=null && serial.IsOpen)
            {
                serial.Write(data);
                
                Thread.Sleep(100);
                getPanel();
            }
        }





        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double vola = Convert.ToDouble(vol.Content);

                double p = Convert.ToDouble(power.Content);

                double ris = Convert.ToDouble(res.Content);

                double value  = Convert.ToDouble(res_val.Text);

                double deltaP = -4.2;


                double deltaR = (-deltaP * ris * ris) / (deltaP * ris + vola * vola);
                
                value += Math.Round(deltaR,2);
                res_val.Text = value.ToString();



                string sendData = "RES:DC " + value.ToString() + "\n";
                serial.Write(sendData);
               
               


            }
            catch(Exception ex)
            {

            }
        }

        private void sub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double vola = Convert.ToDouble(vol.Content);

                double p = Convert.ToDouble(power.Content);

                double ris = Convert.ToDouble(res.Content);

                double value = Convert.ToDouble(res_val.Text);

                double deltaP = 4.2;


                double deltaR = (-deltaP * ris * ris) / (deltaP * ris + vola * vola);

                value += Math.Round(deltaR,2);
                res_val.Text = value.ToString();
                string sendData = "RES:DC " + value.ToString() + "\n";
                serial.Write(sendData);
               

            }
            catch (Exception ex)
            {

            }
        }
    }
}
