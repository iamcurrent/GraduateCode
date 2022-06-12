using DataSystem.Classes;
using Npgsql;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebSocketSharp;
using System.Collections.Concurrent;

namespace DataSystem.Views
{
    /// <summary>
    /// Dev2991View.xaml 的交互逻辑
    /// </summary>
    public partial class Dev2991View : UserControl
    {

        [DllImport("Ws2_32.dll")]
        public static extern int inet_addr(string ipaddr);
        private const int MaxTryCount = 5;

        public delegate void MsgUpdateDelegate(String msg);

        private static BackgroundWorker backgroundWorker_2991;
        private static BackgroundWorker backGroundWorkerDisplay;

        private ulong channelSelected = 0;
        private bool isSelectiongChanging = false;
        //MLApp.MLApp matlab = null;
        private List<WpfPlot> wpfPlots = new List<WpfPlot>();


        List<List<Double>> data = new List<List<Double>>(); //通道数据
        List<double> x = new List<double>(); //对应的X轴坐标

        NET2991A nET2991A = new NET2991A(); //2991设备
        private bool is2991enabled = true;
        private int continueCount = 0;
        volatile bool isContinue = false;
        volatile bool isChangeData = false;
        List<int> channelNum = new List<int>();
        int[] lastDataCount = new int[16];


        List<double> xdisplay = new List<double>();
        List<double> ydisplay = new List<double>();


        private BigPictureView bigPictureView = new BigPictureView(); //详细图
        private DwtConvertView dwtConvertView = new DwtConvertView(); //小波分析图
        private FreConvertView freConvertView = new FreConvertView(); //频谱分析图
        private List<int> loadChannel = new List<int>();
        private int selectChannel = -1;

        //数据库服务器
        static string ConStr = @"PORT=5432;DATABASE=WestData;HOST=192.168.1.87;PASSWORD=root;USER ID=postgres"; //数据库服务地址
        private NpgsqlConnection SqlConn = new NpgsqlConnection(ConStr);


        private WebClient webclient = new WebClient();
        private string ftp_file_path = "D:\\wave_data_ftp\\";


        private List<BlockingCollection<List<double>>> rabbitmqData = new List<BlockingCollection<List<double>>>();

        ConnectionFactory connectionFactory = null;
        IConnection connection = null;
        IModel channel = null;
        List<IModel> channels = new List<IModel>();
        
        WebSocket webSocket = null;

        private bool sendFlag = true;

        Thread rabbitmqThread = null;

        Thread loadThread = null;


        private bool loadFlag = true;
        private void InitWebSocket()
        {
            webSocket = new WebSocket("ws://192.168.1.37:50000");
            webSocket.Connect();
            webSocket.OnMessage += WebSocket_OnMessage;
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            String msg = e.Data;
            if (msg.Contains("upload"))
            {
                String [] strs1 = msg.Split(':')[1].Split(',');
                String type = strs1[0];
                int channel = Convert.ToInt32(strs1[1]);
                if (type.Equals("r1"))
                {
                    loadChannel.Add(channel);
                }
                else
                {
                    loadChannel.Remove(channel);
                }
            }
            
        }

        private void uploadThreadFunc()
        {
            while (loadFlag)
            {
               
                    foreach (int c in loadChannel)
                    {
                        try
                        {
                            SqlConn.Open();
                            String str = String.Join(",", data.ElementAt(c));

                            String name = "ripple_" + DateTime.Now.ToString("MM-dd-H-mm-ss") + c.ToString();
                            byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);

                            byte[] res = Compress(byteArray);

                            Uri uri = new Uri("ftp://192.168.1.87/" + name + ".txt");
                            webclient.UploadDataAsync(uri, "STOR", res);

                            String upload_sql = "insert into wave_info values ('" + name + "','" + ftp_file_path + name + ".txt" + "')";
                            using (NpgsqlCommand command = new NpgsqlCommand(upload_sql, SqlConn))
                            {
                                int r = command.ExecuteNonQuery();

                            }
                        }catch (Exception ex)
                        {

                        }
                        finally
                        {
                            try
                            {
                                SqlConn.Close();
                            }catch(Exception eex)
                            {

                            }
                        }
                        
                    }
                    
                

              
            }
        }


