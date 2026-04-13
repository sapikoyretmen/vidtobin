using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoFileStorage.Core; // Encoder və Decoder buradadır

namespace VideoFileStorage
{
    public partial class Form1 : Form
    {
        // Windows 11 Colors
        private readonly Color bgLayer = Color.FromArgb(32, 32, 32);
        private readonly Color bgLayer2 = Color.FromArgb(43, 43, 43);
        private readonly Color accentBlue = Color.FromArgb(0, 120, 212);
        private readonly Color accentRed = Color.FromArgb(196, 43, 28);
        private readonly Color textPrimary = Color.White;
        private readonly Color textSecondary = Color.FromArgb(200, 200, 200);
        private readonly Color textMuted = Color.FromArgb(150, 150, 150);

        private int selectedTab = 0;
        private Panel contentPanel;
        private Panel encodePanel, decodePanel;
        private RichTextBox txtLog;

        private ModernButton btnSelectFile, btnSelectOutput, btnEncode;
        private ModernButton btnSelectVideo, btnSelectFolder, btnDecode;
        private Label lblFile, lblOutput, lblVideo, lblFolder;
        private ModernProgressBar barEncode, barDecode;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            this.Text = "VidToBin";
            this.Size = new Size(800, 480);
            this.BackColor = bgLayer;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI Variable Text", 9F);

            // Icon
            try { if (File.Exists("icon.ico")) this.Icon = new Icon("icon.ico"); }
            catch { }

            // Main layout
            Panel tabBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = bgLayer,
                Padding = new Padding(16, 8, 16, 0)
            };

            var btnTabEncode = CreateTabButton("Encode", 0);
            var btnTabDecode = CreateTabButton("Decode", 1);
            btnTabEncode.Location = new Point(16, 8);
            btnTabDecode.Location = new Point(100, 8);
            tabBar.Controls.AddRange(new Control[] { btnTabEncode, btnTabDecode });

