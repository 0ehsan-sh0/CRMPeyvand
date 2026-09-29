using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class MessageDAL
    {
        DB db;

        public MessageDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public MessageDAL(DB db)
        {
            this.db = db;
        }

        public string Create(Message m)
        {
            try
            {
                db.Messages.Add(m);
                db.SaveChanges();
                return "ثبت اطلاعات با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }

        }
        public bool Exist(string content)
        {
            return db.Messages.Any(i => i.Content == content);
        }
        // Copied verbatim from the SQL this replaces: the one column is the
        // message text and nothing else.
        private static readonly string[] ReadColumns = { "متن پیام" };

        /// <summary>
        /// The rows behind the grid, newest first. Materialised before the
        /// projection because EF6 cannot translate a projection into
        /// object[] into SQL.
        /// </summary>
        private List<object[]> MessageRows()
        {
            return db.Messages
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[] { i.Content })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, MessageRows());
        }

        public DataTable Search(string Filter)
        {
            var rows = MessageRows()
                .Where(r => GridTable.Matches(Filter, (string)r[0]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
        public string Count()
        {
            return db.Messages.Count().ToString();
        }
        public List<string> First10()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(10).ToList();
        }
        public List<string> First100()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(100).ToList();
        }
        public List<string> First1000()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(1000).ToList();
        }
        public List<string> First10000()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(10000).ToList();
        }
        public List<string> FirstBuy10()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(10).ToList();
        }
        public List<string> FirstBuy100()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(100).ToList();
        }

        public List<string> FirstBuy1000()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(1000).ToList();
        }
        public List<string> FirstBuy10000()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(10000).ToList();
        }
        public List<string> FirstBuy()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false && i.Invoices.Count == 1).Select(i => i.Phone).ToList();
        }
        public List<string> NoBuy()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false && i.Invoices.Count == 0).Select(i => i.Phone).ToList();
        }
        public List<string> IsNotCheckedOut()
        {
            List<string> list = new List<string>();
            foreach (var item in db.Customers.Include("Invoices"))
            {
                bool IsNotCheckedOut = false;
                foreach (var Iitem in item.Invoices)
                {
                    if (Iitem.IsCheckedout == false)
                    {
                        IsNotCheckedOut = true;
                    }
                }
                if (IsNotCheckedOut)
                {
                    list.Add(item.Phone);
                }
            }
            return list;
        }
    }
}
