using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
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
using WpfAnimatedGif;
using static System.Net.Mime.MediaTypeNames;

namespace WindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private static readonly double ScreenWidth = SystemParameters.PrimaryScreenWidth;
        private static readonly double ScreenHeight = SystemParameters.PrimaryScreenHeight;

        /// <summary>
        /// 默认配置文件
        /// </summary>
        private string configFile = "config.json";
        /// <summary>
        /// 配置缓存
        /// </summary>
        private Config config;
        /// <summary>
        /// 处于侧边栏
        /// </summary>
        private bool onSide = false;
        /// <summary>
        /// 处于最大化
        /// </summary>
        private bool onMax = false;
        /// <summary>
        /// 当前位置缓存
        /// </summary>
        private double[] currPosition = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // 初始化配置文件
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                configFile = args[1];
            }
            LoadConfig(configFile);

            // 初始化图标
            if (config.icon != null && config.icon.Trim() != "")
            {
                string iconPath = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + config.icon;
                this.Icon = new BitmapImage(new Uri(iconPath));
            }
            

            // 初始化位置
            string[] position = config.position.Split(",");
            currPosition =  new double[] { double.Parse(position[0]), double.Parse(position[1]), double.Parse(position[2]), double.Parse(position[3]) };
            this.Left = currPosition[0];
            this.Top = currPosition[1];
            this.Width = currPosition[2];
            this.Height = currPosition[3];
            this.tile.Text = config.title;
            this.Title = config.title;

            // 初始化颜色
            InitTheme();

            // 初始化网页
            string welcomeImgPath = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + config.welcomeImg;
            if (File.Exists(welcomeImgPath))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(welcomeImgPath, UriKind.RelativeOrAbsolute);
                image.EndInit();
                ImageBehavior.SetAnimatedSource(this.WelcomeImg, image);
            }
            WelcomeImg.LayoutTransform = new ScaleTransform(0.5, 0.5);
            this.InitializeContentAsync(this.webview);

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

        private async void InitializeContentAsync(WebView2 webView)
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView.CoreWebView2.Navigate(config.uri);
            webView.CoreWebView2.DOMContentLoaded += (sender, args) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            };
            webView.CoreWebView2.NewWindowRequested += (sender, args) =>
            {
                args.Handled = true;
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = args.Uri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            };
            webView.CoreWebView2.NavigationCompleted += (sender, args) =>
            {
                // 缩放比例（1.0 = 100%，0.9 = 90%）
                double value = double.Parse(config.zoom.TrimEnd('%')) / 100;
                webView.ZoomFactor = value;
            };
        }

        private void LoadCompleted(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            
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
                var options = new JsonSerializerOptions
                {
                    // 设置读取器选项以处理特殊字符
                    ReadCommentHandling = JsonCommentHandling.Skip,
                };
                config = JsonSerializer.Deserialize<Config>(configStr, options);

                if (config.theme == null)
                {
                    config.theme = new Theme("#000000", "#FFFFFF");
                }
                if (config.position == null)
                {
                    config.position = string.Join(",", new double[] { this.Left, this.Top, ScreenWidth * 0.75, ScreenHeight * 0.75 });
                }
                if (config.zoom == null)
                {
                    config.zoom = "100%";
                }
            }
            else
            {
                config = new Config();
                config.title = "Welcome";
                config.uri = "http://127.0.0.1";
                // config.icon = "img\\icon32.ico";
                config.welcomeImg = "img\\welcome.png";
                config.zoom = "100%";
                config.theme = new Theme("#000000", "#FFFFFF");
                config.position = string.Join(",", new double[] { this.Left, this.Top, ScreenWidth * 0.75, ScreenHeight * 0.75 });
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
            webview.Dispose();
            config.position = string.Join(",", new double[] { this.Left, this.Top, this.Width, this.Height });
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

        /// <summary>
        /// 侧边栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SideBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (onSide)
            {
                this.Topmost = false;
                this.Left = currPosition[0];
                this.Top = currPosition[1];
                this.Width = currPosition[2];
                this.Height = currPosition[3];
                this.onSide = false;
                this.onMax = false;
            }
            else
            {
                this.Width = ScreenWidth * 0.2;
                this.Height = ScreenHeight - TaskBarUtil.GetTaskbarHeight();
                this.Left = ScreenWidth - this.Width;
                this.Top = 0;
                this.Topmost = true;
                this.onSide = true;
                this.onMax = false;
            }
            
        }

        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (onMax)
            {
                this.Left = currPosition[0];
                this.Top = currPosition[1];
                this.Width = currPosition[2];
                this.Height = currPosition[3];
                this.onMax = false;
                this.onSide = false;
            }
            else
            {
                this.Width = ScreenWidth;
                this.Height = ScreenHeight - TaskBarUtil.GetTaskbarHeight();
                this.Left = 0;
                this.Top = 0;
                this.onMax = true;
                this.onSide = false;
            }
        }

        /// <summary>
        /// 拖动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
                this.currPosition = new double[] { this.Left, this.Top, this.Width, this.Height };
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
            if (onMax || onSide)
                return;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);

            ReleaseCapture();
            SendMessage(handle, 0x112, (IntPtr)direction, IntPtr.Zero);

            this.currPosition = new double[] { this.Left, this.Top, this.Width, this.Height };
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

    }
}