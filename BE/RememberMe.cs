using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class RememberMe
    {
        public RememberMe()
        {
            IsRemembered = true;
            LastLoginTime = DateTime.Now;
        }
        public int id { get; set; }
        public string UserName { get; set; }
        public bool IsRemembered { get; set; }
        public DateTime LastLoginTime { get; set; }
    }
}
