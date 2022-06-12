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
using DataSystem.Classes;
using MathWorks.MATLAB.NET.Arrays;
using FFT;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using FindPeaks;

namespace DataSystem.Views
{
    /// <summary>
    /// FreConvertView.xaml 的交互逻辑
    /// </summary>
    public partial class FreConvertView : UserControl
    {

        private List<double> axis_f = new List<double>();
        private bool computeMaxFrequency = false;
        private bool computeMaxAM = false;
        private bool analysis = false;
        private volatile int selectChannel = -1;
        private bool start_stop = true;
        Thread sendThread = null;
        Thread receiveThread = null;
        ConnectionFactory connectionFactory = null;
        IConnection connection = null;
        IModel channel = null;
        IModel recieveCh = null;
        BlockingCollection<List<double>> queue = new BlockingCollection<List<double>>(1);
        //MLApp.MLApp matlab = null;
        
        FFT.Class1 fft = new FFT.Class1();
        FindPeaks.Class1 FindPeaks = new FindPeaks.Class1();
        private String receiveMsg = "";
        private double startIndex = 0;


        private static List<List<Double>> parameters = new List<List<Double>>() {
            new List<double>(){-1.6114303731727691e-13, 8.995789942798905e-08, -0.017776192617190895, 1374.2099070543757 },
            new List<double>() { -7.710499011023721e-10, - 0.000401885042517353, 152.19366498580783 },
            new List<Double>() {1.6937729313837746e-09, -0.0014028515618654488, 253.99085582499248 },  //24.5
            new List<Double>() {5.2e-09, -0.0028057469, 394.63929418733466 }, //24.4
            new List<Double>() {4.1e-09, -0.0023502101, 347.382012835228 }, //24.3
            new List<Double>() {2.8e-09, -0.0018277928, 293.1887642205437  },//24.2
            new List<Double>() {2.5e-09, -0.0016901039, 278.921710740231 },//24.1
            new List<Double>() {1.7e-09, -0.0013592452, 244.3985840598463 },//24
            new List<Double>() {9e-10, -0.0010559414, 212.5323502410578 },//23.9
            new List<Double>() {-8e-10, -0.0003374776, 137.4752573783787 },//23.8
            new List<Double>() {-1.7e-09, 6.31297e-05, 95.44041392958543 },//23.7
            new List<Double>() {-2.3e-09, 0.0003124412, 69.22205280574725 },//23.6
            new List<Double>() {-2.4e-09, 0.0003349066, 66.65911661644182 }}; //23.5


        private void convertFlag()
        {
            computeMaxAM = false;
            computeMaxFrequency = false;
        }


        //从消息队列获取结果
        private void getOutCome()
        {
            var consumer = new EventingBasicConsumer(recieveCh);
            recieveCh.BasicConsume("call_back", false, consumer);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                ReadOnlyMemory<Byte> res = ea.Body;
                byte[] bytes = res.ToArray();
                String message = Encoding.UTF8.GetString(bytes);
                receiveMsg = message;

               
                
                recieveCh.BasicAck(ea.DeliveryTag, false);
                
                

            };
        }

        //将数据发送到消息队列
        private void sendData()
        {
            while (analysis)
            {
                List<Double> data = null;
                bool flag = queue.TryTake(out data);
                if (flag && data != null)
                {
                    StringBuilder builder = new StringBuilder();
                    double[] outpeaks = findPeaks(data.ToArray());

                    for (int i = 0; i < outpeaks.Length; i++)
                    {
                        int left = (int)outpeaks[i] - 1000;
                        int right = (int)outpeaks[i] + 1000;

                        for(int j = left; j < right; j++)
                        {
                            builder.Append(data[j].ToString() + ",");
                        }
                    }

                    foreach(double index in outpeaks)
                    {
                        builder.Append(index.ToString() + ",");
                    }

                    builder = builder.Remove(builder.Length - 1, 1); 
                    
                    channel.BasicPublish("wave", "wave", null, Encoding.UTF8.GetBytes(builder.ToString()));
                    builder.Clear();
                    Thread.Sleep(1500);


                }
            }

        }


        private double [] findPeaks(double[] data)
        {
            MWNumericArray input = (MWNumericArray)data;
            MWArray[] ots = null;
           
            ots = FindPeaks.FindPeaks(2, input, 0.02, 1000);
            double[,] value = (double[,])ots[0].ToArray();
            double[,] peaks = (double[,])ots[1].ToArray();
            double[] out_value = new double[value.GetLength(1)];
            double[] out_peaks = new double[peaks.GetLength(1)];
            for (int i = 0; i < out_peaks.Length; i++)
            {
                out_value[i] = value[0, i];
                out_peaks[i] = peaks[0, i];
            }
            return out_peaks;
        }



