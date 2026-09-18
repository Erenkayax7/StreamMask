using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            try
            {
                Application.Run(new AppContext());
            }
            catch (Exception ex)
            {
                File.WriteAllText("crash.log", ex.ToString());
            }
        }
    }

    public class AppConfig
    {
        public uint Modifiers { get; set; }
        public uint Key { get; set; }
        public Color BorderColor { get; set; }
        public int BorderThickness { get; set; }
        public string Theme { get; set; }

        private string GetConfigPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        }

        public AppConfig()
        {
            Modifiers = 0x0006; // Ctrl + Shift
            Key = 0x5A; // Z
            BorderColor = Color.Red;
            BorderThickness = 2;
            Theme = "Dark";
        }

        public void Save()
        {
            try
            {
                string json = string.Format("{{\r\n  \"Modifiers\": {0},\r\n  \"Key\": {1},\r\n  \"Color\": {2},\r\n  \"BorderThickness\": {3},\r\n  \"Theme\": \"{4}\"\r\n}}",
                    Modifiers, Key, BorderColor.ToArgb(), BorderThickness, Theme);
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
                    uint m; if (modMatch.Success && uint.TryParse(modMatch.Groups[1].Value, out m)) Modifiers = m;

                    var keyMatch = System.Text.RegularExpressions.Regex.Match(content, @"""Key""\s*:\s*(\d+)");
                    uint k; if (keyMatch.Success && uint.TryParse(keyMatch.Groups[1].Value, out k)) Key = k;

                    var colorMatch = System.Text.RegularExpressions.Regex.Match(content, @"""Color""\s*:\s*(-?\d+)");
                    int c; if (colorMatch.Success && int.TryParse(colorMatch.Groups[1].Value, out c)) BorderColor = Color.FromArgb(c);

                    var thickMatch = System.Text.RegularExpressions.Regex.Match(content, @"""BorderThickness""\s*:\s*(\d+)");
                    int t; if (thickMatch.Success && int.TryParse(thickMatch.Groups[1].Value, out t)) BorderThickness = Math.Max(1, Math.Min(10, t));

                    var themeMatch = System.Text.RegularExpressions.Regex.Match(content, @"""Theme""\s*:\s*""([^""]+)""");
                    if (themeMatch.Success) Theme = themeMatch.Groups[1].Value;
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
        private NotifyIcon trayIcon;
        private bool isMasking = false;
        private SettingsForm settingsFormInstance = null;

        public AppConfig Config { get; private set; }

        public AppContext()
        {
            Config = new AppConfig();
            Config.Load();

            msgWindow = new MessageWindow(this);
            RegisterAppHotkey();
            InitializeTrayIcon();
        }

        public bool RegisterAppHotkey()
        {
            NativeMethods.UnregisterHotKey(msgWindow.Handle, 1);
            bool success = NativeMethods.RegisterHotKey(msgWindow.Handle, 1, Config.Modifiers, Config.Key);
            if (!success)
            {
                MessageBox.Show("The selected hotkey could not be registered! Another application might be using it.", "Hotkey Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return success;
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Brush b = new SolidBrush(Color.Red))
                {
                    g.FillRectangle(b, 2, 4, 12, 8);
                }
                using (Brush b = new SolidBrush(Color.Black))
                {
                    g.FillEllipse(b, 4, 6, 3, 3);
                    g.FillEllipse(b, 9, 6, 3, 3);
                }
            }
            trayIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            trayIcon.Text = "StreamMask";
            trayIcon.Visible = true;

            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem itemSettings = new ToolStripMenuItem("Settings", null, (s, e) => OpenSettings());
            ToolStripMenuItem itemExit = new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit());

            menu.Items.Add(itemSettings);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemExit);

            trayIcon.ContextMenuStrip = menu;
            trayIcon.DoubleClick += (s, e) => OpenSettings();
        }

        public void OpenSettings()
        {
            if (settingsFormInstance != null && !settingsFormInstance.IsDisposed)
            {
                settingsFormInstance.BringToFront();
                settingsFormInstance.Activate();
                return;
            }
            settingsFormInstance = new SettingsForm(this);
            settingsFormInstance.Show();
        }

        public void HandleHotkey()
        {
            if (isMasking || maskForm != null)
            {
                StopMasking();
            }
            else
            {
                StartSelection();
            }
        }

        private void StartSelection()
        {
            if (selectionForm == null)
            {
                selectionForm = new SelectionForm(this);
                selectionForm.Show();
                selectionForm.Activate();
                selectionForm.Focus();
            }
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
                maskForm = new MaskForm(rect, Config.BorderColor, Config.BorderThickness);
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

        protected override void ExitThreadCore()
        {
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
        private Button btnHotkey;
        private Button btnColor;
        private TrackBar trackThickness;
        private Label lblThicknessVal;
        private ComboBox cmbTheme;
        private Panel pnlPreview;
        private Button btnSave;
        private Button btnCancel;

        private Label lblPreviewTitle;
        private Label lblHotkeyTitle;
        private Label lblColorTitle;
        private Label lblThicknessTitle;
        private Label lblThemeTitle;

        private uint tempModifiers;
        private uint tempKey;
        private Color tempColor;
        private int tempThickness;
        private string tempTheme;

        private bool isListeningHotkey = false;

        public SettingsForm(AppContext context)
        {
            this.context = context;
            this.Text = "StreamMask - Settings";
            this.Size = new Size(390, 470);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
            this.KeyPreview = true;

            tempModifiers = context.Config.Modifiers;
            tempKey = context.Config.Key;
            tempColor = context.Config.BorderColor;
            tempThickness = context.Config.BorderThickness;
            tempTheme = string.IsNullOrEmpty(context.Config.Theme) ? "Dark" : context.Config.Theme;

            InitializeComponents();
            ApplyTheme(tempTheme);
            this.KeyDown += SettingsForm_KeyDown;
        }

        private void InitializeComponents()
        {
            lblPreviewTitle = new Label() { Text = "LIVE PREVIEW", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            pnlPreview = new Panel() { Location = new Point(20, 38), Size = new Size(335, 80), BorderStyle = BorderStyle.None };
            pnlPreview.Paint += PnlPreview_Paint;

            lblHotkeyTitle = new Label() { Text = "Hotkey:", Font = new Font("Segoe UI", 9f), Location = new Point(20, 135), AutoSize = true };
            btnHotkey = new Button() { Location = new Point(140, 130), Size = new Size(215, 32), Font = new Font("Segoe UI", 9f, FontStyle.Bold), Text = GetHotkeyString(tempModifiers, (Keys)tempKey), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnHotkey.FlatAppearance.BorderSize = 1;
            btnHotkey.Click += BtnHotkey_Click;

            lblColorTitle = new Label() { Text = "Border Color:", Font = new Font("Segoe UI", 9f), Location = new Point(20, 182), AutoSize = true };
            btnColor = new Button() { Location = new Point(140, 175), Size = new Size(215, 32), Font = new Font("Segoe UI", 9f), Text = "Select Color...", FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnColor.FlatAppearance.BorderSize = 1;
            btnColor.Click += BtnColor_Click;

            lblThicknessTitle = new Label() { Text = "Border Thickness:", Font = new Font("Segoe UI", 9f), Location = new Point(20, 230), AutoSize = true };
            trackThickness = new TrackBar() { Location = new Point(135, 225), Size = new Size(170, 35), Minimum = 1, Maximum = 10, Value = tempThickness, TickStyle = TickStyle.None };
            trackThickness.ValueChanged += (s, e) => { tempThickness = trackThickness.Value; lblThicknessVal.Text = tempThickness.ToString() + " px"; pnlPreview.Invalidate(); };
            
            lblThicknessVal = new Label() { Text = tempThickness.ToString() + " px", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Location = new Point(312, 230), AutoSize = true };
            lblThemeTitle = new Label() { Text = "Theme:", Font = new Font("Segoe UI", 9f), Location = new Point(20, 280), AutoSize = true };
            cmbTheme = new ComboBox() { Location = new Point(140, 277), Size = new Size(215, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            cmbTheme.Items.AddRange(new object[] { "Dark Theme", "Light Theme" });
            cmbTheme.SelectedIndex = tempTheme == "Light" ? 1 : 0;
            cmbTheme.SelectedIndexChanged += (s, e) => { tempTheme = cmbTheme.SelectedIndex == 1 ? "Light" : "Dark"; ApplyTheme(tempTheme); };

            btnSave = new Button() { Text = "Save", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Location = new Point(155, 370), Size = new Size(95, 34), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Cancel", Font = new Font("Segoe UI", 9f), Location = new Point(260, 370), Size = new Size(95, 34), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblPreviewTitle);
            this.Controls.Add(pnlPreview);
            this.Controls.Add(lblHotkeyTitle);
            this.Controls.Add(btnHotkey);
            this.Controls.Add(lblColorTitle);
            this.Controls.Add(btnColor);
            this.Controls.Add(lblThicknessTitle);
            this.Controls.Add(trackThickness);
            this.Controls.Add(lblThicknessVal);
            this.Controls.Add(lblThemeTitle);
            this.Controls.Add(cmbTheme);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void PnlPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color previewBg = tempTheme == "Dark" ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 245);
            g.Clear(previewBg);

            Rectangle outer = new Rectangle(15, 12, pnlPreview.Width - 30, pnlPreview.Height - 24);
            using (Brush b = new SolidBrush(tempTheme == "Dark" ? Color.FromArgb(28, 28, 28) : Color.White))
            {
                g.FillRectangle(b, outer);
            }

            using (Font f = new Font("Segoe UI", 8.5f, FontStyle.Italic))
            using (Brush tb = new SolidBrush(tempTheme == "Dark" ? Color.FromArgb(170, 170, 170) : Color.FromArgb(90, 90, 90)))
            {
                string txt = string.Format("Masked Area (OBS: Black | You: Transparent + {0}px Border)", tempThickness);
                StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(txt, f, tb, outer, sf);
            }

            using (Pen p = new Pen(tempColor, tempThickness))
            {
                p.Alignment = PenAlignment.Inset;
                g.DrawRectangle(p, outer);
            }
        }

        private void ApplyTheme(string theme)
        {
            bool isDark = theme == "Dark";

            Color bg = isDark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(243, 243, 243);
            Color fg = isDark ? Color.White : Color.FromArgb(20, 20, 20);
            Color inputBg = isDark ? Color.FromArgb(45, 45, 48) : Color.White;
            Color borderColor = isDark ? Color.FromArgb(65, 65, 65) : Color.FromArgb(200, 200, 200);

            this.BackColor = bg;
            this.ForeColor = fg;

            lblPreviewTitle.ForeColor = isDark ? Color.FromArgb(0, 120, 215) : Color.FromArgb(0, 102, 204);
            lblHotkeyTitle.ForeColor = fg;
            lblColorTitle.ForeColor = fg;
            lblThicknessTitle.ForeColor = fg;
            lblThicknessVal.ForeColor = fg;
            lblThemeTitle.ForeColor = fg;

            btnHotkey.BackColor = inputBg;
            btnHotkey.ForeColor = fg;
            btnHotkey.FlatAppearance.BorderColor = borderColor;

            btnColor.BackColor = inputBg;
            btnColor.ForeColor = fg;
            btnColor.FlatAppearance.BorderColor = borderColor;

            cmbTheme.BackColor = inputBg;
            cmbTheme.ForeColor = fg;

            btnSave.BackColor = Color.FromArgb(0, 120, 215);
            btnSave.ForeColor = Color.White;

            btnCancel.BackColor = inputBg;
            btnCancel.ForeColor = fg;
            btnCancel.FlatAppearance.BorderColor = borderColor;

            pnlPreview.Invalidate();
        }

        private void BtnHotkey_Click(object sender, EventArgs e)
        {
            isListeningHotkey = true;
            btnHotkey.Text = ". . .";
            btnHotkey.BackColor = Color.FromArgb(0, 120, 215);
            btnHotkey.ForeColor = Color.White;
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isListeningHotkey) return;

            e.SuppressKeyPress = true;
            Keys key = e.KeyCode;

            if (key == Keys.ShiftKey || key == Keys.ControlKey || key == Keys.Menu || key == Keys.LWin || key == Keys.RWin)
                return;

            if (key == Keys.Escape)
            {
                isListeningHotkey = false;
                btnHotkey.Text = GetHotkeyString(tempModifiers, (Keys)tempKey);
                ApplyTheme(tempTheme);
                return;
            }

            uint modifiers = 0;
            if (e.Shift) modifiers |= 0x0004;
            if (e.Control) modifiers |= 0x0002;
            if (e.Alt) modifiers |= 0x0001;

            tempModifiers = modifiers;
            tempKey = (uint)key;
            isListeningHotkey = false;

            btnHotkey.Text = GetHotkeyString(tempModifiers, key);
            ApplyTheme(tempTheme);
        }

        private void BtnColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = tempColor;
                cd.FullOpen = true;
                if (cd.ShowDialog(this) == DialogResult.OK)
                {
                    tempColor = cd.Color;
                    pnlPreview.Invalidate();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            context.Config.Modifiers = tempModifiers;
            context.Config.Key = tempKey;
            context.Config.BorderColor = tempColor;
            context.Config.BorderThickness = tempThickness;
            context.Config.Theme = tempTheme;
            context.Config.Save();

            context.RegisterAppHotkey();
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

    class SelectionForm : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

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
            this.Opacity = 0.3;
            this.Cursor = Cursors.Cross;
            this.DoubleBuffered = true;
            this.KeyPreview = true;
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
            if (isDragging)
            {
                isDragging = false;
                Rectangle screenRect = new Rectangle(
                    this.Left + currentRect.X,
                    this.Top + currentRect.Y,
                    currentRect.Width,
                    currentRect.Height
                );
                context.OnSelected(screenRect);
            }
            else
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
                using (Pen pen = new Pen(context.Config.BorderColor, context.Config.BorderThickness))
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
        private int borderThickness;

        public MaskForm(Rectangle rect, Color borderColor, int borderThickness)
        {
            this.borderColor = borderColor;
            this.borderThickness = borderThickness;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = rect;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;

            // Dış sınır için
            this.HandleCreated += MaskForm_HandleCreated;
            
            fillForm = new ClickThroughForm();
            fillForm.FormBorderStyle = FormBorderStyle.None;
            fillForm.StartPosition = FormStartPosition.Manual;
            fillForm.Bounds = rect;
            fillForm.TopMost = true;
            fillForm.ShowInTaskbar = false;
            
            // WDA_MONITOR requires Opacity=0.01 and BackColor=Black to appear solid black on OBS
            fillForm.BackColor = Color.Lime;
            fillForm.TransparencyKey = Color.Lime;

            fillForm.HandleCreated += (s, e) => {
                NativeMethods.SetWindowDisplayAffinity(fillForm.Handle, NativeMethods.WDA_MONITOR);
            };

            this.FormClosing += (s, e) => {
                if (fillForm != null)
                {
                    fillForm.Close();
                    fillForm = null;
                }
            };
        }

        private void MaskForm_HandleCreated(object sender, EventArgs e)
        {
            NativeMethods.SetWindowDisplayAffinity(this.Handle, NativeMethods.WDA_EXCLUDEFROMCAPTURE);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (fillForm != null && !fillForm.Visible)
            {
                fillForm.Show();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(borderColor, borderThickness))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
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
        public static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        public const uint WDA_NONE = 0x00000000;
        public const uint WDA_MONITOR = 0x00000001;
        public const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
    }
}




