using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HCL_ODA_TestPAD.Splash
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AboutTestPad : Form
    {
        private EventHandler _showGuiEvent = null;
        private float _angle = 0;
        private int _counter = 0;

        /// <summary>
        /// 
        /// </summary>
        public AboutTestPad()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="del"></param>
        public void SuperLogo(EventHandler del)
        {
            _showGuiEvent = del;
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartLogoThread()
        {
            Show();
        }

        private void OnLoadAboutLogo(object sender, EventArgs e)
        {
            BackColor = Color.BlueViolet;
            string strText = "  HCL-ODA-TestPAD \n" +
                             "                   ¤     \n" +
                             "         BU-MES-II    \n" +
                             "                   ¤     \n" +
                             "    Copyright © 2023 \n" +
                             "                   ¤     \n" +
                             "          HILTI AG      ";

            Graphics grfx = CreateGraphics();
            GraphicsPath path = new GraphicsPath();
            FontFamily family = new FontFamily("Times New Roman");
            FontStyle fontStyle = FontStyle.Bold;
            float emSize = 30;
            Font font = new Font(family, emSize, fontStyle);
            float fFontSize = PointsToPageUnits(grfx, font);
            StringFormat strFmt = new StringFormat();
            strFmt.Alignment = StringAlignment.Center;

            // Add text to the path.
            path.AddString(strText, font.FontFamily, (int)font.Style,
                fFontSize, new PointF(0, 0), new StringFormat());

            RectangleF rectfBounds = path.GetBounds();
            ClientSize = new Size((int)rectfBounds.Width + 100, (int)rectfBounds.Height + 100);
            //Location = new Point(Left - 100, Top - 200);
            GetScreenCenterLocation();
            Region = new Region(path);

            Timer timer1 = new Timer();
            timer1.Interval = 100;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Enabled = true;
        }

        private void GetScreenCenterLocation()
        {
            // Position the Form on The screen taking in account the resolution
            //
            Rectangle screenRect = Screen.GetBounds(Bounds);
            // get the Screen Boundy
            //ClientSize = new Size((int)(screenRect.Width / 2) + 100, (int)(screenRect.Height / 2)+ 200 ); // set the size of the form
            Location = new Point(screenRect.Width / 2 - ClientSize.Width / 2, screenRect.Height / 2 - ClientSize.Height / 2); // Center the Location of the form.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grfx"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public float PointsToPageUnits(Graphics grfx, Font font)
        {
            float fFontSize;

            if (grfx.PageUnit == GraphicsUnit.Display)
            {
                fFontSize = 100 * font.SizeInPoints / 72;
            }
            else
            {
                fFontSize = grfx.DpiX * font.SizeInPoints / 72;
            }
            return fFontSize;
        }

        private LinearGradientBrush GetBrush()
        {
            return new LinearGradientBrush(
                this.ClientRectangle,
                Color.Black,
                Color.Red,
                0.0F,
                true);
        }


        private void Rotate(Graphics graphics, LinearGradientBrush brush)
        {
            brush.RotateTransform(_angle);
            brush.SetBlendTriangularShape(.5F);
            graphics.FillRectangle(brush, brush.Rectangle);
        }

        private void Rotate(Graphics graphics)
        {
            _angle += 5 % 360;
            Rotate(graphics, GetBrush());
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            Rotate(CreateGraphics());
            _counter++;
            if (_counter > 100)
            {
                ((Timer)sender).Stop();
                OnShowTestGUI();
                Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Rotate(e.Graphics);
        }

        private void OnShowTestGUI()
        {
            _showGuiEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
