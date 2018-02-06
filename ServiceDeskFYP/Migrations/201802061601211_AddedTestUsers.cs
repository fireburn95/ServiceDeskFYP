namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTestUsers : DbMigration
    {
        public override void Up()
        {
            //Test Data Users
            Sql("INSERT INTO[dbo].[AspNetUsers]([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES(N'8aeafb25-1046-4790-9fae-bc153efedf7d', N'admin@test.com', 0, N'APp32AyDCI2Ty89K/b0ctSjpT8QoybD2LcsHJg6IlIlrx5OB8ElXb2J0NecK3ffN0w==', N'2544d84a-c664-4814-9ce9-996b4a4ba4f0', NULL, 0, 0, NULL, 1, 0, N'admin', N'Adam', N'Alwright', NULL, NULL, NULL, NULL, 0, N'2018-01-15 12:10:37')");
            Sql("INSERT INTO[dbo].[AspNetUsers]([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES(N'ad037d14-40ab-4a9f-a474-aed08ca42a75', N'employee@test.com', 0, N'AEDeHygt46D+o+MJMkNd0YGu/Wec/YzVR+Qd6h60zuEERdbiYsdVcC2Z44unxqmqZQ==', N'94f34109-3566-44c4-a627-abe344fca75a', NULL, 0, 0, NULL, 1, 0, N'employee', N'Eugene', N'Edwinson', NULL, NULL, NULL, NULL, 0, N'2018-01-15 12:08:58')");
            Sql("INSERT INTO[dbo].[AspNetUsers]([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES(N'cd9274b3-4224-4e76-b6f2-0cab5dd1cbdb', N'client@test.com', 0, N'ADv5J97kG0ae36yJ5lqhTWKL9S4bWGLm0PuOn5yVHb0E2DAs1Njv4xElFbbs/7pBZw==', N'6650de73-da21-490f-ae7e-13a0f1ff1a53', NULL, 0, 0, NULL, 1, 0, N'client', N'Charles', N'Cocos', NULL, NULL, NULL, N'Cheese', 0, N'2018-01-15 12:07:24')");

            //Test Data UserRoles
            Sql("INSERT INTO[dbo].[AspNetUserRoles]([UserId], [RoleId]) VALUES(N'8aeafb25-1046-4790-9fae-bc153efedf7d', N'02da0527-7540-4516-a887-993c471c7ce8')");
            Sql("INSERT INTO[dbo].[AspNetUserRoles]([UserId], [RoleId]) VALUES(N'8aeafb25-1046-4790-9fae-bc153efedf7d', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");
            Sql("INSERT INTO[dbo].[AspNetUserRoles]([UserId], [RoleId]) VALUES(N'ad037d14-40ab-4a9f-a474-aed08ca42a75', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");
            Sql("INSERT INTO[dbo].[AspNetUserRoles]([UserId], [RoleId]) VALUES(N'cd9274b3-4224-4e76-b6f2-0cab5dd1cbdb', N'eaca6456-c8dc-4344-96d4-4e49a320ba7c')");
        }

    public override void Down()
        {
        }
    }
}
