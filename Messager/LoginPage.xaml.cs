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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Client = new Core.Client(txtIP.Text, Convert.ToInt32(txtPort.Text));
                App.Client.Send(new Core.Data(Core.Command.Login, txtUname.Text + ":" + txtPass.Text));
                ((MainWindow)Application.Current.MainWindow).messagerTab.frame.Content = new MessagePage();
            }
            catch
            {
                MessageBox.Show("Error while connection");
            }
        }
    }
}
