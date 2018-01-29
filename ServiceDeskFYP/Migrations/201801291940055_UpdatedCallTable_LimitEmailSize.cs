namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedCallTable_LimitEmailSize : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Calls", "Email", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Calls", "Email", c => c.String());
        }
    }
}
