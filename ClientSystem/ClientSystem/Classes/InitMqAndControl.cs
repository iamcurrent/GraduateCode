using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebSocketSharp;
using FFT;
using MathWorks.MATLAB.NET.Arrays;
using FindPeaks;
using System.Collections.Concurrent;

namespace ClientSystem.Classes
{
    class InitMqAndControl
    {

        static ConnectionFactory connectionFactory = null;
        static IConnection connection = null;
        static List<IModel> channels = new List<IModel>();
        public static List<int> selectChannels = new List<int>(); //所有已选择的数据通道
        private static string letter = "";
        public static IDictionary<string, List<double>> temperature_data = new Dictionary<string, List<double>>();//温度数据
        public static IDictionary<string, List<double>> ripple_data = new Dictionary<string, List<double>>();//纹波数据
        public static IDictionary<string, List<double>> efi_data = new Dictionary<string, List<double>>();//电场强度
        public static IDictionary<string, List<double>> mfi_data = new Dictionary<string, List<double>>();//磁场强度
        private static Timer timer = null; //定时询问
        private static WebSocket WebSocketObject = null;
        public static List<int> InOldNotInNow = new List<int>();
        public static List<int> InNowNotInOld = new List<int>();
        public static List<int> nowWorkChannel = new List<int>();
        public static bool changeOver = false;
        public static List<int> newReceive = new List<int>();
        private static FFT.Class1 fft_func = new FFT.Class1();
        private static FindPeaks.Class1 findPeaks = new FindPeaks.Class1();
        public static IModel send_channel = null;
        public static IModel recieve_channel = null;
    
        public static List<List<Double>> parameters = new List<List<Double>>() {
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

        public static Thread receiveThread = null;
        public static BlockingCollection<string> releaze_out = new BlockingCollection<string>(2);

        public InitMqAndControl(WebSocket webSocket)
        {
            WebSocketObject = webSocket;
            StartRabbitMq();
            WebSocketObject.OnMessage += (sender, e) => MessageHandler(e.Data);
            timer = new Timer(new TimerCallback(timeOver), null, 5000, 5000);
        }

        public static double[] computeFFT(double [] data)
        {
            MWNumericArray input = (MWNumericArray)data;
            MWArray[] ots = null;
            ots = fft_func.FFT(1, (MWArray)input);
            double[,] resdata = (double[,])ots[0].ToArray();
            double[] usedata = new double[(int)(resdata.GetLength(1) / 2)];
            for (int j = 0; j < usedata.Length; j++)
            {
                usedata[j] = resdata[0, j];
            }

            usedata[0] = 0;
            return usedata;
        }


        public static string findPeaksStr(List<double> ripple)
        {
            MWNumericArray input = (MWNumericArray)ripple.ToArray();
            MWArray[] ots = null;

            ots = findPeaks.FindPeaks(2, input, 0.01, 1000);
            double[,] value = (double[,])ots[0].ToArray();
            double[,] peaks = (double[,])ots[1].ToArray();
            double[] out_value = new double[value.GetLength(1)];
            double[] out_peaks = new double[peaks.GetLength(1)];
            for (int i = 0; i < out_peaks.Length; i++)
            {
                out_value[i] = value[0, i];
                out_peaks[i] = peaks[0, i];
            }

            List<double> target_data = new List<double>();

            for (int i = 0; i < out_peaks.Length; i++)
            {
                int left = (int)out_peaks[i] - 1000;
                int right = (int)out_peaks[i] + 1000;

                target_data.AddRange(ripple.GetRange(left, right - left));
            }

            foreach (double index in out_peaks)
            {
                target_data.Add(index);
            }

            String str = String.Join(",",target_data.ToArray());

            return str;
        }

        /*public static void InitAllThing(WebSocket webSocket)
        {
            WebSocketObject = webSocket;
            StartRabbitMq();
            webSocket.OnMessage += (sender, e) => MessageHandler(e.Data);
            timer = new Timer(new TimerCallback(timeOver), null, 5000, 5000);
           
        }*/

        public static void destoryTimer()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        private static void timeOver(object a)
        {
            if (WebSocketObject.IsAlive)
                WebSocketObject.Send("query");
        }


        private static void MessageHandler(String msg) //处理rabbitmq消息，进行实时更新
        {
            if (msg.Contains("query_out"))
            {
                String[] strs = msg.Split(':');
                if (strs.Length > 1 && !strs[1].Equals(letter))
                {
                    String[] chs = strs[1].Split(','); //最新状态
                    List<int> workC = new List<int>();
                    foreach (string n in chs)
                    {
                        workC.Add(Convert.ToInt32(n));
                    }

                    newReceive = new List<int>(workC);
                
                    List<int> newNumber = workC.Except(selectChannels).ToList();
                    if (newNumber.Count > 0)
                    {
                        MqControl(newNumber);
                        foreach(int o in newNumber)
                        {
                            selectChannels.Add(o);
                            temperature_data.TryAdd(o.ToString(), new List<double>());
                            efi_data.TryAdd(o.ToString(), new List<double>());
                            mfi_data.TryAdd(o.ToString(), new List<double>());
                            ripple_data.TryAdd(o.ToString(), new List<double>());
                        }
                    }
                    
                    letter = strs[1];
                }

                }
            }


        private static void StartRabbitMq()
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

                IDictionary<String, Object> args = new Dictionary<String, Object>();
                args.Add("x-max-length", 5);

                //订阅温度
                IModel channel = connection.CreateModel();
                channel.BasicQos(0, 1, false);
                var temperature_consumer = new EventingBasicConsumer(channel);
                channel.QueueDeclare("Queue_temperature", false, false, true, args);

                channel.BasicConsume("Queue_temperature", true, temperature_consumer);

                temperature_consumer.Received += temperature_consumer_Received;

                //订阅电磁场强度
                IModel channel1 = connection.CreateModel();
                channel1.BasicQos(0, 1, false);
                var em_consumer = new EventingBasicConsumer(channel1);
                channel1.QueueDeclare("Queue_em", false, false, true, args);

                channel1.BasicConsume("Queue_em", true, em_consumer);

                em_consumer.Received += em_consumer_Received;

                channels.Add(channel1);

                //特征频段发送通道
                send_channel = connection.CreateModel();
                send_channel.BasicQos(0, 1, false);
                //结果返回通道
                recieve_channel = connection.CreateModel();
                IDictionary<String, Object> args1 = new Dictionary<String, Object>();
                args1.Add("x-max-length", 2);
                recieve_channel.QueueDeclare("call_back", true, true, true, args1);
                recieve_channel.ExchangeDeclare("back", "direct", true, true);
                recieve_channel.QueueBind("call_back", "back", "receive");
                receiveThread = new Thread(new ThreadStart(receiveComeOut));
                receiveThread.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void receiveComeOut()
        {
            var consumer = new EventingBasicConsumer(recieve_channel);
            recieve_channel.BasicConsume("call_back", false, consumer);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                ReadOnlyMemory<Byte> res = ea.Body;
                byte[] bytes = res.ToArray();
                String message = Encoding.UTF8.GetString(bytes);
                if (!message.Equals(""))
                {
                    releaze_out.TryAdd(message);
                }
                recieve_channel.BasicAck(ea.DeliveryTag, false);
            };
        }


