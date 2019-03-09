using System;

namespace DNF_Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "DNF-Patch v1.0        By Kyle";
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("系统初始化...");
            Console.WriteLine("侦测DNF文件位置...");

            var path = Utils.Path.Find();

            if (string.IsNullOrEmpty(path))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("您的电脑尚未安装DNF...");
                Console.ReadKey();
                goto done;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("DNF路径为: " + path + Environment.NewLine);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=========================================");
            Console.WriteLine("正在检查游戏进程...");
            Utils.Terminator.FindAndKill();

            if ( Environment.OSVersion.Version.Major == 10 || 
                (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2)
               )
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=========================================");
                Console.WriteLine("正在检查windows8/8.1/10蓝屏修复..." + Environment.NewLine);
                Utils.Win32Api.AutoMaintenance(false);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=========================================");
            Console.WriteLine("正在检查NPK补丁..." + Environment.NewLine);

            var patches = Utils.Patch.Check(path);

            if (patches.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("将安装下列补丁:");

                foreach (var patch in patches)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[{0}]\t{1}", patch.Type, patch.Name);
                }

                Console.WriteLine("");
            }
            else
            {
                // skip
                goto step2;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("初始化下载队列...");

            bool installed = false;

            foreach (var patch in patches)
            {
                try
                {
                    Utils.Downloader.Download(System.IO.Path.Combine(path, "ImagePacks2"), patch.File);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("安装成功 ->\t[{0}]\t{1}", patch.Type, patch.Name);
                    installed = true;
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无法下载补丁 [{0}] -> 异常: {1}", patch.File, e.Message);
                }
            }

            if (installed)
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", "\""+ System.IO.Path.Combine(path, "ImagePacks2") + "\"");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Environment.NewLine + "补丁操作已完成..." + Environment.NewLine);
                }
                catch { }
            }
            else
            {

            }

            step2:
            Console.WriteLine("=========================================");
            Console.WriteLine("正在检查优化工具..." + Environment.NewLine);

            // System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Tool2.exe");
            var dirt = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kxnrl", "DNF");
            var tool = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kxnrl", "DNF", "Tool2.exe");

            if (!System.IO.Directory.Exists(dirt))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(dirt);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无法创建目录 -> 异常: {0}", e.Message);
                    goto done;
                }
            }

            if (!System.IO.File.Exists(tool))
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("正在下载优化工具...");
                    Utils.Downloader.Download(dirt, "Tool2.exe");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("下载工具完成...");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无法下载工具 -> 异常: {0}", e.Message);
                    goto done;
                }
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("请在新打开的窗口中操作..." + Environment.NewLine + "完成后请直接关闭新窗口");
                System.Threading.Thread.Sleep(500);
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = tool;
                proc.Start();
                System.Threading.Thread.Sleep(500);
                Utils.Win32Api.FocusWindow(proc.MainWindowHandle);
                proc.WaitForExit();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("无法启动工具 -> 异常: {0}", e.Message);
            }

            done:
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Environment.NewLine + "所有操作已完成..." + Environment.NewLine + "按下任意键退出...");

            Console.ReadKey();
        }
    }
}
