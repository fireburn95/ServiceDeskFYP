namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedAlertTable_AddedFromGroupIdField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Alerts", "FromGroupId", c => c.Int());
            CreateIndex("dbo.Alerts", "FromGroupId");
            AddForeignKey("dbo.Alerts", "FromGroupId", "dbo.Groups", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Alerts", "FromGroupId", "dbo.Groups");
            DropIndex("dbo.Alerts", new[] { "FromGroupId" });
            DropColumn("dbo.Alerts", "FromGroupId");
        }
    }
}
