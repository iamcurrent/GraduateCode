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
using ClientSystem.Classes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ScottPlot;
using WebSocketSharp;
using System.Threading;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace ClientSystem.Views
{
    /// <summary>
    /// PowerShowView.xaml 的交互逻辑
    /// </summary>
    public partial class PowerShowView : UserControl
    {
        private int Frequency = 1000000;
        WebSocket websocket = null;
        public delegate void DeleFunc();
        public delegate void DataShowFunc();
        volatile bool init_over = false;
     
        private IDictionary<string, ViewObject> viewArr = new Dictionary<string, ViewObject>();
        private IDictionary<string, List<double>> ripple_show_data = new ConcurrentDictionary<string, List<double>>();

        private IDictionary<string, List<double>> feature_vector = new ConcurrentDictionary<string, List<double>>();


        public bool showFlag = true;
        private List<List<double>> receiveData = new List<List<double>>();

        public bool data_flag = true;
        private bool health_option = false;

        private BackgroundWorker dataWorker = null;
        private BackgroundWorker target_thread = null;

        private List<double> axis_f = new List<double>();

        List<List<double>> data_ripple_sync = new List<List<double>>();

        private BlockingCollection<List<double>> target_segment = new BlockingCollection<List<double>>(2);
        private int startIndex = 0;
       

        public PowerShowView( WebSocket webSocket)
        {
            InitializeComponent();
            this.st_option.Click += St_option_Click;
            
        }
       


        private void St_option_Click(object sender, RoutedEventArgs e)
        {
            if (this.st_option.Header.ToString().Equals("开启远程数据"))
            {
                showFlag = true;
                dataWorker = new BackgroundWorker();
                dataWorker.DoWork += ShowDataAndControls;
                dataWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                dataWorker.WorkerSupportsCancellation = true;
                dataWorker.RunWorkerAsync();
                this.st_option.Header = "停止远程数据";

            }
            else
            {
                dataWorker.CancelAsync();
                dataWorker = null;
                this.st_option.Header = "开启远程数据";
                showFlag = false;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("complietetd");
            
        }

        public void closingWindows()
        {
            this.showFlag = false;
            this.data_flag = false;
            InitMqAndControl.destoryTimer();
        }

        //更新界面
        private void ShowDataAndControls(object sender, DoWorkEventArgs e)
        {
            while (data_flag)
            {

                InitMqAndControl.InOldNotInNow = InitMqAndControl.nowWorkChannel.Except(InitMqAndControl.newReceive).ToList();//需要删除的消息
                InitMqAndControl.InNowNotInOld = InitMqAndControl.newReceive.Except(InitMqAndControl.nowWorkChannel).ToList();//需要添加的消息
                

                if (InitMqAndControl.InNowNotInOld.Count > 0)
                {
                    for(int i =0;i< InitMqAndControl.InNowNotInOld.Count; i++)
                    {
                        data_ripple_sync.Add(new List<double>());

                    }
                    System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new DeleFunc(InitDataView));
                    InitMqAndControl.nowWorkChannel = new List<int>(InitMqAndControl.newReceive);
                    Console.WriteLine("Add");
                }
                dataPlot();
                Thread.Sleep(800);
               

            }
        }

        //显示数据
        private void dataPlot()
        {
            
                if (viewArr != null && viewArr.Count > 0 && showFlag)
                {

                    ViewObject redundance_ch = null;
                    viewArr.TryGetValue("15", out redundance_ch);    
                    if(redundance_ch!=null)
                    redundance_ch.plotObject.temperature.Dispatcher.BeginInvoke(new Action(() =>
                    {
                            List<double> P_pred = new List<double>();
                            redundance_ch.plotObject.em.plt.Clear();
                            redundance_ch.plotObject.ripple.plt.Clear();
                            redundance_ch.plotObject.temperature.plt.Clear();
                            List<double> ripple_data_reduncance = null;

                            InitMqAndControl.ripple_data.TryGetValue("15", out ripple_data_reduncance);
                            
                            double[] fft_data = InitMqAndControl.computeFFT(ripple_data_reduncance.ToArray());
                            
                            String scale = redundance_ch.textBox.Text;
                            String[] start_stop = scale.Split('-');
                            try
                            {
                                int start = Convert.ToInt32(start_stop[0]);
                                int stop = Convert.ToInt32(start_stop[1]);
                             
                                List<double> region = fft_data.ToList<double>();
                                
                                
                                axis_f.Clear();
                                for (int ns = 0; ns < stop-start; ns++)
                                {
                                    axis_f.Add(ns + start);
                                }

                                redundance_ch.plotObject.temperature.plt.PlotSignalXY(axis_f.ToArray(),region.GetRange(start, stop - start).ToArray());
                                redundance_ch.plotObject.ripple.plt.PlotSignal(ripple_data_reduncance.ToArray(),Frequency);

                                if (health_option)
                                {
                                    
                                    target_segment.TryAdd(region.GetRange(start, stop - start));
                                    string out_come = "";
                                
                                    InitMqAndControl.releaze_out.TryTake(out out_come);
                                    if (out_come != null && out_come.Length > 0)
                                    {
                                        String[] strs = out_come.Split(',');
                                        int cd = 0;
                                        String ans = "";
                                        for (int i = 0; i < strs.Length; i += 2)
                                        {
                                            try
                                            {
                                                string power = strs[i];
                                                double fre = start + Convert.ToDouble(strs[i + 1]);
                                                double p = InitMqAndControl.parameters.ElementAt(2).ElementAt(0) * fre * fre + InitMqAndControl.parameters.ElementAt(2).ElementAt(1) * fre + InitMqAndControl.parameters.ElementAt(2).ElementAt(2);
                                                p = Math.Round(p, 2);
                                                P_pred.Add(p);
                                                if (cd % 2 == 0)
                                                {
                                                    redundance_ch.plotObject.temperature.plt.PlotText(power, fre - 25000, region.Max() - region.Max() / 4, fontSize: 30);
                                                    cd += 1;
                                                }
                                                else
                                                {
                                                    redundance_ch.plotObject.temperature.plt.PlotText(power, fre + 5000, region.Max() - region.Max() / 4, fontSize: 30);
                                                    cd += 1;
                                                }
                                                redundance_ch.plotObject.temperature.plt.PlotLine(fre - 1000, 0, fre - 1000, region.Max() - region.Max() / 5);
                                                redundance_ch.plotObject.temperature.plt.PlotLine(fre - 1000, region.Max() - region.Max() / 5, fre + 1000, region.Max() - region.Max() / 5);
                                                redundance_ch.plotObject.temperature.plt.PlotLine(fre + 1000, region.Max() - region.Max() / 5, fre + 1000, 0);

                                                ans += power;

                                                ans += ("：" + p.ToString() + "W\n");
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                        }

                                        redundance_ch.plotObject.temperature.plt.PlotAnnotation(ans, 20, 20, fontSize: 30);
                                    }
                                }

                            redundance_ch.plotObject.ripple.Render();
                            redundance_ch.plotObject.temperature.Render();


                        }
                            catch (Exception ex)
                            {

                            }


                        int count = 0;
                        foreach (KeyValuePair<string, ViewObject> o1 in viewArr)
                        {
                            

                            String key = o1.Key;
                            if (!key.Equals("15"))
                            {
                                List<double> f_vector = new List<double>() {0,0,0,0,0,0,0 };
                               
                                

                                o1.Value.plotObject.em.plt.Clear();
                                o1.Value.plotObject.ripple.plt.Clear();
                                o1.Value.plotObject.temperature.plt.Clear();

                                List<double> ripple_data = null;
                                List<double> mfi_data = null;
                                List<double> efi_data = null;
                                List<double> temperature_data = null;

                                InitMqAndControl.ripple_data.TryGetValue(key, out ripple_data);
                                InitMqAndControl.mfi_data.TryGetValue(key, out mfi_data);
                                InitMqAndControl.efi_data.TryGetValue(key, out efi_data);
                                InitMqAndControl.temperature_data.TryGetValue(key, out temperature_data);

                                if (ripple_data != null && ripple_data.Count > 0)
                                {
                                    o1.Value.plotObject.ripple.plt.PlotSignal(ripple_data.ToArray(), Frequency);
                                    if (count < P_pred.Count)
                                    {
                                        f_vector[5] = P_pred[count];
                                        f_vector[6] = 240;
                                    }
                                    else
                                    {
                                        f_vector[5] = 1 * 10e-7;
                                        f_vector[6] = 240;
                                    }
                                }

                                if (efi_data != null && efi_data.Count > 0 && mfi_data != null && mfi_data.Count > 0 && !key.Equals("15"))
                                {
                                    o1.Value.plotObject.em.plt.PlotSignal(efi_data.ToArray());
                                    o1.Value.plotObject.em.plt.PlotSignal(mfi_data.ToArray());
                                    f_vector[3] = efi_data.ElementAt(efi_data.Count - 1);
                                    f_vector[4] = mfi_data.ElementAt(mfi_data.Count - 1);
                                }

                                if (temperature_data != null && temperature_data.Count > 0 && !key.Equals("15"))
                                {
                                    o1.Value.plotObject.temperature.plt.PlotSignal(temperature_data.ToArray());
                                    f_vector[0] = temperature_data.ElementAt(temperature_data.Count-1);
                                    f_vector[1] = -25;
                                    f_vector[2] = 80;
                                }
                                o1.Value.plotObject.em.Render();
                                o1.Value.plotObject.ripple.Render();
                                o1.Value.plotObject.temperature.Render();
                            }
                        }

                    }));


            }


        }


        private Image addImage()
        {
            Image image = new Image();
            BitmapImage bl = new BitmapImage();
            bl.BeginInit();
            bl.UriSource = new Uri("/Views/simple.png", UriKind.RelativeOrAbsolute);
            bl.EndInit();
            image.Source = bl;
            image.Width = 400;
            image.Height = 100;
            return image;
        }

        private PlotObject addPlotView(String tag)
        {
            WpfPlot ripple = new WpfPlot();
           
            //纹波存储
            MenuItem item = new MenuItem();
            item.Name = "item0" +tag;
            item.Header = "保存数据";
            item.Click += Item_Click;
            ripple.ContextMenu.Items.Add(item);

            //纹波上传服务器
            CheckBox upload0 = new CheckBox();
            upload0.Name = "upload0" + tag;
            upload0.Content = "上传服务器";
            upload0.Checked += Upload0_Checked;
            ripple.ContextMenu.Items.Add(upload0);


            ripple.Width = 400;
            ripple.Height = 230;
            ripple.plt.XLabel("t(s)", fontSize: 15);
            ripple.plt.YLabel("A(V)", fontSize: 15);
            ripple.plt.Title("纹波");
            ripple.plt.Ticks(fontSize: 20);

            WpfPlot temperature = new WpfPlot();

            //温度存储
            MenuItem item1 = new MenuItem();
            item1.Name = "item1" + tag;
            item1.Header = "保存数据";
            item1.Click += Item_Click;
            temperature.ContextMenu.Items.Add(item1);

            //温度上传服务器
            CheckBox upload1 = new CheckBox();
            upload1.Name = "upload1" + tag;
            upload1.Content = "上传服务器";
            upload1.Checked += Upload0_Checked;
            ripple.ContextMenu.Items.Add(upload1);


            temperature.Width = 400;
            temperature.Height = 230;
            temperature.plt.XLabel("t(s)", fontSize: 15);
            temperature.plt.YLabel("T(℃)", fontSize: 15);
            temperature.plt.Title("温度");
            temperature.plt.Ticks(fontSize: 20);


            WpfPlot em = new WpfPlot();

            //电磁场强度保存
            MenuItem item2 = new MenuItem();
            item2.Name = "item2" + tag;
            item2.Header = "保存数据";
            item2.Click += Item_Click;
            em.ContextMenu.Items.Add(item2);

            //电磁场强度上传
            CheckBox upload2 = new CheckBox();
            upload2.Name = "upload1" + tag;
            upload2.Content = "上传服务器";
            upload2.Checked += Upload0_Checked;
            ripple.ContextMenu.Items.Add(upload2);


            em.Width = 400;
            em.Height = 230;
            em.plt.XLabel("t(s)", fontSize: 15);
            em.plt.YLabel("efi(V/m),mfi(μT)", fontSize: 15);
            em.plt.Title("电磁场强度");
            em.plt.Ticks(fontSize: 20);
            PlotObject obj = new PlotObject(ripple, temperature, em);

            return obj;
        }

        private void InitDataView()
        {
            List<int> numbers = InitMqAndControl.InNowNotInOld;
            
            if (numbers.Count > 0)
            {
                for (int i = 0; i < numbers.Count; i++)
                {
                    if (numbers.ElementAt(i) == 15)
                    {
                        continue;
                    }

                    Image image = addImage();

                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Width = 400;
                    stackPanel.Height = 900;

                    PlotObject obj = addPlotView(numbers.ElementAt(i).ToString());
                    obj.image = image;
                    Label label1 = new Label();
                    label1.Content = "当前状态:轻载";
                    label1.FontSize = 20;
                    obj.label = label1;
                    stackPanel.Children.Add(image);
                    stackPanel.Children.Add(obj.ripple);
                    stackPanel.Children.Add(obj.temperature);
                    stackPanel.Children.Add(obj.em);
                    stackPanel.Children.Add(obj.label);

                    viewArr.Add(numbers.ElementAt(i).ToString(), new ViewObject(stackPanel, obj));
                    this.InfoPanel.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.InfoPanel.Children.Insert(0,stackPanel);
                    }));
                }

                if (init_over == false)
                {

                    StackPanel stackPanel_all = new StackPanel();
                    stackPanel_all.Width = 800;
                    stackPanel_all.Height = 900;
                    

                    WpfPlot ripple_all = new WpfPlot();
                    WpfPlot fft_all = new WpfPlot();


                    //纹波存储
                    MenuItem item_all = new MenuItem();
                    item_all.Name = "item15";
                    item_all.Header = "保存数据";
                    item_all.Click += Item_Click;
                    ripple_all.ContextMenu.Items.Add(item_all);

                    //纹波上传服务器
                    CheckBox upload15 = new CheckBox();
                    upload15.Name = "upload15";
                    upload15.Content = "上传服务器";
                    upload15.Checked += Upload0_Checked;
                    ripple_all.ContextMenu.Items.Add(upload15);


                    ripple_all.Width = 750;
                    ripple_all.Height = 350;
                    ripple_all.plt.XLabel("t(s)", fontSize: 15);
                    ripple_all.plt.YLabel("A(V)", fontSize: 15);
                    ripple_all.plt.Title("冗余输出纹波");
                    ripple_all.plt.Ticks(fontSize:20);


                    fft_all.Width = 750;
                    fft_all.Height = 350;
                    fft_all.plt.XLabel("f(Hz)", fontSize: 15);
                    fft_all.plt.YLabel("normal value", fontSize: 15);
                    fft_all.plt.Title("总纹波频谱");
                    fft_all.plt.Ticks(fontSize: 20);


                    DockPanel dock = new DockPanel();
                    dock.Width = 300;
                    Label label = new Label();
                    label.Content = "频谱段";

                    TextBox textBox = new TextBox();
                    textBox.Text = "100000-280000";
                    textBox.FontSize = 20;
                    label.Width = 100;
                    label.FontSize = 20;
                    textBox.Width = 200;
                    dock.Children.Add(label);
                    dock.Children.Add(textBox);

                    DockPanel dock1 = new DockPanel();
                    dock1.Width = 750;
                    Label label1 = new Label();
                    label1.Content = "冗余电源当前状态:待检测";
                    label1.FontSize = 20;
                    dock1.Children.Add(label1);
                    
                    stackPanel_all.Children.Add(ripple_all);
                    stackPanel_all.Children.Add(fft_all);
                    stackPanel_all.Children.Add(dock);
                    stackPanel_all.Children.Add(dock1);
                    ViewObject rip = new ViewObject(stackPanel_all, new PlotObject(ripple_all, fft_all, new WpfPlot()));
                    rip.textBox = textBox;
                    viewArr.Add("15", rip);
                    this.InfoPanel.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.InfoPanel.Children.Add(stackPanel_all);
                    }));
                    init_over = true;
                }
            }
        }

        private void StopStart_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String content = (String)btn.Content;
            if (content.Equals("暂停"))
            {
                showFlag = false;
                btn.Content = "重启";
            }
            else
            {
                showFlag = true;
                btn.Content = "暂停";
            }
        }

        private void Upload0_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            String Name = checkBox.Name;
            String type = Name.Substring(0, "upload0".Length);
            String upload_letter = Name.Substring("upload0".Length);
            if (Name.Equals("upload15"))
            {
                upload_letter = "15";
            }
            if (checkBox.IsChecked==true)
            {
                websocket.Send("upload:r1,"+upload_letter);

            }
            else
            {
                websocket.Send("upload:r0,"+upload_letter);
            }


        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            String Name = menuItem.Name;
            String type = Name.Substring(0, 5);
            String key = Name.Substring(5);
            if (Name.Equals("item15"))
            {
                key = "15";
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "mat文件|*.mat|,csv文件|*.csv";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = "";
            //saveFileDialog.FileName = DateTime.Now.ToString("MM-dd-H-mm-ss_");
            if (type.Equals("item0") || Name.Equals("item15"))
            {
                saveFileDialog.FileName = DateTime.Now.ToString("ripple_MM-dd-H-mm-ss_");
                /*Nullable<bool> result = saveFileDialog.ShowDialog();
                List<double> data = null;
                InitMqAndControl.ripple_data.TryGetValue(key, out data);
                if (result == true && data.Count > 0)
                {
                    String fileName = saveFileDialog.FileName;
                    if (fileName.Contains(".csv"))
                    {
                        try
                        {
                            StreamWriter swt = new StreamWriter(fileName);
                            StringBuilder builder = new StringBuilder();
                            data.ForEach(o1 =>
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
                }*/

            }
            else if (type.Equals("item1"))
            {
                saveFileDialog.FileName = DateTime.Now.ToString("temperature_MM-dd-H-mm-ss_");
                Nullable<bool> result = saveFileDialog.ShowDialog();
                List<double> data = null;
                /*InitMqAndControl.temperature_data.TryGetValue(key, out data);
                if (result == true && data.Count > 0)
                {
                    String fileName = saveFileDialog.FileName;
                    if (fileName.Contains(".csv"))
                    {
                        try
                        {
                            StreamWriter swt = new StreamWriter(fileName);
                            StringBuilder builder = new StringBuilder();
                            data.ForEach(o1 =>
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
                }*/
            }
            else
            {

            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String name = btn.Name;
            string key = name.Substring(3);
            List<double> data = null;
            /*InitMqAndControl.ripple_data.TryGetValue(key, out data);
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            
            saveFileDialog.Filter = "mat文件|*.mat|,csv文件|*.csv";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = "";
            saveFileDialog.FileName = DateTime.Now.ToString("MM-dd-H-mm-ss_");

            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true && data.Count > 0)
            {
                String fileName = saveFileDialog.FileName;
                if (fileName.Contains(".csv"))
                {
                    try
                    {
                        StreamWriter swt = new StreamWriter(fileName);
                        StringBuilder builder = new StringBuilder();
                        data.ForEach(o1 =>
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
            }*/

        }

        private void sendTargetStr(object sender, DoWorkEventArgs e)
        {

            while (health_option)
            {

                //发送对应的数据
                List<double> rip_data = null;

                target_segment.TryTake(out rip_data);

                if (rip_data != null && rip_data.Count > 0)
                {
                    string target_str = InitMqAndControl.findPeaksStr(rip_data);
                    InitMqAndControl.send_channel.BasicPublish("wave", "wave", null, Encoding.UTF8.GetBytes(target_str));
                }
                Thread.Sleep(1000);
            }
        }

        private void sys_analyse_option_Click(object sender, RoutedEventArgs e)
        {

            if (this.sys_analyse_option.Header.ToString().Equals("开启系统分析"))
            {
                health_option = true;
                target_thread = new BackgroundWorker();
                target_thread.DoWork += sendTargetStr;
                target_thread.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                target_thread.WorkerSupportsCancellation = true;
                target_thread.RunWorkerAsync();
                this.sys_analyse_option.Header = "停止系统分析";
                

            }
            else
            {
                target_thread.CancelAsync();
                target_thread = null;
                this.sys_analyse_option.Header = "开启系统分析";
                health_option = false;
            }
        }
    }
}
