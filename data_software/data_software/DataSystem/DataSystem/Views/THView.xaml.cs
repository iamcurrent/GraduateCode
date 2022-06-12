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
using System.IO.Ports;
using System.Threading;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Npgsql;
using System.Net;

namespace DataSystem.Views
{
    /// <summary>
    /// THView.xaml 的交互逻辑
    /// </summary>
    public partial class THView : UserControl
    {

        private SerialPort serial = new SerialPort();
        //private List<Double> temp = new List<double>();
        private string selectPortName = "";
        private System.Threading.Timer timer1;
        ConnectionFactory connectionFactory = null;
        IConnection connection = null;
        IModel temperature = null;
        IModel em_queue = null;
        List<List<double>> e_data = new List<List<double>>();   
        List<List<double>> m_data = new List<List<double>>();
        static string ConStr = @"PORT=5432;DATABASE=WestData;HOST=192.168.1.87;PASSWORD=root;USER ID=postgres"; //数据库服务地址
        private NpgsqlConnection SqlConn = new NpgsqlConnection(ConStr);
        double [] mfi = {0.3907,5.61 };
        double[] efi = {5.734,361.6 };
        private WebClient webclient = new WebClient();
        private string ftp_file_path = "D:\\wave_data_ftp\\";
        private List<List<double>> temperatureArr = new List<List<double>>();

        public THView()
        {
            InitializeComponent();
            initPort();
            InitRabbitMq();
            thPlot.plt.XLabel("t", fontSize: 30);
            thPlot.plt.YLabel("T(℃)", fontSize: 30);
            thPlot.plt.Ticks(fontSize: 20);

            ePlot.plt.XLabel("t",fontSize: 30);
            ePlot.plt.YLabel("A(V/m)",fontSize: 30);
            ePlot.plt.Ticks(fontSize:20);

            mPlot.plt.XLabel("t", fontSize: 30);
            mPlot.plt.YLabel("A(μT)", fontSize: 30);
            mPlot.plt.Ticks(fontSize: 20);
            thPlot.plt.Title("电源温度", fontSize: 30);
            ePlot.plt.Title("电场强度", fontSize: 30);
            mPlot.plt.Title("磁场强度", fontSize: 30);
            timer1 = new Timer(new TimerCallback(timeOver), null, 10000, 10000);
            for(int i = 0; i < 16; i++)
            {
                temperatureArr.Add(new List<double>());
                e_data.Add(new List<double>());
                m_data.Add(new List<double>());
            }
        }


        private void InitRabbitMq()
        {
            try
            {
                connectionFactory = new ConnectionFactory();
                connectionFactory.HostName = "192.168.1.99";
                connectionFactory.UserName = "guest";
                connectionFactory.Password = "guest";
                connectionFactory.VirtualHost = "/";//默认情况可省略此行
                connectionFactory.Port = 5672;

                connection = connectionFactory.CreateConnection();
                temperature = connection.CreateModel();
                em_queue = connection.CreateModel();
                temperature.QueueDeclare("", false, false, true, null);
                em_queue.QueueDeclare("", false, false, true, null);
            }catch(Exception ex)
            {

            }

        }

        private void timeOver(object a)
        {
            String[] portName = SerialPort.GetPortNames();

            comboBox1.Dispatcher.BeginInvoke(new Action(() =>
            {
                comboBox1.Items.Clear();
                foreach (String s in portName)
                {
                    comboBox1.Items.Add(s);
                }
                if (!portName.Contains(selectPortName))
                {
                    comboBox1.SelectedItem = portName[0];
                }
                else
                {
                    comboBox1.SelectedItem = selectPortName;
                }
                
            }));

            
        }

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
                    selectPortName = selectPort;
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
            if (!res.Equals(""))
            {
                
                String[] strs = res.Split(',');
                Random random = new Random();
                
                List<String> tmp = new List<string>();
                List<String> efi_data = new List<string>();
                List<String> mfi_data = new List<string>();
                for (int i =0;i<strs.Length;i++)
                {
                    double p = random.NextDouble() * 20;
                    String[] strs1 = strs[i].Split('=');
                    String ts = "em" + i.ToString() + ":{";
                    try
                    {
                        double val = Convert.ToDouble(strs1[strs1.Length - 1]);
                        
                        //temperature.BasicPublish("", "queue_temp", null, System.Text.Encoding.Default.GetBytes(strs1[strs1.Length - 1]));
                        temperatureArr.ElementAt(i).Add(val);
                        double e_val = p * efi[0] + efi[1];
                        double m_val = p * mfi[0] + mfi[1];
                        e_data.ElementAt(i).Add(e_val);
                        m_data.ElementAt(i).Add(m_val);

                        efi_data.Add(e_val.ToString());
                        mfi_data.Add(m_val.ToString());
                        tmp.Add(val.ToString());
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }

                String em_str = String.Join(",", efi_data.ToArray()) + ":" + String.Join(",",mfi_data.ToArray());

               /* try
                {
                    temperature.BasicPublish("", "Queue_temperature", null, System.Text.Encoding.Default.GetBytes(String.Join(",", tmp.ToArray())));
                    em_queue.BasicPublish("", "Queue_em", null, System.Text.Encoding.Default.GetBytes(em_str));
                }catch(Exception ex)
                {

                }*/

                thPlot.Dispatcher.BeginInvoke(new Action(() =>
                {
                    thPlot.plt.Clear();
                    ePlot.plt.Clear();
                    mPlot.plt.Clear();
                    for (int i = 0; i < temperatureArr.Count; i++)
                    {
                        if (temperatureArr.ElementAt(i).Count != 0)
                        {
                            thPlot.plt.PlotSignal(temperatureArr.ElementAt(i).ToArray());
                        }

                        if (e_data.ElementAt(i).Count > 0)
                        {
                            ePlot.plt.PlotSignal(e_data.ElementAt(i).ToArray());
                        }

                        if (m_data.ElementAt(i).Count > 0)
                        {
                            mPlot.plt.PlotSignal(m_data.ElementAt(i).ToArray());
                        }
                    }

                    //thPlot.plt.Legend();
                    //ePlot.plt.Legend();
                    //mPlot.plt.Legend();
                    thPlot.Render();
                    ePlot.Render();
                    mPlot.Render();
                }));


            }


            //String[] strs = res.Split('=');
            /*if (!res.Equals(""))
            {
                try
                {
                    double val = Convert.ToDouble(strs[strs.Length - 1]);
                    temperature.BasicPublish("", "queue_temp", null, System.Text.Encoding.Default.GetBytes(strs[strs.Length - 1]));
                    temp.Add(val);
                    thPlot.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        thPlot.plt.Clear();
                        thPlot.plt.PlotSignal(temp.ToArray());
                        thPlot.plt.Legend();
                        thPlot.Render();
                    }));
                   

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }*/

        }

