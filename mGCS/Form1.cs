using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.IO.Ports;
using System.IO;

namespace mGCS
{
    public partial class mGCS : Form
    {
        public mGCS()
        {
            InitializeComponent();
            getAvailableComPorts();
        }

        //Variables declaration\

        private UserInputHelper mInputHelper;

        //GPS Sim. variables
        private static Socket   mClient;
        bool skSts1;
        private static byte[]   mPkg = new byte[512];
        private static string   gpsSimIP;
        private static int      gpsSimPort;
        private System.Windows.Forms.Timer mTimer;
        private TcpClient client = new TcpClient();
        private bool gpsConnected = false;

        //F/T sensor variables
        private bool ftConnected = false;
        private string ftCom;
        private string ftBaud;
        //private double fx, fy, fz, tx, ty, tz;
        double[] forces = new double[3];
        public double[] forceBias = new double[3];
        private string ftBuffer;
        private Thread ftDataReceiver;
        private string ftLoggingFile;
        private static TextWriter mTwriter;
        PositionMap mPosMap;
        LowPassFilter mLpf;
        PseudoPositioning mPsner;
        private double mVehicleMass = 2.3;
        //Vehicle variables
        private bool vehicleConnected = false;
        
        //Flight Sim. variables
        private bool fsConnected = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            mInputHelper = new UserInputHelper();

            pbarFzCalib.Visible = false;
            pbarFxyCalib.Visible = false;
            btnRefreshAll.Visible = false;
            pnlFtSetting.Visible = false;

            mTimer = new System.Windows.Forms.Timer();
            mTimer.Interval = 100; //100 mili-seconds
            mTimer.Tick += new System.EventHandler(mTimer_Tick);
            //mTimer.Start();

            //Initiate Lowpass filter
            mLpf = new LowPassFilter(2.0, 0.05);
            mPosMap = new PositionMap(chartPosSim);
            chartPosSim.Titles.Add("Vehicle's Position");
            mPsner = new PseudoPositioning(0.3, 0.28, 0.05);
        }
        
        private void tbxIP_TextChanged(object sender, EventArgs e)
        {
            gpsSimIP = tbxIP.Text;
        }

        private void tbxGpsSimPort_TextChanged(object sender, EventArgs e)
        {
            try
            {
                gpsSimPort = Convert.ToInt32(tbxGpsSimPort.Text);
            }
            catch { }
        }

