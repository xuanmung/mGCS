using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace mGCS
{
    class PositionMap
    {
        private static Chart mChart;
        public PositionMap(Chart chart)
        {
            mChart = chart;
            mChart.Series[0].IsVisibleInLegend = false;
        }

        public bool update(double x, double y)
        {
            try
            {
                //mChart.ChartAreas[0].AxisX.ScaleView.Zoom(-10, 10);
                //mChart.ChartAreas[0].AxisY.ScaleView.Zoom(-10, 10);
                mChart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                mChart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                mChart.ChartAreas[0].AxisX.Minimum = -500;
                mChart.ChartAreas[0].AxisX.Maximum = 500;
                mChart.ChartAreas[0].AxisY.Minimum = -500;
                mChart.ChartAreas[0].AxisY.Maximum = 500;

                mChart.ChartAreas[0].CursorX.IsUserEnabled = true;
                mChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                mChart.ChartAreas[0].CursorY.IsUserEnabled = true;
                mChart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                mChart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Gray;
                mChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Gray;

                mChart.Series[0].BorderWidth = 2;
                mChart.Series[0].Color = Color.Red;
                //mChart.Series[0].IsVisibleInLegend = false;
                //mChart.Series[0]["PieLabelStyle"] = "Disabled";

                mChart.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

                mChart.Series[0].Points.AddXY(x, y);

                return true;
            }
            catch (Exception mEx)
            {
                return false;
            }
        }

        public bool refresh()
        {
            try
            {
                mChart.Series[0].Points.Clear();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
