using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using ScottPlot;
namespace ClientSystem.Classes
{
    class PlotObject
    {
        public Image image;
        public Label label;
        public WpfPlot ripple;
        public WpfPlot temperature;
        public WpfPlot em;
        public PlotObject(WpfPlot ripple,WpfPlot temperature,WpfPlot em)
        {
            this.ripple = ripple;
            this.temperature = temperature;
            this.em = em;
        }
    }
}
