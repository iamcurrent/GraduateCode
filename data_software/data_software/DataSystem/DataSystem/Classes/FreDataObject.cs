using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSystem.Classes
{
    class FreDataObject
    {
       private List<double> data;
       private int selectChannel;
       private double startTime;
        public FreDataObject(List<double> data,int selectChannel,double startTime)
        {
            this.data = data;
            this.selectChannel = selectChannel;
            this.startTime = startTime;
        }

        public List<double> getData()
        {
            return this.data;
        }

        public int getChannel()
        {
            return this.selectChannel;
        }

        public double getStartTime()
        {
            return this.startTime;
        }

    }
}
