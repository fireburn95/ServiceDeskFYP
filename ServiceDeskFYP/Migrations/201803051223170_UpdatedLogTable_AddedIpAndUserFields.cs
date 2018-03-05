namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedLogTable_AddedIpAndUserFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "PublicIP", c => c.String(maxLength: 45));
            AddColumn("dbo.Logs", "LocalIP", c => c.String(maxLength: 45));
            AddColumn("dbo.Logs", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Logs", "UserId");
            AddForeignKey("dbo.Logs", "UserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Logs", new[] { "UserId" });
            DropColumn("dbo.Logs", "UserId");
            DropColumn("dbo.Logs", "LocalIP");
            DropColumn("dbo.Logs", "PublicIP");
        }
    }
}
