using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MutiWindows
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        

        public MainWindow()
        {
            //ScreenDebugVisualier.Show();
            MouseDragging.ActiveWindowMouseDragging();
            MouseDragging.AddWindow(this);
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        SubWindow1 subWindow1 = new SubWindow1() { Title = "1" };
        SubWindow1 subWindow2 = new SubWindow1() { Title = "2" };
        SubWindow1 subWindow3 = new SubWindow1() { Title = "3" };
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            MouseDragging.AddWindow(subWindow1);
            subWindow1.Show();
            MouseDragging.AddWindow(subWindow2);
            subWindow2.Show();
            MouseDragging.AddWindow(subWindow3);
            subWindow3.Show();
        }

        private void Sticky_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MouseDragging.StickyPixel = (int)((sender as Slider).Value);
            StickyText.Content = "StickyPixel: "+ (int)((sender as Slider).Value) + " pixel.";
        }

        private void Resize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MouseDragging.ResizePixel = (int)((sender as Slider).Value);
            Ground.BorderThickness = new Thickness((sender as Slider).Value);
            ResizeText.Content = "ResizePixel: " + (int)((sender as Slider).Value) + " pixel.";
            if (subWindow1.IsVisible)
                subWindow1.Ground.BorderThickness = new Thickness((sender as Slider).Value);
            if (subWindow2.IsVisible)
                subWindow2.Ground.BorderThickness = new Thickness((sender as Slider).Value);
            if (subWindow3.IsVisible)
                subWindow3.Ground.BorderThickness = new Thickness((sender as Slider).Value);
        }
    }
}
