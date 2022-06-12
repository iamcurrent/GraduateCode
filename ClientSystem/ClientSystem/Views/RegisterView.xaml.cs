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
using System.Windows.Shapes;
using WebSocketSharp;
using Newtonsoft.Json;
using ClientSystem.Classes;

namespace ClientSystem.Views
{
    /// <summary>
    /// RegisterView.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterView : Window
    {
        WebSocket webSocket = null;
        public RegisterView(WebSocket socket)
        {
            InitializeComponent();
            webSocket = socket;
            webSocket.OnMessage += (sender, e) => MessageHandler(e.Data);
        }

        public void MessageHandler(String msg)
        {
            if (msg.Equals("register_yes"))
            {
                this.Close();
            }
        }

        private void register_Click(object sender, RoutedEventArgs e)
        {
            String user_name = this.userName.Text;
            String pass_word = this.passWord.Text;
            User user = new User(user_name,pass_word,"register");

            string json = JsonConvert.SerializeObject(user);
            webSocket.Send(json);
        }
    }
}
