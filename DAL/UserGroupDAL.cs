using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class UserGroupDAL
    {
        DB db;

        public UserGroupDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public UserGroupDAL(DB db)
        {
            this.db = db;
        }

        public string Create(UserGroup userGroup)
        {
            try
            {
                var grants = userGroup.AccessGrants ?? new List<AccessGrant>();
                userGroup.AccessGrants = new List<AccessGrant>();
                db.UserGroups.Add(userGroup);
                foreach (var grant in grants)
                {
                    grant.UserGroup = userGroup;
                    userGroup.AccessGrants.Add(grant);
                }
                db.SaveChanges();
                return "ثبت گروه کاربری با موفقیت انجام شد";
            }
            catch (Exception e)
            {

                return "در ثبت گروه کاربری مشکلی به وجود آمد" + e.Message;
            }
        }
        // Copied verbatim from the SQL this replaces. The built-in
        // administrator group is not a manageable group and was excluded there,
        // so it stays excluded.
        private static readonly string[] ReadColumns = { "نام گروه کاربری" };

        /// <summary>The rows behind the grid, newest first.</summary>
        private List<object[]> UserGroupRows()
        {
            return db.UserGroups
                .Where(i => i.IsBuiltIn == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[] { i.Title })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, UserGroupRows());
        }

        public void Update(int groupId, string title, List<AccessGrant> grants)
        {
            using (var db = new DB())
            {
                var group = db.UserGroups.Include("AccessGrants")
                                         .SingleOrDefault(g => g.id == groupId);
                if (group == null || group.IsBuiltIn)
                    return;

                group.Title = title;
                db.AccessGrants.RemoveRange(group.AccessGrants.ToList());
                foreach (var grant in grants)
                    db.AccessGrants.Add(new AccessGrant
                    {
                        Section = grant.Section,
                        Operation = grant.Operation,
                        UserGroup = group,
                    });
                db.SaveChanges();
            }
        }

        public bool Exist(string Title)
        {
            return db.UserGroups.Any(x => x.Title == Title);
        }
        public List<string> ReadTitles()
        {
            return db.UserGroups.Where(i => !i.IsBuiltIn).Select(x => x.Title).ToList();
        }
        public bool ReadByTitle(string Title)
        {
            return !db.UserGroups.Any(x => x.Title == Title);
        }

        public UserGroup ReadBySingelTitle(string title)
        {
            return db.UserGroups.Include("AccessGrants").Where(x => x.Title == title).SingleOrDefault();
        }
        public static UserGroup AdminUserGroup()
        {
            using (var db = new DB())
                return db.UserGroups.SingleOrDefault(g => g.IsBuiltIn);
        }
    }
}
