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

namespace ZTM.Forms
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxApiKey.Text = Clipboard.GetText();
            }
            catch { }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TextBoxApiKeySafe.Text);
            }
            catch { }
        }

        private void TextBoxApiKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxApiKeySafe.Text = MainWindow.Encrypt(TextBoxApiKey.Text);
        }
    }
}
