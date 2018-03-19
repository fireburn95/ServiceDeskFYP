namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedClientEmployeeAndAdminRoles : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO[dbo].[AspNetRoles]([Id], [Name]) VALUES(N'02da0527-7540-4516-a887-993c471c7ce8', N'Admin')");
            Sql("INSERT INTO[dbo].[AspNetRoles]([Id], [Name]) VALUES(N'eaca6456-c8dc-4344-96d4-4e49a320ba7c', N'Client')");
            Sql("INSERT INTO[dbo].[AspNetRoles]([Id], [Name]) VALUES(N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905', N'Employee')");
        }
        
        public override void Down()
        {
            Sql("DELETE FROM[dbo].[AspNetRoles]");
        }
    }
}
