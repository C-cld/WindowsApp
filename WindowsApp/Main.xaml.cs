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
        /// <summary>
        /// 当前位置缓存
        /// </summary>
        private double[] currPosition = null;

        public Main()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableShadow(new WindowInteropHelper(this).Handle);
        }


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
            currPosition = new double[] { double.Parse(position[0]), double.Parse(position[1]), double.Parse(position[2]), double.Parse(position[3]) };
            this.Left = currPosition[0];
            this.Top = currPosition[1];
            this.Width = currPosition[2];
            this.Height = currPosition[3];
            this.WindowTitle.Text = config.title;
            this.Title = config.title;
            if (this.Left == 0 && this.Top == 0)
            {
                ToggleMaximizeRestore();
            }
            // 初始化主题
            if (config.theme == null || config.theme.Trim().ToLower() == "light")
            {
                OuterBorder.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                WindowTitle.Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
                SideBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
                MinBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
                MaxBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
                CloseBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            }
            else if (config.theme.Trim().ToLower() == "dark")
            {
                OuterBorder.Background = new SolidColorBrush(Color.FromArgb(0, 51, 51, 51));
                WindowTitle.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                SideBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                MinBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                MaxBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                CloseBtn.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
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
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView.CoreWebView2.Navigate(config.uri);
            webView.CoreWebView2.DOMContentLoaded += (sender, args) =>
            {
                
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
                MaxIcon.Text = "◱";
            }
            else
            {
                this.Width = ScreenWidth * 0.75;
                this.Height = ScreenHeight * 0.75;
                this.Left = (ScreenWidth - this.Width) / 2;
                this.Top = (ScreenHeight - this.Height) / 2;

                //OuterBorder.CornerRadius = new CornerRadius(5);

                this.onMax = false;
                MaxIcon.Text = "▢";
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            this.currPosition = new double[] { this.Left, this.Top, this.Width, this.Height };
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

        // 阴影相关
        private void EnableShadow(IntPtr hwnd)
        {
            if (IsDwmEnabled())
            {
                var margins = new MARGINS { cxLeftWidth = 1, cxRightWidth = 1, cyTopHeight = 1, cyBottomHeight = 1 };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
            else
            {
                OuterBorder.Effect = new DropShadowEffect
                {
                    BlurRadius = 12,
                    ShadowDepth = 0,
                    Opacity = 0.12
                };
            }
        }

        #region DWM Interop
        [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")] private static extern int DwmIsCompositionEnabled(out bool enabled);
        private static bool IsDwmEnabled()
        {
            try { DwmIsCompositionEnabled(out bool enabled); return enabled; } catch { return false; }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }
        #endregion
    }
}
