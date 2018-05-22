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
        private TcpClient client = new TcpClient();
        private bool gpsConnected = false;

        //F/T sensor variables
        private bool ftConnected = false;
        private string ftComPort;
        private int ftBaudRate;
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
        PositionMap mPosMap;
        LowPassFilter mLpf;
        private const double mLpfSamplingTime = 0.05;
        PseudoPositioning mPsner;
        private double mVehicleMass;
        private double mVehicleMassCandidate;
        private double[] drag = new double[2];

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

            try
            {
                if (File.Exists(mParamFileName))
                {
                    StreamReader strRdr = new StreamReader(mParamFileName);

                    ftComPort = strRdr.ReadLine();
                    ftBaudRate = Convert.ToInt32(strRdr.ReadLine());

                    vclComPort = strRdr.ReadLine();
                    vclBaudRate = Convert.ToInt32(strRdr.ReadLine());

                    gpsSimIp = strRdr.ReadLine();
                    gpsSimPort = Convert.ToInt32(strRdr.ReadLine());

                    fsIp = strRdr.ReadLine();

                    mVehicleMass = Convert.ToDouble(strRdr.ReadLine());
                    drag[0] = Convert.ToDouble(strRdr.ReadLine());
                    drag[1] = Convert.ToDouble(strRdr.ReadLine());

                    forceBias[0] = Convert.ToDouble(strRdr.ReadLine());
                    forceBias[1] = Convert.ToDouble(strRdr.ReadLine());
                    forceBias[2] = Convert.ToDouble(strRdr.ReadLine());

                    strRdr.Close();

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

            //Initiate Lowpass filter
            mLpf = new LowPassFilter(2.0, 0.05);
            mPosMap = new PositionMap(chartPosSim);
            chartPosSim.Titles.Add("Vehicle's Position");
            mPsner = new PseudoPositioning(0.3, 0.28, mLpfSamplingTime);
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
                    //Save parameters into an internal file
                    StreamWriter mStrmWrt = new StreamWriter("params.txt"); 
                    Thread.Sleep(10);

                    mStrmWrt.WriteLine(ftComPort);
                    mStrmWrt.WriteLine(ftBaudRate.ToString());

                    mStrmWrt.WriteLine(vclComPort);
                    mStrmWrt.WriteLine(vclBaudRate.ToString());

                    mStrmWrt.WriteLine(gpsSimIp);
                    mStrmWrt.WriteLine(gpsSimPort.ToString());

                    mStrmWrt.WriteLine(fsIp);

                    mStrmWrt.WriteLine(Math.Round(mVehicleMass, 2).ToString());
                    mStrmWrt.WriteLine(Math.Round(drag[0], 2).ToString());
                    mStrmWrt.WriteLine(Math.Round(drag[1], 2).ToString());

                    mStrmWrt.WriteLine(Math.Round(forceBias[0], 2).ToString());
                    mStrmWrt.WriteLine(Math.Round(forceBias[1], 2).ToString());
                    mStrmWrt.WriteLine(Math.Round(forceBias[2], 2).ToString());

                    mStrmWrt.Close(); 

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
                ftConnected = false;

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
                        ftConnected = true;

                        //Visible Refresh Button
                        btnRefreshAll.Visible = true;

                        //MessageBox.Show("F/T sensor connected", "F/T sensor connection");
                    }

                    try
                    {
                        ftDataStr = ftBuffer.Split(new[] { "," }, StringSplitOptions.None);
                        int[] ftData = Array.ConvertAll(ftDataStr, s => int.Parse(s));
                        try
                        {
                            //Processing ftData
                            FtConversion ftConverter = new FtConversion(mLpf);
                            forces = ftConverter.normalize(ftData);

                            if (isFzCalibrating)
                            {
                                calibStep++;
                                forceBiasCandidate[2] = (forceBiasCandidate[2] * (calibStep - 1) + forces[2]) / calibStep;
                                pbarFzCalib.Value = calibStep;
                                lblFz0Calib.Text = Math.Round(forceBiasCandidate[2], 2).ToString();
                                if (calibStep >= numOfCalibSample)
                                {
                                    forceBias[2] = forceBiasCandidate[2];
                                    btnFzCalib.Enabled = true;
                                    btnFxyCalib.Enabled = true;
                                    pbarFzCalib.Visible = false;

                                    isFzCalibrating = false;
                                    calibStep = 0;
                                }
                            }

                            if (isFxyCalibrating)
                            {
                                calibStep++;
                                forceBiasCandidate[0] = (forceBiasCandidate[0] * (calibStep - 1) + forces[0]) / calibStep;
                                forceBiasCandidate[1] = (forceBiasCandidate[1] * (calibStep - 1) + forces[1]) / calibStep;
                                pbarFxyCalib.Value = calibStep;
                                lblFx0Calib.Text = Math.Round(forceBiasCandidate[0], 2).ToString();
                                lblFy0Calib.Text = Math.Round(forceBiasCandidate[1], 2).ToString();
                                if (calibStep >= numOfCalibSample)
                                {
                                    forceBias[0] = forceBiasCandidate[0];
                                    forceBias[1] = forceBiasCandidate[1];
                                    btnFxyCalib.Enabled = true;
                                    btnFzCalib.Enabled = true;
                                    pbarFxyCalib.Visible = false;

                                    isFxyCalibrating = false;
                                    calibStep = 0;
                                }
                            }

                            if (isMassCalibrating)
                            {
                                calibStep++;
                                mVehicleMassCandidate = (mVehicleMassCandidate * (calibStep - 1) + forces[2] - forceBias[2]) / calibStep;
                                pbarMassCalib.Value = calibStep;
                                lblMassCalib.Text = Math.Round(mVehicleMassCandidate, 2).ToString();
                                if (calibStep >= (int)numOfCalibSample/2)
                                {
                                    if (mVehicleMassCandidate <= 0)
                                    {
                                        ftCalibThread = new Thread(new ThreadStart(massCalibError));
                                        ftCalibThread.Start();
                                    }
                                    else
                                    {
                                        mVehicleMass = Math.Round(mVehicleMassCandidate, 2);
                                        lblMass.Text = mVehicleMass.ToString();
                                    }

                                    pbarMassCalib.Visible = false;
                                    btnMassCalib.Visible = true;
                                    btnMassCalib.Enabled = true;
                                    btnFxyCalib.Enabled = true;
                                    btnFzCalib.Enabled = true;

                                    isMassCalibrating = false;
                                    calibStep = 0;
                                }
                            }
                    
                            //Run the estimator of the Pseudo Positioner
                            mPsner.runEstimation((forces[0] - forceBias[0]) / mVehicleMass, (forces[1] - forceBias[1]) / mVehicleMass, (forces[2] - forceBias[2]) / mVehicleMass);

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
                            
                            //Close stream writter after appending text
                            mStrmWrt.Close();
                        }
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
                
                //Update data visualizations
                try
                {
                    mPosMap.update(mPsner.getX(), mPsner.getY());

                    lblFx.Text = Math.Round(forces[0], 2).ToString();
                    lblFy.Text = Math.Round(forces[1], 2).ToString();
                    lblFz.Text = Math.Round(forces[2], 2).ToString();

                    lblAx.Text = Math.Round(mPsner.getAx(), 2).ToString();
                    lblAy.Text = Math.Round(mPsner.getAy(), 2).ToString();
                    lblAz.Text = Math.Round(mPsner.getAz(), 2).ToString();

                    lblVx.Text = Math.Round(mPsner.getVx(), 2).ToString();
                    lblVy.Text = Math.Round(mPsner.getVy(), 2).ToString();
                    lblVz.Text = Math.Round(mPsner.getVz(), 2).ToString();

                    lblX.Text = Math.Round(mPsner.getX(), 2).ToString();
                    lblY.Text = Math.Round(mPsner.getY(), 2).ToString();
                    lblZ.Text = Math.Round(mPsner.getZ(), 2).ToString();


                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "Chart error");
                }
            }
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
            btnFxyCalib.Enabled = true;
            btnFzCalib.Enabled = true;
            pnlFtSetting.Visible    = false;
            isFxyCalibrating        = false;
            isFzCalibrating         = false;
            calibStep = 0;
            forceBiasCandidate[0] = 0;
            forceBiasCandidate[1] = 0;
            forceBiasCandidate[2] = 0;
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
        }

        private void btnApplyDragY_Click(object sender, EventArgs e)
        {
            drag[1] = Convert.ToDouble(tbxDragY.Text);
            btnApplyDragY.Visible = false;
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
    }
}
