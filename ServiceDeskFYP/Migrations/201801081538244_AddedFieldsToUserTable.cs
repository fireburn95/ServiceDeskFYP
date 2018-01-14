namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFieldsToUserTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.AspNetUsers", "LastName", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.AspNetUsers", "Extension", c => c.String(maxLength: 10));
            AddColumn("dbo.AspNetUsers", "OrganisationAlias", c => c.String(maxLength: 15));
            AddColumn("dbo.AspNetUsers", "Organisation", c => c.String(maxLength: 25));
            AddColumn("dbo.AspNetUsers", "Department", c => c.String(maxLength: 20));
            AddColumn("dbo.AspNetUsers", "Disabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "CreatedTimestamp", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "CreatedTimestamp");
            DropColumn("dbo.AspNetUsers", "Disabled");
            DropColumn("dbo.AspNetUsers", "Department");
            DropColumn("dbo.AspNetUsers", "Organisation");
            DropColumn("dbo.AspNetUsers", "OrganisationAlias");
            DropColumn("dbo.AspNetUsers", "Extension");
            DropColumn("dbo.AspNetUsers", "LastName");
            DropColumn("dbo.AspNetUsers", "FirstName");
        }
    }
}
