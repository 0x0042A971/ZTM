using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ZTM.Custom
{
    /// <summary>
    /// UI логина. Тут вводятся API ключ и ID сети. Далее UI скрывается.
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }
        
        private void Checkb_Click(object sender, RoutedEventArgs e)
        {
            bool remem = RememberCB.IsChecked.HasValue ? RememberCB.IsChecked.Value : false;
            if (remem)
            {
                try
                {
                    // Запись ключа и айди сети в папку в моих документах
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ZTM";
                    
                    bool path_exist = System.IO.Directory.Exists(path);
                    if (!path_exist) System.IO.Directory.CreateDirectory(path);

                    FileStream FS = new FileStream(path + "/config.txt", FileMode.Create);
                    StreamWriter SW = new StreamWriter(FS);

                    SW.WriteLine(TextBoxApi.Text);
                    SW.WriteLine(TextBoxNetwork.Text);

                    SW.Close();
                    FS.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            try
            {
                MainWindow.NetID = TextBoxNetwork.Text;
                MainWindow.APIKey = TextBoxApi.Text;
                MainWindow.users = MainWindow.ZTUser.fetch(TextBoxApi.Text, TextBoxNetwork.Text);
                if(MainWindow.ZTUser.IsOK) this.Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Чтение ключа и айди сети из папки в моих документах
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ZTM";
                bool path_exist = System.IO.Directory.Exists(path);
                if (!path_exist) return;

                FileStream FS = new FileStream(path + "/config.txt", FileMode.Open);
                StreamReader SR = new StreamReader(FS);

                TextBoxApi.Text = SR.ReadLine();
                TextBoxNetwork.Text = SR.ReadLine();

                SR.Close();
                FS.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
