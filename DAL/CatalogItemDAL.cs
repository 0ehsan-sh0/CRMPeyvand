using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CatalogItemDAL
    {
        DB db;

        public CatalogItemDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public CatalogItemDAL(DB db)
        {
            this.db = db;
        }

        public string Create(CatalogItem p)
        {
            try
            {
                db.CatalogItems.Add(p);
                db.SaveChanges();
                return "ثبت اطلاعات کالا با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;

            }
        }


        public List<CatalogItem> ReadAll()
        {
            return db.CatalogItems.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Id).ToList();
        }

        // Copied verbatim from the SQL this replaces. The third column is a
        // computed column, not a field, which is why the grid shows a word
        // rather than a number.
        private static readonly string[] ReadColumns =
            { "نام", "قیمت", "نوع", "موجودی" };

        /// <summary>
        /// The CASE expression the old SQL used, kept branch for branch: it had
        /// no ELSE, so a Kind outside the enum came back as NULL and the grid
        /// rendered an empty cell. A two-armed ternary would report it as
        /// "خدمات" instead, which is a different - and wrong - thing to show.
        /// </summary>
        private static string KindLabel(ItemKind kind)
        {
            if (kind == ItemKind.Good) return "محصول";
            if (kind == ItemKind.Service) return "خدمات";
            return null;
        }

        /// <summary>
        /// The rows behind the grid, newest first. Materialised before the
        /// projection because EF6 cannot translate a projection into
        /// object[] into SQL.
        /// </summary>
        private List<object[]> CatalogRows()
        {
            return db.CatalogItems
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.Id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[]
                {
                    i.Name,
                    i.SalePrice,
                    KindLabel(i.Kind),
                    i.Stock,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, CatalogRows());
        }

        /// <summary>
        /// The same four columns as <see cref="Read"/>, filtered on the Kind
        /// label. The old SQL compared the CASE expression to the parameter,
        /// so this matches the label and not the name.
        /// </summary>
        public DataTable Read(string type)
        {
            var rows = CatalogRows()
                .Where(r => GridTable.Matches(type, (string)r[2]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }

        public List<string> ReadNames()
        {
            return db.CatalogItems.Where(i => i.DeleteStatus == false).Select(i => i.Name).ToList();
        }

        public bool Exist(CatalogItem p)
        {
            return db.CatalogItems.Any(i => i.Name == p.Name);
        }


        public CatalogItem ReadById(int id)
        {
            return db.CatalogItems.Find(id);
        }


        public string Update(CatalogItem p, int id)
        {
            try
            {
                CatalogItem product = ReadById(id);
                if (!product.DeleteStatus)
                {
                    product.Name = p.Name;
                    product.SalePrice = p.SalePrice;
                    product.Kind = p.Kind;
                    product.Stock = p.Stock;
                    db.SaveChanges();
                    return "ویرایش با موفیقت انجام شد";
                }
                else return "کالا یافت نشد";
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
                var q = db.CatalogItems.Where(i => i.DeleteStatus == false && i.Id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "کالا مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }

        }
        public DataTable Search(string Filter)
        {
            // The old LIKE clause covered Name only, but the grid also shows the
            // Kind label, and searching for "محصول" is the obvious thing to
            // type. Both are offered here.
            var rows = CatalogRows()
                .Where(r => GridTable.Matches(Filter, (string)r[0], (string)r[2]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }


        public CatalogItem ReadByName(string product)
        {
            return db.CatalogItems.Where(i => i.DeleteStatus == false && i.Name == product).SingleOrDefault();
        }


        public string ProductsCount()
        {
            return db.CatalogItems.Where(i => i.DeleteStatus == false).Count().ToString();
        }

    }
}
