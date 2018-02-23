namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedAlertTable_RemovedDismissedColumn : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Alerts", "Dismissed");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Alerts", "Dismissed", c => c.Boolean(nullable: false));
        }
    }
}
