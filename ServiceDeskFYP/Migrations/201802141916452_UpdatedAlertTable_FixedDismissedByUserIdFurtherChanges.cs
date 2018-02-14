namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedAlertTable_FixedDismissedByUserIdFurtherChanges : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Alerts", "DismissedByUserId");
            AddForeignKey("dbo.Alerts", "DismissedByUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Alerts", "DismissedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Alerts", new[] { "DismissedByUserId" });
        }
    }
}
