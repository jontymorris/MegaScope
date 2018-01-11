using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MagnaScope
{
	public partial class MegaScopeForm : Form
	{
		// Graphics
		private Graphics g;
		private Bitmap bmp;
        
		// Window
		private Rectangle screen;
		private int xPos, yPos;
		private Size size;

		// Key listener
		private Hotkeys.GlobalHotkey ghk;

		// Other
		private bool running;

		private const float screenScale = 3f;
		private const int cropScale = 6;

		public MegaScopeForm()
		{
			InitializeComponent();

			// Transparency
			this.SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = System.Drawing.Color.Transparent;
			pictureBox.BackColor = Color.Transparent;

			this.Opacity = 0;
			Cursor.Hide();

			// Window
			//screen = Screen.PrimaryScreen.Bounds;
			screen = new Rectangle(0, 0, 1920, 1080);

			this.Width = (int)(screen.Width / screenScale);
			this.Height = (int)(screen.Height / screenScale);
			pictureBox.Width = this.Width;
			pictureBox.Height = this.Height;
			
			size = new Size(screen.Width / cropScale, screen.Height / cropScale);
			xPos = (screen.Width - size.Width) / 2;
			yPos = (screen.Height - size.Height) / 2;

			// Timer
			updateTimer.Interval = 1000 / 60;

			// Key listener
			ghk = new Hotkeys.GlobalHotkey(0, Keys.Oemtilde, this);
		}
		
		// Screenshot and scale
		private void updateTimer_Tick(object sender, EventArgs e)
		{
			bmp = new Bitmap(size.Width, size.Height);

			g = this.CreateGraphics();
			g = Graphics.FromImage(bmp);

			g.CopyFromScreen(xPos, yPos, 0, 0, size);

			pictureBox.Image = bmp;
		}
		
		// Global key listener
		private void HandleHotkey()
		{
			running = !running;
			if (running)
			{
				this.Opacity = 100;
				ClickThroughWindow();

				updateTimer.Start();
			}
			else
			{
				this.Opacity = 0;

				updateTimer.Stop();
			}
		}
		
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Hotkeys.Constants.WM_HOTKEY_MSG_ID)
				HandleHotkey();
			base.WndProc(ref m);
		}

		private void Magnifiner_Load(object sender, EventArgs e)
		{
			if (ghk.Register())
				Console.WriteLine("Hotkey registered.");
			else
				Console.WriteLine("Hotkey failed to register");
		}

		private void Magnifiner_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!ghk.Unregiser())
				Console.WriteLine("Hotkey failed to unregister!");
		}

		// Click 'through' the window
		private void ClickThroughWindow()
		{
			int wl = GetWindowLong(this.Handle, GWL.ExStyle);
			wl = wl | 0x80000 | 0x20;
			SetWindowLong(this.Handle, GWL.ExStyle, wl);
			SetLayeredWindowAttributes(this.Handle, 0, 128, LWA.Alpha);
		}

		public enum GWL
		{
			ExStyle = -20
		}

		public enum WS_EX
		{
			Transparent = 0x20,
			Layered = 0x80000
		}

		public enum LWA
		{
			ColorKey = 0x1,
			Alpha = 1
		}

		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

		[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
		public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, LWA dwFlags);

	}

}
