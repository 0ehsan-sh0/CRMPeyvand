namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserAccessRoles", "UserGroup_id", "dbo.UserGroups");
            DropIndex("dbo.UserAccessRoles", new[] { "UserGroup_id" });
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
            
            AddColumn("dbo.UserGroups", "IsBuiltIn", c => c.Boolean(nullable: false));
            DropTable("dbo.UserAccessRoles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserAccessRoles",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Section = c.String(),
                        CanEnter = c.Boolean(nullable: false),
                        CanCreate = c.Boolean(nullable: false),
                        CanUpdate = c.Boolean(nullable: false),
                        CanDelete = c.Boolean(nullable: false),
                        UserGroup_id = c.Int(),
                    })
                .PrimaryKey(t => t.id);
            
            DropForeignKey("dbo.AccessGrants", "UserGroup_id", "dbo.UserGroups");
            DropIndex("dbo.AccessGrants", new[] { "UserGroup_id" });
            DropColumn("dbo.UserGroups", "IsBuiltIn");
            DropTable("dbo.AccessGrants");
            CreateIndex("dbo.UserAccessRoles", "UserGroup_id");
            AddForeignKey("dbo.UserAccessRoles", "UserGroup_id", "dbo.UserGroups", "id");
        }
    }
}
