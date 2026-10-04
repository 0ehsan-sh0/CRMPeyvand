using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CustomerDAL
    {
        DB db;

        public CustomerDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public CustomerDAL(DB db)
        {
            this.db = db;
        }

        public bool Exist(Customer c)
        {
            return db.Customers.Any(i => i.Phone == c.Phone);
        }
        public Customer GetCustomer(string phone)
        {
            var q = db.Customers.Where(i => i.Phone == phone).FirstOrDefault();
            return q;
        }
        public string CustomerCount()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).Count().ToString();
        }
        public string Create(Customer c)
        {
            try
            {
                db.Customers.Add(c);
                db.SaveChanges();
                return "ثبت اطلاعات مشتری با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        private static readonly string[] ReadColumns =
            { "نام", "شماره تماس", "تاریخ ثبت", "مانده حساب" };

        /// <summary>
        /// The customers with what they owe, keyed by customer id.
        ///
        /// Loaded separately from the row projection because the balance is a sum
        /// over the customer's invoices, their lines and their payments, and
        /// SQLite cannot be asked for all three in one query. See BalanceQuery.
        /// </summary>
        private Dictionary<int, CustomerBalance> LoadBalances()
        {
            return BalanceQuery.Load(db).ToDictionary(b => b.Customer.id);
        }

        public DataTable Read()
        {
            var balances = LoadBalances();
            var rows = db.Customers
                .AsNoTracking()
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[]
                {
                    i.Name,
                    i.Phone,
                    i.RegDate,
                    balances.TryGetValue(i.id, out var balance) ? balance.Balance : 0m,
                })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
        public Customer ReadById(int id)
        {
            return db.Customers.Find(id);
        }


        public string Update(Customer c, int id)
        {
            try
            {
                Customer customer = ReadById(id);
                customer.Name = c.Name;
                customer.Phone = c.Phone;
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string Delete(int id)
        {
            try
            {
                var q = db.Customers.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "مشتری مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public List<string> ReadPhoneNumbers()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).Select(i => i.Phone).ToList();
        }
        public DataTable Search(string Filter)
        {
            var balances = LoadBalances();
            var rows = db.Customers
                .AsNoTracking()
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .AsEnumerable()
                .Where(i => GridTable.Matches(Filter, i.Name, i.Phone))
                .Select(i => new object[]
                {
                    i.Name,
                    i.Phone,
                    i.RegDate,
                    balances.TryGetValue(i.id, out var balance) ? balance.Balance : 0m,
                })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
        public List<Customer> ReadWithDateTime()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).ToList();
        }
    }
}
