using System.Drawing.Drawing2D;
using Microsoft.Win32;

namespace SnapPick
{
    public class ColorInfoForm : Form
    {
        // 在声明时初始化字段
        private Label colorInfoLabel = new Label();
        private Panel colorPreviewPanel = new Panel();
        private Color currentColor;

        public ColorInfoForm()
        {
            InitializeComponent();
            
            // 设置窗体透明
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.Opacity = 0.85;
            
            // 确保窗体在创建后正确定位
            this.Load += (s, e) => PositionWindow();
            // 监听屏幕分辨率变化
            SystemEvents.DisplaySettingsChanged += (s, e) => PositionWindow();
        }

        private void InitializeComponent()
        {
            this.colorPreviewPanel = new Panel();
            this.colorInfoLabel = new Label();
            this.SuspendLayout();
            
            // colorPreviewPanel
            this.colorPreviewPanel.BackColor = Color.Black;
            this.colorPreviewPanel.Location = new Point(10, 10);
            this.colorPreviewPanel.Size = new Size(30, 30);
            this.colorPreviewPanel.BorderStyle = BorderStyle.FixedSingle;
            
            // colorInfoLabel
            this.colorInfoLabel.AutoSize = true;
            this.colorInfoLabel.Location = new Point(50, 10);
            this.colorInfoLabel.Text = "RGB: 0, 0, 0";
            this.colorInfoLabel.ForeColor = Color.White;
            this.colorInfoLabel.BackColor = Color.Transparent;
            this.colorInfoLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.colorInfoLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            
            // 创建一个半透明的深色背景面板
            Panel backgroundPanel = new Panel();
            backgroundPanel.BackColor = Color.FromArgb(40, 40, 40);
            backgroundPanel.Dock = DockStyle.Fill;
            backgroundPanel.Padding = new Padding(5);
            
            // 将控件添加到面板
            this.Controls.Add(backgroundPanel);
            backgroundPanel.Controls.Add(this.colorPreviewPanel);
            backgroundPanel.Controls.Add(this.colorInfoLabel);
            
            // ColorInfoForm
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.Opacity = 0.85;
            this.ClientSize = new Size(180, 50);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
            
            // 添加圆角效果
            this.Paint += (s, e) => {
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 10;
                    Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                    path.CloseAllFigures();
                    
                    this.Region = new Region(path);
                }
            };
        }

        private void PositionWindow()
        {
            try
            {
                // 获取主屏幕工作区域（排除任务栏）
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
                
                // 确保在UI线程上执行
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(PositionWindow));
                    return;
                }
                
                // 强制设置窗口位置（右下角，任务栏上方）
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(
                    workingArea.Right - this.Width - 10,
                    workingArea.Bottom - this.Height - 10
                );
                
                // 确保窗口置顶
                this.TopMost = true;
                
                // 调试信息
                Console.WriteLine($"窗口位置: {this.Location}, 屏幕工作区: {workingArea}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置窗口位置出错: {ex.Message}");
            }
        }

        public void UpdateColor(Color color)
        {
            // 在UI线程上更新控件
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateColor(color)));
                return;
            }
            
            // 保存当前颜色
            currentColor = color;
            
            // 更新UI
            colorPreviewPanel.BackColor = color;
            
            // 不再根据背景调整文字颜色，始终使用白色
            // colorInfoLabel.ForeColor = GetContrastColor(color);
            
            // 更新文字内容
            colorInfoLabel.Text = $"RGB: {color.R}, {color.G}, {color.B}\nHEX: #{color.R:X2}{color.G:X2}{color.B:X2}";
            
            // 确保窗口位置正确
            if (!this.Visible)
            {
                this.Show();
                PositionWindow();
            }
        }

        // 添加一个方法来计算对比色，确保文字在任何背景下都清晰可见
        private Color GetContrastColor(Color backgroundColor)
        {
            // 计算亮度 (基于人眼对RGB的感知)
            double brightness = (backgroundColor.R * 0.299 + 
                                 backgroundColor.G * 0.587 + 
                                 backgroundColor.B * 0.114) / 255;
            
            // 如果背景较亮，返回黑色文字；否则返回白色文字
            return brightness > 0.5 ? Color.Black : Color.White;
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
        
        // 确保窗体显示时正确定位
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PositionWindow();
        }
        
        // 确保窗体激活时正确定位
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            PositionWindow();
        }
    }
}