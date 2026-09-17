using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace StreamMaskApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }
    }

    public class AppConfig
    {
        public uint Modifiers { get; set; }
        public uint Key { get; set; }
        public Color BorderColor { get; set; }

        private string GetConfigPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        }

        public AppConfig()
        {
            // VarsayÄ±lan: Ctrl + Shift + Z
            // MOD_CONTROL = 0x0002, MOD_SHIFT = 0x0004 => 0x0006
            Modifiers = 0x0006;
            Key = 0x5A; // Keys.Z
            BorderColor = Color.Red;
        }

        public void Save()
        {
            try
            {
                string json = string.Format("{{\r\n  \"Modifiers\": {0},\r\n  \"Key\": {1},\r\n  \"Color\": {2}\r\n}}", Modifiers, Key, BorderColor.ToArgb());
                File.WriteAllText(GetConfigPath(), json);
            }
            catch {}
        }

        public void Load()
        {
            try
            {
                string path = GetConfigPath();
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    var modMatch = System.Text.RegularExpressions.Regex.Match(content, @"""Modifiers""\s*:\s*(\d+)");
                    if (modMatch.Success)
                    {
                        uint m;
                        if (uint.TryParse(modMatch.Groups[1].Value, out m)) Modifiers = m;
                    }

                    var keyMatch = System.Text.RegularExpressions.Regex.Match(content, @"""Key""\s*:\s*(\d+)");
                    if (keyMatch.Success)
                    {
                        uint k;
                        if (uint.TryParse(keyMatch.Groups[1].Value, out k)) Key = k;
                    }

                    var colorMatch = System.Text.RegularExpressions.Regex.Match(content, @"""Color""\s*:\s*(-?\d+)");
                    if (colorMatch.Success)
                    {
                        int c;
                        if (int.TryParse(colorMatch.Groups[1].Value, out c)) BorderColor = Color.FromArgb(c);
                    }
                }
            }
            catch {}
        }
    }

    public class AppContext : ApplicationContext
    {
        private MessageWindow msgWindow;
        private SelectionForm selectionForm;
        private MaskForm maskForm;
        private bool isMasking = false;
        private NotifyIcon trayIcon;
        public AppConfig Config { get; private set; }

        public AppContext() {
            Config = new AppConfig();
            Config.Load();

            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application; // Basit bir ikon
            trayIcon.Visible = true;
            trayIcon.Text = "StreamMasker";
            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Ayarlar", (s, e) => OpenSettings());
            menu.MenuItems.Add("Ã‡Ä±kÄ±ÅŸ", (s, e) => Application.Exit());
            trayIcon.ContextMenu = menu;

            msgWindow = new MessageWindow(this);
            RegisterHotkey();
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            OpenSettings();
        }

        private SettingsForm settingsFormInstance;

        private void OpenSettings()
        {
            if (settingsFormInstance != null && !settingsFormInstance.IsDisposed)
            {
                settingsFormInstance.Activate();
                return;
            }
            settingsFormInstance = new SettingsForm(this);
            settingsFormInstance.Show();
        }

        public void ApplyConfig()
        {
            NativeMethods.UnregisterHotKey(msgWindow.Handle, 1);
            RegisterHotkey();

            if (maskForm != null)
            {
                maskForm.UpdateBorderColor(Config.BorderColor);
            }
        }

        private void RegisterHotkey()
        {
            if (!NativeMethods.RegisterHotKey(msgWindow.Handle, 1, Config.Modifiers, Config.Key))
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                File.AppendAllText(logPath, "Hotkey failed to register: " + Config.Key + "\n");
                MessageBox.Show("KÄ±sayol tuÅŸu kaydedilemedi! BaÅŸka bir uygulama tarafÄ±ndan kullanÄ±lÄ±yor olabilir.", "StreamMasker Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void HandleHotkey() 
        { 
            System.Media.SystemSounds.Beep.Play();
            if (isMasking || maskForm != null)
            {
                StopMasking();
            }
            else if (selectionForm == null)
            {
                StartSelection();
            }
            else
            {
                selectionForm.Close();
                selectionForm = null;
            }
        }

        private void StartSelection()
        {
            selectionForm = new SelectionForm(this);
            selectionForm.Show(); 
            selectionForm.Activate(); 
            selectionForm.Focus();
        }

        public void OnSelected(Rectangle rect)
        {
            if (selectionForm != null)
            {
                selectionForm.Close();
                selectionForm = null;
            }

            if (rect.Width > 10 && rect.Height > 10)
            {
                maskForm = new MaskForm(rect, Config.BorderColor);
                maskForm.Show();
                isMasking = true;
            }
        }

        private void StopMasking()
        {
            if (maskForm != null)
            {
                maskForm.Close();
                maskForm = null;
            }
            isMasking = false;
        }

        protected override void ExitThreadCore() {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            if (msgWindow != null)
            {
                NativeMethods.UnregisterHotKey(msgWindow.Handle, 1);
                msgWindow.DestroyHandle();
            }
            base.ExitThreadCore();
        }
    }

    public class SettingsForm : Form
    {
        private AppContext context;
        private TextBox txtHotkey;
        private Button btnColor;
        private Button btnSave;
        
        private uint tempModifiers;
        private uint tempKey;
        private Color tempColor;

        public SettingsForm(AppContext context)
        {
            this.context = context;
            this.Text = "StreamMasker AyarlarÄ±";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;

            tempModifiers = context.Config.Modifiers;
            tempKey = context.Config.Key;
            tempColor = context.Config.BorderColor;

            Label lblHotkey = new Label() { Text = "KÄ±sayol TuÅŸu:", Location = new Point(20, 25), AutoSize = true };
            txtHotkey = new TextBox() { Location = new Point(120, 22), Width = 140, ReadOnly = true };
            txtHotkey.Text = GetHotkeyString(tempModifiers, (Keys)tempKey);
            txtHotkey.KeyDown += TxtHotkey_KeyDown;

            Label lblInfo = new Label() { Text = "(DeÄŸiÅŸtirmek iÃ§in kutuya tÄ±klayÄ±p tuÅŸa basÄ±n)", Location = new Point(20, 50), AutoSize = true, ForeColor = Color.Gray };

            Label lblColor = new Label() { Text = "Ã‡erÃ§eve Rengi:", Location = new Point(20, 85), AutoSize = true };
            btnColor = new Button() { Location = new Point(120, 80), Width = 60, Height = 25, BackColor = tempColor, FlatStyle = FlatStyle.Flat };
            btnColor.Click += BtnColor_Click;

            btnSave = new Button() { Text = "Kaydet", Location = new Point(100, 125), Width = 80 };
            btnSave.Click += BtnSave_Click;

            this.Controls.Add(lblHotkey);
            this.Controls.Add(txtHotkey);
            this.Controls.Add(lblInfo);
            this.Controls.Add(lblColor);
            this.Controls.Add(btnColor);
            this.Controls.Add(btnSave);
        }

        private void TxtHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            Keys key = e.KeyCode;
            if (key == Keys.ShiftKey || key == Keys.ControlKey || key == Keys.Menu || key == Keys.LWin || key == Keys.RWin)
                return; // Sadece modifier basÄ±ldÄ±ysa bekle

            uint modifiers = 0;
            if (e.Shift) modifiers |= 0x0004; // MOD_SHIFT
            if (e.Control) modifiers |= 0x0002; // MOD_CONTROL
            if (e.Alt) modifiers |= 0x0001; // MOD_ALT
            
            tempModifiers = modifiers;
            tempKey = (uint)key;
            
            txtHotkey.Text = GetHotkeyString(tempModifiers, key);
        }

        private void BtnColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = tempColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    tempColor = cd.Color;
                    btnColor.BackColor = tempColor;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            context.Config.Modifiers = tempModifiers;
            context.Config.Key = tempKey;
            context.Config.BorderColor = tempColor;
            context.Config.Save();
            context.ApplyConfig();
            this.Close();
        }

        private string GetHotkeyString(uint modifiers, Keys key)
        {
            string s = "";
            if ((modifiers & 0x0002) != 0) s += "Ctrl + ";
            if ((modifiers & 0x0004) != 0) s += "Shift + ";
            if ((modifiers & 0x0001) != 0) s += "Alt + ";
            s += key.ToString();
            return s;
        }
    }

    class MessageWindow : NativeWindow
    {
        private AppContext context;

        public MessageWindow(AppContext context)
        {
            this.context = context;
            this.CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == 1)
            {
                context.HandleHotkey();
            }
            base.WndProc(ref m);
        }
    }

    class SelectionForm : Form { 
        protected override CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= 0x80; return cp; } }
        private AppContext context;
        private Point startPoint;
        private Rectangle currentRect;
        private bool isDragging = false;

        public SelectionForm(AppContext context)
        {
            this.context = context;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.3; // YarÄ± saydam
            this.Cursor = Cursors.Cross;
            this.DoubleBuffered = true; this.KeyPreview = true;
            this.ShowInTaskbar = false;

            Rectangle bounds = Rectangle.Empty;
            foreach (var screen in Screen.AllScreens)
            {
                bounds = Rectangle.Union(bounds, screen.Bounds);
            }
            this.Bounds = bounds;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startPoint = e.Location;
                currentRect = new Rectangle(e.Location, new Size(0, 0));
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                int x = Math.Min(startPoint.X, e.X);
                int y = Math.Min(startPoint.Y, e.Y);
                int width = Math.Abs(startPoint.X - e.X);
                int height = Math.Abs(startPoint.Y - e.Y);
                currentRect = new Rectangle(x, y, width, height);
                this.Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (isDragging && e.Button == MouseButtons.Left)
            {
                isDragging = false;
                Rectangle screenRect = new Rectangle(currentRect.X + this.Left, currentRect.Y + this.Top, currentRect.Width, currentRect.Height);
                context.OnSelected(screenRect);
            }
            else if (e.Button == MouseButtons.Right)
            {
                context.OnSelected(Rectangle.Empty);
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isDragging && currentRect.Width > 0 && currentRect.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, currentRect);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                context.OnSelected(Rectangle.Empty);
            }
            base.OnKeyDown(e);
        }
    }

    class ClickThroughForm : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT | 0x80;
                return cp;
            }
        }
    }

    class MaskForm : ClickThroughForm
    {
        private ClickThroughForm fillForm;
        private Color borderColor;

        public MaskForm(Rectangle rect, Color borderColor)
        {
            this.borderColor = borderColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = rect;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            
            // TÄ±klama-geÃ§irgen ve transparan olmasÄ± iÃ§in (Ã‡erÃ§eve)
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;

            this.HandleCreated += MaskForm_HandleCreated;
            
            // Ä°Ã§ini dolduran ve OBS'te siyah gÃ¶rÃ¼necek form
            fillForm = new ClickThroughForm();
            fillForm.FormBorderStyle = FormBorderStyle.None;
            fillForm.StartPosition = FormStartPosition.Manual;
            fillForm.Bounds = rect;
            fillForm.TopMost = true;
            fillForm.ShowInTaskbar = false;
            fillForm.BackColor = Color.Black;
            // DWM'nin pencereyi Ã§izmesi ama kullanÄ±cÄ±nÄ±n neredeyse hiÃ§ gÃ¶rmemesi iÃ§in %1 opaklÄ±k
            fillForm.Opacity = 0.01; 
            
            fillForm.HandleCreated += FillForm_HandleCreated;
        }

        public void UpdateBorderColor(Color newColor)
        {
            this.borderColor = newColor;
            this.Invalidate();
        }

        public new void Show()
        {
            fillForm.Show();
            base.Show(); // Ã‡erÃ§eveyi Ã¼stte gÃ¶ster
        }

        public new void Close()
        {
            fillForm.Close();
            base.Close();
        }

        private void FillForm_HandleCreated(object sender, EventArgs e)
        {
            // YayÄ±nda siyah (DRM maskesi) olarak gÃ¶rÃ¼nmesi iÃ§in WDA_MONITOR
            NativeMethods.SetWindowDisplayAffinity(fillForm.Handle, NativeMethods.WDA_MONITOR);
        }

        private void MaskForm_HandleCreated(object sender, EventArgs e)
        {
            // Ã‡erÃ§evenin OBS'te hiÃ§ gÃ¶rÃ¼nmemesi iÃ§in WDA_EXCLUDEFROMCAPTURE
            NativeMethods.SetWindowDisplayAffinity(this.Handle, NativeMethods.WDA_EXCLUDEFROMCAPTURE);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(borderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, this.Width - 2, this.Height - 2);
            }
        }
    }

    static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);

        public const uint WDA_NONE = 0x00000000;
        public const uint WDA_MONITOR = 0x00000001;
        public const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
    }
}

