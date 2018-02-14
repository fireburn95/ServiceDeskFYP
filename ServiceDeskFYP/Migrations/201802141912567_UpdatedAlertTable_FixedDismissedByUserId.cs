namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedAlertTable_FixedDismissedByUserId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Alerts", "DismissedByUserId_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Alerts", new[] { "DismissedByUserId_Id" });
            AddColumn("dbo.Alerts", "DismissedByUserId", c => c.String(maxLength: 128));
            DropColumn("dbo.Alerts", "DismissedByUserId_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Alerts", "DismissedByUserId_Id", c => c.String(maxLength: 128));
            DropColumn("dbo.Alerts", "DismissedByUserId");
            CreateIndex("dbo.Alerts", "DismissedByUserId_Id");
            AddForeignKey("dbo.Alerts", "DismissedByUserId_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
