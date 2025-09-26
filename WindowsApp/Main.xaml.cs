using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using WpfAnimatedGif;

namespace WindowsApp
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        private static readonly double ScreenWidth = SystemParameters.PrimaryScreenWidth;
        private static readonly double ScreenHeight = SystemParameters.PrimaryScreenHeight;

        private bool onMax = false;

        /// <summary>
        /// 默认配置文件
        /// </summary>
        private string configFile = "config.json";
        /// <summary>
        /// 配置缓存
        /// </summary>
        private Config config;

        public Main()
        {
            InitializeComponent();
            InitializeWindow();
            this.SourceInitialized += MainWindow_SourceInitialized;

        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            /*HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source != null)
            {
                source.AddHook(WndProc);
            }*/
        }

        /*private const int RESIZE_HANDLE_SIZE = 5;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;


            if (msg == WM_NCHITTEST)
            {
                handled = true;
                return (IntPtr)HTTOPRIGHT;
                *//*Point cursorPos = Mouse.GetPosition(this); // 相对于窗口客户区左上角
                Debug.WriteLine("X:" + cursorPos.X + " Y:" + cursorPos.Y);
                Rect clientRect = new Rect(this.Top, this.Left, this.ActualWidth, this.ActualHeight);

                if (cursorPos.X <= RESIZE_HANDLE_SIZE)
                {
                    if (cursorPos.Y <= RESIZE_HANDLE_SIZE)
                    {
                        handled = true; return (IntPtr)HTTOPLEFT;
                    }
                    else if (cursorPos.Y >= clientRect.Height - RESIZE_HANDLE_SIZE)
                    {
                        handled = true; return (IntPtr)HTBOTTOMLEFT;
                    }
                    else
                    {
                        handled = true; return (IntPtr)HTLEFT;
                    }
                }
                else if (cursorPos.X >= clientRect.Width - RESIZE_HANDLE_SIZE)
                {
                    if (cursorPos.Y <= RESIZE_HANDLE_SIZE)
                    {
                        handled = true; return (IntPtr)HTTOPRIGHT;
                    }
                    else if (cursorPos.Y >= clientRect.Height - RESIZE_HANDLE_SIZE)
                    {
                        handled = true; return (IntPtr)HTBOTTOMRIGHT;
                    }
                    else
                    {
                        handled = true; return (IntPtr)HTRIGHT;
                    }
                }
                else if (cursorPos.Y <= RESIZE_HANDLE_SIZE)
                {
                    handled = true; return (IntPtr)HTTOP;
                }
                else if (cursorPos.Y >= clientRect.Height - RESIZE_HANDLE_SIZE)
                {
                    handled = true; return (IntPtr)HTBOTTOM;
                }*//*
            }

            return IntPtr.Zero;
        }*/


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void TopBtn_Click(object sender, RoutedEventArgs e) => ToggleTopRestore();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaxRestoreButton_Click(object sender, RoutedEventArgs e) => ToggleMaximizeRestore();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();


        private void InitializeWindow()
        {
            // 初始化配置文件
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                configFile = args[1];
            }
            if (!LoadConfig(configFile))
            {
                return;
            }
            // 初始化图标
            if (config.icon != null && config.icon.Trim() != "")
            {
                string iconPath = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + config.icon;
                if (File.Exists(iconPath))
                {
                    this.WindowIcon.Source = new BitmapImage(new Uri(iconPath));
                }
            }
            // 初始化位置
            string[] position = config.position.Split(",");
            this.Left = double.Parse(position[0]);
            this.Top = double.Parse(position[1]);
            this.Width = double.Parse(position[2]);
            this.Height = double.Parse(position[3]);
            this.Title = config.title;
            if (this.Left == 0 && this.Top == 0)
            {
                ToggleMaximizeRestore();
            }
            // 初始化主题
            if (config.theme == null || config.theme.Trim().ToLower() == "light")
            {
            }
            else if (config.theme.Trim().ToLower() == "dark")
            {
            }
            // 初始化网页
            this.InitializeContentAsync(this.webview);
        }

        public bool LoadConfig(string configPath)
        {
            try
            {
                if (File.Exists(configPath))
                {
                    string configStr = File.ReadAllText(configPath);
                    var options = new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                    };
                    config = JsonSerializer.Deserialize<Config>(configStr, options);
                }
                return true;
            }
            catch(Exception e)
            {
                webview.Visibility = Visibility.Collapsed;
                Tips.Visibility = Visibility.Visible;
                Tips.Text = "配置文件读取失败，请检查 " + configPath + " 文件内容是否正确。\n\n" + e.Message;
            }
            return false;
        }

        private async void InitializeContentAsync(WebView2 webView)
        {
            webView.CoreWebView2InitializationCompleted += (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    //webView.CoreWebView2.Settings.UserAgent =
                    //    "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";

                    // 或者设置为 Android 设备的 UA
                    // webView.CoreWebView2.Settings.UserAgent =
                    //     "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Mobile Safari/537.36";

                    webView.CoreWebView2.Navigate(config.uri);
                }
            };
            // 指定一个固定目录保存 WebView2 的用户数据
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WindowsAppData"));
            await webView.EnsureCoreWebView2Async(env);

            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            //webView.CoreWebView2.Navigate(config.uri);
            webView.CoreWebView2.DOMContentLoaded += (sender, args) =>
            {
                
            };
            webView.CoreWebView2.NavigationCompleted += (sender, args) =>
            {
                // 缩放比例（1.0 = 100%，0.9 = 90%）
                double value = double.Parse(config.zoom.TrimEnd('%')) / 100;
                webView.ZoomFactor = value;
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
        }

        private void ToggleTopRestore()
        {
            if (this.Topmost)
            {
                this.TopBtn.Background = Brushes.Transparent;
                this.Topmost = false;
            }
            else
            {
                this.TopBtn.Background = new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
                this.Topmost = true;
            }
        }

        private void ToggleMaximizeRestore()
        {
            if (!onMax)
            {
                // 最大化
                var workingArea = SystemParameters.WorkArea;

                // 设置最大化时的窗口大小
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;

                //OuterBorder.CornerRadius = new CornerRadius(1);

                this.onMax = true;
                MaxIcon.Text = "🗗";
            }
            else
            {
                this.Width = ScreenWidth * 0.75;
                this.Height = ScreenHeight * 0.75;
                this.Left = (ScreenWidth - this.Width) / 2;
                this.Top = (ScreenHeight - this.Height) / 2;

                //OuterBorder.CornerRadius = new CornerRadius(5);

                this.onMax = false;
                MaxIcon.Text = "🗖";
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webview.Dispose();
            if (config != null)
            {
                config.position = string.Join(",", new double[] { this.Left, this.Top, this.Width, this.Height });
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonString = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configFile, jsonString);
        }

        
    }
}
