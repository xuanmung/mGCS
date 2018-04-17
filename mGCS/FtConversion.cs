﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mGCS
{
    class FtConversion
    {
        private double[] rawData, convertedData, filteredData;

        private LowPassFilter mLpf;

        public FtConversion() { }

        double[] convert(int[] data)
        {
             //Convert Fx, Fy, Fz from [Count] to [N]
            convertedData[0] = 0.0061 * data[0];
            convertedData[1] = 0.0061 * data[1];
            convertedData[2] = 0.0061 * data[2];

            return convertedData;
        }

        public double[] normalize(int[] data)
        {
            double[] tempData = this.convert(data);
            for (int i = 0; i <= 2; i++)
            {
                filteredData[i] = mLpf.filter(tempData[i]);
            }
            return filteredData;
        }
    }
}