        //计算主频对应的频率
        private void computeMaxFrequencyFunc(List<Double> data, int lowf, int highf, int channel)
        {
            List<Double> lis = data.ToList<Double>().GetRange(lowf, highf);
            double max = lis.Max();
            double index = (double)(lis.IndexOf(max));

            UserDef.freq.ElementAt(channel).Add(index + lowf);
            if (UserDef.freq.ElementAt(channel).Count >= 2000)
            {
                UserDef.freq.ElementAt(channel).RemoveAt(0);
            }

            frePlot.plt.PlotSignal(UserDef.freq.ElementAt(channel).ToArray(), label: String.Format("通道:{0}", channel));
            frePlot.plt.PlotAnnotation(index + "Hz",fontSize:20);
        }

        private void computeMaxAMFunc(List<double> data, int lowf, int highf, int channel)
        {
            List<Double> lis = data.ToList<Double>().GetRange(lowf, highf);
            double max = lis.Max();
            UserDef.MAXVALUE.ElementAt(channel).Add(max);
            if (UserDef.MAXVALUE.ElementAt(channel).Count > 2000)
            {
                UserDef.MAXVALUE.ElementAt(channel).RemoveAt(0);
            }

            frePlot.plt.PlotSignal(UserDef.MAXVALUE.ElementAt(channel).ToArray(), label: String.Format("通道:{0}", channel));

        }

        private Thread threadSync = null;
        private bool execute = false;
        public FreConvertView()
        {
            InitializeComponent();

            /*Type matlabAppType = System.Type.GetTypeFromProgID("Matlab.Application");
            matlab = System.Activator.CreateInstance(matlabAppType) as MLApp.MLApp;
            string path_project = System.IO.Directory.GetCurrentDirectory();
            string path_matlabwork = "cd('" + path_project + "')";
            matlab.Execute(path_matlabwork);*/
            InitRabbitMq();
            frePlot.plt.Title("frequency spectrum", fontSize:25);
            frePlot.plt.XLabel("frequency[Hz]", fontSize:25);
            frePlot.plt.YLabel("norm value", fontSize: 25);
            frePlot.plt.Ticks(fontSize: 25);
            oriSignal.plt.Ticks(fontSize: 25);
            
        }


