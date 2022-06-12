using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace DataSystem.Views
{
    /// <summary>
    /// BigPictureView.xaml 的交互逻辑
    /// </summary>
    public partial class BigPictureView : UserControl
    {

        private bool flag = false;
        public BigPictureView()
        {
            InitializeComponent();
            plotScott.plt.Title("实时波形");
            
            plotScott.plt.XLabel("时间(s)",fontSize:25);
            plotScott.plt.YLabel("幅值",fontSize:25);
        }

        public void setData(List<double> data, double startTime, int channel)
        {
            if (!flag)
            {
                plotScott.plt.Clear();
                plotScott.plt.PlotSignal(data.ToArray(), UserDef.Frequency, startTime, label: String.Format("通道 {0}", channel));
                double max_value = data.Max();
                double min_value = data.Min();
                double res = (max_value - min_value) / 2 * 8 * 220/Math.Sqrt(2);
                label.Content = res.ToString();
                Thread.Sleep(100);
                plotScott.plt.Legend();
                plotScott.Render();
            }
        }

        private void start_stop_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            if (b.Content.Equals("暂停"))
            {
                flag = true;
                b.Content = "重启";
            }
            else
            {
                flag = false;
                b.Content = "暂停";
            }
        }
    }
}
