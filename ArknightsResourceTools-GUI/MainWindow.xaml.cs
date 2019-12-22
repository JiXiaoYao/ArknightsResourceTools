using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArknightsResourceTools_GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Process process = new Process();
        Task task;
        string InPutPath = "";
        string OutPutPath = "";
        string AlphaPath = "";
        Bitmap bitmap;
        bool CanCel = false;
        bool WorkIng = false;
        public MainWindow()
        {
            InitializeComponent();
            double x = SystemParameters.WorkArea.Width;//得到屏幕工作区域宽度
            double y = SystemParameters.WorkArea.Height;//得到屏幕工作区域高度
            this.MaxHeight = y;
            this.MaxWidth = x;
            bitmap = new Bitmap("background.jpg");
            this.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("background.jpg")))
            };
            this.Width = ((y * 0.75) / bitmap.Height) * bitmap.Width;//设置窗体宽度
            this.Height = y * 0.75;//设置窗体高度
            OutPutBox.FontSize = 0.0125 * this.Width;
            Grid.MaxHeight = this.Height;
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(ReSizeEventArgs);
            process.StartInfo.FileName = "0";
            this.Closing += Window_Closing;
            OutPutBox.IsReadOnly = true;
            //OutPutBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

        }
        public void ReSizeEventArgs(object sender, System.Windows.SizeChangedEventArgs e)
        {

            if (e.WidthChanged)
            {
                this.Height = ((this.Width) / bitmap.Width) * bitmap.Height;
                Grid.MaxHeight = this.Height;
                Grid.Height = this.Height;
                //OutPutBox.MaxHeight= this.Height - Menu.Height;
                // OutPutBox.Height= this.Height-Menu.Height;
            }
            if (e.HeightChanged)
                this.Width = ((this.Height) / bitmap.Height) * bitmap.Width;
            OutPutBox.FontSize = 0.0125 * this.Width;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (process.StartInfo.FileName != "0")
                if (!process.HasExited)
                {
                    process.Kill();
                }
            e.Cancel = false;
        }

        private void GetSrc_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InPutPath = folderBrowserDialog.SelectedPath;
                if (OutPutPath != "")
                    OutPutBox.Text = "已选中:\r\n源目录:" + InPutPath + "\r\n输出目录:" + OutPutPath;
                else
                    OutPutBox.Text = "已选中:\r\n源目录:" + InPutPath;
            }
            else
            {
                MessageBox.Show("请先选择文件夹");
            }
        }

        private void SetOutPutPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutPutPath = folderBrowserDialog.SelectedPath;
                if (InPutPath != "")
                    OutPutBox.Text = "已选中:\r\n源目录:" + InPutPath + "\r\n输出目录:" + OutPutPath;
                else
                    OutPutBox.Text = "已选中:\r\n源目录:未选择\r\n输出目录:" + OutPutPath;
            }
            else
            {
            }
        }

        private void SelectSrcPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AlphaPath = folderBrowserDialog.SelectedPath;
                OutPutBox.Text = "已选中:\r\n" + AlphaPath;
            }
            else
            {

            }
        }

        private void ResourcesRelease_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(InPutPath) && !WorkIng)
            {
                WorkIng = true;
                process = new Process();
                process.StartInfo.FileName = $"{Environment.CurrentDirectory}/ResourcesReleaseTool.exe";
                process.StartInfo.Arguments = $"-t -i \"{InPutPath}\" -o \"{OutPutPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                //process.StartInfo.StandardOutputEncoding = Encoding.ASCII;
                process.Start();
                Cancel.IsEnabled = true;
                OutPutBox.Text = "";
                task = new Task(() =>
                {
                    while (!process.HasExited)
                    {
                        string buffer = process.StandardOutput.ReadLine();
                        this.OutPutBox.Dispatcher.Invoke(new Action(() =>
                        {
                            OutPutBox.AppendText(buffer + Environment.NewLine);
                            OutPutBox.ScrollToEnd();
                        }));
                    }
                    this.OutPutBox.Dispatcher.Invoke(new Action(() =>
                    {
                        OutPutBox.AppendText($"{Environment.NewLine}核心程序已退出");
                        OutPutBox.ScrollToEnd();
                    }));
                    if (Directory.Exists($"{OutPutPath}/Photo"))
                        Process.Start($"{OutPutPath}/Photo");
                    if (CanCel)
                    {
                        CanCel = false;
                    }
                    else
                        this.ChannelSynthesis.Dispatcher.Invoke(new Action(() =>
                        {
                            ChannelSynthesis.IsEnabled = true;
                        }));
                    this.Cancel.Dispatcher.Invoke(new Action(() =>
                    {
                        Cancel.IsEnabled = false;
                    }));
                    WorkIng = false;
                });
                task.Start();
            }
            else if (WorkIng)
            {
                MessageBox.Show("已有项目在运行，请等待");
            }
            else
            {
                MessageBox.Show("源目录不正确");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CanCel = true;
            process.Kill();
        }

        private void AlphaChannelSynthesis_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(AlphaPath) && !WorkIng)
            {
                WorkIng = true;
                process = new Process();
                process.StartInfo.FileName = $"{Environment.CurrentDirectory}/ChannelSynthesisByCPU.exe";
                process.StartInfo.Arguments = $"\"{AlphaPath}\"";
                //process.StartInfo.UseShellExecute = false;
                //process.StartInfo.RedirectStandardOutput = true;
                //process.StartInfo.CreateNoWindow = true;
                Cancel.IsEnabled = true;
                OutPutBox.Text = "";
                task = new Task(() =>
                {
                    this.OutPutBox.Dispatcher.Invoke(new Action(() =>
                    {
                        OutPutBox.AppendText("Alpha通道与RGB通道合成程序已启动，请等待");
                        OutPutBox.ScrollToEnd();
                    }));
                    process.Start();
                    while (!process.HasExited)
                    {
                        Thread.Sleep(100);
                    }
                    this.OutPutBox.Dispatcher.Invoke(new Action(() =>
                    {
                        OutPutBox.AppendText($"{Environment.NewLine}核心程序已退出");
                        OutPutBox.ScrollToEnd();
                    }));
                    if (Directory.Exists($"{AlphaPath}/Photo/OutPut"))
                        Process.Start($"{AlphaPath}/Photo/OutPut");
                    if (CanCel)
                    {
                        CanCel = false;
                    }
                    this.Cancel.Dispatcher.Invoke(new Action(() =>
                    {
                        Cancel.IsEnabled = false;
                    }));
                    WorkIng = false;
                });
                task.Start();
            }
            else if (WorkIng)
            {
                MessageBox.Show("已有项目在运行，请等待");
            }
            else
            {
                MessageBox.Show("源目录不正确");
            }
        }

        private void ChannelSynthesis_Click(object sender, RoutedEventArgs e)
        {
            if (!WorkIng)
            {
                WorkIng = true;
                process = new Process();
                process.StartInfo.FileName = $"{Environment.CurrentDirectory}/ChannelSynthesisByCPU.exe";
                process.StartInfo.Arguments = $"\"{OutPutPath}/Photo\"";
                //process.StartInfo.UseShellExecute = false;
                //process.StartInfo.RedirectStandardOutput = true;
                //process.StartInfo.CreateNoWindow = true;
                Cancel.IsEnabled = true;
                OutPutBox.Text = "";
                task = new Task(() =>
                {
                    this.OutPutBox.Dispatcher.Invoke(new Action(() =>
                    {
                        OutPutBox.AppendText("Alpha通道与RGB通道合成程序已启动，请等待");
                        OutPutBox.ScrollToEnd();
                    }));
                    process.Start();
                    while (!process.HasExited)
                    {
                        Thread.Sleep(100);
                    }
                    this.OutPutBox.Dispatcher.Invoke(new Action(() =>
                    {
                        OutPutBox.Text = OutPutBox.Text + $"{Environment.NewLine}核心程序已退出";
                        OutPutBox.ScrollToEnd();
                    }));
                    if (Directory.Exists($"{OutPutPath}/Photo/OutPut"))
                        Process.Start($"{OutPutPath}/Photo/OutPut");
                    if (CanCel)
                    {
                        CanCel = false;
                    }
                    this.ChannelSynthesis.Dispatcher.Invoke(new Action(() =>
                    {
                        ChannelSynthesis.IsEnabled = false;
                    }));
                    this.Cancel.Dispatcher.Invoke(new Action(() =>
                    {
                        Cancel.IsEnabled = false;
                    }));
                    WorkIng = false;
                });
                task.Start();
            }
            else
                MessageBox.Show("已有项目在运行，请等待");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Info info = new Info();
            info.ShowDialog();
        }
    }
}
