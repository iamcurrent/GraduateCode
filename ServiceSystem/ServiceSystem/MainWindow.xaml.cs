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
using System.Net;
using Fleck;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Npgsql;

namespace ServiceSystem
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<IWebSocketConnection> allSockets = null;
        WebSocketServer server = null;
        private IDictionary<IWebSocketConnection,String> keys = new Dictionary<IWebSocketConnection,String>();
        IWebSocketConnection dataSystemSocket = null;
        static string ConStr = @"PORT=5432;DATABASE=WestData;HOST=192.168.1.87;PASSWORD=root;USER ID=postgres"; //数据库服务地址
        private NpgsqlConnection SqlConn = new NpgsqlConnection(ConStr);
        public string workLetter = "";

        private String getIp()
        {
            string hostName = Dns.GetHostName();   //获取本机名
            IPHostEntry localhost = Dns.GetHostByName(hostName);    //方法已过期，可以获取IPv4的地址
                                                                    //IPHostEntry localhost = Dns.GetHostEntry(hostName);   //获取IPv6地址
            foreach(IPAddress es in localhost.AddressList)
            {
                Console.WriteLine(es.ToString());
            }
            IPAddress localaddr = localhost.AddressList[4];

            return localaddr.ToString();
        }

        public void CreateRSAKey(IWebSocketConnection socket)
        {
            //创建RSA对象
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //生成RSA[公钥私钥]
            string privateKey = rsa.ToXmlString(true);
            string publicKey = rsa.ToXmlString(false);
            keys.Add(socket, privateKey);
            socket.Send("Login_key:"+publicKey);
        }

        //解密函数
        public string RSADecrypt(string data,string privateKey)
        {
            //创建RSA对象并载入[私钥]
            RSACryptoServiceProvider rsaPrivate = new RSACryptoServiceProvider();
            rsaPrivate.FromXmlString(privateKey);
            //对数据进行解密
            byte[] privateValue = rsaPrivate.Decrypt(Convert.FromBase64String(data), false);//使用Base64将string转换为byte
            string privateStr = Encoding.UTF8.GetString(privateValue);
            return privateStr;
        }

        private bool Query(String sql)
        {
            try
            {

                
                SqlConn.Open();
                using (NpgsqlCommand commad = new NpgsqlCommand(sql, SqlConn))
                {
                    NpgsqlDataReader reader = commad.ExecuteReader();
                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            return true;
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
                SqlConn.Close();
            }

            return false;
        }

        private void openService()
        {
            FleckLog.Level = LogLevel.Debug;
            String ip = getIp();
            this.ipAddress.Text = ip;
            Console.WriteLine(ip);
            allSockets = new List<IWebSocketConnection>();
            server = new WebSocketServer("ws://"+ip+":50000");
            server.Start(socket =>
            {
            socket.OnOpen = () =>
            {
                allSockets.Add(socket);
                this.connectionNums.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.connectionNums.Text = allSockets.Count.ToString();

                }));


            };

            socket.OnClose = () =>
            {
                allSockets.Remove(socket);
                this.connectionNums.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.connectionNums.Text = allSockets.Count.ToString();

                }));
            };

            socket.OnMessage = message =>
            {
            this.info.Dispatcher.BeginInvoke(new Action(()=> {

                this.info.AppendText(message+"\n");
                this.info.ScrollToEnd();
            
            }));
                Console.WriteLine(message);
                if (message.Equals("Login"))
                {
                    CreateRSAKey(socket);
                } else if (message.Contains("verify"))
                {
                    User user = JsonConvert.DeserializeObject<User>(message);
                    String userName = user.userName;
                    String publicStr = user.publicStr;
                    String privateKey = null;
                    keys.TryGetValue(socket, out privateKey);
                    String passWord = RSADecrypt(publicStr, privateKey);

                    try
                    {

                        string sql = "select * from user_table where user_Name='" + userName + "'";
                        SqlConn.Open();
                        using (NpgsqlCommand commad = new NpgsqlCommand(sql, SqlConn))
                        {
                            NpgsqlDataReader reader = commad.ExecuteReader();
                            if (reader != null)
                            {
                                if (reader.Read())
                                {
                                    string realPassWord = reader.GetString(1);
                                    if (!realPassWord.Equals(passWord))
                                    {

                                    }
                                    else
                                    {
                                        socket.Send("verify_yes");
                                    }
                                }
                                else
                                {
                                    socket.Send("no_user");
                                }



                            }
                            else
                            {
                                socket.Send("verify_no");
                            }
                        }
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        SqlConn.Close();
                    }
                } else if (message.Contains("register"))
                {
                    User user = JsonConvert.DeserializeObject<User>(message);
                    string sql = "select * from user_table where user_Name='" + user.userName + "'";
                    if (Query(sql))
                    {

                    }
                    else
                    {
                        try
                        {
                                SqlConn.Open();
                                string insert_sql = "insert into user_table values('" + user.userName + "','" + user.publicStr + "')";
                                using (NpgsqlCommand command = new NpgsqlCommand(insert_sql, SqlConn))
                                {
                                    int r = command.ExecuteNonQuery();
                                    if (r != 0)
                                    {
                                        socket.Send("register_yes");
                                    }
                                }
                            }catch(Exception ex)
                            {

                            }
                            finally
                            {
                            SqlConn.Close();
                            }

                        }
                    }else if (message.Contains("channels"))
                    {
                        String[] strs = message.Split(':');
                        workLetter = strs[1];
                        dataSystemSocket = socket;
                        
                }
                else if(message.Equals("query"))
                {
                    socket.Send("query_out:"+workLetter);
                }else if (message.Contains("upload"))
                {
                    dataSystemSocket.Send(message);
                }

                };

            });
            
        }
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            if (start.Content.Equals("启动"))
            {
                if (server == null)
                {
                    openService();
                }
                start.Content = "关闭";
            }
            else
            {
                if (server != null)
                {
                    server.Dispose();
                    server = null;
                }
                start.Content = "启动";
            }
        }
    }
}
