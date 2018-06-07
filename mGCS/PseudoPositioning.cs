using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace mGCS
{
    class PseudoPositioning
    {
        //private static double sampling_time;
        private static double x_drag, y_drag, z_drag;
        private const double DEADZONE = 0.2;
        //double roll, pitch, yaw;
        private static double ax, ay, az, vx, vy, vz, x_estimate, y_estimate, z_estimate;

        public PseudoPositioning(double drag_x, double drag_y)
        {
            //sampling_time = samplingTime;
            x_drag = drag_x;
            y_drag = drag_y;

            vx  = 0;
            vy  = 0;
            vz  = 0;

            x_estimate = 0;
            y_estimate = 0;
            z_estimate = 0;
        }

        private double deadzone(double a, double deadzone)
        {
            if (a < deadzone && a > -deadzone) return 0;
            else return a;
        }

        public bool runEstimation(double accel_x, double accel_y, double accel_z, double samplingTime)
        {
            ax = deadzone(accel_x, DEADZONE);
            ay = deadzone(accel_y, DEADZONE);
            az = deadzone(accel_z, DEADZONE);

            if (ax == 0)
                vx = 0;
            else
            {
                vx += (ax - x_drag * vx) * samplingTime;
            }

            if (ay == 0)
                vy = 0;
            else
                vy += (ay - y_drag * vy) * samplingTime;

            if (az == 0)
                vz = 0;
            else
                vz += (az - z_drag * vz) * samplingTime;

            if (z_estimate <= 0 && vz < 0) vz = 0;

            x_estimate += vx * samplingTime;
            y_estimate += vy * samplingTime;
            z_estimate += vz * samplingTime;
            if (z_estimate < 0) z_estimate = 0;

            return true;
        }

        public void setDrag(double dragX, double dragY, double dragZ)
        {
            x_drag = dragX;
            y_drag = dragY;
            z_drag = dragZ;
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

        public double getVx()
        {
            return vx;
        }

        public double getVy()
        {
            return vy;
        }

        public double getVz()
        {
            return vz;
        }

        public double getAx()
        {
            return ax;
        }

        public double getAy()
        {
            return ay;
        }

        public double getAz()
        {
            return az;
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
