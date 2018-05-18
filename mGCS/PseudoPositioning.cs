using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace mGCS
{
    class PseudoPositioning
    {
        private static double sampling_time, x_drag, y_drag;

        //double roll, pitch, yaw;
        private static double ax, ay, az, vx, vy, vz, x_estimate, y_estimate, z_estimate;

        public PseudoPositioning(double drag_x, double drag_y, double samplingTime)
        {
            sampling_time = samplingTime;
            x_drag = drag_x;
            y_drag = drag_y;

            vx  = 0;
            vy  = 0;
            vz  = 0;
            x_estimate = 0;
            y_estimate = 0;
            z_estimate = 0;
        }

        public bool runEstimation(double ax, double ay, double az)
        {
            vx += (ax - x_drag * vx) * sampling_time;
            vy += (ay - y_drag * vy) * sampling_time;
            vz += az * sampling_time;

            x_estimate += vx * sampling_time;
            y_estimate += vy * sampling_time;
            z_estimate += vz * sampling_time;
            return true;
        }

        public double getX()
        {
            return x_estimate;
        }

        public double getY()
        {
            return y_estimate;
        }

        public double getZ()
        {
            return z_estimate;
        }

        public bool restart()
        {
            try
            {
                vx = 0;
                vy = 0;
                vz = 0;
                x_estimate = 0;
                y_estimate = 0;
                z_estimate = 0;
                return true;
            }
            catch 
            {
                return false;
            }
        }

        private double deg2rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        private double rad2deg(double rad)
        {
            return rad * 180 / Math.PI;
        }
    }
}
