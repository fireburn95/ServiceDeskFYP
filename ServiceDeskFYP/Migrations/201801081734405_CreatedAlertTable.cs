namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedAlertTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alerts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FromUserId = c.String(maxLength: 128),
                        ToUserId = c.String(maxLength: 128),
                        ToGroupId = c.Int(),
                        Text = c.String(nullable: false, maxLength: 500),
                        AssociatedCallRef = c.String(maxLength: 12),
                        AssociatedKnowledgeId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        Dismissed = c.Boolean(nullable: false),
                        DismissedByUserId_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.FromUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.ToUserId)
                .ForeignKey("dbo.Calls", t => t.AssociatedCallRef)
                .ForeignKey("dbo.AspNetUsers", t => t.DismissedByUserId_Id)
                .ForeignKey("dbo.Groups", t => t.ToGroupId)
                .ForeignKey("dbo.Knowledges", t => t.AssociatedKnowledgeId)
                .Index(t => t.FromUserId)
                .Index(t => t.ToUserId)
                .Index(t => t.ToGroupId)
                .Index(t => t.AssociatedCallRef)
                .Index(t => t.AssociatedKnowledgeId)
                .Index(t => t.DismissedByUserId_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Alerts", "AssociatedKnowledgeId", "dbo.Knowledges");
            DropForeignKey("dbo.Alerts", "ToGroupId", "dbo.Groups");
            DropForeignKey("dbo.Alerts", "DismissedByUserId_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Alerts", "AssociatedCallRef", "dbo.Calls");
            DropForeignKey("dbo.Alerts", "ToUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Alerts", "FromUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Alerts", new[] { "DismissedByUserId_Id" });
            DropIndex("dbo.Alerts", new[] { "AssociatedKnowledgeId" });
            DropIndex("dbo.Alerts", new[] { "AssociatedCallRef" });
            DropIndex("dbo.Alerts", new[] { "ToGroupId" });
            DropIndex("dbo.Alerts", new[] { "ToUserId" });
            DropIndex("dbo.Alerts", new[] { "FromUserId" });
            DropTable("dbo.Alerts");
        }
    }
}
