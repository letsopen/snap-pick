namespace SnapPick
{
    public class ColorInfoForm : Form
    {
        private Label colorInfoLabel;
        private Panel colorPreviewPanel;

        public ColorInfoForm()
        {
            InitializeComponent();
            PositionWindow();
            
            // 添加以下代码确保窗体可见
            this.Opacity = 0.9; // 设置透明度
            this.ShowInTaskbar = false;
            this.TopMost = true;
        }

        private void InitializeComponent()
        {
            this.colorPreviewPanel = new Panel();
            this.colorInfoLabel = new Label();
            this.SuspendLayout();
            
            // colorPreviewPanel
            this.colorPreviewPanel.BackColor = Color.Black;
            this.colorPreviewPanel.Location = new Point(10, 10);
            this.colorPreviewPanel.Size = new Size(20, 20);
            this.colorPreviewPanel.BorderStyle = BorderStyle.FixedSingle;
            
            // colorInfoLabel
            this.colorInfoLabel.AutoSize = true;
            this.colorInfoLabel.Location = new Point(40, 12);
            this.colorInfoLabel.Text = "RGB: 0, 0, 0";
            this.colorInfoLabel.ForeColor = Color.White;
            
            // ColorInfoForm
            this.BackColor = Color.FromArgb(50, 50, 50);
            this.ClientSize = new Size(180, 40);
            this.Controls.Add(this.colorPreviewPanel);
            this.Controls.Add(this.colorInfoLabel);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void PositionWindow()
        {
            // 获取屏幕尺寸
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            
            // 设置窗口位置（右下角，任务栏上方）
            this.Location = new Point(
                workingArea.Right - this.Width - 10,
                workingArea.Bottom - this.Height - 10
            );
        }

        // 修改 UpdateColor 方法
        public void UpdateColor(Color color)
        {
            // 在UI线程上更新控件
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateColor(color)));
                return;
            }
            
            // 确保窗体可见
            if (!this.Visible)
            {
                this.Show();
            }
            
            colorPreviewPanel.BackColor = color;
            colorInfoLabel.Text = $"RGB: {color.R}, {color.G}, {color.B} | HEX: #{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // 创建一个无焦点、点击穿透的窗口
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000 | 0x20; // WS_EX_LAYERED | WS_EX_TRANSPARENT
                return cp;
            }
        }
    }
}