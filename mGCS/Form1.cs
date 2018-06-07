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
        private const string mParamFileName = "params.txt";

        //GPS Sim. variables
        private static Socket   mClient;
        bool skSts1;
        private static byte[]   mPkg = new byte[512];
        private static string   gpsSimIp;
        private static int      gpsSimPort;
        private System.Windows.Forms.Timer mTimer;
        double mTimerChecker = 0;
        private TcpClient client = new TcpClient();
        private bool gpsConnected = false;
        private const double initEcefX = -3052685.2;
        private const double initEcefY = 4040266.6;
        private const double initEcefZ = 3866655.3;
        private const double initLon = 127.073428630829;
        private const double initLat = 37.5503616346548;
        private double initLonRad;
        private double initLatRad;
        private MatrixHelper mMatrixHelper;
        private double[][] mRotationMatrixEcef2Ned;
        private double[][] mRotationMatrixNed2Ecef;
        private double[][] posEcef, posEcefRef, posNed;

        //F/T sensor variables
        private bool ftConnected = false;
        private string ftComPort;
        private int ftBaudRate;
        private StreamWriter mStrmWrt;
        private string[] ftDataStr;
        //private double fx, fy, fz, tx, ty, tz;
        private double[] forces     = new double[3];
        public double[] forceBias   = new double[3];
        public double[] forceBiasCandidate = new double[3];
        private bool isFzCalibrating  = false;
        private bool isFxyCalibrating = false;
        private bool isMassCalibrating = false;
        private Thread ftCalibThread;
        private int numOfCalibSample = 100;
        private int calibStep = 0;
        private string ftBuffer;
        private Thread ftDataReceiver;
        private string ftLoggingFile;
        private static TextWriter mTwriter;
        private static PositionMap mPosMap;
        private static Thread updatePosMapThread;
        private static LowPassFilter mLpf;
        //private static DateTime mDateTime;
        private long mTimePoint;
        private double mFtSamplingTime = 0;
        private static PseudoPositioning mPsner;
        private double mVehicleMass;
        private double mVehicleMassCandidate;
        private const double GRAVITY = 9.81;
        private double[] drag = new double[2];
        private double[] estimatedPos = new double[3];
        private double[] estimatedVel = new double[3];
        private double[] estimatedAccel = new double[3];

        //Vehicle variables
        private bool vehicleConnected = false;
        private string vclComPort;
        private int vclBaudRate;
        
        //Flight Sim. variables
        private bool fsConnected = false;
        private string fsIp;

        private void Form1_Load(object sender, EventArgs e)
        {
            mInputHelper = new UserInputHelper();

            mMatrixHelper = new MatrixHelper();

            //Convert lon and lat from [deg] to [rad]
            initLonRad = mMatrixHelper.deg2rad(initLon);
            initLatRad = mMatrixHelper.deg2rad(initLat);

            //Calculate rotation matrix from ECEF to NED
            mRotationMatrixEcef2Ned = mMatrixHelper.MatrixCreate(3,3);

            mRotationMatrixEcef2Ned[0][0] = -Math.Sin(initLatRad) * Math.Cos(initLonRad);
            mRotationMatrixEcef2Ned[0][1] = -Math.Sin(initLatRad) * Math.Sin(initLonRad);
            mRotationMatrixEcef2Ned[0][2] = Math.Cos(initLatRad);

            mRotationMatrixEcef2Ned[1][0] = -Math.Sin(initLonRad);
            mRotationMatrixEcef2Ned[1][1] = Math.Cos(initLonRad);
            mRotationMatrixEcef2Ned[1][2] = 0;

            mRotationMatrixEcef2Ned[2][0] = -Math.Cos(initLatRad) * Math.Cos(initLonRad);
            mRotationMatrixEcef2Ned[2][1] = -Math.Cos(initLatRad) * Math.Sin(initLonRad);
            mRotationMatrixEcef2Ned[2][2] = -Math.Sin(initLatRad);



            //mRotationMatrixEcef2Ned[0][0] = -Math.Sin(initLonRad);
            //mRotationMatrixEcef2Ned[0][1] = Math.Cos(initLonRad);
            //mRotationMatrixEcef2Ned[0][2] = 0;

            //mRotationMatrixEcef2Ned[1][0] = -Math.Sin(initLatRad) * Math.Cos(initLonRad);
            //mRotationMatrixEcef2Ned[1][1] = -Math.Sin(initLatRad) * Math.Sin(initLonRad);
            //mRotationMatrixEcef2Ned[1][2] = Math.Cos(initLatRad);

            //mRotationMatrixEcef2Ned[2][0] = Math.Cos(initLatRad) * Math.Cos(initLonRad);
            //mRotationMatrixEcef2Ned[2][1] = Math.Cos(initLatRad) * Math.Sin(initLonRad);
            //mRotationMatrixEcef2Ned[2][2] = Math.Sin(initLatRad);

            //Calculate rotation matrix from NED to ECEF
            mRotationMatrixNed2Ecef = mMatrixHelper.MatrixCreate(3, 3);
            try
            {
                mRotationMatrixNed2Ecef = mMatrixHelper.MatrixInverse(mRotationMatrixEcef2Ned);
            }
            catch
            {
                MessageBox.Show("Coordinate transform failed");
            }

            //Declare the ECEF reference postion
            posEcefRef = mMatrixHelper.MatrixCreate(3, 1);
            posEcefRef[0][0] = initEcefX;
            posEcefRef[1][0] = initEcefY;
            posEcefRef[2][0] = initEcefZ;

            //Init the ECEF reference postion
            posEcef = mMatrixHelper.MatrixCreate(3, 1);
            posNed = mMatrixHelper.MatrixCreate(3, 1);

            //Inquiry parameters from saved internal file (if any)
            try
            {
                if (File.Exists(mParamFileName))
                {
                    StreamReader streamRdr = new StreamReader(mParamFileName);

                    ftComPort = streamRdr.ReadLine();
                    ftBaudRate = Convert.ToInt32(streamRdr.ReadLine());

                    vclComPort = streamRdr.ReadLine();
                    vclBaudRate = Convert.ToInt32(streamRdr.ReadLine());

                    gpsSimIp = streamRdr.ReadLine();
                    gpsSimPort = Convert.ToInt32(streamRdr.ReadLine());

                    fsIp = streamRdr.ReadLine();

                    mVehicleMass = Convert.ToDouble(streamRdr.ReadLine());
                    drag[0] = Convert.ToDouble(streamRdr.ReadLine());
                    drag[1] = Convert.ToDouble(streamRdr.ReadLine());

                    forceBias[0] = Convert.ToDouble(streamRdr.ReadLine());
                    forceBias[1] = Convert.ToDouble(streamRdr.ReadLine());
                    forceBias[2] = Convert.ToDouble(streamRdr.ReadLine());

                    streamRdr.Close();

                    cbxFtComPort.Text = ftComPort;
                    cbxFtBaudRate.Text = ftBaudRate.ToString();

                    cbxVclComPort.Text = vclComPort;
                    cbxVclBaudRate.Text = vclBaudRate.ToString();

                    tbxGpsSimIP.Text = gpsSimIp;
                    tbxGpsSimPort.Text = gpsSimPort.ToString();
                    tbxFsIp.Text = fsIp;

                    lblMass.Text = mVehicleMass.ToString();
                    lblDragX.Text = drag[0].ToString();
                    lblDragY.Text = drag[1].ToString();
                    tbxDragX.Text = drag[0].ToString();
                    tbxDragY.Text = drag[1].ToString();

                    lblFx0.Text = forceBias[0].ToString();
                    lblFy0.Text = forceBias[1].ToString();
                    lblFz0.Text = forceBias[2].ToString();
                }
                else
                {
                    //gpsSimIp = tbxGpsSimIP.Text;
                    //gpsSimPort = Convert.ToInt32(tbxGpsSimPort.Text);
                    //fsIp = tbxFsIp.Text;                    
                }
            }
            catch (Exception x)
            { }

            pbarFzCalib.Visible = false;
            pbarFxyCalib.Visible = false;
            btnRefreshAll.Visible = false;
            pnlFtSetting.Visible = false;

            mTimer = new System.Windows.Forms.Timer();
            mTimer.Interval = 100; //100 mili-seconds
            mTimer.Tick += new System.EventHandler(mTimer_Tick);
            //mTimer.Start();

            mTimePoint = 0;

            //Initiate Lowpass filter
            mLpf = new LowPassFilter(2.0, mFtSamplingTime);

            //Assign position map
            mPosMap = new PositionMap(chartPosSim);
            mPosMap.setTitle("Vehicle's Position");
            //chartPosSim.Titles.Add("Vehicle's Position");

            updatePosMapThread = new Thread(new ThreadStart(updatePosMap)); 

            //mPsner = new PseudoPositioning(0.3, 0.28);
            mPsner = new PseudoPositioning(drag[0], drag[1]); 
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
                    /***
                     * Save parameters into an internal file
                     * The order of these parameter should NOT be changed
                    ***/
                    StreamWriter mStreamWrt = new StreamWriter("params.txt"); 
                    
                    mStreamWrt.WriteLine(ftComPort);
                    mStreamWrt.WriteLine(ftBaudRate.ToString());

                    mStreamWrt.WriteLine(vclComPort);
                    mStreamWrt.WriteLine(vclBaudRate.ToString());

                    mStreamWrt.WriteLine(gpsSimIp);
                    mStreamWrt.WriteLine(gpsSimPort.ToString());

                    mStreamWrt.WriteLine(fsIp);

                    mStreamWrt.WriteLine(Math.Round(mVehicleMass, 2).ToString());
                    mStreamWrt.WriteLine(Math.Round(drag[0], 2).ToString());
                    mStreamWrt.WriteLine(Math.Round(drag[1], 2).ToString());

                    mStreamWrt.WriteLine(Math.Round(forceBias[0], 2).ToString());
                    mStreamWrt.WriteLine(Math.Round(forceBias[1], 2).ToString());
                    mStreamWrt.WriteLine(Math.Round(forceBias[2], 2).ToString());

                    mStreamWrt.Close(); 

                    if (socketConnected(mClient))
                        mClient.Close(); 
                }
                catch { }
            }
        }

        private void tbxIP_TextChanged(object sender, EventArgs e)
        {
            gpsSimIp = tbxGpsSimIP.Text;
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
                    IPEndPoint iep = new IPEndPoint(IPAddress.Parse(gpsSimIp), gpsSimPort);
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
            //mTimerChecker = (DateTime.Now.Millisecond - milisecondTimePoint) * 0.001;
            //milisecondTimePoint = DateTime.Now.Millisecond;
            const int INT_SIZE = 4, DOUBLE_SIZE = 8;
            byte[] msg = new byte[40];

            byte[] dataType = BitConverter.GetBytes((uint)3593);
            Array.Reverse(dataType, 0, dataType.Length);

            byte[] dataLen = BitConverter.GetBytes((uint)32);
            Array.Reverse(dataLen, 0, dataLen.Length);

            //Assign NED position
            posNed[0][0] = mPsner.getX();
            posNed[1][0] = mPsner.getY();
            posNed[2][0] = -mPsner.getZ();

            //Convert pseudo position from NED to ECEF
            posEcef = mMatrixHelper.MatrixAdd(mMatrixHelper.MatrixProduct(mRotationMatrixNed2Ecef, posNed), posEcefRef); 

            //byte[] time = BitConverter.GetBytes((double)10.1), ecef_x = BitConverter.GetBytes((double)(-3121354.2511 + mPsner.getX())), ecef_y = BitConverter.GetBytes((double)(4085516.7232 + mPsner.getY())), ecef_z = BitConverter.GetBytes((double)3761774.7523);
            byte[] time = BitConverter.GetBytes((double)10.1), ecef_x = BitConverter.GetBytes(posEcef[0][0]), ecef_y = BitConverter.GetBytes(posEcef[1][0]), ecef_z = BitConverter.GetBytes(posEcef[2][0]);

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
            cbxFtComPort.Items.Clear();
            cbxFtComPort.Items.AddRange(ports);
            cbxVclComPort.Items.Clear();
            cbxVclComPort.Items.AddRange(ports);
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
                if (cbxFtComPort.Text == "" || cbxFtBaudRate.Text == "")
                {
                    MessageBox.Show("Please fully choose a COM port and a Baudrate", "F/T sensor connection");
                }
                else //If both COM port and Baudrate selected then try to connect
                {
                    ftSerialPort.PortName = ftComPort; // cbxFtComPort.Text;
                    ftSerialPort.BaudRate = ftBaudRate;
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
                    catch (IOException)
                    {
                        MessageBox.Show("There is no comport: " + ftComPort, "F/T sensor connection");
 
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Unauthorized Access", "F/T sensor connection");
                    }
                }
            }
            else 
            {
                ftVisualizationTimer.Stop();

                ftPositioningTimer.Stop();

                //while (ftDataReceiver.IsAlive)
                //    try
                //    {
                //        //Dismiss the Thread
                //        ftDataReceiver.Suspend();
                //    }
                //    catch { }
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
                ftConnected = false;

                //Invisible Refresh button
                btnRefreshAll.Visible = false; ;

                //MessageBox.Show("F/T sensor disconnected", "F/T sensor connection");
            }
        }
        void ftDataReceive()
        {
            if (ftSerialPort.IsOpen)
            {
                //request code
                char requestCode = (char)20;
                
                try
                {
                    ////just for debugging
                    //this.Invoke((MethodInvoker)delegate()
                    //{
                    //    lbxView.Items.Add("start");
                    //});
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }

                //Send request to FT controller
                try
                {
                    //while (ftBuffer == null)
                    //{
                        ftSerialPort.WriteLine(requestCode.ToString()); 
                        //Thread.Sleep(1);
                        //if (ftSerialPort.BaseStream == null) { Thread.Sleep(10); };
                    //}
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
                            //while (ftBuffer == null) 
                            ftBuffer = ftSerialPort.ReadLine(); 

                            ////Just for Debugging
                            //this.Invoke((MethodInvoker)delegate()
                            //{
                            //    if (lbxView.Items.Count > 10) lbxView.Items.Clear();
                            //    lbxView.Items.Add(ftBuffer);
                            //});
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
                        if (!ftConnected)
                        {
                            //Change button's Icon
                            btnCntFt.Image = ((System.Drawing.Image)(Properties.Resources.btnDisConnect));
                            ftConnected = true;

                            //Visible Refresh Button
                            btnRefreshAll.Visible = true;

                            ftPositioningTimer.Start();
                            ftVisualizationTimer.Start();
                        }
                        //MessageBox.Show("F/T sensor connected", "F/T sensor connection");
                    }
                
                //DO FT DATA PROCESSING
                ftDataProcess(); 
            }
        }

        private void ftVisualize()
        {
            //Just for Debugging
            this.Invoke((MethodInvoker)delegate()
            {
                if (lbxView.Items.Count > 10) 
                    lbxView.Items.Clear();

                //if (ftBuffer.IsNormalized()) 
                //    lbxView.Items.Add(ftBuffer);

                lbxView.Items.Add(mFtSamplingTime.ToString());
            });

            //Assign values
            estimatedPos[0] = mPsner.getX();
            estimatedPos[1] = mPsner.getY();
            estimatedPos[2] = mPsner.getZ();

            estimatedVel[0] = mPsner.getVx();
            estimatedVel[1] = mPsner.getVy();
            estimatedVel[2] = mPsner.getVz();

            estimatedAccel[0] = mPsner.getAx();
            estimatedAccel[1] = mPsner.getAy();
            estimatedAccel[2] = mPsner.getAz();

            //Update data visualizations
            try
            {
                if (!string.IsNullOrEmpty(ftLoggingFile))
                {
                    //Open file to append text
                    mStrmWrt = File.AppendText(ftLoggingFile);

                    //For stabilizing the streamWriter
                    while (mStrmWrt.BaseStream == null) { };

                    //LOGGING
                    //Position
                    mStrmWrt.Write((Math.Round(estimatedPos[0], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(estimatedPos[1], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(estimatedPos[2], 2)).ToString() + "\t");

                    //Velocity
                    mStrmWrt.Write((Math.Round(estimatedVel[0], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(estimatedVel[1], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(estimatedVel[2], 2)).ToString() + "\t");

                    //Acceleration
                    mStrmWrt.Write((Math.Round(estimatedAccel[0], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(estimatedAccel[1], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(estimatedAccel[2], 2)).ToString() + "\t");

                    //Processed Forces
                    mStrmWrt.Write((Math.Round(forces[0] - forceBias[0], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(forces[1] - forceBias[1], 2)).ToString() + "\t");
                    mStrmWrt.Write((Math.Round(forces[2] - forceBias[2], 2)).ToString() + "\t");
                    
                    //mStrmWrt.Write((Math.Round(forces[0], 2)).ToString().TrimEnd('0') + "\t");

                    //Write ftData into text file
                    if (ftDataStr.Length > 0)
                        foreach (string str in ftDataStr)
                        {
                            //Write "\t" following each element of ftDataStr
                            if (str.GetEnumerator().MoveNext())
                                mStrmWrt.Write(str + "\t");
                            else
                            {
                                //Write the last element of ftDataStr, new line is already included
                                mStrmWrt.Write(str);
                                //mStrmWrt.WriteLine(str);
                            }
                        }

                    //Close stream writter after appending text
                    mStrmWrt.Close();
                }

                //Update Position Map
                mPosMap.update(estimatedPos[0], estimatedPos[1]);

                //Update other detailed information
                lblFx.Text = Math.Round(forces[0], 2).ToString();
                lblFy.Text = Math.Round(forces[1], 2).ToString();
                lblFz.Text = Math.Round(forces[2], 2).ToString();

                lblAx.Text = Math.Round(estimatedAccel[0], 2).ToString();
                lblAy.Text = Math.Round(estimatedAccel[1], 2).ToString();
                lblAz.Text = Math.Round(estimatedAccel[2], 2).ToString();

                lblVx.Text = Math.Round(estimatedVel[0], 2).ToString();
                lblVy.Text = Math.Round(estimatedVel[1], 2).ToString();
                lblVz.Text = Math.Round(estimatedVel[2], 2).ToString();

                lblX.Text = Math.Round(estimatedPos[0], 2).ToString();
                lblY.Text = Math.Round(estimatedPos[1], 2).ToString();
                lblZ.Text = Math.Round(estimatedPos[2], 2).ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "Chart error");
            }
        }

        protected void ftDataProcess()
        {
            //Processing ftData
            try
            {
                ftDataStr = ftBuffer.Split(new[] { "," }, StringSplitOptions.None);
                int[] ftData = Array.ConvertAll(ftDataStr, s => int.Parse(s));
                try
                {
                    //Processing ftData: From [count] to [N]
                    FtConversion ftConverter = new FtConversion(mLpf);
                    forces = ftConverter.normalize(ftData);

                    //Do calibrations
                    if (isFzCalibrating)
                    {
                        calibStep++;

                        //Calculate average value
                        forceBiasCandidate[2] = (forceBiasCandidate[2] * (calibStep - 1) + forces[2]) / calibStep;

                        //Update progress bar status
                        pbarFzCalib.Value = calibStep;

                        //Update indicating lable's text
                        lblFz0Calib.Text = Math.Round(forceBiasCandidate[2], 2).ToString();

                        //When calibration done
                        if (calibStep >= numOfCalibSample)
                        {
                            //Get the candidate data as formal data
                            forceBias[2] = Math.Round(forceBiasCandidate[2], 2);

                            //Update visualization
                            btnFzCalib.Enabled = true;
                            btnFxyCalib.Enabled = true;
                            pbarFzCalib.Visible = false;
                            lblFz0.Text = forceBias[2].ToString();

                            //Reset calibrating values
                            isFzCalibrating = false;
                            calibStep = 0;
                        }
                    }

                    if (isFxyCalibrating)
                    {
                        calibStep++;
                        //Calculate average value
                        forceBiasCandidate[0] = (forceBiasCandidate[0] * (calibStep - 1) + forces[0]) / calibStep;
                        forceBiasCandidate[1] = (forceBiasCandidate[1] * (calibStep - 1) + forces[1]) / calibStep;

                        //Update progress bar status
                        pbarFxyCalib.Value = calibStep;

                        //Update indicating lable's text
                        lblFx0Calib.Text = Math.Round(forceBiasCandidate[0], 2).ToString();
                        lblFy0Calib.Text = Math.Round(forceBiasCandidate[1], 2).ToString();

                        //When calibration done
                        if (calibStep >= numOfCalibSample)
                        {
                            //Get the candidate data as formal data
                            forceBias[0] = Math.Round(forceBiasCandidate[0], 2);
                            forceBias[1] = Math.Round(forceBiasCandidate[1], 2);

                            //Update visualization
                            btnFxyCalib.Enabled = true;
                            btnFzCalib.Enabled = true;
                            pbarFxyCalib.Visible = false;
                            lblFx0.Text = forceBias[0].ToString();
                            lblFy0.Text = forceBias[1].ToString();

                            //Reset calibrating values
                            isFxyCalibrating = false;
                            calibStep = 0;
                        }
                    }

                    if (isMassCalibrating)
                    {
                        calibStep++;

                        //Calculate average value
                        mVehicleMassCandidate = (mVehicleMassCandidate * (calibStep - 1) + (forces[2] - forceBias[2]) / GRAVITY) / calibStep;

                        //Update progress bar status
                        pbarMassCalib.Value = calibStep;

                        //Update indicating lable's text
                        lblMassCalib.Text = Math.Round(-mVehicleMassCandidate, 2).ToString();

                        //When calibration done
                        if (calibStep >= (int)numOfCalibSample / 2)
                        {
                            if (mVehicleMassCandidate >= 0)
                            {
                                ftCalibThread = new Thread(new ThreadStart(massCalibError));
                                ftCalibThread.Start();
                            }
                            else
                            {
                                //Get the candidate data as formal data
                                mVehicleMass = -Math.Round(mVehicleMassCandidate, 2);
                                lblMass.Text = mVehicleMass.ToString();
                            }

                            //Update visualization
                            pbarMassCalib.Visible = false;
                            btnMassCalib.Visible = true;
                            btnMassCalib.Enabled = true;
                            btnFxyCalib.Enabled = true;
                            btnFzCalib.Enabled = true;

                            //Reset calibrating values
                            isMassCalibrating = false;
                            calibStep = 0;
                        }
                    }

                    //if (mTimePoint == 0)
                    //{
                    //    mTimePoint = DateTime.Now.Ticks - mTimePoint;
                    //}
                    //else
                    //{
                    //    mFtSamplingTime = (DateTime.Now.Ticks - mTimePoint) * Math.Pow(10, -7);
                    //    mTimePoint = DateTime.Now.Ticks;
                    //}

                    ////Run the estimator of the Pseudo Positioner
                    //if (mVehicleMass <= 0) mVehicleMass = 0.01;
                    //mPsner.runEstimation((forces[0] - forceBias[0]) / mVehicleMass, (forces[1] - forceBias[1]) / mVehicleMass, (forces[2] - forceBias[2]) / mVehicleMass, mFtSamplingTime);

                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
 
        }

        private void updatePosMap()
        {
            mPosMap.update(mPsner.getX(), mPsner.getY());
            while (updatePosMapThread.IsAlive)
                try
                {
                    //Dismiss the Thread
                    updatePosMapThread.Suspend();
                }
                catch { }
        }

        private void massCalibError()
        {
            DialogResult dlgRsl = MessageBox.Show("Mass Calib Failed. Need to calibrate Fz0 first", "FT Calibration", MessageBoxButtons.OK);
            if (dlgRsl == DialogResult.OK)
            {
                while (ftCalibThread.IsAlive)
                    try
                    {
                        //Dismiss the Thread
                        ftCalibThread.Suspend();
                    }
                    catch { }
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
            btnFxyCalib.Enabled     = true;
            btnFzCalib.Enabled      = true;
            pnlFtSetting.Visible    = false;
            isFxyCalibrating        = false;
            isFzCalibrating         = false;
            isMassCalibrating = false;

            pbarFxyCalib.Value = 0;
            pbarFxyCalib.Visible = false;
            pbarFzCalib.Value = 0;
            pbarFzCalib.Visible = false;
            pbarMassCalib.Value = 0;
            pbarMassCalib.Visible = false;
            btnMassCalib.Visible = true;
            btnMassCalib.Enabled = true;

            lblFx0Calib.Text = "0.00";
            lblFy0Calib.Text = "0.00";
            lblFz0Calib.Text = "0.00";
            lblMassCalib.Text = "0.00";

            calibStep = 0;
            forceBiasCandidate[0]   = 0;
            forceBiasCandidate[1]   = 0;
            forceBiasCandidate[2]   = 0;
        }

        private void btnFzCalib_Click(object sender, EventArgs e)
        {
            if (ftConnected)
            {
                btnFzCalib.Enabled = false;
                btnFxyCalib.Enabled = false;

                calibStep = 0;
                forceBiasCandidate[2] = 0;

                pbarFzCalib.Visible = true;
                pbarFzCalib.Maximum = numOfCalibSample;
                pbarFzCalib.Step = 1;
                pbarFzCalib.Value = 1;

                isFzCalibrating = true;
            }
            else
            {
                MessageBox.Show("FT sensor is not connected", "FT Connection");
            }
        }

        private void btnFxyCalib_Click(object sender, EventArgs e)
        {
            if (ftConnected)
            {
                btnFxyCalib.Enabled = false;
                btnFzCalib.Enabled = false;

                calibStep = 0;
                forceBiasCandidate[0] = 0;
                forceBiasCandidate[1] = 0;

                pbarFxyCalib.Visible = true;
                pbarFxyCalib.Maximum = numOfCalibSample;
                pbarFxyCalib.Step = 1;
                pbarFxyCalib.Value = 1;

                isFxyCalibrating = true;
            }
            else
            {
                MessageBox.Show("FT sensor is not connected", "FT Connection");
            }
        }

        private void btnVclMassCalib_Click(object sender, EventArgs e)
        {
            if (ftConnected)
            {
                DialogResult dlgRsl = MessageBox.Show("You need to calibrate Fz0 first. \nClick OK to continue Mass Calib. Click Cancel to cancle", "FT Calibration", MessageBoxButtons.OKCancel);
                if (dlgRsl == DialogResult.OK)
                {
                    btnFxyCalib.Enabled = false;
                    btnFzCalib.Enabled = false;
                    btnMassCalib.Enabled = false;
                    btnMassCalib.Visible = false;

                    calibStep = 0;
                    mVehicleMassCandidate = 0;
                    pbarMassCalib.Visible = true;
                    pbarMassCalib.Maximum = numOfCalibSample/2;
                    pbarFxyCalib.Step = 1;
                    pbarFxyCalib.Value = 1;

                    isMassCalibrating = true;
                }
            }
            else
            {
                MessageBox.Show("FT sensor is not connected", "FT Connection");
            }
        }

        private void tbxDragX_KeyPress(object sender, KeyPressEventArgs e)
        {
            mInputHelper.pressNumberOnly(sender, e);
        }

        private void tbxGpsSimPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            mInputHelper.pressNumberOnly(sender, e);
        }

        private void tbxDragX_TextChanged(object sender, EventArgs e)
        {
            btnApplyDragX.Visible = true;
        }

        private void tbxDragY_TextChanged(object sender, EventArgs e)
        {
            btnApplyDragY.Visible = true;
        }

        private void btnApplyDragX_Click(object sender, EventArgs e)
        {
            drag[0] = Convert.ToDouble(tbxDragX.Text);
            btnApplyDragX.Visible = false;
            lblDragX.Text = Math.Round(drag[0], 2).ToString();
            mPsner.setDrag(drag[0], drag[1], 0.2);
        }

        private void btnApplyDragY_Click(object sender, EventArgs e)
        {
            drag[1] = Convert.ToDouble(tbxDragY.Text);
            btnApplyDragY.Visible = false;
            lblDragY.Text = Math.Round(drag[1], 2).ToString();
            mPsner.setDrag(drag[0], drag[1], 0.2);
        }

        private void cbxFtComPort_TextChanged(object sender, EventArgs e)
        {
            ftComPort = cbxFtComPort.Text;
        }

        private void cbxFtBaudRate_TextChanged(object sender, EventArgs e)
        {
            ftBaudRate = Convert.ToInt32(cbxFtBaudRate.Text);
        }

        private void cbxVclComPort_TextChanged(object sender, EventArgs e)
        {
            vclComPort = cbxVclComPort.Text;
        }

        private void cbxVclBaudRate_TextChanged(object sender, EventArgs e)
        {
            vclBaudRate = Convert.ToInt32(cbxVclBaudRate.Text);
        }

        private void ftPositioningTimer_Tick(object sender, EventArgs e)
        {

            if (mTimePoint == 0)
            {
                mTimePoint = DateTime.Now.Ticks - mTimePoint;
            }
            else
            {
                //Update sampling time
                mFtSamplingTime = (DateTime.Now.Ticks - mTimePoint) * Math.Pow(10, -7);

                //Update time point
                mTimePoint = DateTime.Now.Ticks;
            }
            //Run the estimator of the Pseudo Positioner 
            if (mVehicleMass <= 0) mVehicleMass = 0.01;
            mPsner.runEstimation((forces[0] - forceBias[0]) / mVehicleMass, (forces[1] - forceBias[1]) / mVehicleMass, (forces[2] - forceBias[2]) / mVehicleMass, mFtSamplingTime);
        }

        private void ftVisualizationTimer_Tick(object sender, EventArgs e)
        {
            ftVisualize();
        }

        private void btnCntFs_Click(object sender, EventArgs e)
        {
            /*** This part is just for testing the NED to ECEF conversion
            MatrixHelper mMatrixHelper = new MatrixHelper();
            this.Invoke((MethodInvoker)delegate()
            {
                lbxView.Items.Add("\nBegin matrix inverse using Crout LU decomp demo \n");
            });

            double[][] e2n = mMatrixHelper.MatrixCreate(3, 3);
            e2n[0][0] = 3.0; e2n[0][1] = 7.0; e2n[0][2] = 2.0;
            e2n[1][0] = 1.0; e2n[1][1] = 8.0; e2n[1][2] = 4.0;
            e2n[2][0] = 2.0; e2n[2][1] = 1.0; e2n[2][2] = 9.0;

            e2n = mRotationMatrixEcef2Ned;
            this.Invoke((MethodInvoker)delegate()
            {
                lbxView.Items.Add("Inverse matrix is ");
                lbxView.Items.Add(mMatrixHelper.MatrixAsString(e2n));
            });

            double[][] per = mMatrixHelper.MatrixCreate(3, 1);
            per[0][0] = 3.0; per[1][0] = 7.0; per[2][0] = 2.0;

            double[][] pn = mMatrixHelper.MatrixCreate(3, 1);
            pn[0][0] = 0.1; pn[1][0] = 0.1; pn[2][0] = 10.10;

            double[][] n2e = mMatrixHelper.MatrixInverse(e2n);
            this.Invoke((MethodInvoker)delegate()
            {
                lbxView.Items.Add("Inverse matrix inv is ");
                lbxView.Items.Add(mMatrixHelper.MatrixAsString(n2e));
            });

            double[][] pe = mMatrixHelper.MatrixAdd(mMatrixHelper.MatrixProduct(mRotationMatrixNed2Ecef, pn), posEcefRef);

            //double[][] pe = mMatrixHelper.MatrixAdd(mMatrixHelper.MatrixProduct(n2e, pn), per);
            this.Invoke((MethodInvoker)delegate()
            {
                lbxView.Items.Add("The Pe is ");
                lbxView.Items.Add(mMatrixHelper.MatrixAsString(pe));
            });
            ***/
        }
    }
}
