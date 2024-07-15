using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace HCL_ODA_TestPAD.Splash
{
	// The SplashScreen class definition.  AKO Form
    /// <summary>
    /// 
    /// </summary>
	public partial class SplashTestPad : Form
	{
		#region Member Variables
		// Threading
		private static SplashTestPad _msFrmSplash = null;
		private static Thread _msOThread = null;
		private static int _ssProgressDelay = 250;
        // Fade in and out.
        private double _mDblOpacityIncrement = .05;
		private double _mDblOpacityDecrement = .08;
		private const int TimerInterval = 50;

		// Status and progress bar
		private string _mSStatus;
		private string _mSTimeRemaining;
		private double _mDblCompletionFraction = 0.0;
		private Rectangle _mRProgress;

		// Progress smoothing
		private double _mDblLastCompletionFraction = 0.0;
		private double _mDblPbIncrementPerTimerInterval = .015;

		// Self-calibration support
		private int _mIIndex = 1;
		private int _mIActualTicks = 0;
		private ArrayList _mAlPreviousCompletionFraction;
		private ArrayList _mAlActualTimes = new ArrayList();
		private DateTime _mDtStart;
		private bool _mBFirstLaunch = false;
        private Label _lblTestPaD;
        private bool _mBDtSet = false;

		#endregion Member Variables

		/// <summary>
		/// Constructor
		/// </summary>
		public SplashTestPad()
		{
			InitializeComponent();
			this.Opacity = 0.0;
			_updateTimer.Interval = TimerInterval;
			_updateTimer.Start();
			this.ClientSize = this.BackgroundImage.Size;
		}

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashTestPad));
            this._lblStatus = new System.Windows.Forms.Label();
            this._pnlStatus = new System.Windows.Forms.Panel();
            this._updateTimer = new System.Windows.Forms.Timer(this.components);
            this._lblTestPaD = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this._lblStatus.BackColor = System.Drawing.Color.Transparent;
            this._lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this._lblStatus.ForeColor = System.Drawing.Color.Yellow;
            this._lblStatus.Location = new System.Drawing.Point(2, 87);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(167, 17);
            this._lblStatus.TabIndex = 0;
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblStatus.DoubleClick += new System.EventHandler(this.SplashScreen_DoubleClick);
            // 
            // pnlStatus
            // 
            this._pnlStatus.BackColor = System.Drawing.Color.Transparent;
            this._pnlStatus.Location = new System.Drawing.Point(2, 292);
            this._pnlStatus.Name = "_pnlStatus";
            this._pnlStatus.Size = new System.Drawing.Size(167, 20);
            this._pnlStatus.TabIndex = 1;
            this._pnlStatus.DoubleClick += new System.EventHandler(this.SplashScreen_DoubleClick);
            // 
            // UpdateTimer
            // 
            this._updateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // lblTestPaD
            // 
            this._lblTestPaD.BackColor = System.Drawing.Color.Transparent;
            this._lblTestPaD.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._lblTestPaD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._lblTestPaD.Font = new System.Drawing.Font("Arial Black", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this._lblTestPaD.ForeColor = System.Drawing.Color.Red;
            this._lblTestPaD.Location = new System.Drawing.Point(18, 238);
            this._lblTestPaD.Name = "_lblTestPaD";
            this._lblTestPaD.Size = new System.Drawing.Size(135, 32);
            this._lblTestPaD.TabIndex = 7;
            this._lblTestPaD.Text = "TestPAD";
            this._lblTestPaD.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SplashTestPAD
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.BackColor = System.Drawing.Color.LightGray;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(169, 313);
            this.Controls.Add(this._lblTestPaD);
            this.Controls.Add(this._pnlStatus);
            this.Controls.Add(this._lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SplashTestPad";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashScreen";
            this.DoubleClick += new System.EventHandler(this.SplashScreen_DoubleClick);
            this.ResumeLayout(false);

        }
        #endregion


        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.Timer _updateTimer;
        private System.Windows.Forms.Panel _pnlStatus;

        #region Public Static Methods
        // A static method to create the thread and 
        // launch the SplashScreen.
        /// <summary>
        /// 
        /// </summary>
        static public void ShowSplashScreen()
		{
			// Make sure it's only launched once.
			if (_msFrmSplash != null)
				return;
			_msOThread = new Thread(new ThreadStart(SplashTestPad.ShowForm));
			_msOThread.IsBackground = true;
			_msOThread.SetApartmentState(ApartmentState.STA);
			_msOThread.Start();
			while (_msFrmSplash == null || _msFrmSplash.IsHandleCreated == false)
			{
				Thread.Sleep(TimerInterval);
			}
		}

		// Close the form without setting the parent.
        /// <summary>
        /// 
        /// </summary>
		static public void CloseForm()
		{
			if (_msFrmSplash != null && _msFrmSplash.IsDisposed == false)
			{
				// Make it start going away.
				_msFrmSplash._mDblOpacityIncrement = -_msFrmSplash._mDblOpacityDecrement;
			}
			_msOThread = null;	// we don't need these any more.
			_msFrmSplash = null;
		}

		// A static method to set the status and update the reference.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newStatus"></param>
		static public void SetStatus(string newStatus)
		{
			SetStatus(newStatus, true);
		}

		// A static method to set the status and optionally update the reference.
		// This is useful if you are in a section of code that has a variable
		// set of status string updates.  In that case, don't set the reference.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newStatus"></param>
        /// <param name="setReference"></param>
		static public void SetStatus(string newStatus, bool setReference)
		{
			if (_msFrmSplash == null)
				return;

			_msFrmSplash._mSStatus = newStatus;

			if (setReference)
				_msFrmSplash.SetReferenceInternal();
		}

		// Static method called from the initializing application to 
		// give the splash screen reference points.  Not needed if
		// you are using a lot of status strings.
        /// <summary>
        /// 
        /// </summary>
		static public void SetReferencePoint()
		{
			if (_msFrmSplash == null)
				return;
			_msFrmSplash.SetReferenceInternal();

		}
		#endregion Public Static Methods

		#region Private Methods

		// A private entry point for the thread.
		static private void ShowForm()
		{
			_msFrmSplash = new SplashTestPad();
			Application.Run(_msFrmSplash);
		}

		// Internal method for setting reference points.
		private void SetReferenceInternal()
		{
			if (_mBDtSet == false)
			{
				_mBDtSet = true;
				_mDtStart = DateTime.Now;
				ReadIncrements();
			}
			double dblMilliseconds = ElapsedMilliSeconds();
			_mAlActualTimes.Add(dblMilliseconds);
			_mDblLastCompletionFraction = _mDblCompletionFraction;
			if (_mAlPreviousCompletionFraction != null && _mIIndex < _mAlPreviousCompletionFraction.Count)
				_mDblCompletionFraction = (double)_mAlPreviousCompletionFraction[_mIIndex++];
			else
				_mDblCompletionFraction = (_mIIndex > 0) ? 1 : 0;
		}

		// Utility function to return elapsed Milliseconds since the 
		// SplashScreen was launched.
		private double ElapsedMilliSeconds()
		{
			TimeSpan ts = DateTime.Now - _mDtStart;
			return ts.TotalMilliseconds;
		}

		// Function to read the checkpoint intervals from the previous invocation of the
		// splashscreen from the XML file.
		private void ReadIncrements()
		{
			string sPbIncrementPerTimerInterval = SplashScreenXmlStorage.Interval;
			double dblResult;

			if (Double.TryParse(sPbIncrementPerTimerInterval, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out dblResult) == true)
				_mDblPbIncrementPerTimerInterval = dblResult;
			else
				_mDblPbIncrementPerTimerInterval = .0015;

			string sPbPreviousPctComplete = SplashScreenXmlStorage.Percents;

			if (sPbPreviousPctComplete != "")
			{
				string[] aTimes = sPbPreviousPctComplete.Split(null);
				_mAlPreviousCompletionFraction = new ArrayList();

				for (int i = 0; i < aTimes.Length; i++)
				{
					double dblVal;
					if (Double.TryParse(aTimes[i], System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out dblVal) == true)
						_mAlPreviousCompletionFraction.Add(dblVal);
					else
						_mAlPreviousCompletionFraction.Add(1.0);
				}
			}
			else
			{
				_mBFirstLaunch = true;
				_mSTimeRemaining = "";
			}
		}

		// Method to store the intervals (in percent complete) from the current invocation of
		// the splash screen to XML storage.
		private void StoreIncrements()
		{
			string sPercent = "";
			double dblElapsedMilliseconds = ElapsedMilliSeconds();
			for (int i = 0; i < _mAlActualTimes.Count; i++)
				sPercent += ((double)_mAlActualTimes[i] / dblElapsedMilliseconds).ToString("0.####", System.Globalization.NumberFormatInfo.InvariantInfo) + " ";

			SplashScreenXmlStorage.Percents = sPercent;

			_mDblPbIncrementPerTimerInterval = 1.0 / (double)_mIActualTicks;

			SplashScreenXmlStorage.Interval = _mDblPbIncrementPerTimerInterval.ToString("#.000000", System.Globalization.NumberFormatInfo.InvariantInfo);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public static SplashTestPad GetSplashScreen()
		{
			return _msFrmSplash;
		}

		#endregion Private Methods

		#region Event Handlers
		// Tick Event handler for the Timer control.  Handle fade in and fade out and paint progress bar. 
		private void UpdateTimer_Tick(object sender, System.EventArgs e)
		{
			_lblStatus.Text = _mSStatus;

			// Calculate opacity
			if (_mDblOpacityIncrement > 0)		// Starting up splash screen
			{
				_mIActualTicks++;
				if (this.Opacity < 1)
					this.Opacity += _mDblOpacityIncrement;
			}
			else // Closing down splash screen
			{
				if (this.Opacity > 0)
					this.Opacity += _mDblOpacityIncrement;
				else
				{
					StoreIncrements();
					_updateTimer.Stop();
					this.Close();
				}
			}

			// Paint progress bar
			if (_mBFirstLaunch == false && _mDblLastCompletionFraction < _mDblCompletionFraction)
			{
				_mDblLastCompletionFraction += _mDblPbIncrementPerTimerInterval;
				int width = (int)Math.Floor(_pnlStatus.ClientRectangle.Width * _mDblLastCompletionFraction);
				int height = _pnlStatus.ClientRectangle.Height;
				int x = _pnlStatus.ClientRectangle.X;
				int y = _pnlStatus.ClientRectangle.Y;
				if (width > 0 && height > 0)
				{
					_mRProgress = new Rectangle(x, y, width, height);
					if (!_pnlStatus.IsDisposed)
					{
						Graphics g = _pnlStatus.CreateGraphics();
						LinearGradientBrush brBackground = new LinearGradientBrush(_mRProgress, Color.FromArgb(58, 96, 151), Color.FromArgb(181, 237, 254), LinearGradientMode.Horizontal);
						g.FillRectangle(brBackground, _mRProgress);
						g.Dispose();
					}
					int iSecondsLeft = 1 + (int)(TimerInterval * ((1.0 - _mDblLastCompletionFraction) / _mDblPbIncrementPerTimerInterval)) / 1000;
					_mSTimeRemaining = (iSecondsLeft == 1) ? string.Format("1 second remaining") : string.Format("{0} seconds remaining", iSecondsLeft);
				}
			}
			//lblTimeRemaining.Text = m_sTimeRemaining;
		}

		// Close the form if they double click on it.
		private void SplashScreen_DoubleClick(object sender, System.EventArgs e)
		{
			// Use the overload that doesn't set the parent form to this very window.
			CloseForm();
		}
        #endregion Event Handlers

        /// <summary>
        /// 
        /// </summary>
        public static void ShowSplashScreenEvents()
        {
            ShowSplashScreen();
            Application.DoEvents();
			SplashTestPad.SetStatus("ODA activated..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("ODA Runtime..");
            Thread.Sleep(_ssProgressDelay*2);
            SplashTestPad.SetStatus("ODA Modules..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Prism Modules..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("IOC Container..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Model Views..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Model ViewModels..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Common Libs..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Avalon Dock Manager..");
            Thread.Sleep(_ssProgressDelay*2);
            SplashTestPad.SetStatus("Profiler initiated..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Overlay loading..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Settings loading..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Settings parsing..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("HCL Services..");
            Thread.Sleep(_ssProgressDelay*2);
            SplashTestPad.SetStatus("Main Window layouting..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("SeriLog creating..");
            Thread.Sleep(_ssProgressDelay);
            SplashTestPad.SetStatus("Starting HCL-ODA-TestPAD..");
            Thread.Sleep(_ssProgressDelay*3);
            CloseForm();
        }
    }

    #region Auxiliary Classes 
    /// <summary>
    /// A specialized class for managing XML storage for the splash screen.
    /// </summary>
    internal class SplashScreenXmlStorage
	{
		private static string _msStoredValues = "SplashScreen.xml";
		private static string _msDefaultPercents = "";
		private static string _msDefaultIncrement = ".015";


		// Get or set the string storing the percentage complete at each checkpoint.
		static public string Percents
		{
			get { return GetValue("Percents", _msDefaultPercents); }
			set { SetValue("Percents", value); }
		}
		// Get or set how much time passes between updates.
		static public string Interval
		{
			get { return GetValue("Interval", _msDefaultIncrement); }
			set { SetValue("Interval", value); }
		}

		// Store the file in a location where it can be written with only User rights. (Don't use install directory).
		static private string StoragePath
		{
			get {return Path.Combine(Application.UserAppDataPath, _msStoredValues);}
		}

		// Helper method for getting inner text of named element.
		static private string GetValue(string name, string defaultValue)
		{
			if (!File.Exists(StoragePath))
				return defaultValue;

			try
			{
				XmlDocument docXml = new XmlDocument();
				docXml.Load(StoragePath);
				XmlElement elValue = docXml.DocumentElement.SelectSingleNode(name) as XmlElement;
				return (elValue == null) ? defaultValue : elValue.InnerText;
			}
			catch
			{
				return defaultValue;
			}
		}

		// Helper method for setting inner text of named element.  Creates document if it doesn't exist.
		static public void SetValue(string name,
			 string stringValue)
		{
			XmlDocument docXml = new XmlDocument();
			XmlElement elRoot = null;
			if (!File.Exists(StoragePath))
			{
				elRoot = docXml.CreateElement("root");
				docXml.AppendChild(elRoot);
			}
			else
			{
				docXml.Load(StoragePath);
				elRoot = docXml.DocumentElement;
			}
			XmlElement value = docXml.DocumentElement.SelectSingleNode(name) as XmlElement;
			if (value == null)
			{
				value = docXml.CreateElement(name);
				elRoot.AppendChild(value);
			}
			value.InnerText = stringValue;
			docXml.Save(StoragePath);
    }
}
	#endregion Auxiliary Classes
}

