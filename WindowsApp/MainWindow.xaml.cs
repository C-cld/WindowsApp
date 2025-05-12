using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string configFile = "config.json";
        private Config config;
        private static readonly double ScreenWidth = SystemParameters.PrimaryScreenWidth;
        private static readonly double ScreenHeight = SystemParameters.PrimaryScreenHeight;
        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            InitializeContentAsync();
        }

        private void InitializeWindow()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args == null || args.Length <= 1)
            {
                LoadConfig(configFile);
            }
            else
            {
                configFile = args[1];
                LoadConfig(configFile);
            }
            
            // this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Left = config.position[0];
            this.Top = config.position[1];
            this.Width = config.position[2];
            this.Height = config.position[3];
            //this.Width = ScreenWidth * 0.75;
            //this.Height = ScreenHeight * 0.75;
            this.tile.Text = config.title;

            InitTheme();

            //string iconPath = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + config.icon;
            //if (File.Exists(iconPath))
            //{
            //    this.Icon = new BitmapImage(new Uri(config.icon, UriKind.RelativeOrAbsolute));
            //}

            string welcomeImgPath = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + config.welcomeImg;
            if (File.Exists(welcomeImgPath))
            {
                this.WelcomeImg.Source = new BitmapImage(new Uri(welcomeImgPath, UriKind.RelativeOrAbsolute));
            }
            
        }

        private void InitTheme()
        {
            string color = config.theme.foreColor.Replace("#", "");
            byte r = Convert.ToByte(color.Substring(0, 2), 16);
            byte g = Convert.ToByte(color.Substring(2, 2), 16);
            byte b = Convert.ToByte(color.Substring(4, 2), 16);
            this.tile.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.close.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.max.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.min.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.sideBar.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));

            string bgColor = config.theme.bgColor.Replace("#", "");
            r = Convert.ToByte(bgColor.Substring(0, 2), 16);
            g = Convert.ToByte(bgColor.Substring(2, 2), 16);
            b = Convert.ToByte(bgColor.Substring(4, 2), 16);
            this.customHeaderBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.close.Background = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.max.Background = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.min.Background = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
            this.sideBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
        }

        private async void InitializeContentAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView.CoreWebView2.Navigate(config.uri);
            webView.CoreWebView2.DOMContentLoaded += LoadCompleted;
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            //webView.CoreWebView2.NavigationCompleted += webView_NavigationCompleted;
        }

        private void LoadCompleted(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                webView.Visibility = Visibility.Visible;
                // DisableContextMenue();
            });
        }

        /// <summary>
        /// 使用默认浏览器打开新窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = e.Uri,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        public void LoadConfig(string configPath)
        {
            if (File.Exists(configPath))
            {
                string configStr = File.ReadAllText(configPath);
                config = JsonSerializer.Deserialize<Config>(configStr);

                if (config.theme == null)
                {
                    config.theme = new Theme("#000000", "#FFFFFF");
                }
                if (config.position == null)
                {
                    config.position = new double[] { this.Left, this.Top, ScreenWidth * 0.75, ScreenHeight * 0.75 };
                }
            }
            else
            {
                config = new Config();
                config.title = "Blank";
                config.uri = "http://127.0.0.1";
                config.icon = "img\\icon32.ico";
                config.welcomeImg = "img\\welcome.png";
                config.theme = new Theme("#000000", "#FFFFFF");
                // SaveConfig();
            }

        }

        public void SaveConfig()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonString = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configFile, jsonString);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webView.Dispose();
            config.position = new double[] { this.Left, this.Top, this.Width, this.Height };
            SaveConfig();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SideBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Width = ScreenWidth * 0.2;
            this.Height = ScreenHeight - TaskBarUtil.GetTaskbarHeight();
            this.Left = ScreenWidth - this.Width;
            this.Top = 0;
            this.Topmost = true;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // P/Invoke 方法
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        // 调整窗口大小的方向
        private void ResizeWindow(ResizeDirection direction)
        {
            if (WindowState == WindowState.Maximized)
                return;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);

            ReleaseCapture();
            SendMessage(handle, 0x112, (IntPtr)direction, IntPtr.Zero);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x46) // WM_WINDOWPOSCHANGING
            {
                handled = true;
            }
            return IntPtr.Zero;
        }

        // 调整大小的方向枚举
        private enum ResizeDirection
        {
            Left = 61441,
            Right = 61442,
            Top = 61443,
            TopLeft = 61444,
            TopRight = 61445,
            Bottom = 61446,
            BottomLeft = 61447,
            BottomRight = 61448
        }

        // 鼠标事件处理
        private void ResizeLeft(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.Left);
        private void ResizeRight(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.Right);
        private void ResizeTop(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.Top);
        private void ResizeTopLeft(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.TopLeft);
        private void ResizeTopRight(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.TopRight);
        private void ResizeBottom(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.Bottom);
        private void ResizeBottomLeft(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.BottomLeft);
        private void ResizeBottomRight(object sender, MouseButtonEventArgs e) => ResizeWindow(ResizeDirection.BottomRight);

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Topmost = false;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            this.Topmost = false;
        }
    }
}