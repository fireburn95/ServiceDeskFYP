namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedSLAPolicyTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SLAPolicies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 25),
                        LowMins = c.Int(nullable: false),
                        MedMins = c.Int(nullable: false),
                        HighMins = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SLAPolicies");
        }
    }
}