        private void SendRabbitMqData()
        {
            while (sendFlag)
            {
                for(int i =0; i<channelNum.Count; i++)
                {
                    try
                    {
                        List<double> ds = null;
                        rabbitmqData[channelNum.ElementAt(i)].TryTake(out ds);
                        if (ds != null)
                        {
                            String str = String.Join(",", ds.ToArray());
                            str = channelNum.ElementAt(i).ToString() + ":" + str;
                            byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
                            byte[] bs = Compress(byteArray);
                            channels.ElementAt(channelNum.ElementAt(i)).BasicPublish("", "Queue_Ripple" + channelNum.ElementAt(i), null, bs);
                        }
                    }catch(Exception ex)
                    {

                    }
                }

            

            }
            
        }



        public void CloseWebSocket()
        {
            if(webSocket != null)
            {
                webSocket.Close();
            }
        }

        public Dev2991View()
        {
            InitializeComponent();
            
            //启动matlab程序
            /* Type matlabAppType = System.Type.GetTypeFromProgID("Matlab.Application");
             matlab = System.Activator.CreateInstance(matlabAppType) as MLApp.MLApp;
             string path_project = System.IO.Directory.GetCurrentDirectory();
             string path_matlabwork = "cd('" + path_project + "')";
             matlab.Execute(path_matlabwork);*/
            wpfPlots.Add(ScottPlot0);
            wpfPlots.Add(ScottPlot1);
            wpfPlots.Add(ScottPlot2);
            wpfPlots.Add(ScottPlot3);
            wpfPlots.Add(ScottPlot4);
            wpfPlots.Add(ScottPlot5);
            wpfPlots.Add(ScottPlot6);
            wpfPlots.Add(ScottPlot7);
            wpfPlots.Add(ScottPlot8);
            wpfPlots.Add(ScottPlot9);
            wpfPlots.Add(ScottPlot10);
            wpfPlots.Add(ScottPlot11);
            wpfPlots.Add(ScottPlot12);
            wpfPlots.Add(ScottPlot13);
            wpfPlots.Add(ScottPlot14);
            wpfPlots.Add(ScottPlot15);

            foreach (WpfPlot wpf in wpfPlots)
            {
                
                MenuItem item = new MenuItem();
                MenuItem item1 = new MenuItem();
                MenuItem item2 = new MenuItem();
                item.Header = "详细图";
                item1.Header = "频谱分析图";
                item2.Header = "小波分解图";
                item.Click += Item_Click;
                item1.Click += Item1_Click;
                item2.Click += Item2_Click;
                Console.WriteLine(wpf.ContextMenu);
                wpf.ContextMenu.Items.Add(item);
                wpf.ContextMenu.Items.Add(item1);
                wpf.ContextMenu.Items.Add(item2);
                wpf.MouseRightButtonDown += Wpf_MouseRightButtonDown;
                wpf.plt.Ticks(fontSize: 20);
            }


            for (int i = 0; i < 16; i++)
            {
                //初始化通道和对应的数据
                ((CheckBox)channelControl.Items[i]).Checked += Tmp_Checked;
                ((CheckBox)channelControl.Items[i]).Unchecked += Tmp_Checked;
                List<Double> y = new List<Double>();
                BlockingCollection<List<double>> queue = new BlockingCollection<List<double>>(1);
                rabbitmqData.Add(queue);
                data.Add(y);
            }


            this.upload_sql.Click += Upload_sql_Click;
            webclient.UploadDataCompleted += Webclient_UploadDataCompleted;
            InitRabbitMq();
            InitWebSocket();
            rabbitmqThread = new Thread(new ThreadStart(SendRabbitMqData));
            rabbitmqThread.Start();
            loadThread = new Thread(new ThreadStart(uploadThreadFunc));
            loadThread.Start();
        
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

                Parallel.For(0, 18, i =>
                {
                    IModel channel = connection.CreateModel();
                    IDictionary<String, Object> args = new Dictionary<String, Object>();
                    args.Add("x-max-length",5);
                    if (i < 16)
                    {
                        channel.QueueDeclare("Queue_Ripple" + i, false, false, true, args);
                    }else if (i == 16)
                    {
                        channel.QueueDeclare("Queue_temperature", false, false, true, args);
                    }
                    else
                    {
                        channel.QueueDeclare("Queue_em", false,false, true, args);
                    }
                        
                    channels.Add(channel);

                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
            }
        }
        private void Webclient_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            Console.WriteLine("Over!!");
        }

