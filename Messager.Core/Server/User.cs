using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager.Core.Server
{
    public class User
    {
        public string UserName { get; }
        public string Password { get; }
        public bool CanSend { get; set; }
        public bool CanCode { get; set; }
        
        public User(string username, string password = "1234", bool canSend = true, bool canCode = false)
        {
            UserName = username; Password = password; CanSend = canSend; CanCode = canCode;
        }
    }
}
