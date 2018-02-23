namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedAlertTable_AddedDismissedWhen : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Alerts", "DismissedWhen", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Alerts", "DismissedWhen");
        }
    }
}
