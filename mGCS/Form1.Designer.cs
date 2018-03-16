namespace mGCS
{
    partial class mGCS
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.lblCntFt = new System.Windows.Forms.Label();
            this.btnCntFt = new System.Windows.Forms.Button();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.lblCntGpsSim = new System.Windows.Forms.Label();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxIP = new System.Windows.Forms.TextBox();
            this.btnCntGpsSim = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbxFsIp = new System.Windows.Forms.TextBox();
            this.btnCntFs = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.cbxFtCom = new System.Windows.Forms.ComboBox();
            this.cbxFtBaud = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.ftSerialPort = new System.IO.Ports.SerialPort(this.components);
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.cbxVhclBaud = new System.Windows.Forms.ComboBox();
            this.cbxVhclCom = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 110);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chart1);
            this.splitContainer1.Size = new System.Drawing.Size(1023, 499);
            this.splitContainer1.SplitterDistance = 238;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer2.Size = new System.Drawing.Size(238, 499);
            this.splitContainer2.SplitterDistance = 274;
            this.splitContainer2.TabIndex = 0;
            // 
            // chart1
            // 
            chartArea6.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea6);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend6.Name = "Legend1";
            this.chart1.Legends.Add(legend6);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            series6.ChartArea = "ChartArea1";
            series6.Legend = "Legend1";
            series6.Name = "Series1";
            this.chart1.Series.Add(series6);
            this.chart1.Size = new System.Drawing.Size(779, 497);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Left;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.splitContainer3.Panel1.Controls.Add(this.lblCntFt);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.label6);
            this.splitContainer3.Panel2.Controls.Add(this.label5);
            this.splitContainer3.Panel2.Controls.Add(this.cbxFtBaud);
            this.splitContainer3.Panel2.Controls.Add(this.cbxFtCom);
            this.splitContainer3.Panel2.Controls.Add(this.btnCntFt);
            this.splitContainer3.Size = new System.Drawing.Size(242, 110);
            this.splitContainer3.SplitterDistance = 37;
            this.splitContainer3.TabIndex = 2;
            // 
            // lblCntFt
            // 
            this.lblCntFt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCntFt.Font = new System.Drawing.Font("Gulim", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCntFt.Location = new System.Drawing.Point(0, 0);
            this.lblCntFt.Name = "lblCntFt";
            this.lblCntFt.Size = new System.Drawing.Size(240, 35);
            this.lblCntFt.TabIndex = 0;
            this.lblCntFt.Text = "Connection to F/T sensor";
            this.lblCntFt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCntFt
            // 
            this.btnCntFt.BackgroundImage = global::mGCS.Properties.Resources.btnConnect;
            this.btnCntFt.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCntFt.Location = new System.Drawing.Point(165, 0);
            this.btnCntFt.Name = "btnCntFt";
            this.btnCntFt.Size = new System.Drawing.Size(75, 67);
            this.btnCntFt.TabIndex = 1;
            this.btnCntFt.UseVisualStyleBackColor = true;
            this.btnCntFt.Click += new System.EventHandler(this.btnCntFt_Click);
            // 
            // splitContainer4
            // 
            this.splitContainer4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Left;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(242, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.splitContainer4.Panel1.Controls.Add(this.lblCntGpsSim);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.label7);
            this.splitContainer4.Panel2.Controls.Add(this.label8);
            this.splitContainer4.Panel2.Controls.Add(this.cbxVhclBaud);
            this.splitContainer4.Panel2.Controls.Add(this.cbxVhclCom);
            this.splitContainer4.Panel2.Controls.Add(this.button2);
            this.splitContainer4.Size = new System.Drawing.Size(266, 110);
            this.splitContainer4.SplitterDistance = 37;
            this.splitContainer4.TabIndex = 3;
            // 
            // lblCntGpsSim
            // 
            this.lblCntGpsSim.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCntGpsSim.Font = new System.Drawing.Font("Gulim", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCntGpsSim.Location = new System.Drawing.Point(0, 0);
            this.lblCntGpsSim.Name = "lblCntGpsSim";
            this.lblCntGpsSim.Size = new System.Drawing.Size(264, 35);
            this.lblCntGpsSim.TabIndex = 1;
            this.lblCntGpsSim.Text = "Connection to Vehicle";
            this.lblCntGpsSim.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitContainer5
            // 
            this.splitContainer5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Left;
            this.splitContainer5.IsSplitterFixed = true;
            this.splitContainer5.Location = new System.Drawing.Point(508, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.splitContainer5.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.btnSend);
            this.splitContainer5.Panel2.Controls.Add(this.label1);
            this.splitContainer5.Panel2.Controls.Add(this.tbxIP);
            this.splitContainer5.Panel2.Controls.Add(this.btnCntGpsSim);
            this.splitContainer5.Size = new System.Drawing.Size(266, 110);
            this.splitContainer5.SplitterDistance = 37;
            this.splitContainer5.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Gulim", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 35);
            this.label2.TabIndex = 1;
            this.label2.Text = "Connection to GPS Sim.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitContainer6
            // 
            this.splitContainer6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Left;
            this.splitContainer6.IsSplitterFixed = true;
            this.splitContainer6.Location = new System.Drawing.Point(774, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.splitContainer6.Panel1.Controls.Add(this.label4);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.label3);
            this.splitContainer6.Panel2.Controls.Add(this.tbxFsIp);
            this.splitContainer6.Panel2.Controls.Add(this.btnCntFs);
            this.splitContainer6.Size = new System.Drawing.Size(248, 110);
            this.splitContainer6.SplitterDistance = 37;
            this.splitContainer6.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("Gulim", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(246, 35);
            this.label4.TabIndex = 1;
            this.label4.Text = "Connection to Flight Sim.";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(125, 30);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(53, 23);
            this.btnSend.TabIndex = 9;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "IP adress";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbxIP
            // 
            this.tbxIP.Location = new System.Drawing.Point(75, 4);
            this.tbxIP.Name = "tbxIP";
            this.tbxIP.Size = new System.Drawing.Size(103, 21);
            this.tbxIP.TabIndex = 7;
            this.tbxIP.Text = "192.168.0.41";
            this.tbxIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbxIP.TextChanged += new System.EventHandler(this.tbxIP_TextChanged);
            // 
            // btnCntGpsSim
            // 
            this.btnCntGpsSim.BackgroundImage = global::mGCS.Properties.Resources.btnConnect;
            this.btnCntGpsSim.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCntGpsSim.Location = new System.Drawing.Point(189, 0);
            this.btnCntGpsSim.Name = "btnCntGpsSim";
            this.btnCntGpsSim.Size = new System.Drawing.Size(75, 67);
            this.btnCntGpsSim.TabIndex = 6;
            this.btnCntGpsSim.UseVisualStyleBackColor = true;
            this.btnCntGpsSim.Click += new System.EventHandler(this.btnCntGpsSim_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(55, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "IP adress";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbxFsIp
            // 
            this.tbxFsIp.Location = new System.Drawing.Point(36, 32);
            this.tbxFsIp.Name = "tbxFsIp";
            this.tbxFsIp.Size = new System.Drawing.Size(101, 21);
            this.tbxFsIp.TabIndex = 10;
            this.tbxFsIp.Text = "192.168.0.41";
            this.tbxFsIp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnCntFs
            // 
            this.btnCntFs.BackgroundImage = global::mGCS.Properties.Resources.btnConnect;
            this.btnCntFs.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCntFs.Location = new System.Drawing.Point(171, 0);
            this.btnCntFs.Name = "btnCntFs";
            this.btnCntFs.Size = new System.Drawing.Size(75, 67);
            this.btnCntFs.TabIndex = 9;
            this.btnCntFs.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.BackgroundImage = global::mGCS.Properties.Resources.btnConnect;
            this.button2.Dock = System.Windows.Forms.DockStyle.Right;
            this.button2.Location = new System.Drawing.Point(189, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 67);
            this.button2.TabIndex = 7;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // cbxFtCom
            // 
            this.cbxFtCom.FormattingEnabled = true;
            this.cbxFtCom.Location = new System.Drawing.Point(19, 33);
            this.cbxFtCom.Name = "cbxFtCom";
            this.cbxFtCom.Size = new System.Drawing.Size(65, 20);
            this.cbxFtCom.TabIndex = 2;
            this.cbxFtCom.TextChanged += new System.EventHandler(this.cbxCom_TextChanged);
            // 
            // cbxFtBaud
            // 
            this.cbxFtBaud.FormattingEnabled = true;
            this.cbxFtBaud.Items.AddRange(new object[] {
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200",
            "230400"});
            this.cbxFtBaud.Location = new System.Drawing.Point(90, 33);
            this.cbxFtBaud.Name = "cbxFtBaud";
            this.cbxFtBaud.Size = new System.Drawing.Size(65, 20);
            this.cbxFtBaud.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "COM port";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(94, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "Baudrate";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(109, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "Baudrate";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(37, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 12);
            this.label8.TabIndex = 16;
            this.label8.Text = "COM port";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbxVhclBaud
            // 
            this.cbxVhclBaud.FormattingEnabled = true;
            this.cbxVhclBaud.Items.AddRange(new object[] {
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200",
            "230400"});
            this.cbxVhclBaud.Location = new System.Drawing.Point(105, 35);
            this.cbxVhclBaud.Name = "cbxVhclBaud";
            this.cbxVhclBaud.Size = new System.Drawing.Size(65, 20);
            this.cbxVhclBaud.TabIndex = 13;
            // 
            // cbxVhclCom
            // 
            this.cbxVhclCom.FormattingEnabled = true;
            this.cbxVhclCom.Location = new System.Drawing.Point(34, 35);
            this.cbxVhclCom.Name = "cbxVhclCom";
            this.cbxVhclCom.Size = new System.Drawing.Size(65, 20);
            this.cbxVhclCom.TabIndex = 14;
            // 
            // mGCS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(1023, 609);
            this.Controls.Add(this.splitContainer6);
            this.Controls.Add(this.splitContainer5);
            this.Controls.Add(this.splitContainer4);
            this.Controls.Add(this.splitContainer3);
            this.Controls.Add(this.splitContainer1);
            this.Name = "mGCS";
            this.Text = "mGCS";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mGCS_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            this.splitContainer6.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Label lblCntFt;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.Label lblCntGpsSim;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCntFt;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbxIP;
        private System.Windows.Forms.Button btnCntGpsSim;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbxFsIp;
        private System.Windows.Forms.Button btnCntFs;
        private System.Windows.Forms.ComboBox cbxFtBaud;
        private System.Windows.Forms.ComboBox cbxFtCom;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.IO.Ports.SerialPort ftSerialPort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbxVhclBaud;
        private System.Windows.Forms.ComboBox cbxVhclCom;

    }
}