        private void btnCntGpsSim_Click(object sender, EventArgs e)
        {
            mClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            if (!gpsConnected)
            {
                try
                {
                    IPEndPoint iep = new IPEndPoint(IPAddress.Parse(gpsSimIP), gpsSimPort);
                    mClient.BeginConnect(iep, new AsyncCallback(gpsSimConnected), mClient);
                    Thread.Sleep(10);
                    skSts1 = mClient.Connected;
                    //bool skSts1 = socketConnected(mClient);
                    btnCntGpsSim.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.btnDisConnect));
                    gpsConnected = true;
                }
                catch
                {
                    MessageBox.Show("IP address problem. \nConnection Failed!", "GPS Sim. Connection");
                }
            }
            else
            {
                try
                {
                    while (mTimer.Enabled)
                        try
                        {
                            mTimer.Stop();
                        }
                        catch { }
                    if (socketConnected(mClient))
                        mClient.Close();
                    MessageBox.Show("GPS Sim. disconnected", "GPS Sim. Connection");
                    btnCntGpsSim.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.btnConnect));
                    gpsConnected = false;
                }
                catch { }
            }
        }

        void gpsSimConnected(IAsyncResult iar)
        {
            try
            {
                mClient.EndConnect(iar);
                MessageBox.Show("GPS Sim. Connected (" + mClient.RemoteEndPoint.ToString() + ")", "GPS Sim. Connection"); 
                //Thread receiver = new Thread(new ThreadStart(receiveData));
                //Thread sender = new Thread(new ThreadStart(sendData));
                //mTimer.Start();
            }
            catch
            {
                MessageBox.Show("No response. \nConnection failed!", "GPS Sim. Connection");
            }
            return;
        }

        void receiveData()
        {
            //int receiver;
            //string strData;
            //while (true)
            //{
            //    receiver = mClient.Receive(mPkg);
            //    strData = Encoding.ASCII.GetString(mPkg, 0, receiver); 
            //    if (strData == "bye") break;
            //}
            //strData = "bye";
            //byte[] msg = Encoding.ASCII.GetBytes(strData);
            //mClient.Send(msg);
            //mClient.Close();
            //return;
        }

        void mTimer_Tick(object sender, EventArgs e)
        {
            const int INT_SIZE = 4, DOUBLE_SIZE = 8;
            byte[] msg = new byte[40];

            byte[] dataType = BitConverter.GetBytes((uint)3593);
            Array.Reverse(dataType, 0, dataType.Length);

            byte[] dataLen = BitConverter.GetBytes((uint)32);
            Array.Reverse(dataLen, 0, dataLen.Length);

            byte[] time = BitConverter.GetBytes((double)10.1), ecef_x = BitConverter.GetBytes((double)(-3121354.2511 + mPsner.getX())), ecef_y = BitConverter.GetBytes((double)(4085516.7232 + mPsner.getY())), ecef_z = BitConverter.GetBytes((double)3761774.7523);
            Array.Reverse(time, 0, time.Length);
            Array.Reverse(ecef_x, 0, ecef_x.Length);
            Array.Reverse(ecef_y, 0, ecef_y.Length);
            Array.Reverse(ecef_z, 0, ecef_z.Length);

            List<byte> dataTypeLst  = new List<byte>(dataType);
            List<byte> dataLenLst   = new List<byte>(dataLen);
            List<byte> timeLst      = new List<byte>(time);
            List<byte> ecef_xLst    = new List<byte>(ecef_x);
            List<byte> ecef_yLst    = new List<byte>(ecef_y);
            List<byte> ecef_zLst    = new List<byte>(ecef_z);

            dataTypeLst.AddRange(dataLenLst);
            dataTypeLst.AddRange(timeLst);
            dataTypeLst.AddRange(ecef_xLst);
            dataTypeLst.AddRange(ecef_yLst);
            dataTypeLst.AddRange(ecef_zLst);

            msg = dataTypeLst.ToArray();

            ////Other ways to merge the above byte arrays:
            ////The first way:
            //dataType.CopyTo(msg, dataType.Length);
            //dataLen.CopyTo(msg, dataLen.Length);
            //time.CopyTo(msg, time.Length);
            //ecef_x.CopyTo(msg, ecef_x.Length);
            //ecef_y.CopyTo(msg, ecef_y.Length);
            //ecef_z.CopyTo(msg, ecef_z.Length);

            ////The second way:
            //Buffer.BlockCopy(dataType, 0, msg, 0 * INT_SIZE, 1 * INT_SIZE);
            //Buffer.BlockCopy(dataLen, 0, msg, 1 * INT_SIZE, 1 * INT_SIZE);
            //Buffer.BlockCopy(time, 0, msg, 2 * INT_SIZE, 2 * INT_SIZE);
            //Buffer.BlockCopy(ecef_x, 0, msg, 4 * INT_SIZE, 2 * INT_SIZE);
            //Buffer.BlockCopy(ecef_y, 0, msg, 6 * INT_SIZE, 2 * INT_SIZE);
            //Buffer.BlockCopy(ecef_z, 0, msg, 8 * INT_SIZE, 2 * INT_SIZE);

            if (socketConnected(mClient))
                mClient.BeginSend(msg, 0, msg.Length, 0, new AsyncCallback(sendData), mClient);
            //return
        }

        private void mGCS_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dlgRslt = MessageBox.Show("Are you sure you want to quit?", "mGCS", MessageBoxButtons.YesNo);
            if (dlgRslt == DialogResult.No)
                e.Cancel = true;
            else
            {
                try
                {
                    if (socketConnected(mClient))
                        mClient.Close();
                }
                catch { }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!mTimer.Enabled)
            {
                mTimer.Start();
            }
            return;
        }

        void sendData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
            //return;
        }

        bool socketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2) return false;
            else return true;
        }

        private void btnDisConn_Click(object sender, EventArgs e)
        {
            try
            {
                if (socketConnected(mClient))
                    mClient.Close();
            }
            catch { }
        }

        /// <summary>
        ///  F/T sensor functions
        /// </summary>
        void getAvailableComPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            //Clear all the items in the lists then re-add
            cbxFtCom.Items.Clear();
            cbxFtCom.Items.AddRange(ports);
            cbxVhclCom.Items.Clear();
            cbxVhclCom.Items.AddRange(ports);
        }

        private void cbxFtCom_Click(object sender, EventArgs e)
        {
            getAvailableComPorts();
        }

        private void btnCntFt_Click(object sender, EventArgs e)
        {
            if (!ftSerialPort.IsOpen)
            {
                //If no COM port or no Baudrate selected, pop up a warning
                if (cbxFtCom.Text == "" || cbxFtBaud.Text == "")
                {
                    MessageBox.Show("Please fully choose a COM port and a Baudrate", "F/T sensor connection");
                }
                else //If both COM port and Baudrate selected then try to connect
                {
                    ftSerialPort.PortName = cbxFtCom.Text;
                    ftSerialPort.BaudRate = Convert.ToInt32(cbxFtBaud.Text);
                    try
                    {
                        //Open Serial Port
                        ftSerialPort.Open();

                        ////Change button's Icon
                        //btnCntFt.Image = ((System.Drawing.Image)(Properties.Resources.btnDisConnect));
                        ////MessageBox.Show("F/T sensor connected", "F/T sensor connection");

                        //Show Save-file Dialog for Logging FT data
                        SaveFileDialog sfDlg = new SaveFileDialog();
                        sfDlg.DefaultExt = "txt";
                        sfDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                        sfDlg.FilterIndex = 2;
                        sfDlg.RestoreDirectory = true;
                        sfDlg.Title = "Save F/T Logging File";
                        DialogResult dlgResult = sfDlg.ShowDialog();
                        if (dlgResult == DialogResult.OK && !string.IsNullOrWhiteSpace(sfDlg.FileName))
                        {
                            ftLoggingFile = sfDlg.FileName; 
                        }

                        //Launch a new Thread for FT data receiving
                        ftDataReceiver = new Thread(new ThreadStart(ftDataReceive));
                        //ftDataReceiver.Start();
                        mFtTimer.Start();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Unauthorized Access", "F/T sensor connection");
                    }
                }
            }
            else 
            {
                while (ftDataReceiver.IsAlive)
                    try
                    {
                        //Dismiss the Thread
                        ftDataReceiver.Suspend();
                    }
                    catch { }

                mFtTimer.Stop();

                while (ftSerialPort.IsOpen)
                    try
                    {
                        //Close the serial port
                        ftSerialPort.Close();
                    }
                    catch { }
                    
                //Return button's Icon
                btnCntFt.Image = ((System.Drawing.Image)(Properties.Resources.btnConnect));

                //Invisible Refresh button
                btnRefreshAll.Visible = false; ;

                //MessageBox.Show("F/T sensor disconnected", "F/T sensor connection");
            }
        }
        void ftDataReceive()
        {
            StreamWriter mStrmWrt;
            string[] ftDataStr;

            if (ftSerialPort.IsOpen)
            {
                //request code
                char requestCode = (char)20;
                
                try
                {
                    //just for debugging
                    this.Invoke((MethodInvoker)delegate()
                    {
                        lbxView.Items.Add("start");
                    });
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }

                //Send request to FT controller
                try
                {
                    ftSerialPort.WriteLine(requestCode.ToString());
                    Thread.Sleep(20);
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString(), "F/T sensor connection");
                }

                //Read FT data
                try
                    {
                        if (ftSerialPort.BytesToRead > 0)
                        {
                            ftBuffer = ftSerialPort.ReadLine();

                            //Just for Debugging
                            this.Invoke((MethodInvoker)delegate()
                            {
                                lbxView.Items.Add(ftBuffer);
                            });
                        }
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show("F/T data reading Timeout", "F/T sensor connection");
                    }

                //Check if F/T data is read appropriately
                    if (ftBuffer == null)
                    {
                        MessageBox.Show("F/T data reading Failed", "F/T sensor connection");
                        return;
                    }
                    else
                    {
                        //Change button's Icon
                        btnCntFt.Image = ((System.Drawing.Image)(Properties.Resources.btnDisConnect));

                        //Visible Refresh Button
                        btnRefreshAll.Visible = true;

                        //MessageBox.Show("F/T sensor connected", "F/T sensor connection");
                    }

                    //just for debuging
                    
                    //mPsner.runEstimation(0.01, 0.1, 1);

                    try
                    {
                    //string[] ftDataStr = { "" };
                    //if (ftBuffer.Length > 0)
                    //{
                        ftDataStr = ftBuffer.Split(new[] { "," }, StringSplitOptions.None);
                        int[] ftData = Array.ConvertAll(ftDataStr, s => int.Parse(s));
                        try
                        {
                            //Processing ftData
                            FtConversion ftConverter = new FtConversion(mLpf);
                            forces = ftConverter.normalize(ftData);
                    
                            //Run the estimator of the Pseudo Positioner
                            mPsner.runEstimation(forces[0] / mVehicleMass, forces[1] / mVehicleMass, forces[2] / mVehicleMass);
                            //mPsner.runEstimation(forces[0], forces[1], forces[2]);
                        }
                        catch (Exception x)
                        {
                            MessageBox.Show(x.Message.ToString());
                        }

                        if (!string.IsNullOrEmpty(ftLoggingFile))
                        {
                            //Open file to append text
                            mStrmWrt = File.AppendText(ftLoggingFile);

                            //stable
                            Thread.Sleep(10);

                            mStrmWrt.Write((Math.Round(mPsner.getX(), 2)).ToString().TrimEnd('0') + "\t");
                            mStrmWrt.Write((Math.Round(mPsner.getY(), 2)).ToString().TrimEnd('0') + "\t");
                            mStrmWrt.Write((Math.Round(mPsner.getZ(), 2)).ToString().TrimEnd('0') + "\t");

                            foreach (double d in forces)
                            {
                                mStrmWrt.Write((Math.Round(d, 2)).ToString().TrimEnd('0') + "\t");
                            }

                            //Write data into text file
                            if (ftDataStr.Length > 0)
                                foreach (string str in ftDataStr)
                                {
                                    //Write "\t" following each element of ftDataStr
                                    if (str.GetEnumerator().MoveNext())
                                        mStrmWrt.Write(str + "\t");
                                    else
                                    {
                                        //Write new line after fully writting ftDataStr
                                        //string lastStr = str.Replace("\r", string.Empty);
                                        //mStrmWrt.WriteLine(lastStr);
                                        mStrmWrt.Write(str);
                                        //mStrmWrt.WriteLine(str);
                                    }
                                }
                            
                            //mStrmWrt.WriteLine();

                            //Close stream writter after appending text
                            mStrmWrt.Close();
                        }
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
                
                //Update data visualization onto the Chart
                try
                {
                    mPosMap.update(mPsner.getX(), mPsner.getY());
                    //mPosMap.update(-1, -2);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "Chart error");
                }
            }
        }

        private void mFtTimer_Tick(object sender, EventArgs e)
        {
            ftDataReceive();
        }

        private void lblCntFt_Click(object sender, EventArgs e)
        {
            pnlFtSetting.Visible = true;
        }

        private void btnRefreshAll_Click(object sender, EventArgs e)
        {
            mPsner.restart();
            mPosMap.refresh();
        }

        private void btnCancelFtSetting_Click(object sender, EventArgs e)
        {
            pnlFtSetting.Visible = false;
        }

        private void btnApplyFtSetting_Click(object sender, EventArgs e)
        {
            try
            {
                mVehicleMass = Convert.ToDouble(tbxVehicleMass.Text);
            }
            catch (Exception x)
            { }

            pnlFtSetting.Visible = false;
        }

        private void btnFzCalib_Click(object sender, EventArgs e)
        {
            pbarFzCalib.Visible = true;
        }

        private void tbxVehicleMass_TextChanged(object sender, EventArgs e)
        {
        }

        private void tbxVehicleMass_KeyPress(object sender, KeyPressEventArgs e)
        {
            mInputHelper.pressNumberOnly(sender, e);
        }

        private void tbxGpsSimPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            mInputHelper.pressNumberOnly(sender, e);
        }

    }
}
