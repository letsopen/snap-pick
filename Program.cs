namespace SnapPick
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 启用视觉样式
            ApplicationConfiguration.Initialize();
            
            // 创建并运行主应用
            Application.Run(new MainForm());
        }
    }
}