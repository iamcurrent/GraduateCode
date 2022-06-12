using Neuronic.TimeFrequency;
using Neuronic.TimeFrequency.Transforms;
using Neuronic.TimeFrequency.Wavelets;
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
    /// DwtConvertView.xaml 的交互逻辑
    /// </summary>
    public partial class DwtConvertView : UserControl
    {
        private bool startFlag = false;
        private int level = 0;
        private int showType = 1;
        private bool stop = true;
        public DwtConvertView()
        {
            InitializeComponent();
            levelNum.TextChanged += LevelNum_TextChanged;
            dwtPlot.plt.Title("小波分解图");

        }

        Thread thread = null;
        public void startDwtThread()
        {
            this.startFlag = true;
            thread = new Thread(new ThreadStart(doDwtWork));
            thread.Start();
        }

        public void setFlag()
        {
            this.startFlag = false;
        }




        private void doDwtWork()
        {
            while (startFlag)
            {
                if (!stop)
                {
                    Console.WriteLine("DWT");
                    List<double> data;
                    bool flag = UserDef.syncQueDwt.TryTake(out data);
                    if (flag && data != null)
                    {
                        dwtPlot.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DWT(data);
                        }));


                    }
                }

            }

        }

        private void LevelNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                level = Convert.ToInt32(levelNum.Text);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private List<Double> changeValue(List<Double> list, double v, int level)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list.ElementAt(i) + v * level;
            }

            return list;
        }


        private void showGraph(DiscreteWaveletTransform rs, int level, int flag)
        {
            if (level >= 0 && flag == 1)
            {

                dwtPlot.plt.PlotSignal(changeValue(rs.Detail.ToList<Double>(), 0.1, level).ToArray(), label: "第" + level + "层的细节");
                showGraph(rs.UpperScale, level - 1, flag);

            }
            else if (level >= 0 && flag == 2)
            {
                dwtPlot.plt.PlotSignal(changeValue(rs.Approximation.ToList<Double>(), 0.1, level).ToArray(), label: "第" + level + "层的概貌");

                showGraph(rs.UpperScale, level - 1, flag);
            }

        }



        private void DWT(List<double> data)
        {

            Signal<Double> sig = new Signal<double>(data.ToArray());
            dwtPlot.plt.Clear();
            oriSignal.plt.Clear();
            oriSignal.plt.PlotSignal(data.ToArray(), UserDef.Frequency);
            try
            {
                DiscreteWaveletTransform rs = DiscreteWaveletTransform.Estimate(sig, Wavelets.Daubechies(2), new ZeroPadding<Double>());
                DiscreteWaveletTransform rs1 = rs.EstimateMultiscale(new ZeroPadding<Double>(), level);

                if (showType == 1)
                {
                    showGraph(rs1, level, showType);
                }
                else
                {
                    showGraph(rs1, level, showType);
                }
                dwtPlot.plt.Legend();
                oriSignal.plt.Legend();
                oriSignal.Render();
                dwtPlot.Render();

            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }


        }



        private void de_ap_btn_Click(object sender, RoutedEventArgs e)
        {
            if (de_ap_btn.Content.ToString().Equals("细节"))
            {
                showType = 1;
                de_ap_btn.Content = "概貌";
            }
            else
            {
                showType = 2;
                de_ap_btn.Content = "细节";
            }

        }

        private void st_sp_btn_Click(object sender, RoutedEventArgs e)
        {
            if (st_sp_btn.Content.ToString().Equals("暂停"))
            {
                stop = true;
                st_sp_btn.Content = "开始";

            }
            else
            {
                stop = false;
                st_sp_btn.Content = "暂停";
            }
        }
    }
}
