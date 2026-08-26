namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccessGrants",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Section = c.Int(nullable: false),
                        Operation = c.Int(nullable: false),
                        UserGroup_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.UserGroups", t => t.UserGroup_id)
                .Index(t => t.UserGroup_id);
            
            CreateTable(
                "dbo.UserGroups",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        IsBuiltIn = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        UserName = c.String(),
                        Password = c.String(),
                        Picture = c.String(),
                        RegDate = c.DateTime(nullable: false),
                        DeleteStatus = c.Boolean(nullable: false),
                        UserGroup_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.UserGroups", t => t.UserGroup_id)
                .Index(t => t.UserGroup_id);
            
            CreateTable(
                "dbo.Activities",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Info = c.String(),
                        RegDate = c.DateTime(nullable: false),
                        DeleteStatus = c.Boolean(nullable: false),
                        ActivityCategory_id = c.Int(),
                        Customer_id = c.Int(),
                        User_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.ActivityCategories", t => t.ActivityCategory_id)
                .ForeignKey("dbo.Customers", t => t.Customer_id)
                .ForeignKey("dbo.Users", t => t.User_id)
                .Index(t => t.ActivityCategory_id)
                .Index(t => t.Customer_id)
                .Index(t => t.User_id);
            
            CreateTable(
                "dbo.ActivityCategories",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(),
                        DeleteStatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Phone = c.String(),
                        RegDate = c.DateTime(nullable: false),
                        DeleteStatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        RegDate = c.DateTime(nullable: false),
                        IsCheckedout = c.Boolean(nullable: false),
                        CheckoutDate = c.DateTime(),
                        DeleteStatus = c.Boolean(nullable: false),
                        OffCode = c.String(),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Customer_id = c.Int(),
                        User_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Customers", t => t.Customer_id)
                .ForeignKey("dbo.Users", t => t.User_id)
                .Index(t => t.Customer_id)
                .Index(t => t.User_id);
            
            CreateTable(
                "dbo.InvoiceLines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InvoiceId = c.Int(nullable: false),
                        CatalogItemId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CatalogItems", t => t.CatalogItemId, cascadeDelete: true)
                .ForeignKey("dbo.Invoices", t => t.InvoiceId, cascadeDelete: true)
                .Index(t => t.InvoiceId)
                .Index(t => t.CatalogItemId);
            
            CreateTable(
                "dbo.CatalogItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Kind = c.Int(nullable: false),
                        SalePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Stock = c.Int(nullable: false),
                        DeleteStatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Reminders",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Info = c.String(),
                        RegDate = c.DateTime(nullable: false),
                        RemindDate = c.DateTime(nullable: false),
                        DeleteStatus = c.Boolean(nullable: false),
                        IsReminded = c.Boolean(nullable: false),
                        User_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.User_id)
                .Index(t => t.User_id);
            
            CreateTable(
                "dbo.MessagePanels",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        APIToken = c.String(),
                        LineNumber = c.String(),
                        RegDate = c.DateTime(nullable: false),
                        EditDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        DeleteStatus = c.Boolean(nullable: false),
                        RegDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.OffCodes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        IsPrice = c.Boolean(nullable: false),
                        Price = c.Decimal(precision: 18, scale: 2),
                        Percent = c.Int(),
                        RegDate = c.DateTime(nullable: false),
                        ExpireDate = c.DateTime(),
                        LimitCount = c.Int(),
                        DeleteStatus = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.RememberMes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        IsRemembered = c.Boolean(nullable: false),
                        LastLoginTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "UserGroup_id", "dbo.UserGroups");
            DropForeignKey("dbo.Reminders", "User_id", "dbo.Users");
            DropForeignKey("dbo.Activities", "User_id", "dbo.Users");
            DropForeignKey("dbo.Invoices", "User_id", "dbo.Users");
            DropForeignKey("dbo.InvoiceLines", "InvoiceId", "dbo.Invoices");
            DropForeignKey("dbo.InvoiceLines", "CatalogItemId", "dbo.CatalogItems");
            DropForeignKey("dbo.Invoices", "Customer_id", "dbo.Customers");
            DropForeignKey("dbo.Activities", "Customer_id", "dbo.Customers");
            DropForeignKey("dbo.Activities", "ActivityCategory_id", "dbo.ActivityCategories");
            DropForeignKey("dbo.AccessGrants", "UserGroup_id", "dbo.UserGroups");
            DropIndex("dbo.Reminders", new[] { "User_id" });
            DropIndex("dbo.InvoiceLines", new[] { "CatalogItemId" });
            DropIndex("dbo.InvoiceLines", new[] { "InvoiceId" });
            DropIndex("dbo.Invoices", new[] { "User_id" });
            DropIndex("dbo.Invoices", new[] { "Customer_id" });
            DropIndex("dbo.Activities", new[] { "User_id" });
            DropIndex("dbo.Activities", new[] { "Customer_id" });
            DropIndex("dbo.Activities", new[] { "ActivityCategory_id" });
            DropIndex("dbo.Users", new[] { "UserGroup_id" });
            DropIndex("dbo.AccessGrants", new[] { "UserGroup_id" });
            DropTable("dbo.RememberMes");
            DropTable("dbo.OffCodes");
            DropTable("dbo.Messages");
            DropTable("dbo.MessagePanels");
            DropTable("dbo.Reminders");
            DropTable("dbo.CatalogItems");
            DropTable("dbo.InvoiceLines");
            DropTable("dbo.Invoices");
            DropTable("dbo.Customers");
            DropTable("dbo.ActivityCategories");
            DropTable("dbo.Activities");
            DropTable("dbo.Users");
            DropTable("dbo.UserGroups");
            DropTable("dbo.AccessGrants");
        }
    }
}