        static byte[] Compress(byte[] rawData)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.GZipStream compressedzipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }


        public static byte[] Decompress(byte[] zippedData)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(zippedData);
            System.IO.Compression.GZipStream compressedzipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress);
            System.IO.MemoryStream outBuffer = new System.IO.MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }

        private void Upload_sql_Click(object sender, RoutedEventArgs e)
        {
            SqlConn.Open();
            try
            {
                
                for (int i = 0; i < channelNum.Count; i++)
                {
                    
                    String str = String.Join(",", data.ElementAt(channelNum.ElementAt(i)));

                    String name = "ripple_"+DateTime.Now.ToString("MM-dd-H-mm-ss");
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);

                    byte[] res = Compress(byteArray);

                    Uri uri = new Uri("ftp://192.168.1.87/" + name + ".txt");
                    webclient.UploadDataAsync(uri, "STOR", res);
                    String file_path_name = ftp_file_path + name + ".txt";
                    String time = DateTime.Now.ToString("yyyy-MM-dd:H:m:s");
                    String dtype = "ripple";
                    String upload_data_sql = "insert into data_table values('"+file_path_name+"','"+time+"','"+dtype+"')";
                    //String upload_sql = "insert into wave_info values ('" + name + "','" + ftp_file_path + name + ".txt" + "')";
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

        }

        private void Wpf_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            clearQueue();
            String controlName = ((WpfPlot)sender).Name;
            selectChannel = Convert.ToInt32(controlName.Substring(9));
        }

        private void OriSignalPlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            changeSelect();
            oriAllPlot.Visibility = Visibility.Visible;
        }

        private bool showBigPictureView = false;
        private bool showFreView = false;
        private bool showDwtView = false;


        public void changeSelect()
        {
            this.freConvertView.setFlag();
            this.dwtConvertView.setFlag();
            this.showBigPictureView = false;
            this.showDwtView = false;
            this.showFreView = false;
        }

        //小波分析
        private void Item2_Click(object sender, RoutedEventArgs e)
        {
            oriAllPlot.Visibility = Visibility.Hidden;
            funcContentControl.Content = dwtConvertView;

            changeSelect();
            showDwtView = true;
            dwtConvertView.startDwtThread();
        }

        private void clearQueue()
        {
            FreDataObject tmp;
            List<double> ts;
            for (int i = 0; i < UserDef.syncQueFre.Count; i++)
            {
                UserDef.syncQueFre.TryTake(out tmp);
            }

            for (int i = 0; i < UserDef.syncQueDwt.Count; i++)
            {
                UserDef.syncQueDwt.TryTake(out ts);
            }
        }

        //频谱分析
        private void Item1_Click(object sender, RoutedEventArgs e)
        {
            oriAllPlot.Visibility = Visibility.Hidden;
            funcContentControl.Content = freConvertView;

            changeSelect();
            showFreView = true;
            freConvertView.startFreThread();
        }
        //详细大图
        private void Item_Click(object sender, RoutedEventArgs e)
        {
            oriAllPlot.Visibility = Visibility.Hidden;
            funcContentControl.Content = bigPictureView;
            changeSelect();
            showBigPictureView = true;

        }


        //日志打印
        private void addMsg(String s)
        {

            textbox.Dispatcher.Invoke(new Action(delegate
            {
                String st = textbox.Text + s + '\r';

                textbox.Text = st.Substring(st.Length > 20000 ? st.Length - 20000 : 0, st.Length > 20000 ? 20000 : st.Length);
                textbox.ScrollToEnd();
            }));

        }

        //通道选择
        private void Tmp_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox tmp = (CheckBox)sender;
            addMsg(tmp.Tag + (tmp.IsChecked == true ? "checked" : "unchecked"));
            isSelectiongChanging = true;
            if (tmp.IsChecked == true)
            {
                channelSelected |= ((ulong)0x01 << Convert.ToInt32(tmp.Tag));
            }
            else
            {
                channelSelected &= (~((ulong)0x01 << Convert.ToInt32(tmp.Tag)));
            }
        }


        private double temp_value = 0;
        bool needPause = false;

        //读取采集卡数据
        private void backgroundWorker_2991_doWork(object sender, DoWorkEventArgs e)
        {

            try
            {


                while (!NET2991A.InitDevice("192.168.0.145", "9000", "8000", new MsgUpdateDelegate(addMsg))) //初始化设备
                {
                    addMsg("初始化2991设备失败!");
                    if (!is2991enabled)
                    {
                        return;
                    }
                    Thread.Sleep(3000);
                }

                while (!NET2991A.start()) //启动设备
                {
                    if (!is2991enabled)
                    {
                        return;
                    }
                    addMsg("启动2991设备失败!");
                    Thread.Sleep(3000);
                }
                addMsg("启动2991设备成功!");
                ulong currentCount = 0;
                while (true)
                {


                    int step = 1;
                    if (!is2991enabled)
                    {
                        return;
                    }
                    if (needPause)
                    {
                        continue;
                    }

                    if (currentCount < NET2991A.currentFrameCount)
                    {
                        Console.WriteLine("working");
                        if (isContinue)
                        {
                            continueCount++;
                            if (continueCount > 1000)
                            {
                                needPause = true;
                                isContinue = false;
                                continueCount = 0;
                            }
                        }
                        else
                        {
                            continueCount = 0;
                        }
                        if (needPause)
                        {
                            continue;
                        }
                        isChangeData = true;
                        //清除当前数据，显示下一帧数据
                        if (!isContinue || continueCount < 0)
                        {
                            Console.WriteLine("clear");
                            x.Clear();
                            Parallel.For(0, 16, (i) =>
                            {
                                data.ElementAt(i).Clear();
                                lastDataCount[i] = 0;
                            });
                        }

                        int displayFrame = (int)(NET2991A.currentFrameCount + 1) % 2;
                        int count = 0;
                        if (isSelectiongChanging)
                        {
                            channelNum.Clear();
                            for (var i = 0; i < 16; i++)
                            {
                                if (((channelSelected >> i) & (ulong)0x01) > 0)
                                {
                                    channelNum.Add(i);
                                    lastDataCount[i] = data.ElementAt(i).Count;

                                }


                            }

                            String selectChannels = "";
                            for(int k = 0; k < channelNum.Count; k++)
                            {
                                if (k < channelNum.Count - 1)
                                {
                                    selectChannels+=(channelNum[k].ToString()+",");
                                }
                                else
                                {
                                    selectChannels+=channelNum[k].ToString();
                                }
                            }

                            webSocket.Send("channels:"+selectChannels);
                            isSelectiongChanging = false;
                        }
                        for (var i = 0; i < 16; i++)
                        {
                            lastDataCount[i] = data.ElementAt(i).Count;
                        }

                        int yindex = 0;

                        double lastTimeEnd = 0;
                        if (x.Count > 0)
                        {
                            lastTimeEnd = x.ElementAt(x.Count - 1);
                        }

                        //一直读取UserDef.Frequency个数据点，
                        //i < readStep && 
                        for (int i = 0; count < UserDef.Frequency; i = i + step)
                        {
                            x.Add(+((i + 1) * 1.0 / UserDef.Frequency));


                            //Console.WriteLine(UserDef.NowRes);
                            yindex = 0;
                            //Parallel.For(0, channelNum.Count, (j) =>
                            foreach (int n in channelNum)
                            {
                                temp_value = ((NET2991A.buffer[displayFrame, n, i] - 32768) * NET2991A.fPerLsb / 1000);
                                data.ElementAt(n).Add(temp_value);
                                yindex++;
                            }



                            count++;
                        }

                        wpfPlots.ElementAt(0).Dispatcher.BeginInvoke(new Action(() =>
                        {
                            playSelectLines();
                        }));
                        currentCount = NET2991A.currentFrameCount;
                    }

                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                is2991enabled = false;
                addMsg("failed!" + ex.Message);
            }
        }

        uint testStep = 0; //测试标记，0表示一趟，1表示暂停，2表示返回趟，即奇数不处理，偶数表示 数据
        int stepInterval = 1;
        double startTime = 0, endTime = 0;


        private String convertString(Double [] data)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (i <data.Length - 1)
                {
                    builder.Append(data[i].ToString() + ",");
                }
                else
                {
                    builder.Append(data[i].ToString());
                }
            }


            return builder.ToString();
        }
        //显示数据
        private void playSelectLines()
        {

            if (xdisplay.Count <= 0 && x.Count <= 0)
            {
                return;
            }
            if (isSelectiongChanging)
            {

                return;
            }
            int halfWaveLength = UserDef.Frequency / UserDef.signalFrequency / 2;
            //Console.WriteLine("display lines");
            if (testStep % 2 == 1) //奇数不处理
            {
                //清除原来的数据
                return;
            }

            Parallel.For(0, 16, (i) =>
            {
                wpfPlots.ElementAt(i).plt.Clear();
            });

            //heatPlotWindow.plt.Clear();
            xdisplay.Clear();
            List<int> pointIndex = new List<int>();
            int stepCount = 0;
            if (stepInterval <= 0)
            {
                stepInterval = 1;
            }
            if (startTime != 0 && endTime > startTime)
            {
                for (int index = 0; index < x.Count; index++)
                {
                    double v = x.ElementAt(index);
                    if (v > startTime && v < endTime)
                    {
                        if (stepCount % stepInterval == 0)
                        {
                            xdisplay.Add(v);
                            pointIndex.Add(index);
                        }
                        if (stepInterval > 1)
                        {
                            stepCount++;
                        }

                    }
                }
            }

            for (int i = 0; i < channelNum.Count; i++)
            {

                List<double> y = data.ElementAt(channelNum.ElementAt(i));
                if (pointIndex.Count > 0)
                {
                    ydisplay.Clear();
                    foreach (int m in pointIndex)
                    {
                        ydisplay.Add(y.ElementAt(m));
                    }
                    if (ydisplay.Count > 0)
                    {
                        if (!showBigPictureView && !showDwtView && !showFreView)
                        {
                            wpfPlots.ElementAt(channelNum.ElementAt(i)).plt.PlotSignal(ydisplay.ToArray(), UserDef.Frequency, startTime, markerSize: 20, label: String.Format("通道 {0}", channelNum.ElementAt(i)));

                            rabbitmqData.ElementAt(channelNum.ElementAt(i)).TryAdd(new List<double>(ydisplay));                            
                        }
                        else if (showBigPictureView && channelNum.ElementAt(i) == selectChannel)
                        {

                            bigPictureView.setData(ydisplay, startTime, channelNum.ElementAt(i));

                        }
                        else if (showFreView && channelNum.ElementAt(i) == selectChannel)
                        {
                            //Console.WriteLine("Fre:"+selectChannel.ToString());
                            List<double> tmp = new List<double>(ydisplay);
                            UserDef.syncQueFre.TryAdd(new FreDataObject(tmp, selectChannel, startTime));
                        }
                        else if (showDwtView && channelNum.ElementAt(i) == selectChannel)
                        {
                            UserDef.syncQueDwt.TryAdd(ydisplay);
                            //dwtConvertView.setData(ydisplay);
                        }

                    }
                }

                else
                {
                    if (y.Count > 0)
                    {
                        ydisplay.Clear();
                        for (int n = 0; n < y.Count; n++)
                        {
                            ydisplay.Add(y.ElementAt(n));
                        }

                        if (!showBigPictureView && !showDwtView && !showFreView)
                        {
                            wpfPlots.ElementAt(channelNum.ElementAt(i)).plt.PlotSignal(ydisplay.ToArray(), UserDef.Frequency, startTime, markerSize: 20, label: String.Format("通道 {0}", channelNum.ElementAt(i)));
                            rabbitmqData.ElementAt(channelNum.ElementAt(i)).TryAdd(new List<double>(ydisplay));
                        }
                        else if (showBigPictureView && channelNum.ElementAt(i) == selectChannel)
                        {

                            bigPictureView.setData(ydisplay, startTime, channelNum.ElementAt(i));
                        }
                        else if (showFreView && channelNum.ElementAt(i) == selectChannel)
                        {

                            //Console.WriteLine("Fre:" + selectChannel.ToString());
                            List<double> tmp = new List<double>(ydisplay);
                            UserDef.syncQueFre.TryAdd(new FreDataObject(tmp, selectChannel, startTime));
                        }
                        else if (showDwtView && channelNum.ElementAt(i) == selectChannel)
                        {
                            UserDef.syncQueDwt.TryAdd(ydisplay);
                            //dwtConvertView.setData(ydisplay);
                        }
                    }
                }

            }

            for (int j = 0; j < 16; j++)
            {
                wpfPlots.ElementAt(j).plt.Legend(true,"kaishi",20);
                wpfPlots.ElementAt(j).Render();
            }
        }

        //通道全选和全不选
        private void cb_Check_All_Click(object sender, RoutedEventArgs e)
        {
            if (cb_Check_All.IsChecked == false)
            {
                cb_Check_All.IsChecked = false;
                cb_Check_All.Content = "通道全不选";
                for (var i = 0; i < channelControl.Items.Count - 1; i++)
                {
                    CheckBox tmp = channelControl.Items[i] as CheckBox;
                    tmp.IsChecked = false;
                }
                for (var i = 0; i < channelControl.Items.Count - 1; i++)
                {
                    CheckBox tmp = channelControl.Items[i] as CheckBox;
                    tmp.IsChecked = false;
                }
            }
            else
            {
                cb_Check_All.IsChecked = true;
                cb_Check_All.Content = "通道全选";
                for (var i = 0; i < channelControl.Items.Count - 1; i++)
                {
                    CheckBox tmp = channelControl.Items[i] as CheckBox;
                    tmp.IsChecked = true;
                }
                for (var i = 0; i < channelControl.Items.Count - 1; i++)
                {
                    CheckBox tmp = channelControl.Items[i] as CheckBox;
                    tmp.IsChecked = true;
                }
            }
        }


        private async void clearData()
        {
            needPause = true;

            x.Clear();
            Parallel.For(0, 16, (i) =>
            {
                List<Double> y = data.ElementAt(i);
                //List<Double> y1 = UserDef.freq.ElementAt(i);
                //y1.Clear();
                y.Clear();
                lastDataCount[i] = 0;
            });

            Parallel.For(0, 2, (i) => {
                List<Double> ls = UserDef.freq11.ElementAt(i);
                ls.Clear();

            });

            needPause = false;
        }

        private void Start2991_Click(object sender, RoutedEventArgs e)
        {
            if (start2991.Header.Equals("启动2991"))
            {
                clearData();
                is2991enabled = true;
                backgroundWorker_2991 = new BackgroundWorker();
                backgroundWorker_2991.DoWork += backgroundWorker_2991_doWork;
                backgroundWorker_2991.RunWorkerCompleted += BackgroundWorker_2991_RunWorkerCompleted;
                backgroundWorker_2991.WorkerSupportsCancellation = true;
                backgroundWorker_2991.RunWorkerAsync();
                start2991.Header = "停止2991";
            }
            else
            {
                is2991enabled = false;
                backgroundWorker_2991.CancelAsync();
                backgroundWorker_2991 = null;
                NET2991A.stopDevice();
                start2991.Header = "启动2991";
                start2991.IsEnabled = false;
            }

        }


        private void BackgroundWorker_2991_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("complietetd");
            start2991.IsEnabled = true;
            start2991.Header = is2991enabled ? "停止2991" : "启动2991";
        }

        public void closeWindow()
        {
            freConvertView.setFlag();
            sendFlag = false;
            loadFlag = false;
            NET2991A.sample = false;


        }


        private void timeOver(object a)
        {
            UserDef.flagRecord = false;


            if (channelNum.Count != 0 && UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count != 0)
            {

                int count = channelNum.Count;
                int save_index = 0;


                double[,] matdata = new double[count + 1, UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count];
                if (UserDef.xToSave.Count == 0)
                {

                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count; i++)
                    {

                        matdata[save_index, i] = UserDef.NowRes;
                    }

                }

                else if (UserDef.xToSave.Count * (UserDef.Frequency / 2) >= UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count)
                {

                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count; i++)
                    {
                        int n = i / (UserDef.Frequency / 2);
                        matdata[save_index, i] = UserDef.xToSave.ElementAt(n);
                    }

                }
                else
                {

                    for (int i = 0; i < UserDef.xToSave.Count * (UserDef.Frequency / 2); i++)
                    {
                        int n = i / (UserDef.Frequency / 2);
                        matdata[save_index, i] = UserDef.xToSave.ElementAt(n);
                    }



                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count - UserDef.xToSave.Count * (UserDef.Frequency / 2); i++)
                    {
                        matdata[save_index, i + UserDef.xToSave.Count * (UserDef.Frequency / 2)] = UserDef.xToSave.ElementAt(UserDef.xToSave.Count - 1);

                    }


                }

            }

            UserDef.flagRecord = true;
        }



        private System.Threading.Timer timer1;

        private void timeOutSave_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            timer1 = new Timer(new TimerCallback(timeOver), null, 10000, 10000);

            UserDef.flagRecord = true;
        }

        private void bt_save_all_mat_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "mat文件|*.mat|,csv文件|*.csv";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = UserDef.PathDir;
            saveFileDialog.FileName = DateTime.Now.ToString("MM-dd-H-mm-ss_");/* + tb_comment.Text*/
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                String fileName = saveFileDialog.FileName;
                StreamWriter swt = new StreamWriter(fileName);
                //double[,] matdata = new double[channelNum.Count, data.ElementAt(channelNum.ElementAt(0)).Count];
                int save_index = 0;
                for (int i = 0; i < channelNum.Count; i++)
                {
                    for (int j = 0; j < data.ElementAt(channelNum.ElementAt(i)).Count; j++)
                    {
                        if (j == data.ElementAt(channelNum.ElementAt(i)).Count - 1)
                        {
                            swt.Write(data.ElementAt(channelNum.ElementAt(i)).ElementAt(j).ToString());
                        }
                        else
                            swt.Write(data.ElementAt(channelNum.ElementAt(i)).ElementAt(j).ToString() + ",");
                        // matdata[save_index, j] = data.ElementAt(channelNum.ElementAt(i)).ElementAt(j);
                    }
                    swt.Write("\r\n");
                    save_index++;

                }

                swt.Flush();
                swt.Close();



                /*if (fileName.Contains(".csv"))
                {
                   
                    for (int i = 0; i < matdata.GetLength(1); i++)
                    {
                        for (int j = 0; j < matdata.GetLength(0); j++)
                        {

                            swt.Write(matdata[j, i].ToString() + ",");

                        }

                        swt.Write("\r\n");
                    }
                    swt.Flush();
                    swt.Close();
                }
                else
                {

                    Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matdata);

                    String path = fileName;//UserDef.PathDir + @"mat\" + DateTime.Now.ToString("MM-dd-H-mm-ss_") + tb_comment.Text + ".mat";
                    MatlabWriter.Write(path, matrix, "data");

                }*/

            }
        }


        private void bt_pause_Click(object sender, RoutedEventArgs e)
        {
            if (bt_pause.Header.ToString() == "暂停采集")
            {
                bt_pause.Header = "重启";
                needPause = true;
                isContinue = false;

            }
            else
            {
                clearData();
                bt_pause.Header = "暂停采集";
                needPause = false;
                isContinue = false;
                cb_continueLog.IsChecked = false;
            }
        }


        private void cb_continueLog_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_continueLog.IsChecked == true)
            {
                isContinue = true;
            }
            else
            {
                isContinue = false;
            }
        }
        //功率谱变换

    }
}
