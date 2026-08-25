using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using BE;

namespace DAL
{
    public class DB :DbContext
    {
        public DB() : base("conStr")
        {

        }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserAccessRole> UserAccessRoles { get; set; }
        public DbSet<OffCode> OffCodes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessagePanel> MessagePanels { get; set; }
        public DbSet<RememberMe> RememberMe { get; set; }
    }
}
