using AppRTC.RctPoint;
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

namespace SocketServer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        RtcServer _SocketServer;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServerStarup_Click(object sender, RoutedEventArgs e)
        {
            if (_SocketServer != null) return;
            try
            {
                _SocketServer = new RtcServer(ServerLocation.Text);
            }
            catch (Exception ex)
            { }
        }
    }
}
