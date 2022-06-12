using Npgsql;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace ClientSystem.Views
{
    /// <summary>
    /// DataView.xaml 的交互逻辑
    /// </summary>
    public partial class DataView : UserControl
    {

        //数据库服务器
        static string ConStr = @"PORT=5432;DATABASE=WestData;HOST=192.168.1.87;PASSWORD=root;USER ID=postgres"; //数据库服务地址
        private NpgsqlConnection SqlConn = new NpgsqlConnection(ConStr);
        private List<string> rippleNames = new List<string>();
        private List<string> tmpNames = new List<string>();
        private List<string> emNames = new List<string>();
        IDictionary<String, String> ripple = new Dictionary<String, String>();
        IDictionary<String, String> tmp = new Dictionary<String, String>();
        IDictionary<String, String> em = new Dictionary<String, String>();
        private WebClient webclient = new WebClient();
        private void loadTemperature()
        {
            try
            {
                SqlConn.Open();
                String sql = "select * from data_table where dtype='temperature'";
                using (NpgsqlCommand command = new NpgsqlCommand(sql, SqlConn))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {
                        String file_name = reader.GetString(0);
                        Button btn_temp = new Button();

                        String Name = "temperature" + i.ToString();
                        btn_temp.Name = Name;
                        btn_temp.Content = file_name;
                        btn_temp.FontSize = 15;
                        btn_temp.Click += Btn_temp_Click; 
                        tmp.TryAdd(Name, file_name);
                        i++;
                        this.tempPanel.Children.Add(btn_temp);


                    }
                        

                    }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    if (SqlConn != null)
                    {
                        SqlConn.Close();
                    }
                }
                catch (Exception eex)
                {
                    Console.WriteLine(eex);
                }
            }
        }

        private void Btn_temp_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String key = btn.Name;
            String file_path = "";
            tmp.TryGetValue(key, out file_path);
            if (!file_path.Equals(""))
            {
                String[] strs1 = file_path.Split('\\');
                Console.WriteLine(strs1[strs1.Length - 1]);
                Uri uri = new Uri("ftp://192.168.1.87/" + strs1[strs1.Length - 1]);
                byte[] bytes = webclient.DownloadData(uri);
                String dds = System.Text.Encoding.Default.GetString(bytes);
                dds = dds.Substring(1, dds.Length - 2);
                String[] strs = dds.Split(',');
                List<double> data = new List<double>();
                for (int j = 0; j < strs.Length; j++)
                {
                    data.Add(Convert.ToDouble(strs[j]));
                }
                temperature_plot.plt.Clear();
                temperature_plot.plt.PlotSignal(data.ToArray());
                temperature_plot.Render();
            }
        }

        private void loadEM()
        {
            try
            {
                SqlConn.Open();
                String sql = "select * from data_table where dtype='em'";
                using (NpgsqlCommand command = new NpgsqlCommand(sql, SqlConn))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {
                        String file_name = reader.GetString(0);
                        Button btn_em = new Button();
                       
                        String Name = "em" + i.ToString();
                        btn_em.Name = Name;
                        btn_em.Content = file_name;
                        btn_em.FontSize = 15;
                        btn_em.Click += Btn_em_Click;
                        em.TryAdd(Name, file_name);
                        i++;
                        this.emPanel.Children.Add(btn_em);

                    }


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    if (SqlConn != null)
                    {
                        SqlConn.Close();
                    }
                }
                catch (Exception eex)
                {
                    Console.WriteLine(eex);
                }
            }
        }

        private void Btn_em_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String key = btn.Name;
            String file_path = "";
            em.TryGetValue(key, out file_path);
            if (!file_path.Equals(""))
            {
                String[] strs1 = file_path.Split('\\');
                Console.WriteLine(strs1[strs1.Length - 1]);
                Uri uri = new Uri("ftp://192.168.1.87/" + strs1[strs1.Length - 1]);
                byte[] bytes = webclient.DownloadData(uri);
                String dds = System.Text.Encoding.Default.GetString(bytes);
                dds = dds.Substring(1, dds.Length - 2);

                String[] em_data = dds.Split(":");
                String[] estr_data = em_data[0].Split(',');
                String[] mstr_data = em_data[1].Split(',');

                List<double> e_data = new List<double>();
                for (int j = 0; j < estr_data.Length; j++)
                {
                    e_data.Add(Convert.ToDouble(estr_data[j]));
                }

                List<double> m_data = new List<double>();
                for (int j = 0; j < mstr_data.Length; j++)
                {
                    m_data.Add(Convert.ToDouble(mstr_data[j]));
                }

                em_plot.plt.Clear();
                em_plot.plt.PlotSignal(e_data.ToArray());
                em_plot.plt.PlotSignal(m_data.ToArray());
                em_plot.Render();
            }
        }

        private void loadRipple()
        {
            try
            {
                SqlConn.Open();
                String sql = "select * from data_table where dtype='ripple'";
                using (NpgsqlCommand command = new NpgsqlCommand(sql, SqlConn))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {
                        String file_name = reader.GetString(0);
                      
                        Button btn = new Button();
                        //btn.Width = 200;
                        //btn.Height = 100;
                        String Name = "ripple" + i.ToString();
                        btn.Name = Name;
                        btn.Content = file_name;
                        btn.FontSize = 15;
                        btn.Click += Btn_Click;
                        ripple.TryAdd(Name,file_name);
                        i++;
                        this.ripplePanel.Children.Add(btn);
                    }

                }


                }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    if (SqlConn != null)
                    {
                        SqlConn.Close();
                    }
                }catch(Exception eex)
                {
                    Console.WriteLine(eex);
                }
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

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String key = btn.Name;
            String file_path = "";
            ripple.TryGetValue(key, out file_path);
            if (!file_path.Equals(""))
            {
                String[] strs1 = file_path.Split('\\');
                Console.WriteLine(strs1[strs1.Length - 1]);
                Uri uri = new Uri("ftp://192.168.1.87/" + strs1[strs1.Length-1]);
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
                ripple_plot.plt.Clear();
                ripple_plot.plt.PlotSignal(data.ToArray(), 1000000);
                ripple_plot.Render();
            }


        }

        public DataView()
        {
            InitializeComponent();
            this.ripple_plot.plt.XLabel("t(s)", fontSize: 20);
            this.ripple_plot.plt.YLabel("A(V)", fontSize: 20);
            this.ripple_plot.plt.Title("纹波", fontSize: 20);
            ripple_plot.plt.Ticks(fontSize: 20);

            this.temperature_plot.plt.XLabel("t(s)", fontSize: 20);
            this.temperature_plot.plt.YLabel("T(℃)", fontSize: 20);
            this.temperature_plot.plt.Title("温度", fontSize: 20);
            temperature_plot.plt.Ticks(fontSize: 20);

            this.em_plot.plt.XLabel("t(s)", fontSize: 20);
            this.em_plot.plt.YLabel("efi(V/m),mfi(μT)", fontSize: 20);
            this.em_plot.plt.Title("电磁场强度", fontSize: 20);
            em_plot.plt.Ticks(fontSize: 20);

            this.feature_plot.plt.XLabel("t(s)", fontSize: 20);
            this.feature_plot.plt.YLabel("normal value", fontSize: 20);
            this.feature_plot.plt.Title("谱图", fontSize: 20);
            feature_plot.plt.Ticks(fontSize: 20);
            loadEM();
            loadRipple();
            loadTemperature();
        }
    }
}
