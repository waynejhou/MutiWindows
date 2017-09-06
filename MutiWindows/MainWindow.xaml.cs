using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
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

namespace MutiWindows
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents mouseGlobalEvents;
        public MainWindow()
        {
            // Init MouseDragging Event;
            mouseGlobalEvents = Hook.GlobalEvents();
            mouseGlobalEvents.MouseMoveExt += MouseDragging.GlobalMouseMoveExt;
            mouseGlobalEvents.MouseUpExt += MouseDragging.GlobalMouseUpExt;
            this.MouseLeftButtonDown += MouseDragging.MouseLeftButtonDown;
            //
            InitializeComponent();
            MouseDragging.ActivitedWindows.Add(this);
            // Init SubWindows
            SubWindows1 subWindows1 = new SubWindows1();
            subWindows1.Show();
            MouseDragging.ActivitedWindows.Add(subWindows1);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new System.Windows.Forms.OpenFileDialog();
            filePicker.Filter = "Audio files(*.mp3;*.wav;*.flac)|*.mp3;*.wav;*.flac";
            switch (filePicker.ShowDialog())
            {
                case System.Windows.Forms.DialogResult.None:
                    break;
                case System.Windows.Forms.DialogResult.OK:
                    TagLib.Tag tag = TagLib.File.Create(filePicker.FileName).Tag;
                    LabelTitle.Content = tag.Title;
                    LabelAlbumArtists.Content = tag.Album + "◆" + ((tag.Performers.Length > 0) ? tag.Performers[0] : "");
                    ImageAlbum.Source = (tag.Pictures.Length > 0) ? BytesToImageSource(tag.Pictures[0].Data.Data) : null;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                    break;
                case System.Windows.Forms.DialogResult.Abort:
                    break;
                case System.Windows.Forms.DialogResult.Retry:
                    break;
                case System.Windows.Forms.DialogResult.Ignore:
                    break;
                case System.Windows.Forms.DialogResult.Yes:
                    break;
                case System.Windows.Forms.DialogResult.No:
                    break;
                default:
                    break;
            }
        }
        BitmapImage BytesToImageSource(Byte[] b)
        {
            using (MemoryStream memory = new MemoryStream(b))
            {
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
    }
}
