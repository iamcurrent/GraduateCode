using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Npgsql;
using MathWorks.MATLAB;
using FindPeaks;
using MathWorks.MATLAB.NET.Arrays;

namespace DataSystem.Views
{
    /// <summary>
    /// DataCollectionView.xaml 的交互逻辑
    /// </summary>
    public partial class DataCollectionView : Window
    {

        private Class1 class1 = new Class1();
        private String fileDirectory = "";
        private int count = -1;
        private int cur = 0;
        string[] fileNames = null;
        private StreamWriter writer = null;
        string save_path = "";
        private int click_index = 0;
        private int data_length = 0;

        static string ConStr = @"PORT=5432;DATABASE=WestData;HOST=192.168.1.87;PASSWORD=root;USER ID=postgres"; //数据库服务地址
        private NpgsqlConnection SqlConn = new NpgsqlConnection(ConStr);
        private Int64 numbers = 0;
        private int cursql = 0;
        private WebClient webclient = new WebClient();
        private List<double> curData = new List<double>();
        public DataCollectionView()
        {
            InitializeComponent();
            formsPlot.MouseDoubleClick += FormsPlot_MouseDoubleClick;
            webclient.DownloadDataCompleted += Webclient_DownloadDataCompleted;
            SqlConn.Open();
            using (NpgsqlCommand command = new NpgsqlCommand("select count(*) from wave_info", SqlConn))
            {
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    numbers = reader.GetInt64(0);
                    this.total.Text = numbers.ToString();
                }

            }
            SqlConn.Close();
        }



        private void Webclient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {

        }

        private void FormsPlot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (double a, double b) = formsPlot.GetMouseCoordinates();
            int pos = Convert.ToInt32(a);
            if (click_index % 2 == 0)
            {
                this.startPos.Text = pos.ToString();
                this.text_flag.AppendText(pos.ToString() + ",");
            }
            else
            {
                this.endPos.Text = pos.ToString();
                this.text_flag.AppendText(pos.ToString() + "," + data_length.ToString() + "," + label_control.Text + "\n");
            }

