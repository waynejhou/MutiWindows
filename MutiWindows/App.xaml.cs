using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MutiWindows
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        private void Application_Activated(object sender, EventArgs e)
        {
            foreach(Window w in Windows)
            {
                w.Activate();
            }
        }
    }
}
