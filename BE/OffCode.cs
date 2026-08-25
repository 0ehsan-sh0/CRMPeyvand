using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class OffCode
    {
        public OffCode()
        {
            DeleteStatus = false;
        }
        public int id { get; set; }
        public string Code { get; set; }
        public bool IsPrice { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> Percent { get; set; }
        public DateTime RegDate { get; set; }
        public Nullable<DateTime> ExpireDate { get; set; }
        public Nullable<int> LimitCount { get; set; }
        public bool DeleteStatus { get; set; }
    }
}
