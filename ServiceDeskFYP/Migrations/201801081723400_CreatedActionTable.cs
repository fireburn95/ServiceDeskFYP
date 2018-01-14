namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedActionTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Actions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CallReference = c.String(nullable: false, maxLength: 12),
                        ActionedByUserId = c.String(nullable: false, maxLength: 128),
                        Created = c.DateTime(nullable: false),
                        Type = c.String(nullable: false, maxLength: 20),
                        TypeDetails = c.String(maxLength: 200),
                        Comments = c.String(),
                        Attachment = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ActionedByUserId, cascadeDelete: false)
                .ForeignKey("dbo.Calls", t => t.CallReference, cascadeDelete: true)
                .Index(t => t.CallReference)
                .Index(t => t.ActionedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Actions", "CallReference", "dbo.Calls");
            DropForeignKey("dbo.Actions", "ActionedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Actions", new[] { "ActionedByUserId" });
            DropIndex("dbo.Actions", new[] { "CallReference" });
            DropTable("dbo.Actions");
        }
    }
}
