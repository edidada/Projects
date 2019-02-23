using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Excute(Action<int> action)
        {
            for (int i = 0; i < 1; i++)
            {
                action(i);
            }
        }

        private void Button_Click_default(object sender, RoutedEventArgs e)
        {
            Excute((i) => {
                Console.WriteLine($"default-->{i.ToString()}");
            });
        }

        private void Button_Click_error(object sender, RoutedEventArgs e)
        {
            Excute((i) => {
                ConsoleLogHelper.WriteLineError($"error-->{i.ToString()}");
            });
        }

        private void Button_Click_exception(object sender, RoutedEventArgs e)
        {
            try
            {
                string a = null;
                a.ToString();
            }
            catch (Exception ex)
            {
                Excute((i) => {
                    ConsoleLogHelper.WriteLineError(ex);
                });
            }
        }

        private void Button_Click_info(object sender, RoutedEventArgs e)
        {
            Excute((i) => {
                ConsoleLogHelper.WriteLineInfo($"info-->{i.ToString()}");
            });
            Excute((i) => {
                ConsoleLogHelper.WriteLineInfo($"info-->{GetAddressIP()}");
            });
        }

        private String GetAddressIP()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            ArrayList txtLocalIP = new ArrayList();
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    txtLocalIP.Add(AddressIP);
                }
            }
            StringBuilder str = new StringBuilder();
            foreach (string s in txtLocalIP)
            {
                
                str.Append(s) ;
                str.Append("/r/n");
            }
            return str.ToString();
        }
    }
}