        private void clear_btn_Click(object sender, RoutedEventArgs e)
        {
            Parallel.For(0, 16, (i)=>{
                temperatureArr.ElementAt(i).Clear();
            });
        }

        private void saveTH_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter sw = new StreamWriter("./temperature.csv"))
            {
                
               
                for(int i = 0; i < temperatureArr.Count; i++)
                {
                    if (temperatureArr.Count > 0)
                    {
                        String s = String.Join(",",temperatureArr.ElementAt(i).ToArray());
                        sw.WriteLine(s);
                        //sw.Write("\t\n");
                        sw.Flush();
                    }
                }
                
              

            }
        }

        private void loadSql_Click(object sender, RoutedEventArgs e)
        {
           
            for (int i = 0; i < 2; i++)
            {

                String temp_data = String.Join(",",temperatureArr.ElementAt(i).ToArray());
                String em_str = "";
                if (e_data.Count > i)
                {
                    String e_str = String.Join(",", e_data.ElementAt(i).ToArray());
                    em_str += e_str;
                }

                if (m_data.Count > i)
                {
                    String m_str = String.Join(",", m_data.ElementAt(i).ToArray());
                    em_str += (":" + m_str);
                }
                String load_t = DateTime.Now.ToString("MM-dd-H-mm-ss");
                String temperature_name = "temperature_" + load_t+".txt";
                String em_name = "em_" + load_t+".txt";
                String time = DateTime.Now.ToString("yyyy-MM-dd:H:m:s");
                try
                {
                    SqlConn.Open();
                    Uri uri = new Uri("ftp://192.168.1.87/" + temperature_name);
                    byte[] temp_byte = System.Text.Encoding.Default.GetBytes(temp_data);
                    webclient.UploadDataAsync(uri, "STOR", temp_byte);
                    String file_path_name = ftp_file_path + temperature_name;
                    String dtype = "temperature";
                    String upload_data_sql = "insert into data_table values('" + file_path_name + "','" + time + "','" + dtype + "')";
                    using (NpgsqlCommand command = new NpgsqlCommand(upload_data_sql, SqlConn))
                    {
                        int r = command.ExecuteNonQuery();
                        if (r == 0)
                        {
                            MessageBox.Show("上传成功!");
                        }
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    try { SqlConn.Close(); }
                        catch (Exception eex)
                        {
                            Console.WriteLine("Close");
                        }
                }

                Thread.Sleep(200);

                try
                {
                    SqlConn.Open();
                    if (!em_str.Equals(""))
                    {
                        Uri uri = new Uri("ftp://192.168.1.87/" + em_name);
                        byte[] em_byte = System.Text.Encoding.Default.GetBytes(em_str);
                        webclient.UploadDataAsync(uri, "STOR", em_byte);
                        String file_path_name = ftp_file_path + em_name;
                        String dtype = "em";
                        String upload_data_sql = "insert into data_table values('" + file_path_name + "','" + time + "','" + dtype + "')";
                        using (NpgsqlCommand command = new NpgsqlCommand(upload_data_sql, SqlConn))
                        {
                            int r = command.ExecuteNonQuery();
                            if (r == 0)
                            {
                                MessageBox.Show("上传成功!");
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    try { SqlConn.Close(); }
                    catch (Exception eex)
                    {
                        Console.WriteLine("Close");
                    }
                }
                Thread.Sleep(200);
            }
        }
    }
}



