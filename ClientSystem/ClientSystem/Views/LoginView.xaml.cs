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
using WebSocketSharp;
using System.Security.Cryptography;
using ClientSystem.Classes;
using Newtonsoft.Json;
using System.Threading;
namespace ClientSystem.Views
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : UserControl
    {
        WebSocket webSocket = null;
        String login_key = null;
        public static bool login_flag = false;
        private void InitWebSocket()
        {
            webSocket = new WebSocket("ws://192.168.1.37:50000");
            webSocket.Connect();
            webSocket.OnMessage += (sender, e) => MessageHandler(e.Data);
            webSocket.Send("Login");
        }


        public void MessageHandler(String msg)
        {
            if (msg.Contains("Login_key"))
            {
                String[] strs = msg.Split(':');
                login_key = strs[1];
                Console.WriteLine(login_key);
            }else if (msg.Equals("verify_yes"))
            {
                login_flag = true;
            }else if (msg.Equals("no_user"))
            {
                MessageBox.Show("用户不存在");
            }
        }
        public LoginView()
        {
            InitializeComponent();
            InitWebSocket();
        }

        //加密函数
        public string RSAEncrypt(string passWord,string key)
        {
            //创建RSA对象并载入[公钥]
            RSACryptoServiceProvider rsaPublic = new RSACryptoServiceProvider();
            rsaPublic.FromXmlString(key);
            //对数据进行加密
            byte[] publicValue = rsaPublic.Encrypt(Encoding.UTF8.GetBytes(passWord), false);
            string publicStr = Convert.ToBase64String(publicValue);//使用Base64将byte转换为string
           
            return publicStr;
        }

        private void verify(string key,string userName,string password)
        {
            string publicStr = RSAEncrypt(password, key);
            User user = new User(userName,publicStr,"verify");
            string resposeMsg = JsonConvert.SerializeObject(user);
            webSocket.Send(resposeMsg);
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            string userName = this.userName.Text;
            string passWord = this.password.Text;
            verify(login_key, userName, passWord);
        }
    }
}
