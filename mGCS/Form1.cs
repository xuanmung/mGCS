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

        //GPS Sim. variables
        private static Socket mClient;
        private static byte[] mPkg = new byte[512];
        private static string gpsSimIP;
        private static string gpsSimGateway;
        private System.Windows.Forms.Timer mTimer;
        private TcpClient client = new TcpClient();
        private bool gpsConnected = false;


        //F/T sensor variables
        private bool ftConnected = false;
        private string ftCom;
        private string ftBaud;
        private double fx, fy, fz, tx, ty, tz;
        private string ftBuffer;
        private Thread ftDataReceiver;
        private string ftLoggingFile;
        private static TextWriter mTwriter;

        //Vehicle variables
        private bool vehicleConnected = false;

        //Flight Sim. variables
        private bool fsConnected = false;


        private void Form1_Load(object sender, EventArgs e)
        {
            mTimer = new System.Windows.Forms.Timer();
            mTimer.Interval = 100; //100 mili-seconds
            mTimer.Tick += new System.EventHandler(mTimer_Tick);
            //mTimer.Start();
        }
        
        private void tbxIP_TextChanged(object sender, EventArgs e)
        {
            gpsSimIP = tbxIP.Text;
        }

        private void btnCntGpsSim_Click(object sender, EventArgs e)
        {
            mClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (!gpsConnected)
            {
                try
                {
                    IPEndPoint iep = new IPEndPoint(IPAddress.Parse(gpsSimIP), 57000);
                    mClient.BeginConnect(iep, new AsyncCallback(gpsSimConnected), mClient);
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
                Thread receiver = new Thread(new ThreadStart(receiveData));
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
            
            int receiver;
            string strData;
            while (true)
            {
                receiver = mClient.Receive(mPkg);
                strData = Encoding.ASCII.GetString(mPkg, 0, receiver); 
                if (strData == "bye") break;
            }
            strData = "bye";
            byte[] msg = Encoding.ASCII.GetBytes(strData);
            mClient.Send(msg);
            mClient.Close();
            return;
        }

        void mTimer_Tick(object sender, EventArgs e)
        {
            //byte[] msg = BitConverter.GetBytes(1.2);
            //mClient.BeginSend(msg, 0, msg.Length, 0, new AsyncCallback(sendData), mClient);
            //return;
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
            const int INT_SIZE = 4,  DOUBLE_SIZE = 8;
            byte[] msg = new byte[1024];
            byte[] dataType = BitConverter.GetBytes((uint)3593);
            byte[] dataLen  = BitConverter.GetBytes((uint)32);
            byte[] time = BitConverter.GetBytes((double)10.1), ecef_x = BitConverter.GetBytes((double)1.1), ecef_y = BitConverter.GetBytes((double)2.1), ecef_z = BitConverter.GetBytes((double)3.1);

            Buffer.BlockCopy(dataType, 0, msg, 0 * INT_SIZE, 1* INT_SIZE);
            Buffer.BlockCopy(dataLen, 0, msg, 1 * INT_SIZE, 1* INT_SIZE);
            Buffer.BlockCopy(time, 0, msg, 2 * INT_SIZE, 2 * INT_SIZE);
            Buffer.BlockCopy(ecef_x, 0, msg, 4 * INT_SIZE, 2 * INT_SIZE);
            Buffer.BlockCopy(ecef_y, 0, msg, 6 * INT_SIZE, 2 * INT_SIZE);
            Buffer.BlockCopy(ecef_z, 0, msg, 8 * INT_SIZE, 2 * INT_SIZE);

            //byte[] msg = Encoding.ASCII.GetBytes(data.ToString()); 
            
            //Buffer.BlockCopy(BitConverter.GetBytes(dataLen), 0, msg, msg.Length,
            mClient.BeginSend(msg, 0, msg.Length, 0, new AsyncCallback(sendData), mClient);
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

                        //Change button's Icon
                        btnCntFt.Image = ((System.Drawing.Image)(Properties.Resources.btnDisConnect));
                        //MessageBox.Show("F/T sensor connected", "F/T sensor connection");

                        //Show Save-file Dialog for Logging FT data
                        SaveFileDialog sfDlg = new SaveFileDialog();
                        sfDlg.DefaultExt = "txt";
                        sfDlg.Title = "Save F/T Logging File";
                        DialogResult dlgResult = sfDlg.ShowDialog();
                        if (dlgResult == DialogResult.OK && !string.IsNullOrWhiteSpace(sfDlg.FileName))
                        {
                            ftLoggingFile = sfDlg.FileName;
                        }

                        //Launch a new Thread for FT data receiving
                        ftDataReceiver = new Thread(new ThreadStart(ftDataReceive));
                        ftDataReceiver.Start();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Unauthorized Access", "F/T sensor connection");
                    }
                }
            }
            else 
            {
                //Dismiss the Thread
                ftDataReceiver.Suspend();

                //Close the serial port
                ftSerialPort.Close();

                //Return button's Icon
                btnCntFt.Image = ((System.Drawing.Image)(Properties.Resources.btnConnect));
                //MessageBox.Show("F/T sensor disconnected", "F/T sensor connection");
            }
        }
        void ftDataReceive()
        {
            while (ftSerialPort.IsOpen)
            {
                char requestCode = (char)20;

                //Just for Debugging
                this.Invoke((MethodInvoker)delegate()
                {
                    lbxView.Items.Add("START");
                });
                
                //Send request to FT controller
                try
                {
                    ftSerialPort.WriteLine(requestCode.ToString());
                }
                catch (Exception x)
                {
                MessageBox.Show(x.Message.ToString(), "F/T sensor connection");
                }

                //Read FT data
                try
                {
                    ftBuffer = ftSerialPort.ReadLine();

                    //Just for Debugging
                    this.Invoke((MethodInvoker)delegate()
                    {
                        lbxView.Items.Add(ftBuffer);
                    });
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("F/T data reading Timeout", "F/T sensor connection");
                }
                catch (Exception x)
                { MessageBox.Show(x.Message.ToString(), "F/T sensor connection"); }

                StreamWriter mStrmWtr;
                if (!string.IsNullOrEmpty(ftLoggingFile))
                {
                    //Open file to append text
                    mStrmWtr = File.AppendText(ftLoggingFile);

                    string[] ftDataStr = ftBuffer.Split(new[] { "," }, StringSplitOptions.None);
                    //int[] ftData = Array.ConvertAll(ftDataStr, s => int.Parse(s));

                    foreach (string str in ftDataStr)
                    {
                        //Write "\t" following each element of ftDataStr
                        if (str.GetEnumerator().MoveNext())
                            mStrmWtr.Write(str + "\t");
                        else
                            //Write new line after fully writting ftDataStr
                            mStrmWtr.WriteLine(str);
                    }

                    //Close stream writter after appending text
                    mStrmWtr.Close();
                }
            }
        }
    }
}
