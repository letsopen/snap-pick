using System.Runtime.InteropServices;

namespace SnapPick
{
    public class ColorPickerService
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int GetPixel(IntPtr hdc, int x, int y);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private System.Threading.Timer? timer;
        private bool isRunning;

        public event EventHandler<Color>? ColorChanged;

        public void Start()
        {
            if (isRunning)
                return;

            isRunning = true;
            timer = new System.Threading.Timer(CheckColorUnderCursor, null, 0, 50); // 每50毫秒检查一次
        }

        public void Stop()
        {
            if (!isRunning)
                return;

            timer?.Dispose();
            timer = null;
            isRunning = false;
        }

        private void CheckColorUnderCursor(object? state)
        {
            if (!isRunning)
                return;

            try
            {
                if (GetCursorPos(out POINT point))
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    int colorRef = GetPixel(hdc, point.X, point.Y);
                    ReleaseDC(IntPtr.Zero, hdc);

                    Color color = Color.FromArgb(
                        (colorRef >> 0) & 0xFF,   // R
                        (colorRef >> 8) & 0xFF,   // G
                        (colorRef >> 16) & 0xFF   // B
                    );

                    // 使用主线程调用事件
                    if (ColorChanged != null)
                    {
                        var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                        if (mainForm != null)
                        {
                            mainForm.Invoke(() => ColorChanged?.Invoke(this, color));
                        }
                        else
                        {
                            ColorChanged?.Invoke(this, color);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录异常，防止定时器停止
                Console.WriteLine($"取色错误: {ex.Message}");
            }
        }
    }
}