using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mGCS
{
    class FtConversion
    {
        protected double[] rawData;

        private LowPassFilter mLpf;

        //Constructor
        public FtConversion(LowPassFilter lpf) 
        {
            this.mLpf = lpf;
        }

        double[] convert(int[] ftData)
        {
            double[] convertedData = new double[3];
             //Convert Fx, Fy, Fz from [Count] to [N]
            convertedData[0] = 0.0061 * ftData[1];
            convertedData[1] = -0.0061 * ftData[2];
            convertedData[2] = 0.0061 * ftData[3];

            return convertedData;
        }

        public double[] normalize(int[] data)
        {
            double[] filteredData = new double[3];

            double[] tempData = this.convert(data);
            for (int i = 0; i <= 2; i++)
            {
                //filteredData[i] = mLpf.filter(tempData[i]);
                filteredData[i] = tempData[i];
            }
            return filteredData;
        }
    }
}
