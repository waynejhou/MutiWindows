using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MutiWindows
{
    class MouseDragging
    {
        static IKeyboardMouseEvents mouseGlobalEvents;
        static bool isDragging = false;
        static bool isResizeing = false;
        static bool preResizeing = false;
        static Point offsetPoint = new Point();
        static Point offsetScreen = new Point();
        static Rect offsetRect = new Rect();
        static int Counter = 0;
        static Window senderWindow;
        static int stickyWindowPixel = 15;
        static public int StickyPixel { get => stickyWindowPixel; set => stickyWindowPixel = value; }
        static int resizeWindowPixel = 10;
        static public int ResizePixel { get => resizeWindowPixel; set => resizeWindowPixel = value; }
        static List<Window> ActivitedWindows = new List<Window>();
        static List<Window> StickToMain = new List<Window>();
        static RelativeDirection LastDirect = RelativeDirection.None;
        static RelativeDirection Direct = RelativeDirection.None;

        static public void ActiveWindowMouseDragging()
        {
            mouseGlobalEvents = Hook.GlobalEvents();
            mouseGlobalEvents.MouseMoveExt += GlobalMouseMoveExt;
            mouseGlobalEvents.MouseUpExt += GlobalMouseUpExt;
            mouseGlobalEvents.MouseDoubleClick += GlobalMouseDoubleClick;
        }
        static public void AddWindow( Window window, bool enableResize=true)
        {
            if (!ActivitedWindows.Contains(window))
                ActivitedWindows.Add(window);
            window.MouseLeftButtonDown += WindowMouseLeftButtonDown;
            if (enableResize)
            {
                window.MouseMove += WindowMouseMove;
                window.MouseLeave += WindowMouseLeave;
            }
        }

        static void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            senderWindow = Window.GetWindow(sender as DependencyObject);
            if (Dilate(new Rect(0,0,senderWindow.ActualWidth,senderWindow.ActualHeight), -resizeWindowPixel).Contains(e.GetPosition(null)))
            {
                isDragging = true;
                offsetPoint.X = e.GetPosition(null).X;
                offsetPoint.Y = e.GetPosition(null).Y;
            }
            else
            {
                isResizeing = true;
                offsetScreen.X = System.Windows.Forms.Control.MousePosition.X;
                offsetScreen.Y = System.Windows.Forms.Control.MousePosition.Y;
                offsetRect = senderWindow.RestoreBounds;
            }

        }
        static void WindowMouseLeave(object sender, MouseEventArgs e)
        {
            LastDirect = RelativeDirection.None;
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        static void WindowMouseMove(object sender, MouseEventArgs e)
        {
            if (!isResizeing)
            {
                Direct = WindowsBoundaryMouseDirection((sender as Window).RenderSize, e.GetPosition(null), resizeWindowPixel);
                if (LastDirect != Direct)
                {
                    if (Direct == RelativeDirection.Top || Direct == RelativeDirection.Bottom)
                        Mouse.OverrideCursor = Cursors.SizeNS;
                    if (Direct == RelativeDirection.Left || Direct == RelativeDirection.Right)
                        Mouse.OverrideCursor = Cursors.SizeWE;
                    if (Direct == RelativeDirection.TopLeft || Direct == RelativeDirection.BottomRight)
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                    if (Direct == RelativeDirection.BottomLeft || Direct == RelativeDirection.TopRight)
                        Mouse.OverrideCursor = Cursors.SizeNESW;
                    if (Direct == RelativeDirection.None)
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }
                    LastDirect = Direct;
                }
            }

        }

        static void GlobalMouseMoveExt(object sender, MouseEventExtArgs e)
        {
            if (isDragging)
            {
                Point fp = new Point(e.X - offsetPoint.X, e.Y - offsetPoint.Y);
                Rect senderRect = new Rect(fp.X, fp.Y, senderWindow.ActualWidth, senderWindow.Height);
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    var result = ScreenBoundaryCollisionDirection(senderRect,
                        new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height));
                    if (result.Is1stDirection(BaseDirection.Left))
                        fp.X = screen.Bounds.Left;
                    if (result.Is1stDirection(BaseDirection.Right))
                        fp.X = screen.Bounds.Right - senderRect.Width;
                    if (result.Is1stDirection(BaseDirection.Top))
                        fp.Y = screen.Bounds.Top;
                    if (result.Is1stDirection(BaseDirection.Bottom))
                        fp.Y = screen.Bounds.Bottom - senderRect.Height;
                }
                foreach (var window in ActivitedWindows.FindAll((Window x) => x.IsVisible))
                {
                    if (window != senderWindow)
                    {
                        var result = WindowBoundaryCollisionDirection(senderRect, window.RestoreBounds);
                        if (result.Is1stDirection(BaseDirection.Left))
                            fp.X = window.RestoreBounds.Left - senderRect.Width;
                        if (result.Is1stDirection(BaseDirection.Right))
                            fp.X = window.RestoreBounds.Right;
                        if (result.Is1stDirection(BaseDirection.Top))
                            fp.Y = window.RestoreBounds.Top - senderRect.Height;
                        if (result.Is1stDirection(BaseDirection.Bottom))
                            fp.Y = window.RestoreBounds.Bottom;
                        if (result.Is2ndDirection(BaseDirection.Left))
                            fp.X = window.RestoreBounds.Left;
                        if (result.Is2ndDirection(BaseDirection.Right))
                            fp.X = window.RestoreBounds.Right - senderRect.Width;
                        if (result.Is2ndDirection(BaseDirection.Top))
                            fp.Y = window.RestoreBounds.Top;
                        if (result.Is2ndDirection(BaseDirection.Bottom))
                            fp.Y = window.RestoreBounds.Bottom - senderRect.Height;
                    }

                }
                senderWindow.Left = fp.X;
                senderWindow.Top = fp.Y;
            }
            if (isResizeing)
            {
                Point Axis = new Point(
                    System.Windows.Forms.Control.MousePosition.X - offsetScreen.X,
                    System.Windows.Forms.Control.MousePosition.Y - offsetScreen.Y);
                Rect fr = offsetRect;
                if (Direct.Is1stDirection(BaseDirection.Top))
                {
                    if (offsetRect.Height - Axis.Y > senderWindow.MinHeight)
                    {
                        fr.Y = offsetRect.Y + Axis.Y;
                        fr.Height = offsetRect.Height - Axis.Y;
                    }

                }
                if (Direct.Is1stDirection(BaseDirection.Bottom))
                {
                    fr.Height = Math.Max(offsetRect.Height + Axis.Y, senderWindow.MinHeight);
                }
                if (Direct.Is1stDirection(BaseDirection.Left))
                {
                    if (offsetRect.Width - Axis.X > senderWindow.MinWidth)
                    {
                        fr.X = offsetRect.X + Axis.X;
                        fr.Width = offsetRect.Width - Axis.X;
                    }
                }
                if (Direct.Is1stDirection(BaseDirection.Right))
                {
                    fr.Width = Math.Max(offsetRect.Width + Axis.X, senderWindow.MinWidth);
                }
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    var result = ScreenBoundaryCollisionDirection(fr,
                        new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height));
                    if (result.Is1stDirection(BaseDirection.Left))
                    {
                        fr.X = screen.Bounds.Left;
                        if (offsetRect.Left != screen.Bounds.Left)
                            fr.Width = offsetRect.Width + offsetRect.X;
                    }
                    if (result.Is1stDirection(BaseDirection.Right))
                    {
                        if (offsetRect.Right != screen.Bounds.Right)
                            fr.Width = screen.Bounds.Right - offsetRect.X;
                    }
                    if (result.Is1stDirection(BaseDirection.Top))
                    {
                        fr.Y = screen.Bounds.Top;
                        if (offsetRect.Top != screen.Bounds.Top)
                            fr.Height = offsetRect.Height + offsetRect.Y;
                    }
                    if (result.Is1stDirection(BaseDirection.Bottom))
                    {
                        if (offsetRect.Bottom != screen.Bounds.Bottom)
                            fr.Height = screen.Bounds.Bottom - offsetRect.Y;
                    }
                }
                foreach (var window in ActivitedWindows.FindAll((Window x)=>x.IsVisible))
                {
                    if (window != senderWindow)
                    {
                        var result = WindowBoundaryCollisionDirection(fr, window.RestoreBounds);
                        var wr = window.RestoreBounds;
                        if (result.Is1stDirection(BaseDirection.Left))
                        {
                            if (offsetRect.Right != wr.Left)
                                fr.Width = wr.Left - offsetRect.X;
                        }
                        if (result.Is1stDirection(BaseDirection.Right))
                        {
                            fr.X = wr.Right;
                            if (offsetRect.Left != wr.Right)
                                fr.Width = offsetRect.Width + (offsetRect.X - wr.Right);
                        }
                        if (result.Is1stDirection(BaseDirection.Top))
                        {
                            if (offsetRect.Bottom != wr.Top)
                                fr.Height = wr.Top - offsetRect.Y;
                        }
                        if (result.Is1stDirection(BaseDirection.Bottom))
                        {
                            fr.Y = wr.Bottom;
                            if (offsetRect.Top != wr.Bottom)
                                fr.Height = offsetRect.Height + (offsetRect.Y - wr.Bottom);
                        }
                        if (result.Is2ndDirection(BaseDirection.Left))
                        {
                            fr.X = wr.Left;
                            if (offsetRect.Left != wr.Left)
                                fr.Width = offsetRect.Width + (offsetRect.X - wr.Left);
                        }
                        if (result.Is2ndDirection(BaseDirection.Right))
                        {
                            if (offsetRect.Right != wr.Right)
                                fr.Width = wr.Right - offsetRect.X;
                        }
                        if (result.Is2ndDirection(BaseDirection.Top))
                        {
                            fr.Y = wr.Top;
                            if (offsetRect.Top != wr.Top)
                                fr.Height = offsetRect.Height + (offsetRect.Y-wr.Top);
                        }
                        if (result.Is2ndDirection(BaseDirection.Bottom))
                        {
                            if (offsetRect.Bottom != wr.Bottom)
                                fr.Height = wr.Bottom - offsetRect.Y;
                        }
                    }
                }
                senderWindow.Top = fr.Y;
                senderWindow.Left = fr.X;
                senderWindow.Width = fr.Width;
                senderWindow.Height = fr.Height;
            }
            Counter += 1;
        }
        static void GlobalMouseUpExt(object sender, MouseEventExtArgs e)
        {
            isDragging = false;
            isResizeing = false;
        }
        static void GlobalMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isDragging = false;
        }

        static private RelativeDirection ScreenBoundaryCollisionDirection(Rect A, Rect Screen)
        {

            RelativeDirection ret = new RelativeDirection(0x00);
            if (Dilate(Screen, stickyWindowPixel).IntersectsWith(A))
            {
                var TopOffset = Math.Abs(A.Top - Screen.Top);
                var LeftOffset = Math.Abs(A.Left - Screen.Left);
                var BottomOffset = Math.Abs(A.Bottom - Screen.Bottom);
                var RightOffset = Math.Abs(A.Right - Screen.Right);
                if (TopOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Top);
                if (LeftOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Left);
                if (BottomOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Bottom);
                if (RightOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Right);
            }
            return ret;
        }
        static private RelativeDirection WindowBoundaryCollisionDirection(Rect Actived, Rect Fixed)
        {
            var dilateB = Dilate(Fixed, stickyWindowPixel);
            RelativeDirection ret = new RelativeDirection(0x00);
            if(dilateB.IntersectsWith(Actived))
            {
                var TopOffset = Math.Abs(Actived.Bottom - Fixed.Top);
                var LeftOffset = Math.Abs(Actived.Right - Fixed.Left);
                var BottomOffset = Math.Abs(Actived.Top - Fixed.Bottom);
                var RightOffset = Math.Abs(Actived.Left - Fixed.Right);
                if (TopOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Top);
                if (LeftOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Left);
                if (BottomOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Bottom);
                if (RightOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Right);
                TopOffset = Math.Abs(Actived.Top - Fixed.Top);
                LeftOffset = Math.Abs(Actived.Left - Fixed.Left);
                BottomOffset = Math.Abs(Actived.Bottom - Fixed.Bottom);
                RightOffset = Math.Abs(Actived.Right - Fixed.Right);
                if (TopOffset < stickyWindowPixel)
                    ret.Add2ndDirection(BaseDirection.Top);
                if (LeftOffset < stickyWindowPixel)
                    ret.Add2ndDirection(BaseDirection.Left);
                if (BottomOffset < stickyWindowPixel)
                    ret.Add2ndDirection(BaseDirection.Bottom);
                if (RightOffset < stickyWindowPixel)
                    ret.Add2ndDirection(BaseDirection.Right);
            }

            return ret;
        }
        static private RelativeDirection WindowsBoundaryMouseDirection(Size size,Point pt, int pixel)
        {
            RelativeDirection ret = RelativeDirection.None;
            if (pt.Y < pixel)
                ret.Add1stDirection(BaseDirection.Top);
            if (pt.Y > size.Height - pixel)
                ret.Add1stDirection(BaseDirection.Bottom);
            if (pt.X < pixel)
                ret.Add1stDirection(BaseDirection.Left);
            if (pt.X > size.Width - pixel)
                ret.Add1stDirection(BaseDirection.Right);
            return ret;
        }
        static private Rect Dilate( Rect rect, int pixel)
        {
            rect.X -= pixel;
            rect.Y -= pixel;
            rect.Width += pixel * 2;
            rect.Height += pixel * 2;
            return rect;
        }
    }
    /// byte
    /// touched   untouched
    /// t b l r   t b l r
    /// 0 0 0 0   0 0 0 0
    /// 1000 0000         0x80
    ///  0100 0000        0x40
    ///   0010 0000       0x20
    ///    0001 0000      0x10
    ///      0000 1000    0x08
    ///       0000 0100   0x04
    ///        0000 0010  0x02
    ///         0000 0001 0x01
    public enum BaseDirection { None = 0, Top = 8, Bottom = 4, Left = 2, Right = 1 }
    struct RelativeDirection
    {
        private int Direct;
        public RelativeDirection(int direct)
        {
            Direct = direct;
        }
        static public RelativeDirection None => new RelativeDirection(0x00);
        static public RelativeDirection Top => new RelativeDirection(0x80);
        static public RelativeDirection Bottom => new RelativeDirection(0x40);
        static public RelativeDirection Left => new RelativeDirection(0x20);
        static public RelativeDirection Right => new RelativeDirection(0x10);
        static public RelativeDirection TopLeft => new RelativeDirection(0xa0);
        static public RelativeDirection TopRight => new RelativeDirection(0x90);
        static public RelativeDirection BottomLeft => new RelativeDirection(0x60);
        static public RelativeDirection BottomRight => new RelativeDirection(0x50);
        static public bool operator ==(RelativeDirection A, RelativeDirection B) => A.Direct == B.Direct;
        static public bool operator !=(RelativeDirection A, RelativeDirection B) => A.Direct != B.Direct;
        public bool Is1stDirection(BaseDirection bd)
        {
            return (Direct & (int)bd * 0x10) == (int)bd * 0x10;
        }
        public bool Is2ndDirection(BaseDirection bd)
        {
            return (Direct & (int)bd) == (int)bd;
        }
        public RelativeDirection Add1stDirection(BaseDirection bd)
        {
            Direct = Direct + (int)bd * 0x10;
            return this;
        }
        public RelativeDirection Add2ndDirection(BaseDirection bd)
        {
            Direct = Direct + (int)bd;
            return this;
        }
        public RelativeDirection Minus1stDirection(BaseDirection bd)
        {
            Direct = Direct - (int)bd * 0x10;
            return this;
        }
        public RelativeDirection Minus2ndDirection(BaseDirection bd)
        {
            Direct = Direct - (int) bd;
            return this;
        }
        public RelativeDirection Invert()
        {
            int a = 0, b = 0, c = 0, d = 0;
            if ((Direct & 0x80) != (Direct & 0x40))
                a = (~(Direct & 0xc0)) & 0xc0;
            if ((Direct & 0x20) != (Direct & 0x10))
                b = (~(Direct & 0x30)) & 0x30;
            if ((Direct & 0x08) != (Direct & 0x04))
                c = (~(Direct & 0x0c)) & 0x0c;
            if ((Direct & 0x02) != (Direct & 01))
                d = (~(Direct & 0x03)) & 0x03;
            return new RelativeDirection(a + b + c + d);
        }
        public override string ToString()
        {
            string f = "", a = "";
            if ((Direct & 0x80) == 0x80)
                f += "Top";
            if ((Direct & 0x40) == 0x40)
                f += "Bottom";
            if ((Direct & 0x20) == 0x20)
                f += "Left";
            if ((Direct & 0x10) == 0x10)
                f += "Right";
            if ((Direct & 0x08) == 0x08)
                a += "Top";
            if ((Direct & 0x04) == 0x04)
                a += "Bottom";
            if ((Direct & 0x02) == 0x02)
                a += "Left";
            if ((Direct & 0x01) == 0x01)
                a += "Right";
            return f + "-" + a;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
