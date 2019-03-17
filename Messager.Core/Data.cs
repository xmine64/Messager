using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;

namespace Messager.Core
{
    public class Data
    {
        public Command Command { get; set; }
        public string Content { get; set; }
        public string Sender { get; set; }

        public Data() : this(Command.None)
        {

        }

        public Data(Command cmd, string ctx = "", string sender = "")
        {
            Command = cmd; Content = ctx; Sender = sender;
        }

        public static Data Deserialize(string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                    throw new ArgumentNullException(nameof(str));
                return new JavaScriptSerializer().Deserialize<Data>(str);
            }
            catch
            {
                return new Data(Command.Message, str, "unknown_client");
            }
        }

        public string Serialize()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }

    public class ParseDataException : Exception
    {
        public ParseDataException() : base("Error in parsing data")
        {

        }
    }

    public enum Command
    {
        None,
        SendMsg,
        Message,
        Status,
        Login,
        Disconnect,
    }
}
