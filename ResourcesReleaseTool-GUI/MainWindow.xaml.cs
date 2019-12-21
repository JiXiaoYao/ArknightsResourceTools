using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

namespace ResourcesReleaseTool_GUI
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
        Bitmap bitmap;
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
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(ReSizeEventArgs);
            this.Closing += Window_Closing;
            process.StartInfo.FileName = "0";
        }
        public void ReSizeEventArgs(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                this.Height = ((this.Width) / bitmap.Width) * bitmap.Height;
            if (e.HeightChanged)
                this.Width = ((this.Height) / bitmap.Height) * bitmap.Width;
            //else if (e.NewSize.Height != e.PreviousSize.Height)
            //    this.Width = ((this.Height) / 27) * 40;
            //else if (e.NewSize.Width != e.PreviousSize.Width)
            //    this.Height = ((this.Width) / 40) * 27;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InPutPath = folderBrowserDialog.SelectedPath;
                if (OutPutPath != "")
                    OutPutLabel.Content = "已选中:\r\n源目录:" + InPutPath + "\r\n输出目录:" + OutPutPath;
                else
                    OutPutLabel.Content = "已选中:\r\n源目录:" + InPutPath;
            }
            else
            {
                MessageBox.Show("请先选择文件夹");
            }
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutPutPath = folderBrowserDialog.SelectedPath;
                if (InPutPath != "")
                    OutPutLabel.Content = "已选中:\r\n源目录:" + InPutPath + "\r\n输出目录:" + OutPutPath;
                else
                    OutPutLabel.Content = "已选中:\r\n源目录:未选择\r\n输出目录:" + OutPutPath;
            }
            else
            {
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(InPutPath))
            {
                process = new Process();
                process.StartInfo.FileName = $"{Environment.CurrentDirectory}/ResourcesReleaseTool.exe";
                process.StartInfo.Arguments = $"-t -i \"{InPutPath}\" -o \"{OutPutPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                //process.StartInfo.StandardOutputEncoding = Encoding.ASCII;
                process.Start();
                Cancel.IsEnabled = true;
                OutPutLabel.Content = "";
                task = new Task(() =>
                {
                    while (!process.HasExited)
                    {
                        string buffer = process.StandardOutput.ReadLine();
                        this.OutPutLabel.Dispatcher.Invoke(new Action(() =>
                        {
                            OutPutLabel.Content = Environment.NewLine + buffer + (string)OutPutLabel.Content;
                        }));
                    }
                    this.OutPutLabel.Dispatcher.Invoke(new Action(() =>
                    {
                        OutPutLabel.Content = $"{Environment.NewLine}核心程序已退出" + (string)OutPutLabel.Content;
                    }));
                    if (Directory.Exists($"{OutPutPath}/Photo"))
                        Process.Start($"{OutPutPath}/Photo");

                });
                task.Start();
            }
            else
            {
                MessageBox.Show("源目录不正确");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            process.Kill();
            Cancel.IsEnabled = false;
        }
    }
}
