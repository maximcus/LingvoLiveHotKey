using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Console = System.Console;

namespace WpfHotKeyTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Subject<Keys> KeyPressedSubject = new Subject<Keys>();

        const string AppName = "LingvoLive Hot Key";
        const string AutostartSubKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string CheckPath = "M10,17L5,12L6.41,10.58L10,14.17L17.59,6.58L19,8M19,3H5C3.89,3 3,3.89 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5C21,3.89 20.1,3 19,3Z";
        private const string UnCheckPath = "M19,3H5C3.89,3 3,3.89 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5C21,3.89 20.1,3 19,3M19,5V19H5V5H19Z";

        public MainWindow()
        {
            InterceptKeys.Start();
            InterceptKeys.KeyPressed += (sender, keys) => 
                KeyPressedSubject.OnNext(keys);

            KeyPressedSubject.ObserveOn(Scheduler.Default)
                .Where(key => key == Keys.LControlKey || key == Keys.C)
                .Select(key => new Tuple<Keys, DateTime>(key, DateTime.Now))
                .Buffer(3, 1)
                .Where(list => list[0].Item1 == Keys.LControlKey && list[1].Item1 == Keys.C && list[2].Item1 == Keys.C && list[2].Item2 - list[0].Item2 < TimeSpan.FromMilliseconds(600))
                .ObserveOnDispatcher()
                .Subscribe(list =>
                {
                    if (Clipboard.ContainsText())
                        Process.Start("https://www.lingvolive.com/en-us/translate/en-ru/" + Clipboard.GetText());
                });

            InitializeComponent();

            if (GetStartup())
                AutostartIconPath.Data = Geometry.Parse(CheckPath);
        }

        private void TaskbarIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void CloseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            InterceptKeys.Stop();
            Close();
        }

        private void AutostartMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            bool Autostart = GetStartup();
            SetStartup(!Autostart);
            SetAutostartIcon(!Autostart);
        }

        private void SetStartup(bool autostart)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(AutostartSubKey, true);

            if (autostart)
                rk?.SetValue(AppName, Assembly.GetExecutingAssembly().Location);
            else
                rk?.DeleteValue(AppName, false);

        }

        private bool GetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(AutostartSubKey, true);

            return rk?.GetValue(AppName) != null;
        }

        private void SetAutostartIcon(bool autostart)
        {
            AutostartIconPath.Data = Geometry.Parse(autostart ? CheckPath : UnCheckPath);
        }
    }
}
