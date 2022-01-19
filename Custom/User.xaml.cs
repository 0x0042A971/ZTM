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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZTM.Custom
{
    /// <summary>
    /// Ячейка пользователя. Ник, статус, иконка, айпи и кнопка для копирования.
    /// </summary>
    public partial class User : UserControl
    {
        public User()
        {
            InitializeComponent();
            IP.MouseLeftButtonDown += IP_MouseLeftButtonDown;
        }
        private readonly BrushConverter bc = new BrushConverter();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(IP.Text);
            IP.Foreground = MainWindow.brush_copy;
            await Task.Delay(300);
            IP.Foreground = MainWindow.brush_text;
        }

        private async void IP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(IP.Text);
            IP.Foreground = MainWindow.brush_copy;
            await Task.Delay(300);
            IP.Foreground = MainWindow.brush_text;
        }
    }
}
