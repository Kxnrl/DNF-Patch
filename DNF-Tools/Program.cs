using System;

namespace DNF_Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "DNF-Patch v1.1 by Kyle";
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("系统初始化...");
            Console.WriteLine("侦测DNF文件位置...");

            var path = Utils.Path.Find();
            var installEX = false;
            var installUI = false;

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
                System.Threading.Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=========================================");
                Console.WriteLine("正在检查windows8/8.1/10蓝屏修复..." + Environment.NewLine);
                Utils.Win32Api.AutoMaintenance(false);
            }

            System.Threading.Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=========================================");
            Console.WriteLine("正在检查NPK补丁..." + Environment.NewLine);

            var patches = Utils.Patch.Check(path);

            if (patches.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("将安装下列补丁:");

                foreach (var patch in patches)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("[{0}]\t{1}", patch.Type, patch.Name);
                }

                Console.WriteLine("");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Environment.NewLine + "无需安装补丁..." + Environment.NewLine);
                goto step2;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("初始化下载队列...");

            foreach (var patch in patches)
            {
                if (Utils.Patch.Install(patch, path))
                {
                    // flag
                    installEX = true;
                }
            }

            if (installEX)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Environment.NewLine + "补丁安装已完成..." + Environment.NewLine);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Environment.NewLine + "某些补丁无法安装, 请手动安装补丁..." + Environment.NewLine);
            }

            step2:
            System.Threading.Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=========================================");
            Console.WriteLine("是否需要安装界面补丁..." + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("1. 光之心 - 卢克主题" + Environment.NewLine + "2. 反重力 - 超时空主题" + Environment.NewLine + "3. 不安装主题");
            Console.ForegroundColor = ConsoleColor.Black;
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("初始化下载队列...");
                    installUI = Utils.Interface.Luke.Install(path);
                    break;
                case ConsoleKey.D2:
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("初始化下载队列...");
                    installUI = Utils.Interface.Rosh.Install(path);
                    break;
                case ConsoleKey.D3:
                    Console.WriteLine("");
                    goto step3;
                default:
                    Console.WriteLine("");
                    goto step2;
            }

            if (installUI)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Environment.NewLine + "界面安装完成..." + Environment.NewLine);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Environment.NewLine + "界面已安装..." + Environment.NewLine);
            }

            step3:
            if (installEX || installUI)
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", "\"" + System.IO.Path.Combine(path, "ImagePacks2") + "\"");
                }
                catch { }
            }

            System.Threading.Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=========================================");
            Console.WriteLine("正在检查优化工具..." + Environment.NewLine);

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
