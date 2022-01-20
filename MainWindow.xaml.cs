using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZTM
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetupColors();
        }

        #region ZeroTier
        public class ZTUser
        {
            private string _Id;
            private string _Name;
            private string _Description;
            private string _IpAddress;
            private bool _IsOnline;
            private bool _IsAuthorized;
            private DateTime _LastSeen;
            private static int _UserCount;
            private static bool _IsOK;

            public string Id { get { return _Id; } }
            public string Name { get { return _Name; } }
            public string Description { get { return _Description; } }
            public string IpAddress { get { return _IpAddress; } }
            public bool IsOnline { get { return _IsOnline; } }
            public bool IsAuthorized { get { return _IsAuthorized; } }
            public DateTime LastSeen { get { return _LastSeen; } }
            public static int UserCount { get { return _UserCount; } }
            public static bool IsOK { get { return _IsOK; } }

            public ZTUser(string id, string name, string description, string ipAddress, bool isOnline, bool isAuthorized, DateTime lastSeen)
            {
                _Id = id;
                _Name = name;
                _Description = description;
                _IpAddress = ipAddress;
                _IsOnline = isOnline;
                _IsAuthorized = isAuthorized;
                _LastSeen = lastSeen;
            }

            public ZTUser(string id, string name, string description, string ipAddress, string isOnline, string isAuthorized, string lastSeen)
            {
                _Id = id;
                _Name = name;
                _Description = description;
                _IpAddress = ipAddress;
                _IsOnline = StringToBool(isOnline);
                _IsAuthorized = StringToBool(isAuthorized);
                _LastSeen = JsDateTime(lastSeen);
            }

            private static bool StringToBool(string input)
            {
                if (input[0] == 't') return true;
                else return false;
            }

            private static DateTime JsDateTime(string input)
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(Convert.ToInt64(input))
                    .ToLocalTime();
            }

            private string fancy_dt()
            {
                //>year OR (if)days, (if)hours, (if)minutes OR less a minute
                string answer;
                TimeSpan TimeDifference = DateTime.Now - LastSeen;
                if (TimeDifference.TotalDays >= 365) return ">year";
                bool days = TimeDifference.Days >= 1 ? true : false;
                bool hours = TimeDifference.Hours >= 1 ? true : false;
                bool minutes = TimeDifference.Minutes >= 1 ? true : false;

                StringBuilder result = new StringBuilder();
                if (days) result.Append(TimeDifference.Days).Append("D");
                if (days && hours) result.Append(" ");
                if (hours) result.Append(TimeDifference.Hours).Append("H");
                if (hours && minutes) result.Append(" ");
                if (minutes) result.Append(TimeDifference.Minutes).Append("M");
                if (!days && !hours && !minutes) result.Append("Less then a minute");
                answer = result.ToString();

                return answer;
            }

            public void ConsoleCheck()
            {
                Console.WriteLine();
                Console.WriteLine($"User: {Name}");
                Console.WriteLine($"Description: {Description}");
                Console.WriteLine($"Id: {Id}");
                Console.WriteLine($"Ip Address: {IpAddress}");
                if (IsOnline) Console.WriteLine($"Online");
                else Console.WriteLine($"Offline. Last seen: {fancy_dt()}");
                Console.WriteLine($"Authorized: {IsAuthorized}");
            }

            public static ZTUser[] fetch(string bearer, string network)
            {
                _IsOK = false;

                HttpWebRequest Req = (HttpWebRequest)WebRequest.Create($"https://my.zerotier.com/api/v1/network/{network}/member");
                Req.Headers.Add("authorization", $"bearer {bearer}");
                WebResponse ReqResponse;

                try
                {
                    ReqResponse = Req.GetResponse();
                }
                catch
                {
                    return new ZTUser[1];
                }

                string ReqText;

                using (Stream dataStream = ReqResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    ReqText = reader.ReadToEnd();
                }

                ReqResponse.Close();

                string[] ReqTextSplit = ReqText.Split(',');

                _UserCount = 0;
                List<string> user_id_list = new List<string>();
                List<string> user_name_list = new List<string>();
                List<string> user_ip_list = new List<string>();
                List<string> user_online_list = new List<string>();
                List<string> user_description_list = new List<string>();
                List<string> user_authorized_list = new List<string>();
                List<string> user_ot_list = new List<string>();

                foreach (string Line in ReqTextSplit)
                {
                    if (Line[0] == '{' || Line[0] == '[') { user_id_list.Add(Line); _UserCount++; }
                    if (Line[1] == 'n' && Line[2] == 'a') user_name_list.Add(Line);
                    if (Line[1] == 'i' && Line[2] == 'p') user_ip_list.Add(Line);
                    if (Line[1] == 'o' && Line[2] == 'n') user_online_list.Add(Line);
                    if (Line[1] == 'd' && Line[2] == 'e') user_description_list.Add(Line);
                    if (Line[1] == 'a' && Line[2] == 'u') user_authorized_list.Add(Line);
                    if (Line[1] == 'l' && Line[5] == 'O') user_ot_list.Add(Line);
                }

                string[] user_id = new string[_UserCount];
                string[] user_name = new string[_UserCount];
                string[] user_ip = new string[_UserCount];
                string[] user_online = new string[_UserCount];
                string[] user_description = new string[_UserCount];
                string[] user_authorized = new string[_UserCount];
                string[] user_ot = new string[_UserCount];
                ZTUser[] answer = new ZTUser[_UserCount];

                for (int i = 0; i < _UserCount; i++)
                {
                    user_id[i] = i == 0 ? user_id_list[i].Substring(8, user_id_list[i].Length - 9) : user_id_list[i].Substring(7, user_id_list[i].Length - 8);
                    user_name[i] = user_name_list[i].Substring(8, user_name_list[i].Length - 9);
                    user_ip[i] = user_ip_list[i].Substring(18, user_ip_list[i].Length - 20);
                    user_online[i] = user_online_list[i].Substring(9, user_online_list[i].Length - 9);
                    user_description[i] = user_description_list[i].Substring(15, user_description_list[i].Length - 16);
                    user_authorized[i] = user_authorized_list[i].Substring(13, user_authorized_list[i].Length - 13);
                    user_ot[i] = user_ot_list[i].Substring(13, user_ot_list[i].Length - 13);
                    answer[i] = new ZTUser(user_id[i], user_name[i], user_description[i], user_ip[i], user_online[i], user_authorized[i], user_ot[i]);
                }
                
                for (int i = 0; i < _UserCount; i++)
                {
                    if (!answer[i].IsOnline)
                    {
                        for (int j = i; j < _UserCount; j++)
                        {
                            if (answer[j].IsOnline)
                            {
                                ZTUser temp = answer[i];
                                answer[i] = answer[j];
                                answer[j] = temp;
                            }
                        }
                    }
                }

                _IsOK = true;
                return answer;
            }
        }
        
        private static ZTUser[] _users;
        public static ZTUser[] users
        {
            set { _users = value; UpdateUsers(); } // После каждого обновления списка пользователей на форме обновляются данные
            get { return _users; }
        }
        private static Custom.User[] UIusers;
        #endregion

        #region Colors
        private readonly BrushConverter bc = new BrushConverter();
        private static Brush brush_offline; // Цвет круга статуса возле аватарки - Не в сети
        private static Brush brush_online;  // Цвет круга статуса возле аватарки - В сети
        private static Brush brush_even;    // Цвет заднего плана блока Custom.User
        private static Brush brush_odd;     // Цвет заднего плана блока Custom.User 
        private static Brush brush_back;    // Цвет заднего плана самой формы
        public static Brush brush_text;    // Цвет текста
        public static Brush brush_copy;     // Цвет текста при копировании
        private string arrows_path;         // Путь до иконки со стрелочками
        private string moon_path;         // Путь до иконки с луной
        private string cross_path;         // Путь до иконки с крестиком
        private string minus_path;         // Путь до иконки с минусом
        private string copy_path;         // Путь до иконки с копированием

        /// <summary>
        /// Переключает темную/светлую тему
        /// </summary>
        private void SetupColors()
        {
            brush_offline = (Brush)bc.ConvertFrom("Gray");
            brush_online = (Brush)bc.ConvertFrom("#008000");

            if (darkmode)
            {
                brush_even = (Brush)bc.ConvertFrom("#4f4f4f");
                brush_odd = (Brush)bc.ConvertFrom("#3f3f3f");
                brush_back = (Brush)bc.ConvertFrom("#2f2f2f");
                brush_text = (Brush)bc.ConvertFrom("#dddddd");
                brush_copy = (Brush)bc.ConvertFrom("#30a030");

                arrows_path = "pack://application:,,,/ZTM;component/Pics/rotating_arrows_white.png";
                moon_path = "pack://application:,,,/ZTM;component/Pics/moon_white.png";
                cross_path = "pack://application:,,,/ZTM;component/Pics/cross_white.png";
                minus_path = "pack://application:,,,/ZTM;component/Pics/minus_white.png";
                copy_path = "pack://application:,,,/ZTM;component/Pics/copy_white.png";
            }
            else
            {
                /*
                brush_even = (Brush)bc.ConvertFrom("#e6d2b5");
                brush_odd = (Brush)bc.ConvertFrom("#ffebcd");
                brush_back = (Brush)bc.ConvertFrom("#ffebcd");
                brush_text = (Brush)bc.ConvertFrom("#000000");
                brush_copy = (Brush)bc.ConvertFrom("#008000");
                */
                brush_even = (Brush)bc.ConvertFrom("#dddddd");
                brush_odd = (Brush)bc.ConvertFrom("#eeeeee");
                brush_back = (Brush)bc.ConvertFrom("#ffffff");
                brush_text = (Brush)bc.ConvertFrom("#121212");
                brush_copy = (Brush)bc.ConvertFrom("#008000");

                arrows_path = "pack://application:,,,/ZTM;component/Pics/rotating_arrows.png";
                moon_path = "pack://application:,,,/ZTM;component/Pics/moon.png";
                cross_path = "pack://application:,,,/ZTM;component/Pics/cross.png";
                minus_path = "pack://application:,,,/ZTM;component/Pics/minus.png";
                copy_path = "pack://application:,,,/ZTM;component/Pics/copy.png";
            }

            Recolor();
        }

        private void Recolor()
        {
            if (!first_UpdateUsers)
            {
                for (int i = 0; i < UIusers.Length; i++)
                {
                    UIusers[i].Backg.Background = i % 2 == 0 ? brush_even : brush_odd;
                    UIusers[i].OnlineStatus.Stroke = i % 2 == 0 ? brush_even : brush_odd;

                    UIusers[i].NameUser.Foreground = brush_text;
                    UIusers[i].IP.Foreground = brush_text;
                    UIusers[i].Index.Foreground = brush_text;
                    UIusers[i].CopyB.Tag = copy_path;
                }
            }
            Backg.Background = brush_back;
            NetworkID.Foreground = brush_text;

            RButton.Tag = arrows_path;
            RecolorButton.Tag = moon_path;
            CloseButton.Tag = cross_path;
            MinimizeButton.Tag = minus_path;
        }

        private bool _darkmode = false;
        public bool darkmode
        {
            set { _darkmode = value; SetupColors(); }
            get { return _darkmode; }
        }

        private void RecolorButton_Click(object sender, RoutedEventArgs e)
        {
            darkmode = !darkmode;
        }
        #endregion

        #region Update
        private static bool first_UpdateUsers = true;

        /// <summary>
        /// Обновляет форму. Добавляет на форму пользователей, если их еще нет. Далее обновляет информацию о пользователях и их иконки.
        /// </summary>
        private static void UpdateUsers()
        {
            if (!first_UpdateUsers)
            {
                if (users.Length != UIusers.Length)
                {
                    int temp_count = UIusers.Length;
                    Array.Resize<Custom.User>(ref UIusers, users.Length);
                    if (temp_count > UIusers.Length) // Если людей стало меньше - скрывает неиспользуемые позиции
                    {
                        for (int i = UIusers.Length; i < temp_count ; i++)
                        {
                            UIusers[i].Visibility = Visibility.Collapsed;
                        }
                    }
                    if (temp_count < UIusers.Length) // Если людей стало больше - создает новые позиции
                    {
                        for (int i = temp_count; i < UIusers.Length ; i++)
                        {
                            UIusers[i] = new Custom.User();
                            ((MainWindow)Application.Current.MainWindow).UsersStackPanel.Children.Add(UIusers[i]);
                        }
                    }
                }
            }
            else
            {
                UIusers = new Custom.User[users.Length];

                ((MainWindow)Application.Current.MainWindow).RButton.Visibility = Visibility.Visible;
                 SetTimer();

                for (int i = 0; i < users.Length ; i++)
                {
                    UIusers[i] = new Custom.User();
                    ((MainWindow)Application.Current.MainWindow).UsersStackPanel.Children.Add(UIusers[i]);
                }
            }


            UpdateInfo();
            UpdateIcons();
            ((MainWindow)Application.Current.MainWindow).RotateArrows();

            ((MainWindow)Application.Current.MainWindow).NetworkID.Text = $"Network: {_NetID}";

            first_UpdateUsers = false;
            ((MainWindow)Application.Current.MainWindow).Recolor();
        }

        /// <summary>
        /// Обновляет информацию о каждом пользователе.
        /// </summary>
        private static void UpdateInfo()
        {
            if (UIusers is null) return;
            for (int i = 0; i < users.Length; i++)
            {
                UIusers[i].Visibility = Visibility.Visible;
                UIusers[i].NameUser.Text = users[i].Name;
                UIusers[i].IP.Text = users[i].IpAddress;
                UIusers[i].OnlineStatus.Fill = users[i].IsOnline ? brush_online : brush_offline;
                UIusers[i].Index.Text = i + "";
            }
        }
        
         /// <summary>
         /// Обновляет все иконки пользователей на форме.
         /// Путь до иконок хранится в desription пользователя.
         /// </summary>
        private static void UpdateIcons()
        {
            if (UIusers is null) return;
            for (int i = 0; i < users.Length; i++)
            {
                BitmapImage pic = new BitmapImage();
                try
                {
                    pic.BeginInit();
                    pic.UriSource = new Uri(users[i].Description);
                    pic.EndInit();
                }
                catch
                {
                    pic = new BitmapImage(new Uri("pack://application:,,,/ZTM;component/Pics/default.png"));
                }

                UIusers[i].Avatar.Fill = new ImageBrush(pic);
            }

            ((MainWindow)Application.Current.MainWindow).NetworkID.Text = $"Network: {_NetID}";
        }

        /// <summary>
        /// Настройка таймера. Таймер раз в 15 секунд обновляет пользователей. 
        /// </summary>
        private static void SetTimer()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 15);
            dispatcherTimer.Start();
        }
        private static System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private static async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            users = ZTUser.fetch(_APIKey, _NetID);
            ((MainWindow)Application.Current.MainWindow).RButton.IsEnabled = false;
            await Task.Delay(1000);
            ((MainWindow)Application.Current.MainWindow).RButton.IsEnabled = true;
        }

        private static string _APIKey;
        private static string _NetID;

        public static string APIKey
        {
            set { _APIKey = value; }
        }
        public static string NetID
        {
            set
            {
                _NetID = value;
            }
        }

        private void RotateArrows()
        {
            DoubleAnimation DA = new DoubleAnimation(360, new Duration(TimeSpan.FromSeconds(1)));
            RotateTransform RT = new RotateTransform();

            RButton.RenderTransform = RT;
            RButton.RenderTransformOrigin = new Point(0.5, 0.5);

            RT.BeginAnimation(RotateTransform.AngleProperty, DA);
        }

        private async void RButton_Click(object sender, RoutedEventArgs e)
        {
            users = ZTUser.fetch(_APIKey, _NetID);
            RButton.IsEnabled = false;
            await Task.Delay(1000);
            RButton.IsEnabled = true;
        }
        #endregion

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
