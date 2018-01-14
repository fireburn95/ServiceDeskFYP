namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedKnowledgeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Knowledges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Created = c.DateTime(nullable: false),
                        Group_Id = c.Int(nullable: false),
                        Summary = c.String(nullable: false, maxLength: 150),
                        Description = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.Group_Id, cascadeDelete: false)
                .Index(t => t.Group_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Knowledges", "Group_Id", "dbo.Groups");
            DropIndex("dbo.Knowledges", new[] { "Group_Id" });
            DropTable("dbo.Knowledges");
        }
    }
}
