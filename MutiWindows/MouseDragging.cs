using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MutiWindows
{
    //Windows Dragging System
    class MouseDragging
    {
        // User is Dragging or not
        static bool isDragging = false;
        // the offset of global mouse positioin and window position
        static Point offset = new Point();
        // Dragged Window
        static Window senderWindow;
        // Adjust How much will the window stick to other
        static int stickyWindowPixel = 15 ;
        // List of Active Windows
        static public List<Window> ActivitedWindows = new List<Window>();
        static public List<Window> StickToMain = new List<Window>();

        // Moving Function with Global Mouse Event
        static public void GlobalMouseMoveExt(object sender, MouseEventExtArgs e)
        {


            if (isDragging)
            {
                double X = ((double)e.X - offset.X);
                double Y = ((double)e.Y - offset.Y);
                double Right = X + senderWindow.Width;
                double Bottom = Y + senderWindow.Height;
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    if (Math.Abs(X - screen.Bounds.X) < stickyWindowPixel)
                        X = screen.Bounds.X;
                    if (Math.Abs(Y - screen.Bounds.Y) < stickyWindowPixel)
                        Y = screen.Bounds.Y;
                    if (Math.Abs(Right - screen.Bounds.Right) < stickyWindowPixel)
                        X = screen.Bounds.Right-senderWindow.Width;
                    if (Math.Abs(Bottom - screen.Bounds.Bottom) < stickyWindowPixel)
                        Y = screen.Bounds.Bottom-senderWindow.Height;
                }
                foreach( var window in ActivitedWindows)
                {
                    if (senderWindow != window || StickToMain.Contains( senderWindow))
                    {
                        if (Math.Abs(X - window.Left) < stickyWindowPixel)
                            X = window.Left;
                        if (Math.Abs(X - (window.Left + window.Width)) < stickyWindowPixel)
                            X = (window.Left + window.Width) ;

                        if (Math.Abs(Y - window.Top) < stickyWindowPixel)
                            Y = window.Top;
                        if (Math.Abs(Y - (window.Top + window.Height)) < stickyWindowPixel)
                            Y = (window.Top + window.Height) ;

                        if (Math.Abs(Right - (window.Left + window.Width)) < stickyWindowPixel)
                            X = (window.Left + window.Width)-senderWindow.Width;
                        if (Math.Abs(Right - window.Left) < stickyWindowPixel)
                            X = window.Left - senderWindow.Width;

                        if (Math.Abs(Bottom - (window.Top + window.Height)) < stickyWindowPixel)
                            Y = (window.Top + window.Height)-senderWindow.Height;
                        if (Math.Abs(Bottom - window.Top) < stickyWindowPixel)
                            Y = window.Top - senderWindow.Height;
                    }
                }

                if ( senderWindow == ActivitedWindows[0])
                {
                    foreach (var window in ActivitedWindows.FindAll(x => x != senderWindow))
                    {
                        if (
                            (window.Top+window.Height == senderWindow.Top && (window.Left > senderWindow.Left-window.Width && window.Left< senderWindow.Left+senderWindow.Width)) ||
                            (window.Left+window.Width == senderWindow.Left && (window.Top > senderWindow.Top - window.Height && window.Top < senderWindow.Top + senderWindow.Height )) ||
                            (window.Top ==senderWindow.Top+senderWindow.Height && (window.Left > senderWindow.Left - window.Width && window.Left < senderWindow.Left + senderWindow.Width))||
                            (window.Left == senderWindow.Left+senderWindow.Width && (window.Top > senderWindow.Top - window.Height && window.Top < senderWindow.Top + senderWindow.Height))
                        )
                        {
                            if (!StickToMain.Contains(window)) StickToMain.Add(window);
                            window.Left = X + (window.Left - senderWindow.Left);
                            window.Top = Y + (window.Top - senderWindow.Top);
                        }else
                            if (StickToMain.Contains(window)) StickToMain.Remove(window);


                    }
                }
                senderWindow.Left = X;
                senderWindow.Top = Y;

            }

        }

        // Click Function with Local Mouse Event (down) and Global Event (Up)
        static public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            senderWindow = sender as Window;
            isDragging = true;
            offset.X = e.GetPosition(sender as Window).X;
            offset.Y = e.GetPosition(sender as Window).Y;
        }
        static public void GlobalMouseUpExt(object sender, MouseEventExtArgs e)
        {
            isDragging = false;
        }
    }
}
