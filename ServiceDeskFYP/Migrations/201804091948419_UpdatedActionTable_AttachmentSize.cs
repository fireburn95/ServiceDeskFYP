namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedActionTable_AttachmentSize : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Actions", "Attachment", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Actions", "Attachment", c => c.String());
        }
    }
}