            Panel logPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                BackColor = Color.FromArgb(28, 28, 28),
                Padding = new Padding(1)
            };

            txtLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                BorderStyle = BorderStyle.None,
                Font = new Font("Cascadia Mono", 9F),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            logPanel.Controls.Add(txtLog);

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bgLayer,
                Padding = new Padding(32, 24, 32, 24)
            };

            encodePanel = CreateEncodePanel();
            decodePanel = CreateDecodePanel();
            decodePanel.Visible = false;

            contentPanel.Controls.Add(encodePanel);
            contentPanel.Controls.Add(decodePanel);

            this.Controls.Add(contentPanel);
            this.Controls.Add(tabBar);
            this.Controls.Add(logPanel);

            btnTabEncode.Click += (s, e) => SwitchTab(0);
            btnTabDecode.Click += (s, e) => SwitchTab(1);
        }

        private Button CreateTabButton(string text, int index)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = index == 0 ? bgLayer2 : Color.Transparent,
                ForeColor = index == 0 ? textPrimary : textSecondary,
                Font = new Font("Segoe UI Variable Display", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = index
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Paint += (s, e) =>
            {
                if ((int)btn.Tag == selectedTab)
                    e.Graphics.FillRectangle(new SolidBrush(accentBlue),
                        new Rectangle(0, btn.Height - 3, btn.Width, 3));
            };
            return btn;
        }

        private void SwitchTab(int index)
        {
            selectedTab = index;
            encodePanel.Visible = index == 0;
            decodePanel.Visible = index == 1;

            foreach (Control c in this.Controls[1].Controls)
            {
                if (c is Button btn && btn.Tag != null)
                {
                    bool active = (int)btn.Tag == index;
                    btn.BackColor = active ? bgLayer2 : Color.Transparent;
                    btn.ForeColor = active ? textPrimary : textSecondary;
                    btn.Invalidate();
                }
            }
        }

        private Panel CreateEncodePanel()
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = bgLayer };
            int y = 10;
            int gap = 60;

            btnSelectFile = new ModernButton("Select File", accentBlue, 130);
            btnSelectFile.Location = new Point(0, y);
            btnSelectFile.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == DialogResult.OK) lblFile.Text = dlg.FileName;
            };

            lblFile = new Label
            {
                Location = new Point(140, y + 10),
                Size = new Size(600, 20),
                ForeColor = textMuted,
                Font = new Font("Segoe UI Variable Text", 9F, FontStyle.Italic),
                AutoEllipsis = true,
                Text = "No file selected"
            };

            btnSelectOutput = new ModernButton("Output Path", accentBlue, 130);
            btnSelectOutput.Location = new Point(0, y + gap);
            btnSelectOutput.Click += (s, e) =>
            {
                using var dlg = new SaveFileDialog { Filter = "MP4 Video|*.mp4" };
                if (dlg.ShowDialog() == DialogResult.OK) lblOutput.Text = dlg.FileName;
            };

            lblOutput = new Label
            {
                Location = new Point(140, y + gap + 10),
                Size = new Size(600, 20),
                ForeColor = textMuted,
                Font = new Font("Segoe UI Variable Text", 9F, FontStyle.Italic),
                AutoEllipsis = true,
                Text = "No output path"
            };

            btnEncode = new ModernButton("START ENCODING", accentBlue, 180, true);
            btnEncode.Location = new Point(0, y + gap * 2 + 10);
            btnEncode.Click += async (s, e) => await HandleEncodeAsync();

            barEncode = new ModernProgressBar();
            barEncode.Location = new Point(200, y + gap * 2 + 18);
            barEncode.Size = new Size(520, 20);

            p.Controls.AddRange(new Control[] { btnSelectFile, lblFile, btnSelectOutput, lblOutput, btnEncode, barEncode });
            return p;
        }

        private Panel CreateDecodePanel()
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = bgLayer };
            int y = 10;
            int gap = 60;

            btnSelectVideo = new ModernButton("Select Video", accentBlue, 130);
            btnSelectVideo.Location = new Point(0, y);
            btnSelectVideo.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog { Filter = "Video Files|*.mp4;*.avi" };
                if (dlg.ShowDialog() == DialogResult.OK) lblVideo.Text = dlg.FileName;
            };

            lblVideo = new Label
            {
                Location = new Point(140, y + 10),
                Size = new Size(600, 20),
                ForeColor = textMuted,
                Font = new Font("Segoe UI Variable Text", 9F, FontStyle.Italic),
                AutoEllipsis = true,
                Text = "No video selected"
            };

            btnSelectFolder = new ModernButton("Output Folder", accentBlue, 130);
            btnSelectFolder.Location = new Point(0, y + gap);
            btnSelectFolder.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK) lblFolder.Text = dlg.SelectedPath;
            };

            lblFolder = new Label
            {
                Location = new Point(140, y + gap + 10),
                Size = new Size(600, 20),
                ForeColor = textMuted,
                Font = new Font("Segoe UI Variable Text", 9F, FontStyle.Italic),
                AutoEllipsis = true,
                Text = "No folder selected"
            };

            btnDecode = new ModernButton("START DECODING", accentRed, 180, true);
            btnDecode.Location = new Point(0, y + gap * 2 + 10);
            btnDecode.Click += async (s, e) => await HandleDecodeAsync();

            barDecode = new ModernProgressBar();
            barDecode.Location = new Point(200, y + gap * 2 + 18);
            barDecode.Size = new Size(520, 20);

            p.Controls.AddRange(new Control[] { btnSelectVideo, lblVideo, btnSelectFolder, lblFolder, btnDecode, barDecode });
            return p;
        }

        // ========== ACTUAL FUNCTIONALITY ==========

        private async Task HandleEncodeAsync()
        {
            if (!File.Exists(lblFile.Text))
            {
                MessageBox.Show("Please select input file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(lblOutput.Text))
            {
                MessageBox.Show("Please select output path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetUIState(false);
            barEncode.Value = 0;
            Log("ENCODE: Job started...");

            try
            {
                var encoder = new Encoder();
                var progress = new Progress<int>(v => barEncode.Value = v);
                await Task.Run(() => encoder.EncodeFileToVideo(lblFile.Text, lblOutput.Text, progress, Log));
                Log("ENCODE: Success! File created.");
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"Encoding failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true);
            }
        }

        private async Task HandleDecodeAsync()
        {
            if (!File.Exists(lblVideo.Text))
            {
                MessageBox.Show("Please select video!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(lblFolder.Text))
            {
                MessageBox.Show("Please select output folder!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetUIState(false);
            barDecode.Value = 0;
            Log("DECODE: Job started...");

            try
            {
                var decoder = new Decoder();
                var progress = new Progress<int>(v => barDecode.Value = Math.Min(100, v));
                await Task.Run(() => decoder.DecodeVideoToFile(lblVideo.Text, lblFolder.Text, progress, Log));
                Log("DECODE: Success! File extracted.");
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"Decoding failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true);
            }
        }

        private void SetUIState(bool enabled)
        {
            btnEncode.Enabled = btnDecode.Enabled = enabled;
            btnSelectFile.Enabled = btnSelectOutput.Enabled = enabled;
            btnSelectVideo.Enabled = btnSelectFolder.Enabled = enabled;
            this.Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
                return;
            }

            txtLog.SelectionStart = txtLog.TextLength;

            if (message.Contains("Success"))
                txtLog.SelectionColor = Color.FromArgb(0, 255, 136);
            else if (message.Contains("ERROR") || message.Contains("Error"))
                txtLog.SelectionColor = Color.FromArgb(255, 100, 100);
            else if (message.Contains("started"))
                txtLog.SelectionColor = accentBlue;
            else
                txtLog.SelectionColor = textSecondary;

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            txtLog.ScrollToCaret();
        }

        // Modern UI Controls
        public class ModernButton : Button
        {
            private bool isPrimary;
            private Color baseColor;
            private bool hover = false;

            public ModernButton(string text, Color color, int width, bool primary = false)
            {
                this.Text = text;
                this.baseColor = color;
                this.isPrimary = primary;
                this.Size = new Size(width, 36);
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.ForeColor = Color.White;
                this.Font = new Font("Segoe UI Variable Text",
                    isPrimary ? 9F : 9F,
                    isPrimary ? FontStyle.Bold : FontStyle.Regular);
                this.Cursor = Cursors.Hand;

                this.MouseEnter += (s, e) => { hover = true; Invalidate(); };
                this.MouseLeave += (s, e) => { hover = false; Invalidate(); };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color bg = hover ? ControlPaint.Light(baseColor, 0.1f) : baseColor;

                using (GraphicsPath path = GetRoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 6))
                {
                    e.Graphics.FillPath(new SolidBrush(bg), path);
                    TextRenderer.DrawText(e.Graphics, Text, Font,
                        new Rectangle(0, 0, Width, Height),
                        ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        public class ModernProgressBar : Control
        {
            private int val = 0;

            public int Value
            {
                get => val;
                set { val = Math.Min(100, Math.Max(0, value)); Invalidate(); }
            }

            public ModernProgressBar()
            {
                this.Size = new Size(200, 20);
                this.BackColor = Color.FromArgb(60, 60, 60);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath bg = GetRoundedRect(ClientRectangle, 6))
                {
                    e.Graphics.FillPath(new SolidBrush(BackColor), bg);
                }

                if (val > 0)
                {
                    int w = (int)((Width - 4) * val / 100.0);
                    Rectangle fillRect = new Rectangle(2, 2, w, Height - 4);
                    using (GraphicsPath fill = GetRoundedRect(fillRect, 4))
                    {
                        using (LinearGradientBrush b = new LinearGradientBrush(
                            fillRect,
                            Color.FromArgb(0, 150, 240),
                            Color.FromArgb(0, 120, 212),
                            0f))
                        {
                            e.Graphics.FillPath(b, fill);
                        }
                    }
                }
            }
        }

        private static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
