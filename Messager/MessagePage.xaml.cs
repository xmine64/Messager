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
    /// Interaction logic for MessagePage.xaml
    /// </summary>
    public partial class MessagePage : Page
    {
        public MessagePage()
        {
            InitializeComponent();
            App.Client.DataRecevied += (s, d) =>
            {
                try
                {
                    if (d.Command == Core.Command.Message)
                    {
                        messages.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                messages.Items.Add(d);
                                scroll.ScrollToBottom();
                            }
                            catch
                            {

                            }
                        });
                    }
                }
                catch { }
            };
            App.Client.Disconnected += (s, d) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("You've removed from chat.");
                    ((MainWindow)Application.Current.MainWindow).messagerTab.frame.Content = new LoginPage();
                });
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Client.Send(new Core.Data(Core.Command.SendMsg, txtMsg.Text));
            txtMsg.Text = "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                ((MainWindow)Application.Current.MainWindow).messagerTab.frame.Content = new LoginPage();
                App.Client.Send(new Core.Data(Core.Command.Disconnect));
                App.Client = null;
            }
            catch
            {

            }
        }
    }
}
