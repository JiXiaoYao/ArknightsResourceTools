using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using System.Windows.Shapes;

namespace ResourcesReleaseTool_GUI
{
    /// <summary>
    /// Info.xaml 的交互逻辑
    /// </summary>
    public partial class Info : Window
    {
        Bitmap bitmap;
        public Info()
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
            this.Width = ((y * 0.45) / bitmap.Height) * bitmap.Width;//设置窗体宽度
            this.Height = y * 0.45;//设置窗体高度
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(ReSizeEventArgs);
        }
        public void ReSizeEventArgs(object sender, System.Windows.SizeChangedEventArgs e)
        {

            if (e.WidthChanged)
                this.Height = ((this.Width) / bitmap.Width) * bitmap.Height;
            if (e.HeightChanged)
                this.Width = ((this.Height) / bitmap.Height) * bitmap.Width;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/JiXiaoYao/ArknightsResourceTools");
        }
    }
}
