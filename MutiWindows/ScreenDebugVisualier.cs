using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MutiWindows
{
    static class ScreenDebugVisualier
    {
        static Window FullScreen = new Window()
        {
            Left = 0,
            Top = 0,
            WindowStyle = WindowStyle.None,
            Width = 1920,
            Height = 1080,
            ResizeMode = ResizeMode.NoResize,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Topmost = true,
            ShowInTaskbar = false
        };
        static Canvas canvas = null;
        static public void Show()
        {
            if (canvas == null)
            {
                canvas = new Canvas();
                FullScreen.Content = canvas;
                FullScreen.Show();
            }
        }

        static public void AddRect(Rect rect)
        {
            Rectangle newone = new Rectangle()
            {
                Height = rect.Height,
                Width = rect.Width,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            canvas.Children.Add(newone);
            Canvas.SetLeft(newone, rect.Left);
            Canvas.SetTop(newone, rect.Top);
            Label newone2 = new Label()
            {
                Content = "[" + rect.X + ", " + rect.Y + "]" + "[" + rect.Width + ", " + rect.Height + "]"
            };
            canvas.Children.Add(newone2);
            Canvas.SetLeft(newone2, rect.X);
            Canvas.SetTop(newone2, rect.Y);
        }
        static public void AddPoint(Point pt, Point offset)
        {
            Label newone = new Label()
            {
                Content = "["+pt.X+", "+pt.Y+"]"
            };
            canvas.Children.Add(newone);
            Canvas.SetLeft(newone, pt.X+offset.X);
            Canvas.SetTop(newone, pt.Y+offset.Y);
        }
    }
}
