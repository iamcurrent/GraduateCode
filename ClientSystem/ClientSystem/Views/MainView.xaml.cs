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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebSocketSharp;


namespace ClientSystem.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl
    {
        PowerShowView powerShowView = null;
        DataView dataView = new DataView();
        WebSocket websocket = null;

        public MainView(WebSocket webSocket)
        {
            InitializeComponent();
            powerShowView = new PowerShowView(webSocket);
            this.showPanel.Content = powerShowView;
           
        }

        public void closingWindows()
        {
            powerShowView.closingWindows(); 
            
        }

        private void system_options_Click(object sender, RoutedEventArgs e)
        {
            this.showPanel.Content = powerShowView;
        }

        private void data_options_Click(object sender, RoutedEventArgs e)
        {
            this.showPanel.Content = dataView;
        }
    }
}
