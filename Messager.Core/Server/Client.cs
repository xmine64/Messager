using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Messager.Core.Server
{
    public class Client
    {
        private static int _instanceId = 0;

        public Client(TcpClient tc, User user)
        {
            _client = tc;
            Id = _instanceId++;
            User = user;
            _reader = new StreamReader(tc.GetStream());
            _writer = new StreamWriter(tc.GetStream());
            _waiter = new Thread(() =>
             {
                 while (Thread.CurrentThread.IsAlive)
                 {
                     if (_client.Connected)
                     {
                         try
                         {
                             var d = Data.Deserialize(_reader.ReadLine());
                             DataRecevied?.Invoke(this, d);

                         }
                         catch
                         {
                             Disconnect();
                             break;
                         }
                     }
                     else
                     {
                         Disconnected?.Invoke(this, _client);
                         break;
                     }
                 }
             });
            _waiter.Start();
        }

        public void Disconnect()
        {
            try
            {
                Send(new Data(Command.Disconnect));
                _client.Close();
                _waiter.Abort();                
            }
            catch { }
            Disconnected?.Invoke(this, _client);
        }

        public void Send(Data data)
        {
            lock (_sendlock)
            {
                _writer.WriteLine(data.Serialize());
                _writer.Flush();
            }
        }

        public event EventHandler<Data> DataRecevied;
        public event EventHandler<TcpClient> Disconnected;

        public int Id { get; }
        public User User { get; internal set; }

        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Thread _waiter;
        private object _sendlock = new object();
    }
}
