using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

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
            "a(其他补丁-角色定位)",
            "a(其他补丁-空中坐标)",
            "a(其他补丁-蓝冰翼喷射器)",
            "a(其他补丁-去失明、过图黑屏、死亡黑屏)",
            "a(其他补丁-透明血条)",
            "a(其他补丁-隐藏名字)",
            "a(角色补丁-红狗反和谐)",
            "a(角色补丁-剑豪觉醒)",
            "a(超时空漩涡-便利补丁)",
            "a(泰波尔斯-暗能量优化)",
            "a(泰波尔斯-翅膀怪)",
            "a(泰波尔斯-翅膀优化)",
            "a(泰波尔斯-地图优化)",
            "a(泰波尔斯-斗气释放)",
            "a(泰波尔斯-风阻优化)",
            "a(泰波尔斯-怪暗能量以及移动风优化)",
            "a(泰波尔斯-破防提醒)",
            "a(泰波尔斯-切图优化)",
            "a(泰波尔斯-太空列车)",
            "a(泰波尔斯-终极优化)"
        };

        public static List<Patches> Check(string path)
        {
            return CheckInstalled(path, _patches);
        }

        public static List<Patches> CheckInstalled(string path, string[] list)
        {
            List<Patches> patches = new List<Patches>();

            foreach (var file in list)
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

        public static bool Install(Patches patch, string path)
        {
            try
            {
                Downloader.Download(System.IO.Path.Combine(path, "ImagePacks2"), patch.File);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("安装成功 ->\t[{0}]\t{1}", patch.Type, patch.Name);
                return true;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("无法下载补丁 [{0}] -> 异常: {1}", patch.File, e.Message);
                return false;
            }
        }
    }

    class Downloader
    {
        const string url_pref = "https://static.kxnrl.com/DNF/";

        public static void Download(string path, string file)
        {
            using (var completedEvent = new ManualResetEventSlim(false))
            using (var client = new WebClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var text = Console.Title;

                client.DownloadProgressChanged += (sender, e) =>
                {
                    Console.Title = "[" + e.ProgressPercentage.ToString() + "%]" + 
                                    " " + "正在下载" + " " + file + " ..." + "         " + 
                                    " " + e.BytesReceived / 1024 + "KB" + 
                                    " " + "/" +
                                    " " + e.TotalBytesToReceive / 1024 + "KB";
                };

                client.DownloadFileCompleted += (sender, args) =>
                {
                    Console.Title = text;
                    completedEvent.Set();
                };

                client.DownloadFileAsync(new Uri(url_pref + file), System.IO.Path.Combine(path, file));
                completedEvent.Wait();
                Console.Title = text;
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

    class Interface
    {
        public class Luke
        {
            private static readonly string[] _patches = new string[]
            {
                "a(卢克之心界面-DT)",
                "a(卢克之心界面-EZ8S)",
                "a(卢克之心界面-宝石边框)",
                "a(卢克之心界面-电流CD)",
                "a(卢克之心界面-光能伤害字体)",
                "a(卢克之心界面-绝望之塔)",
                "a(卢克之心界面-深水女妖)",
                "a(卢克之心界面-旋风)"
            };

            public static bool Install(string path)
            {
                bool ret = false;
                foreach (var patch in Patch.CheckInstalled(path, _patches))
                {
                    if (Patch.Install(patch, path))
                    {
                        // flag
                        ret = true;
                    }
                }
                return ret;
            }
        }

        public class Rosh
        {
            private static readonly string[] _patches = new string[]
            {
                "a(反重力界面-boss坐标)",
                "a(反重力界面-登陆图)",
                "a(反重力界面-翻牌评分1)",
                "a(反重力界面-翻牌评分2)",
                "a(反重力界面-挤频道)",
                "a(反重力界面-角色光环)",
                "a(反重力界面-聊天框)",
                "a(反重力界面-毛线团)",
                "a(反重力界面-冒险团字体1)",
                "a(反重力界面-冒险团字体2)",
                "a(反重力界面-赛丽亚)",
                "a(反重力界面-赛丽亚房间)",
                "a(反重力界面-选角特效)",
                "a(反重力界面-选人界面)",
                "a(反重力界面-血槽1)",
                "a(反重力界面-血槽2)",
                "a(反重力界面-装备等级框)"
            };

            public static bool Install(string path)
            {
                bool ret = false;
                foreach (var patch in Patch.CheckInstalled(path, _patches))
                {
                    if (Patch.Install(patch, path))
                    {
                        // flag
                        ret = true;
                    }
                }
                return ret;
            }
        }
    }

}
