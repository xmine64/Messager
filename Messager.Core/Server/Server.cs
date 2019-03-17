using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Messager.Core.Server
{
    public class Server
    {
        public Server(int port)
        {
            Clients = new List<Client>();
            Users = new List<User> { new User("anonymous", "", false, false) };
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _response = new Thread(() => {
                while (Thread.CurrentThread.IsAlive)
                {
                    var c = new Client(_listener.AcceptTcpClient(), Users[0]);
                    Clients.Add(c);
                    NewClientConnected?.Invoke(this, c);
                }
            });
            _response.Start();
            NewClientConnected += (s, c) =>
            {
                c.DataRecevied += OnClientDataRecevied;
                c.Disconnected += (o, ca) =>
                {
                    try
                    { Clients.RemoveAt(Clients.FindIndex(a => a.Id == c.Id)); }
                    catch { }
                    SendToAllAsRoot("#" + c.Id + " disconnected.");
                };
                SendToAllAsRoot("#" + c.Id + " joined the chat.");
            };
        }

        private void OnClientDataRecevied(object sender, Data e)
        {
            var c = (Client)sender;
            Console.WriteLine(c.User.UserName + "@" + c.Id + ": " + e.Command.ToString() + ": " + e.Content);
            switch (e.Command)
            {
                case Command.Status:
                    c.Send(new Data(Command.Message, "Client #" + c.Id + ", Logged as" + c.User.UserName + ", Can Send: " + c.User.CanSend + ", Can Code: " + c.User.CanCode, "root"));
                    return;
                case Command.SendMsg:
                    if (c.User.CanSend)
                    {
                        if (c.User.CanCode && e.Content.StartsWith("sh: "))
                        {
                            RunCode(e.Content.Substring(4), c);
                        }
                        else
                        {
                            foreach (var client in Clients)
                                client.Send(new Data(Command.Message, e.Content, c.User.UserName));
                        }
                    }
                    else
                    {
                        c.Send(new Data(Command.Message, "Dear " + c.User.UserName + ", You can't send any message because you are limited.", "root"));
                        foreach (var client in Clients.Where(a => a.User.CanCode))
                            client.Send(new Data(Command.Message, c.User.UserName + " says:\r\n\t" + e.Content, "Limited users"));
                    }
                    return;
                case Command.Disconnect:
                    c.Disconnect();
                    return;
                case Command.Login:
                    if (c.User.UserName == "anonymous")
                    {
                        foreach (var u in Users)
                        {
                            if (e.Content == u.UserName + ":" + u.Password)
                            {
                                c.User = u;
                                SendToAllAsRoot("#" + c.Id + " logged in as " + u.UserName);
                                return;
                            }
                        }
                        c.Send(new Data(Command.Message, "Bad User Name/Password", "root"));
                        SendToAllAsRoot("An attempt to login from #" + c.Id);
                    }
                    else
                    {
                        c.Send(new Data(Command.Message, "You've logged in already.", "root"));
                    }
                    return;
                case Command.Message:
                    foreach (var cl in Clients.Where(a => a.User.UserName == Users[1].UserName))
                        cl.Send(new Data(Command.Message, "Message: " + e.Content, "root - Msg from Client #" + c.Id));
                    break;
                default:
                    InvalidData?.Invoke(c, e);
                    return;
            }
        }

        private void RunCode(string cd, Client cl)
        {
            switch (cd.Substring(0, 3))
            {
                case "hlp":
                    cl.Send(new Data(Command.Message, "Command Line Help: \r\n\tnur username:password -> Create new user\r\n\tdur username -> Delete an existing user\r\n\tblk username -> Limit a user\r\n\tulk username -> Unlimit a user\r\n\tshd -> Shutdown Server\r\n\tdsc ## -> Force Disconnect client number ##\r\n\tmsg ##:message -> Send message to client number ##\r\n\tcls -> Get list of connects clients\r\n\tuss -> Get list of users", "shell"));
                    break;
                case "blk":
                    {
                        var idx = Users.FindIndex(a => a.UserName == cd.Substring(4));
                        var u = Users[idx];
                        u.CanSend = false;
                        Users[idx] = u;
                        foreach (var cla in Clients.Where(a => a.User.UserName == u.UserName))
                        {
                            cla.Send(new Data(Command.Message, "Dear " + u.UserName + ", You've blocked.", "root"));
                        }
                        cl.Send(new Data(Command.Message, u.UserName + " blocked.", "shell"));
                    }
                    break;
                case "ulk":
                    {
                        var idx = Users.FindIndex(a => a.UserName == cd.Substring(4));
                        var u = Users[idx];
                        u.CanSend = true;
                        Users[idx] = u;
                        foreach (var cla in Clients.Where(a => a.User.UserName == u.UserName))
                        {
                            cla.Send(new Data(Command.Message, "Dear " + u.UserName + ", You've unblocked.", "root"));
                        }
                        cl.Send(new Data(Command.Message, u.UserName + " unblocked.", "shell"));
                    }
                    break;
                case "dsc":
                    {
                        var cid = Convert.ToInt32(cd.Substring(4,2));
                        var cla = Clients.First(a => a.Id == cid);
                        cla.Send(new Data(Command.Message, $"Dear {cla.User.UserName}, You've removed from the chat by {cl.User.UserName}. Bye, Hope to see you again. ;-)", "root"));
                        cla.Disconnect();
                        cl.Send(new Data(Command.Message, "#" + cid + " removed from chat.", "shell"));
                    }
                    break;
                case "shd":
                    SendToAllAsRoot("Server is Shutting Down.");
                    Shutdown();
                    break;
                case "msg":
                    {
                        var cid = Convert.ToInt32(cd.Substring(4, 2));
                        Clients.First(a => a.Id == cid).Send(new Data(Command.Message, cd.Substring(7), "Private Message from Client #" + cl.Id));
                    }
                    break;
                case "nur":
                    {
                        var cert = cd.Substring(4).Split(':');
                        Users.Add(new User(cert[0], cert[1], true, false));
                    }
                    break;
                case "dur":
                    {
                        var idx = Users.FindIndex(a => a.UserName == cd.Substring(4));
                        foreach (var cli in Clients.Where(a => a.User.UserName == Users[idx].UserName))
                        {
                            cli.Send(new Data(Command.Message, "Dear User, You've logged into 'anonymous' forcely for security causes. Please connect again. Thanks.", "root"));
                            cli.User = Users[0];
                        }
                        Users.RemoveAt(idx);
                    }
                    break;
                case "cls":
                    {
                        var str = "Clients Report:";
                        foreach(var c in Clients)
                        {
                            str += $"\r\n\t#{c.Id} logged as {c.User.UserName} with password {c.User.Password}, and permission of send={c.User.CanSend};code={c.User.CanCode}";
                        }
                        cl.Send(new Data(Command.Message, str, "shell"));
                    }
                    break;
                case "uss":
                    {
                        var str = "Users Report:";
                        foreach (var u in Users)
                        {
                            str += $"\r\n\t{u.UserName}:{u.Password}, send={u.CanSend};code={u.CanCode}";
                        }
                        cl.Send(new Data(Command.Message, str, "shell"));
                    }
                    break;
                default:
                    cl.Send(new Data(Command.Message, "Unknown Command, Use 'hlp' to get help.", "shell"));
                    break;
            }
        }

        public void SendToAllAsRoot(string msg)
        {
            foreach (var client in Clients)
                client.Send(new Data(Command.Message, msg, "root"));
        }

        public void Shutdown()
        {
            try
            {
                _listener.Stop();
                _response.Abort();
                _response = null;
                _listener = null;
                foreach (var client in Clients)
                    client.Disconnect();
            }
            catch { }
            Shutteddown?.Invoke(this, null);
        }

        public List<Client> Clients { get; }
        public List<User> Users { get; }

        public event EventHandler<Client> NewClientConnected;
        public event EventHandler<Data> InvalidData;
        public event EventHandler Shutteddown;

        private TcpListener _listener;
        private Thread _response;
    }
}
