using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Message
    {
        public Message()
        {
            DeleteStatus = false;
            RegDate = DateTime.Now;
        }
        public int id { get; set; }
        public string Content { get; set; }
        public bool DeleteStatus { get; set; }
        public DateTime RegDate { get; set; }
    }
}
