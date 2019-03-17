using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Messager.Core.Server;

namespace Messager.Android
{
    [Activity(Label = "Messager.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
        }

        protected override void OnDestroy()
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

            base.OnDestroy();
        }

        public static Server Server;
        public static Core.Client Client;
    }
}

