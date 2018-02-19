namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedKnowledgeTable_AddedUpdatedDateTimeAndUpdatedByFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Knowledges", "Updated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Knowledges", "LastUpdatedByUserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Knowledges", "LastUpdatedByUserId");
            AddForeignKey("dbo.Knowledges", "LastUpdatedByUserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Knowledges", "LastUpdatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Knowledges", new[] { "LastUpdatedByUserId" });
            DropColumn("dbo.Knowledges", "LastUpdatedByUserId");
            DropColumn("dbo.Knowledges", "Updated");
        }
    }
}
