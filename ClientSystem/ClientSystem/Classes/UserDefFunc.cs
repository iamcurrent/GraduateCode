using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFT;
using MathWorks.MATLAB.NET.Arrays;

namespace ClientSystem.Classes
{
    class UserDefFunc
    {

        public static Double[] computeFFT(FFT.Class1 fftClass,Double[] data, int low, int countPoint)
        {

            MWNumericArray input = (MWNumericArray)data;
            MWArray[] ots = null;
            ots = fftClass.FFT(1, (MWArray)input);
            double[,] resdata = (double[,])ots[0].ToArray();
            double[] usedata = new double[(int)(resdata.GetLength(1) / 2)];
            for (int j = 0; j < usedata.Length; j++)
            {
                usedata[j] = resdata[0, j];
            }

            usedata[0] = 0;
            return usedata;

        }





    }
}
