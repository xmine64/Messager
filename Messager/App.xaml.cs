using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Messager.Core;
using Messager.Core.Server;

namespace Messager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Exit += (o, e) =>
            {
                if (Client != null)
                {
                    Client.Disconnect();
                    Client = null;
                }
                if (Server != null)
                {
                    Server.Shutdown();
                    Server = null;
                }
            };
        }

        public static Server Server;
        public static Core.Client Client;
    }
}