        private static void Consumer_Received1(object sender, BasicDeliverEventArgs e)
        {
            //var body = e.Body;
            ReadOnlyMemory<Byte> res = e.Body;
            byte[] bytes = res.ToArray();
            String message = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(message);
            if (!message.Equals(""))
            {
                releaze_out.TryAdd(message);
            }

            recieve_channel.BasicAck(e.DeliveryTag, false);
        }

        //电磁场强度
        private static void em_consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            string message = Encoding.UTF8.GetString(body.ToArray());
            String[] data = message.Split(':');
            String[] efi = data[0].Split(',');
            String[] mfi = data[1].Split(',');

            for (int i = 0; i < efi.Length && efi_data.Count>0&&mfi_data.Count>0; i++)
            {
                double e_val = Math.Round(Convert.ToDouble(efi[i]), 2);
                double m_val = Math.Round(Convert.ToDouble(mfi[i]), 2);
                efi_data.ElementAt(i).Value.Add(e_val);
                mfi_data.ElementAt(i).Value.Add(m_val);
            }
        }


        //温度
        private static void temperature_consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            string message = Encoding.UTF8.GetString(body.ToArray());
            Console.WriteLine(message);
            string[] strs = message.Split(',');


            for (int i = 0; i < strs.Length && temperature_data.Count>0; i++)
            {
                double tmp = Convert.ToDouble(strs[i]);
                temperature_data.ElementAt(i).Value.Add(tmp);
            }
        }

        private static void MqControl(List<int> newNumber)
        {

            IDictionary<String, Object> args = new Dictionary<String, Object>();
            args.Add("x-max-length", 5);
            for (int i = 0; i < newNumber.Count; i++)
            {
                rabbitCreate("Queue_Ripple" + newNumber.ElementAt(i), "Queue_Ripple" + newNumber.ElementAt(i), args);
                
            }
        }

        private static void rabbitCreate(string queue_name, string routing_key, IDictionary<string, Object> args) //启动消息队列
        {
            IModel channel = connection.CreateModel();
            channel.QueueDeclare(queue_name, false, false, true, args);
            //channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(routing_key, true, consumer);
            consumer.Received += Consumer_Received;
            //channels.Add(channel);
            channels.Insert(0, channel);
        }

        private static List<double> convertDouble(string str)//字符串数据转换成数字
        {
            List<double> res = new List<double>();
            string[] strs = str.Split(',');
            foreach (string s in strs)
            {
                try
                {
                    res.Add(Convert.ToDouble(s));
                }
                catch (Exception ex) { }
            }

            return res;
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e) //接受消息队列数据
        {
            ReadOnlyMemory<Byte> res = e.Body;
            byte[] bytes = res.ToArray();

            byte[] data = Decompress(bytes);
            String str = System.Text.Encoding.Default.GetString(data);
            String[] strs = str.Split(':');
            List<double> plotData = convertDouble(strs[1]);
            List<double> ridata = null;
            ripple_data.TryGetValue(strs[0], out ridata);

            if (ridata != null)
            {
                ridata.Clear();
                for(int i = 0; i < plotData.Count; i++)
                {
                    ridata.Add(plotData.ElementAt(i));
                }
            }

            //ripple_data.TryAdd(strs[0],new List<double>(plotData));
            //Console.WriteLine("ripple");
            
        }


        public static byte[] Decompress(byte[] zippedData) //数据解压缩
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

    }
}
