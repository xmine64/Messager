using Messager.Core.Server;
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

namespace Messager
{
    /// <summary>
    /// Interaction logic for CtlServer.xaml
    /// </summary>
    public partial class CtlServer : UserControl
    {
        public CtlServer()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var port = Convert.ToInt32(txtPort.Text);
            var u = new User(txtRoot.Text, txtPass.Text, true, true);
            App.Server = new Server(port);
            App.Server.Users.Add(u);
            App.Client = new Core.Client("localhost", port);
            App.Client.Send(new Core.Data(Core.Command.Login, u.UserName + ":" + u.Password));
            ((MainWindow)Application.Current.MainWindow).messagerTab.frame.Content = new MessagePage();
            ((MainWindow)Application.Current.MainWindow).tabControl.SelectedIndex = 1;
            btnStart.IsEnabled = false;
            btnStart.Content = "Listening on port 260";
        }
    }
}
