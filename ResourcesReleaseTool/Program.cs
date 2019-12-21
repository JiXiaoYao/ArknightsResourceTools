using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AssetStudio;

namespace ResourcesReleaseTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("help: program.exe -i [InPut] -o [OutPut]");
            Console.WriteLine("InPut:含有要解包的游戏文件");
            Console.WriteLine("OutPut:输出解包内容的文件夹");
            string[] parameter = GetArgs(args);
            if (parameter[0] == "0" || parameter[0] == "2" || parameter[1] == "0")
            {
                if (parameter[0] == "0")
                    Console.WriteLine("InPut路径不存在");
                if (parameter[1] == "0")
                    Console.WriteLine("OutPut路径不存在");
                if (parameter[0] == "2")
                    Console.WriteLine("没有输入InPut");
                Console.WriteLine("参数错误，程序退出");
                return;
            }
            string OutPut = parameter[1];
            if (parameter[1] == "2")
            {
                Console.WriteLine("未设置输出路径，程序将在运行目录中输出文件");
                OutPut = Environment.CurrentDirectory;
            }
            Console.WriteLine("输入路径:" + parameter[0]);
            Console.WriteLine("输出路径:" + OutPut);
            string[] FilePath = GetFiles(parameter[0]).ToArray();
            Console.WriteLine($"共有:{FilePath.Length}个文件");
            Console.WriteLine("开始载入游戏资源文件");
            AssetsManager assetsManager = new AssetsManager();
            assetsManager.LoadFiles(FilePath);
            Console.WriteLine("载入完毕");
            Console.WriteLine($"载入的有效项目数：{assetsManager.assetsFileList.Count}");
            Console.WriteLine("当前进程占用内存：" + GetDataSize(Process.GetCurrentProcess().PrivateMemorySize64));
            if (!Directory.Exists(OutPut + "/Photo"))
                Directory.CreateDirectory(OutPut + "/Photo");
            if (!Directory.Exists(OutPut + "/Txtout"))
                Directory.CreateDirectory(OutPut + "/Txtout");
            Console.WriteLine("开始处理数据");
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("此程序使用并发模式，做好CPU 100%的准备");
            Console.ResetColor();
            double Works = 0;
            double AllWorks = assetsManager.assetsFileList.Count;
            Task task = new Task(() =>
            {
                Thread.Sleep(1000);
                Console.WriteLine($"完成:{((Works / AllWorks) * 100).ToString("00.00")}%");
                while (Works != AllWorks)
                {
                    Thread.Sleep(1000);
                    if (parameter[2] != "1")
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine($"完成:{((Works / AllWorks) * 100).ToString("00.00")}%");
                    //GC.Collect();
                }
            });
            task.Start();
            Parallel.ForEach(assetsManager.assetsFileList, asset =>
            {
                //foreach (var asset in assetsManager.assetsFileList)
                //{
                foreach (var texture2D in asset.Objects.Where(D => D.Value.type == ClassIDType.Texture2D).ToArray())
                {
                    try
                    {
                        Texture2D photo = (Texture2D)texture2D.Value;
                        var converter = new Texture2DConverter(photo).ConvertToBitmap(true);
                        converter.Save(OutPut + "/Photo/" + photo.m_Name + ".png");

                    }
                    catch (Exception e)
                    {

                    }
                }
                foreach (var textasset in asset.Objects.Where(D => D.Value.type == AssetStudio.ClassIDType.TextAsset).ToArray())
                {
                    TextAsset Text = ((TextAsset)textasset.Value);
                    File.WriteAllText(OutPut + "/Txtout/" + asset.fileName + "." + Text.m_Name + ".txt", Encoding.UTF8.GetString(Text.m_Script));
                }
                Works++;
            });
            Works = AllWorks;
            task.Wait();
            Console.WriteLine($"处理完成,成功处理{Directory.GetFiles(OutPut + "/Photo/").Length}张图片,{Directory.GetFiles(OutPut + "/Txtout/").Length}段文本");
            if (parameter[2] == "2")
            {
                Console.ReadKey();
                Console.WriteLine("按任意键退出........");
            }
        }
        public static string[] GetArgs(string[] args)
        {
            string[] Info = new string[3] { "2", "2", "2" };
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].First() == '-')
                    if (args[i][1] == 'i' && args[i].Length > 2)
                        if (Directory.Exists(args[i].Remove(0, 2)))
                            Info[0] = args[i].Remove(0, 2);
                        else
                            Info[0] = "0";
                    else if (args[i][1] == 'i' && args[i + 1].First() != '-')
                    {
                        if (Directory.Exists(args[i + 1]))
                            Info[0] = args[i + 1];
                        else
                            Info[0] = "0";
                        i = i + 1;
                    }
                    else if (args[i][1] == 'o' && args[i].Length > 2)
                        if (Directory.Exists(args[i].Remove(0, 2)))
                            Info[1] = args[i].Remove(0, 2);
                        else
                            Info[1] = "0";
                    else if (args[i][1] == 'o' && args[i + 1].First() != '-')
                    {
                        if (Directory.Exists(args[i + 1]))
                            Info[1] = args[i + 1];
                        else
                            Info[1] = "0";
                        i = i + 1;
                    }
                    else if (args[i][1] == 't')
                        Info[2] = "1";
            }
            return Info;
        }
        public static List<string> GetFiles(string DirPath)
        {
            List<string> FilePath = new List<string>();
            foreach (string path in Directory.GetFiles(DirPath))
                FilePath.Add(path);
            foreach (string path in Directory.GetDirectories(DirPath))
                foreach (string Filepath in GetFiles(path))
                    FilePath.Add(Filepath);
            return FilePath;
        }
        public static string GetDataSize(long Bytes)
        {
            if (Bytes < 1024)
                return Bytes + "Byte";
            else if (Bytes < 1024 * 1024)
                return (((double)Bytes) / 1024).ToString("0.000") + "KB";
            else if (Bytes < 1024 * 1024 * 1024)
                return (((double)Bytes) / (1024 * 1024)).ToString("0.000") + "MB";
            else
                return (((double)Bytes) / (1024 * 1024 * 1024)).ToString("0.000") + "GB";
        }
    }
}