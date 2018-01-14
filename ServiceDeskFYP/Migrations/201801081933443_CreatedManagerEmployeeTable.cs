namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedManagerEmployeeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ManagerEmployees",
                c => new
                    {
                        ManagerUserId = c.String(nullable: false, maxLength: 128),
                        SubUserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ManagerUserId, t.SubUserId })
                .ForeignKey("dbo.AspNetUsers", t => t.ManagerUserId, cascadeDelete: false)
                .ForeignKey("dbo.AspNetUsers", t => t.SubUserId, cascadeDelete: false)
                .Index(t => t.ManagerUserId)
                .Index(t => t.SubUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ManagerEmployees", "SubUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ManagerEmployees", "ManagerUserId", "dbo.AspNetUsers");
            DropIndex("dbo.ManagerEmployees", new[] { "SubUserId" });
            DropIndex("dbo.ManagerEmployees", new[] { "ManagerUserId" });
            DropTable("dbo.ManagerEmployees");
        }
    }
}
