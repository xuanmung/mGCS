using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace mGCS
{
    class LowPassFilter
    {
        //Primary variables
        private static double   cutoff_freq, sampling_time, filtered_datum = 0;

        //Secondary variables
        private static double   time_constant, filter_weight;

        double current_datum;

        //Constructor
        public LowPassFilter(double cutoffFrequency, double samplingTime)
        {
            cutoff_freq = cutoffFrequency;
            sampling_time  = samplingTime;

            try
            {
                time_constant = 1 / (2 * Math.PI * cutoff_freq);
                filter_weight = time_constant / (time_constant + sampling_time);
            }
            catch (DivideByZeroException e)
            {
                //Show Divided by Zero error if any
                MessageBox.Show(e.Message.ToString(), "F/T sensor processing");
            }
        }

        public double filter(double data)
        {
            current_datum   = data;
            filtered_datum  = filter_weight * filtered_datum + (1 - filter_weight) * current_datum;

            return filtered_datum;
        }

        public double getCutoffFrequency()
        {
            return cutoff_freq;
        }

        public double getSamplingTime()
        {
            return sampling_time;
        }
    }
}
