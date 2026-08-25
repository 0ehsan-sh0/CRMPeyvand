using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class MessagePanel
    {
        public MessagePanel()
        {
            RegDate = DateTime.Now;
            EditDate = DateTime.Now;
        }
        public int id { get; set; }
        public string APIToken { get; set; }
        public string LineNumber { get; set; }
        public DateTime RegDate { get; set; }
        public DateTime EditDate { get; set; }
    }
}
