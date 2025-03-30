namespace SnapPick
{
    public class ScreenshotService
    {
        private ScreenshotForm? screenshotForm;
        
        public event EventHandler? ScreenshotCaptured;

        public void Start()
        {
            // 创建并显示截图窗体
            screenshotForm = new ScreenshotForm();
            screenshotForm.ScreenshotCompleted += ScreenshotForm_ScreenshotCompleted;
            screenshotForm.Show();
        }

        public void Stop()
        {
            // 关闭截图窗体
            if (screenshotForm != null && !screenshotForm.IsDisposed)
            {
                screenshotForm.Close();
                screenshotForm = null;
            }
        }

        private void ScreenshotForm_ScreenshotCompleted(object? sender, EventArgs e)
        {
            // 通知截图完成
            ScreenshotCaptured?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ScreenshotForm : Form
    {
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing;
        private Bitmap? screenBitmap;

        public event EventHandler? ScreenshotCompleted;

        public ScreenshotForm()
        {
            // 设置窗体属性
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Opacity = 0.3;
            this.BackColor = Color.Black;
            this.Cursor = Cursors.Cross;
            
            // 捕获整个屏幕
            CaptureScreen();
            
            // 注册事件处理
            this.MouseDown += ScreenshotForm_MouseDown;
            this.MouseMove += ScreenshotForm_MouseMove;
            this.MouseUp += ScreenshotForm_MouseUp;
            this.KeyDown += ScreenshotForm_KeyDown;
            this.Paint += ScreenshotForm_Paint;
        }

        private void CaptureScreen()
        {
            // 获取屏幕尺寸
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            
            // 创建位图
            screenBitmap = new Bitmap(bounds.Width, bounds.Height);
            
            // 捕获屏幕
            using (Graphics g = Graphics.FromImage(screenBitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
        }

        private void ScreenshotForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                isDrawing = true;
            }
        }

        private void ScreenshotForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                endPoint = e.Location;
                this.Invalidate();
            }
        }

        private void ScreenshotForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                endPoint = e.Location;
                
                // 计算截图区域
                Rectangle rect = CalculateRectangle(startPoint, endPoint);
                
                // 如果区域太小，则忽略
                if (rect.Width < 5 || rect.Height < 5)
                {
                    this.Close();
                    return;
                }
                
                // 截取选定区域
                CaptureSelectedArea(rect);
                
                // 通知截图完成
                ScreenshotCompleted?.Invoke(this, EventArgs.Empty);
                
                // 关闭窗体
                this.Close();
            }
        }

        private void ScreenshotForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void ScreenshotForm_Paint(object? sender, PaintEventArgs e)
        {
            if (isDrawing)
            {
                Rectangle rect = CalculateRectangle(startPoint, endPoint);
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private Rectangle CalculateRectangle(Point start, Point end)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(start.X - end.X);
            int height = Math.Abs(start.Y - end.Y);
            
            return new Rectangle(x, y, width, height);
        }

        private void CaptureSelectedArea(Rectangle rect)
        {
            if (screenBitmap == null)
                return;
                
            // 创建截图区域的位图
            using (Bitmap selectedArea = new Bitmap(rect.Width, rect.Height))
            {
                using (Graphics g = Graphics.FromImage(selectedArea))
                {
                    g.DrawImage(screenBitmap, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
                }
                
                // 复制到剪贴板
                Clipboard.SetImage(selectedArea);
                
                // 显示通知
                ShowToastNotification();
            }
        }

        private void ShowToastNotification()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                BalloonTipTitle = "截图完成",
                BalloonTipText = "图像已复制到剪贴板",
                BalloonTipIcon = ToolTipIcon.Info
            };
            
            notifyIcon.ShowBalloonTip(3000);
            
            // 3秒后自动清理通知图标
            System.Threading.Timer timer = new System.Threading.Timer((obj) =>
            {
                notifyIcon.Dispose();
            }, null, 3000, Timeout.Infinite);
        }
        
        private NotifyIcon notifyIcon = new NotifyIcon();
    }
}