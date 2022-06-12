using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
using DataSystem.Views;
using ScottPlot;

namespace DataSystem
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void MsgUpdateDelegate(String msg);
        private Dev2991View dev2991View = new Dev2991View();
        private THView tH_View = new THView();
        private int clickChannel = 0;
        public MainWindow()
        {
            InitializeComponent();
            net_2991.Click += Net_2991_Click;
            contentControl.Content = dev2991View;

        }


        private void ConnetionRabbitMq(ConnectionFactory connectionFactory, IConnection connection)
        {
            try
            {
                connectionFactory = new ConnectionFactory();
                connectionFactory.HostName = "localhost";
                connectionFactory.UserName = "admin";
                connectionFactory.Password = "123456";
                connectionFactory.VirtualHost = "/";//默认情况可省略此行
                connectionFactory.Port = 5672;
                connection = connectionFactory.CreateConnection();
            }catch(Exception ex)
            {

            }
        }

        private void Net_2991_Click(object sender, RoutedEventArgs e)
        {
            dev2991View.oriAllPlot.Visibility = Visibility.Visible;
            dev2991View.changeSelect();
            contentControl.Content = dev2991View;

        }

        private void Wpf_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WpfPlot w = (WpfPlot)sender;
            String str = w.Name;
            clickChannel = Convert.ToInt32(str.Substring(7));
        }

        private void net_2991_Click_1(object sender, RoutedEventArgs e)
        {

            contentControl.Content = dev2991View;
            dev2991View.changeSelect();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            dev2991View.CloseWebSocket();
            dev2991View.closeWindow();
        }

        private void monitorControl_Click(object sender, RoutedEventArgs e)
        {
            MonitorView serialPortView = new MonitorView();
            serialPortView.Show();
        }

        private void make_data_collection_Click(object sender, RoutedEventArgs e)
        {
            DataCollectionView dataCollectionView = new DataCollectionView();
            dataCollectionView.Show();
        }

        private void tHView_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = tH_View;
        }

        private void Chrome_Click(object sender, RoutedEventArgs e)
        {
            ChromeDevice chromeDevice = new ChromeDevice();
            chromeDevice.Show();
        }
    }
}