        private void InitRabbitMq()
        {
            try
            {
                connectionFactory = new ConnectionFactory();
                //connectionFactory.Uri = new Uri("amqp://admin:123456@localhost:5672/vhost");
                connectionFactory.HostName = "192.168.1.99";
                connectionFactory.UserName = "guest";
                connectionFactory.Password = "guest";
                connectionFactory.VirtualHost = "/";//默认情况可省略此行
                connectionFactory.Port = 5672;

                connection = connectionFactory.CreateConnection();
                channel = connection.CreateModel();
                channel.BasicQos(0, 1, false);
                //识别结果回传
                /*recieveCh = connection.CreateModel();
                IDictionary<String, Object> args = new Dictionary<String, Object>();
                args.Add("x-max-length", 2);
                recieveCh.QueueDeclare("call_back", true, true, true,args);
                recieveCh.ExchangeDeclare("back", "direct", true, true);
                recieveCh.QueueBind("call_back", "back", "receive");*/
                Console.WriteLine("INIT_OVER");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public void startFreThread()
        {
            this.execute = true;
            threadSync = new Thread(new ThreadStart(workThreadFunc));
            threadSync.Start();
        }


        private Double[] computeFFT(Double[] data, int low, int countPoint)
        {

            MWNumericArray input = (MWNumericArray)data;
            MWArray[] ots = null;
            ots = fft.FFT(1, (MWArray)input);
            double[,] resdata = (double[,])ots[0].ToArray();
            double[] usedata = new double[(int)(resdata.GetLength(1) / 2)];
            for (int j = 0; j < usedata.Length; j++)
            {
                usedata[j] = resdata[0, j];
            }

            usedata[0] = 0;
            return usedata;

        }



        private void workThreadFunc()
        {
            while (execute)
            {

                FreDataObject input = null;
                bool flag = UserDef.syncQueFre.TryTake(out input);

                if (flag && input != null && start_stop)
                {
                    this.selectChannel = input.getChannel();
                    double[] fre_data = computeFFT(input.getData().ToArray(), 0, 0);

                    frePlot.Dispatcher.BeginInvoke(new Action(() =>
                    {

                        frePlot.plt.Clear();
                        oriSignal.plt.Clear();


                        int lowf = 0;
                        int highf = input.getData().Count / 2;
                        if (textBox.Text != "0")
                        {
                            String[] ts = textBox.Text.Split('-');
                            try
                            {
                                lowf = Convert.ToInt32(ts[0]);
                                highf = Convert.ToInt32(ts[1]);
                                startIndex = lowf;

                            }
                            catch (Exception) { }

                        }

                        if (computeMaxFrequency)
                        {
                            computeMaxFrequencyFunc(fre_data.ToList<Double>(), lowf, highf, input.getChannel());
                        }
                        else if (computeMaxAM)
                        {
                            computeMaxAMFunc(fre_data.ToList<Double>(), lowf, highf, input.getChannel());
                        }
                        else
                        {

                            UserDef.dataToSave.ElementAt(input.getChannel()).Clear();
                            List<Double> lis = fre_data.ToList<Double>().GetRange(lowf, highf);
                            if (analysis)
                            {

                                List<double> ds = new List<double>(lis);
                                queue.TryAdd(ds);

                                if (!receiveMsg.Equals(""))
                                {
                                    String ans = "";
                                    String[] strs = receiveMsg.Split(',');
                                    int cd = 0;
                                    for (int i = 0; i < strs.Length; i += 2)
                                    {
                                        try
                                        {
                                            string power = strs[i];
                                            double fre = startIndex + Convert.ToDouble(strs[i + 1]);
                                            double p = parameters.ElementAt(2).ElementAt(0) * fre * fre + parameters.ElementAt(2).ElementAt(1) * fre + parameters.ElementAt(2).ElementAt(2);
                                            p = Math.Round(p, 2);
                                            if (cd % 2 == 0)
                                            {
                                                frePlot.plt.PlotText(power, fre - 25000, lis.Max() - lis.Max() / 4, fontSize: 30);
                                                cd += 1;
                                            }
                                            else
                                            {
                                                frePlot.plt.PlotText(power, fre + 5000, lis.Max() - lis.Max() / 4, fontSize: 30);
                                                cd += 1;
                                            }
                                            frePlot.plt.PlotLine(fre - 1000, 0, fre - 1000, lis.Max()-lis.Max()/5);
                                            frePlot.plt.PlotLine(fre - 1000, lis.Max() - lis.Max() / 5, fre + 1000, lis.Max() - lis.Max() / 5);
                                            frePlot.plt.PlotLine(fre + 1000, lis.Max() - lis.Max() / 5, fre + 1000, 0);

                                            ans += power;

                                            ans += ("：" + p.ToString() + "W\n");
                                        }catch(Exception ex)
                                        {

                                        }

                                    }

                                    frePlot.plt.PlotAnnotation(ans, 20, 20, fontSize: 30);
                                    receiveMsg = "";

                                }

                               
                               
                            }
                            UserDef.dataToSave[input.getChannel()] = lis;
                            axis_f.Clear();
                            for (int ns = 0; ns < lis.Count; ns++)
                            {
                                axis_f.Add(ns + lowf);
                            }
                            //label: String.Format("Channel:{0}", input.getChannel())
                            frePlot.plt.PlotSignalXY(axis_f.ToArray(), lis.ToArray());
                            oriSignal.plt.PlotSignal(input.getData().ToArray(), UserDef.Frequency,input.getStartTime());
                            
                        }

                        frePlot.plt.Legend();
                        oriSignal.plt.Legend();
                        frePlot.Render();
                        oriSignal.Render();

                    }));
                }
                Thread.Sleep(500);
            }

        }

        public void setFlag()
        {
            this.execute = false;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            convertFlag();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            convertFlag();
            computeMaxFrequency = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            convertFlag();
            computeMaxAM = true;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (!analysis)
            {
                analysis = true;
                if (sendThread == null && receiveThread == null)
                {
                    sendThread = new Thread(new ThreadStart(sendData));
                    receiveThread = new Thread(new ThreadStart(getOutCome));
                    
                }
                sendThread.Start();
                receiveThread.Start();
                button2.Content = "关闭网络分析";

            }
            else
            {
                button2.Content = "开启网络分析";
                analysis = false;
            }
        }

        private void saveCharacter_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            //System.Windows.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "csv文件|*.csv";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = UserDef.PathDir;
            saveFileDialog.FileName = DateTime.Now.ToString("MM-dd-H-mm-ss_") + this.ass_info.Text;
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                String fileName = saveFileDialog.FileName;

                if (fileName.Contains(".csv"))
                {
                    try
                    {
                        StreamWriter swt = new StreamWriter(fileName);
                        StringBuilder builder = new StringBuilder();
                        UserDef.dataToSave.ElementAt(selectChannel).ForEach(o1 =>
                        {
                            builder.Append(o1.ToString() + "\t" + ",");
                        });
                        String s = builder.ToString().Substring(0, builder.Length - 1);
                        swt.WriteLine(s);
                        //swt.Write("\t\n");
                        swt.Flush();
                        swt.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }

            }
        }
        private void stopShow_Click(object sender, RoutedEventArgs e)
        {
            if (stopShow.Content.Equals("暂停"))
            {
                start_stop = false;
                stopShow.Content = "重启";
            }
            else
            {
                start_stop = true;
                stopShow.Content = "暂停";
            }
        }
    }
}