            click_index++;
        }


        private void filePath_btn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择csv所在文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    System.Windows.MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
                filePath.Text = dialog.SelectedPath;
                fileDirectory = dialog.SelectedPath;
                fileNames = Directory.GetFiles(fileDirectory);

                Console.WriteLine(fileNames[0]);
            }
        }

        private List<Double> LoadData()
        {
            List<double> tmp = new List<double>();
            StreamReader reader = new StreamReader(fileNames[count]);
            String d = reader.ReadLine();
            String[] strs = d.Split(',');
            foreach (string s in strs)
            {
                tmp.Add(Convert.ToDouble(s));
            }
            return tmp;
        }



        private void loadWave()
        {
            if (count < fileNames.Length)
            {
                if (writer != null)
                    writer.Close();
                String[] sp = fileNames[count].Split('\\');
                String name = sp[sp.Length - 1].Substring(0, sp[sp.Length - 1].Length - 4) + ".txt";
                if (!Directory.Exists(save_path + name))
                {
                    FileStream file = File.Create(save_path + name);
                    file.Close();
                }

                writer = new StreamWriter(save_path + name, true);
                click_index = 0;
                this.curFile.Text = fileNames[count];
                this.label_control.Text = fileNames[count].Substring(fileNames[count].Length - 6, 2);
                formsPlot.plt.Clear();
                List<double> data = LoadData();
                curData = data;
                data_length = data.Count;
                formsPlot.plt.PlotSignal(data.ToArray());
                cur = count;
                formsPlot.plt.Legend();
                formsPlot.Render();



            }
        }



        private void findPeaks(double [] data)
        {
            MWNumericArray input = (MWNumericArray)data;
            MWArray[] ots = null;
            Class1 class1 = new Class1();
            double minHeigth = Convert.ToDouble(min_height.Text);
            double minDistance = Convert.ToDouble(min_distance.Text);
            
            ots = class1.FindPeaks(2, input,minHeigth,minDistance);
            double[,] value = (double[,])ots[0].ToArray();
            double[,] peaks = (double[,])ots[1].ToArray();
            double[] out_value = new double[value.GetLength(1)];
            double[] out_peaks = new double[peaks.GetLength(1)];
            for (int i = 0; i < out_peaks.Length; i++)
            {
                out_value[i] = value[0, i];
                out_peaks[i] = peaks[0, i];
            }

            string curFileName = curFile.Text;
            String[] strs = curFileName.Split('\\');
            string name = strs[strs.Length - 1];
            String[] sps = name.Split('_');
            string flags = sps[sps.Length - 1].Split('.')[0];

            string p1 = flags.Substring(0,flags.Length-1);
            string p2 = "p" + flags.Substring(flags.Length - 1);
            int count = 0;
            foreach(double index in out_peaks)
            {
                double left = index - 1000;
                double right = index + 1000;
                text_flag.AppendText(left + "," + right + ",0" + "\n");
               /* if (count % 2 == 0)
                {
                    text_flag.AppendText(left + "," + right + ",0" + "\n");
                }
                else
                {
                    text_flag.AppendText(left + "," + right + ",0"+ "\n");
                }
                count++;*/
            }
        }

        private void preData_Click(object sender, RoutedEventArgs e)
        {
            if (count > 0)
            {
                this.text_flag.Clear();
                count--;
                loadWave();
            }
        }

        private void nextData_Click(object sender, RoutedEventArgs e)
        {
            if (count < fileNames.Length)
            {
                this.text_flag.Clear();
                count++;
                loadWave();
            }
        }


        private void label_btn(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String s = btn.Content.ToString();
            this.label_control.Text = s;
        }


        private void save_Click(object sender, RoutedEventArgs e)
        {

            if (writer != null)
            {
                String s = this.text_flag.Text;
                String[] strs = s.Split('\n');
                foreach (String t in strs)
                {
                    if (!t.Equals(""))
                        writer.WriteLine(t);
                }
                writer.Close();
            }

        }

        private void savePath_btn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择Txt所在文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    System.Windows.MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }

                save_path = dialog.SelectedPath + "\\";
                savePath.Text = save_path;
            }
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

        private void sqlPre_Click(object sender, RoutedEventArgs e)
        {
            SqlConn.Open();
            try
            {
                if (numbers > 0)
                {
                    if (cursql > 0)
                    {
                        String sql = "select * from wave_info limit 1 offset " + cursql;
                        using (NpgsqlCommand command = new NpgsqlCommand(sql, SqlConn))
                        {
                            NpgsqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                String file_name = reader.GetString(0);
                                Uri uri = new Uri("ftp://192.168.1.87/" + file_name + ".txt");
                                byte[] bytes = webclient.DownloadData(uri);
                                byte[] res = Decompress(bytes);
                                String dds = System.Text.Encoding.Default.GetString(res);
                                dds = dds.Substring(1, dds.Length - 2);
                                String[] strs = dds.Split(',');
                                List<double> data = new List<double>();
                                for (int j = 0; j < strs.Length; j++)
                                {
                                    data.Add(Convert.ToDouble(strs[j]));
                                }
                                formsPlot.plt.Clear();
                                formsPlot.plt.PlotSignal(data.ToArray());
                                formsPlot.plt.Legend();
                                formsPlot.Render();

                            }
                        }
                        cursql--;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                SqlConn.Close();
            }

        }

        private void sqlNext_Click(object sender, RoutedEventArgs e)
        {

            SqlConn.Open();
            try
            {
                if (numbers > 0)
                {
                    if (cursql < numbers)
                    {
                        String sql = "select * from wave_info limit 1 offset " + cursql;
                        using (NpgsqlCommand command = new NpgsqlCommand(sql, SqlConn))
                        {
                            NpgsqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                String file_name = reader.GetString(0);
                                String addr = "ftp://192.168.1.87/" + file_name + ".txt";
                                this.curFile.Text = file_name;
                                Uri uri = new Uri(addr);
                                byte[] bytes = webclient.DownloadData(uri);
                                byte[] res = Decompress(bytes);
                                String dds = System.Text.Encoding.Default.GetString(res);
                                dds = dds.Substring(1, dds.Length - 2);
                                String[] strs = dds.Split(',');
                                List<double> data = new List<double>();
                                for (int j = 0; j < strs.Length; j++)
                                {
                                    string t = strs[j];
                                    data.Add(Convert.ToDouble(t));
                                }
                                formsPlot.plt.Clear();
                                formsPlot.plt.PlotSignal(data.ToArray(), UserDef.Frequency);
                                formsPlot.plt.Legend();
                                formsPlot.Render();

                            }
                        }
                        cursql++;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                SqlConn.Close();
            }

        }

        private void sqlRead_Click(object sender, RoutedEventArgs e)
        {

        }

        private void find_peaks_Click(object sender, RoutedEventArgs e)
        {
            findPeaks(curData.ToArray());
        }

        
    }
}
