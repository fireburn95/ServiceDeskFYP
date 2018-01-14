namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeEmailUnique : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.AspNetUsers", "Email", unique: true, name: "EmailIndex");
        }
        
        public override void Down()
        {
        }
    }
}
