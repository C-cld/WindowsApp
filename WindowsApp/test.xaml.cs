using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WindowsApp
{
    /// <summary>
    /// test.xaml 的交互逻辑
    /// </summary>
    public partial class test : Window
    {
        public test()
        {
            InitializeComponent();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaxRestoreButton_Click(object sender, RoutedEventArgs e) => ToggleMaximizeRestore();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void ToggleMaximizeRestore()
        {
            // 最大化
            var workingArea = SystemParameters.WorkArea;

            // 设置最大化时的窗口大小
            this.Width = workingArea.Width;
            this.Height = workingArea.Height;
            this.Left = workingArea.Left;
            this.Top = workingArea.Top;
        }
    }
}
