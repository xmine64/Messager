using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messager.Core
{
    public class Client
    {
        public Client(string ip, int port)
        {
            _client = new TcpClient();
            _client.Connect(ip, port);
            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream());
            _waiter = new Thread(() =>
            {
                while (Thread.CurrentThread.IsAlive)
                {
                    if (_client.Connected)
                    {
                        try
                        {
                            var d = Data.Deserialize(_reader.ReadLine());
                            if (d.Command == Command.Disconnect)
                                Disconnect();
                            else
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
                        break;
                    }
                }
                Disconnected?.Invoke(this, null);
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
        public event EventHandler Disconnected;

        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Thread _waiter;
        private object _sendlock = new object();
    }
}
