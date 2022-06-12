using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClientSystem.Classes
{
    class ViewObject
    {
        public StackPanel stackPanel;
        public PlotObject plotObject;
        public TextBox textBox;
        public ViewObject(StackPanel stackPanel,PlotObject plotObject)
        {
            this.plotObject = plotObject;
            this.stackPanel = stackPanel;
        }
    }
}
