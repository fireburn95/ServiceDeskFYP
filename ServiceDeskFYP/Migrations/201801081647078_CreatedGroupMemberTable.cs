namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedGroupMemberTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupMembers",
                c => new
                    {
                        Group_Id = c.Int(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                        Owner = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.Group_Id, t.User_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Groups", t => t.Group_Id, cascadeDelete: true)
                .Index(t => t.Group_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GroupMembers", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.GroupMembers", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.GroupMembers", new[] { "User_Id" });
            DropIndex("dbo.GroupMembers", new[] { "Group_Id" });
            DropTable("dbo.GroupMembers");
        }
    }
}
