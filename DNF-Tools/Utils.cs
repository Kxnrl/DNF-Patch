using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace DNF_Tools.Utils
{
    class Path
    {
        public static string Find()
        {
            var ret = string.Empty;

            if (Reg(ref ret))
            {
                // find
                return ret;
            }

            if (Default(ref ret))
            {
                // find 
                return ret;
            }

            if (Search(ref ret))
            {
                // find
                return ret;
            }

            return string.Empty;
        }

        private static bool Reg(ref string path)
        {
            try
            {
                var reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\地下城与勇士");
                var val = reg.GetValue("InstallSource", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames).ToString();

                if (!string.IsNullOrEmpty(val))
                {
                    path = val;
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("异常: " + e.Message);
            }

            return false;
        }

        private static bool Default(ref string path)
        {
            var list = new string[]
            {
                "地下城与勇士",
                System.IO.Path.Combine("Program Files (x86)", "地下城与勇士"),
                System.IO.Path.Combine("Program Files", "地下城与勇士"),
                System.IO.Path.Combine("Program Files (x86)", "腾讯游戏", "地下城与勇士"),
                System.IO.Path.Combine("网络游戏", "地下城与勇士"),
            };

            foreach (DriveInfo driver in DriveInfo.GetDrives().Where(x => x.IsReady == true))
            {
                try
                {
                    foreach (var c in list)
                    {
                        var p = System.IO.Path.Combine(driver.RootDirectory.FullName, c);

                        if (!Directory.Exists(p))
                        {
                            //exists?
                            continue;
                        }

                        if (Directory.GetFiles(p, "DNF.exe", SearchOption.TopDirectoryOnly).Length > 0)
                        {
                            // find
                            path = p;
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("异常: " + e.Message);
                }
            }

            return false;
        }

        private static bool Search(ref string path)
        {
            foreach (DriveInfo driver in DriveInfo.GetDrives().Where(x => x.IsReady == true))
            {
                try
                {
                    Foreach(driver.RootDirectory.FullName, ref path);

                    if (!string.IsNullOrEmpty(path))
                    {
                        // done
                        return true;
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("异常: " + e.Message);
                }
            }

            return false;
        }

        private static void Foreach(string path, ref string find)
        {
            var c = 0;
            var d = Directory.GetDirectories(path);

            foreach (var dir in d)
            {
                if (dir.Contains("\\ImagePacks2") || dir.Contains("\\SoundPacks"))
                {
                    // 资源文件夹

                    if (++c == 2)
                    {
                        if (Directory.GetFiles(path, "DNF.exe", SearchOption.TopDirectoryOnly).Length > 0)
                        {
                            find = path;
                            return;
                        }
                    }
                }
            }

            foreach (var dir in d)
            {
                try
                {
                    Foreach(dir, ref find);
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("异常: " + e.Message);
                }
            }
        }
    }

    class Patch
    {
        public struct Patches
        {
            public string Type;
            public string Name;
            public string File;

            public Patches(string t, string n, string f)
            {
                Type = t;
                Name = n;
                File = f;
            }
        }

        private static readonly string[] _patches = new string[]
        {
            
            "a(其他补丁-空中坐标)",
            "a(其他补丁-蓝冰翼喷射器)",
            "a(其他补丁-去失明、过图黑屏、死亡黑屏)",
            "a(其他补丁-透明血条)",
            "a(超时空漩涡-便利补丁)",
            "a(泰波尔斯-暗能量优化)",
            "a(泰波尔斯-翅膀怪)",
            "a(泰波尔斯-翅膀优化)",
            "a(泰波尔斯-地图优化)",
            "a(泰波尔斯-斗气释放)",
            "a(泰波尔斯-风阻优化)",
            "a(泰波尔斯-怪暗能量以及移动风优化)",
            "a(泰波尔斯-切图优化)",
            "a(泰波尔斯-太空列车)",
            "a(泰波尔斯-终极优化)"
        };

        public static List<Patches> Check(string path)
        {
            List<Patches> patches = new List<Patches>();


            foreach (var file in _patches)
            {
                if (File.Exists(System.IO.Path.Combine(path, "ImagePacks2", file + ".NPK")))
                {
                    // 已安装
                    continue;
                }

                var data = file.Split('-');

                if (data.Length != 2)
                {
                    // ??
                    continue;
                }

                patches.Add(new Patches(data[0].Replace("a(", ""), data[1].Replace(")", ""), file + ".NPK"));
            }

            return patches;
        }
    }

    class Downloader
    {
        const string url_pref = "https://static.kxnrl.com/DNF/";

        public static void Download(string path, string file)
        {
            using (var client = new WebClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.DownloadFile(url_pref + file, System.IO.Path.Combine(path, file));
            }
        }
    }

    class Win32Api
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hwnd, int cmd);

        public static void FocusWindow(IntPtr window)
        {
            ShowWindow(window, 5);
            SetForegroundWindow(window);
        }

        public static void AutoMaintenance(bool enabled = false)
        {
            try
            {
                var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", true);

                var set = true;

                var val = enabled ? 0 : 1;

                foreach (var key in reg.GetValueNames())
                {
                    if (key.Equals("MaintenanceDisabled"))
                    {
                        if (reg.GetValueKind("MaintenanceDisabled") != RegistryValueKind.DWord)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("侦测到无效的数据类型...");
                            reg.DeleteValue("MaintenanceDisabled");
                        }
                        else if (int.Parse(reg.GetValue("MaintenanceDisabled").ToString()) == val)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("侦测到值[{0}/{1}]...", reg.GetValue("MaintenanceDisabled").ToString(), val);
                            set = false;
                        }

                        break;
                    }
                }

                if (set)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("重新设置注册表值...");
                    reg.SetValue("MaintenanceDisabled", 1);//, RegistryValueKind.DWord
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("已修复Windows10挂机蓝屏..." + Environment.NewLine + "操作将在下次重启后生效...");
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("异常: " + e.Message + Environment.NewLine + e.StackTrace);
            }
            finally
            {
                Console.WriteLine("");
            }
        }
    }

    class Terminator
    {
        public static void FindAndKill()
        {
            try
            {
                var list = new string[] { "CrossProxy", "TPHelper", "TQMCenter", "tgp_gamead", "GameLoader", "DNF" };

                foreach (var n in new string[] { "CrossProxy", "TPHelper", "TQMCenter", "tgp_gamead", "GameLoader", "DNF" })
                {
                    foreach (var p in Process.GetProcessesByName(n))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("结束进程 {0} -> {1}", p.ProcessName, p.Id);
                        p.Kill();
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("异常: " + e.Message);
            }
            finally
            {
                Console.WriteLine("");
            }
        }
    }
}
