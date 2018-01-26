namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedCallTable_AllowNullDates : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Calls", "Required_By", c => c.DateTime());
            AlterColumn("dbo.Calls", "SLAResetTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Calls", "SLAResetTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Calls", "Required_By", c => c.DateTime(nullable: false));
        }
    }
}
