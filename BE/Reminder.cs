using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Reminder
    {
        public Reminder()
        {
            DeleteStatus = false;
            IsReminded = false;
        }
        public int id { get; set; }
        public string Title { get; set; }
        public string Info { get; set; }
        public DateTime RegDate { get; set; }
        public DateTime RemindDate { get; set; }
        public bool DeleteStatus { get; set; }
        public bool IsReminded { get; set; }
        public User User { get; set; }

    }
}
