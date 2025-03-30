using System.Diagnostics;
using Microsoft.Win32;

namespace SnapPick
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem colorPickerMenuItem;
        private ToolStripMenuItem screenshotMenuItem;
        private ToolStripMenuItem autoStartMenuItem;
        private ToolStripMenuItem exitMenuItem;

        private ColorPickerService colorPickerService;
        private ScreenshotService screenshotService;
        private ColorInfoForm colorInfoForm;
        
        private bool isAutoStartEnabled;

        public MainForm()
        {
            InitializeComponent();
            
            // 初始化服务
            colorPickerService = new ColorPickerService();
            screenshotService = new ScreenshotService();
            colorInfoForm = new ColorInfoForm();
            
            // 设置窗体属性
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 0;
            
            // 初始化系统托盘图标
            InitializeNotifyIcon();
            
            // 加载自启动设置
            LoadAutoStartSetting();
            
            // 默认启动取色模式
            StartColorPickerMode();
            
            // 注册事件处理
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            // 窗体加载后隐藏
            this.Hide();
        }

        private void InitializeNotifyIcon()
        {
            // 创建上下文菜单
            contextMenu = new ContextMenuStrip();
            
            colorPickerMenuItem = new ToolStripMenuItem("取色模式");
            colorPickerMenuItem.Click += (s, e) => StartColorPickerMode();
            
            screenshotMenuItem = new ToolStripMenuItem("截图模式");
            screenshotMenuItem.Click += (s, e) => StartScreenshotMode();
            
            autoStartMenuItem = new ToolStripMenuItem("开机自启");
            autoStartMenuItem.Click += (s, e) => ToggleAutoStart();
            
            exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += (s, e) => ExitApplication();
            
            // 添加菜单项
            contextMenu.Items.Add(colorPickerMenuItem);
            contextMenu.Items.Add(screenshotMenuItem);
            contextMenu.Items.Add(autoStartMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);
            
            // 创建通知图标
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Assets\\icon.ico"),
                Text = "SnapPick - 截图取色工具",
                Visible = true,
                ContextMenuStrip = contextMenu
            };
        }

        private void StartColorPickerMode()
        {
            // 停止截图服务
            screenshotService.Stop();
            
            // 显示颜色信息窗体
            colorInfoForm.Show();
            
            // 启动取色服务
            colorPickerService.Start();
            colorPickerService.ColorChanged += (sender, color) =>
            {
                colorInfoForm.UpdateColor(color);
            };
            
            // 更新菜单项状态
            colorPickerMenuItem.Checked = true;
            screenshotMenuItem.Checked = false;
        }

        private void StartScreenshotMode()
        {
            // 停止取色服务
            colorPickerService.Stop();
            
            // 隐藏颜色信息窗体
            colorInfoForm.Hide();
            
            // 启动截图服务
            screenshotService.Start();
            screenshotService.ScreenshotCaptured += (sender, e) =>
            {
                // 截图完成后自动回到取色模式
                StartColorPickerMode();
            };
            
            // 更新菜单项状态
            colorPickerMenuItem.Checked = false;
            screenshotMenuItem.Checked = true;
        }

        private void LoadAutoStartSetting()
        {
            // 检查注册表中是否有自启动项
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key != null)
                {
                    isAutoStartEnabled = key.GetValue("SnapPick") != null;
                    autoStartMenuItem.Checked = isAutoStartEnabled;
                }
            }
        }

        private void ToggleAutoStart()
        {
            isAutoStartEnabled = !isAutoStartEnabled;
            
            // 更新注册表
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key != null)
                {
                    if (isAutoStartEnabled)
                    {
                        string appPath = Application.ExecutablePath;
                        key.SetValue("SnapPick", appPath);
                    }
                    else
                    {
                        key.DeleteValue("SnapPick", false);
                    }
                }
            }
            
            // 更新菜单项状态
            autoStartMenuItem.Checked = isAutoStartEnabled;
        }

        private void ExitApplication()
        {
            // 停止所有服务
            colorPickerService.Stop();
            screenshotService.Stop();
            
            // 关闭颜色信息窗体
            colorInfoForm.Close();
            
            // 移除通知图标
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            
            // 退出应用
            Application.Exit();
        }

        // 设计器生成的代码
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.Name = "MainForm";
            this.Text = "SnapPick";
            this.ResumeLayout(false);
        }
    }
}