namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedCallTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Calls",
                c => new
                    {
                        Reference = c.String(nullable: false, maxLength: 12),
                        ResourceUserId = c.String(maxLength: 128),
                        ResourceGroupId = c.Int(),
                        SlaId = c.Int(nullable: false),
                        SlaLevel = c.String(nullable: false, maxLength: 10),
                        Category = c.String(nullable: false, maxLength: 30),
                        Created = c.DateTime(nullable: false),
                        Required_By = c.DateTime(nullable: false),
                        SLAResetTime = c.DateTime(nullable: false),
                        Summary = c.String(nullable: false, maxLength: 75),
                        Description = c.String(),
                        ForUserId = c.String(maxLength: 128),
                        Closed = c.Boolean(nullable: false),
                        Hidden = c.Boolean(nullable: false),
                        LockedToUserId = c.String(maxLength: 128),
                        Email = c.String(),
                        FirstName = c.String(maxLength: 20),
                        Lastname = c.String(maxLength: 20),
                        PhoneNumber = c.String(maxLength: 40),
                        Extension = c.String(maxLength: 10),
                        OrganisationAlias = c.String(maxLength: 15),
                        Organisation = c.String(maxLength: 25),
                        Department = c.String(maxLength: 20),
                        Regarding_Ref = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Reference)
                .ForeignKey("dbo.AspNetUsers", t => t.ForUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.LockedToUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.ResourceUserId)
                .ForeignKey("dbo.Groups", t => t.ResourceGroupId)
                .ForeignKey("dbo.SLAPolicies", t => t.SlaId, cascadeDelete: false)
                .Index(t => t.ResourceUserId)
                .Index(t => t.ResourceGroupId)
                .Index(t => t.SlaId)
                .Index(t => t.ForUserId)
                .Index(t => t.LockedToUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Calls", "SlaId", "dbo.SLAPolicies");
            DropForeignKey("dbo.Calls", "ResourceGroupId", "dbo.Groups");
            DropForeignKey("dbo.Calls", "ResourceUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Calls", "LockedToUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Calls", "ForUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Calls", new[] { "LockedToUserId" });
            DropIndex("dbo.Calls", new[] { "ForUserId" });
            DropIndex("dbo.Calls", new[] { "SlaId" });
            DropIndex("dbo.Calls", new[] { "ResourceGroupId" });
            DropIndex("dbo.Calls", new[] { "ResourceUserId" });
            DropTable("dbo.Calls");
        }
    }
}
