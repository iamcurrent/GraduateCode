using ClientSystem.Views;
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
using Fleck;
using System.Threading;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Security.Cryptography;
using ClientSystem.Classes;

namespace ClientSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        WebSocket webSocket = null;
        String login_key = null;
        RegisterView registerView = null;
        MainView mainView = null;

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
            }
            else if (msg.Equals("verify_yes"))
            {
                this.login_view.Dispatcher.BeginInvoke(new Action(() =>
                {
                    login_view.Visibility = Visibility.Collapsed;
                    mainView = new MainView(webSocket);
                    this.back_panel.Content = mainView;
                    this.menu.Visibility = Visibility.Visible;

                }));
            }
            else if (msg.Equals("no_user"))
            {
                MessageBox.Show("用户不存在");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitWebSocket();
            this.Closing += MainWindow_Closing;
            InitMqAndControl initMqAndControl = new InitMqAndControl(webSocket);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (webSocket != null)
            {
                webSocket.Close();
            }

            if (mainView != null)
            {
                mainView.closingWindows();
            }
        }

        public string RSAEncrypt(string passWord, string key)
        {
            //创建RSA对象并载入[公钥]
            RSACryptoServiceProvider rsaPublic = new RSACryptoServiceProvider();
            rsaPublic.FromXmlString(key);
            //对数据进行加密
            byte[] publicValue = rsaPublic.Encrypt(Encoding.UTF8.GetBytes(passWord), false);
            string publicStr = Convert.ToBase64String(publicValue);//使用Base64将byte转换为string

            return publicStr;
        }

        private void verify(string key, string userName, string password)
        {
            string publicStr = RSAEncrypt(password, key);
            User user = new User(userName, publicStr, "verify");
            string resposeMsg = JsonConvert.SerializeObject(user);
            webSocket.Send(resposeMsg);
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            string userName = this.userName.Text;
            string passWord = this.password.Text;
            verify(login_key, userName, passWord);
        }

        private void register_Click(object sender, RoutedEventArgs e)
        {
            if (registerView == null)
            {
                registerView = new RegisterView(webSocket);
            }
            registerView.ShowDialog();

        }

        private void login_out_Click(object sender,RoutedEventArgs e) 
        {
            this.login_view.Visibility = Visibility.Visible;
            this.menu.Visibility = Visibility.Collapsed;
            this.back_panel.Content = null;
            mainView = null;
            
        }
    }
}
