namespace ServiceDeskFYP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddedVariousTestDataForTables : DbMigration
    {
        public override void Up()
        {
            //Groups
            Sql("SET IDENTITY_INSERT [dbo].[Groups] ON");
            Sql("INSERT INTO [dbo].[Groups] ([Id], [Name], [Description]) VALUES (1, N'IT Helpdesk', N'The IT Helpdesk and Services team - Servicing the global regions for this organisation. We handle and deal with IT-related issues, as well as act as a contact point between other IT Departments.')");
            Sql("INSERT INTO [dbo].[Groups] ([Id], [Name], [Description]) VALUES (2, N'HR Group', N'The Human Resources team for the organisation, dealing with issues internally and externally regarding the extended workforce and relations.')");
            Sql("INSERT INTO [dbo].[Groups] ([Id], [Name], [Description]) VALUES (3, N'IT Network Dept', N'The IT Network team, handling issues concerning the organisations Network and Communication infrastructure across the global region.')");
            Sql("SET IDENTITY_INSERT [dbo].[Groups] OFF");

            //Employees & Clients
            Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES (N'534da83b-4a29-4c33-a5f1-39f68ef93c26', N'jones123@gmail.com', 1, N'AB2Uj5kqE8u65Tfuomj19BtisBHbR0+HzzMQ6bvm2Xrhm7LRJe93PhoX96GCfl/gdA==', N'0184f173-cde4-48b8-a722-1c1b60731597', N'07847545568', 0, 0, NULL, 1, 0, N'e_jones', N'Phil', N'Jones', N'+3387', NULL, NULL, N'IT', 0, N'2018-03-19 17:58:01')");
            Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES (N'6d10a7fb-34da-442f-adce-90d10fd81406', N'sada_mana@outlook.com', 1, N'AEJACVAd/zeB0Kej77G7CcS0HdJsQPr4Oz+aFi8jEAIs21bdYYDVOlCemdZeBJB7hw==', N'c169a981-959a-4fbd-bbc7-52effa9269a6', N'07847515563', 0, 0, NULL, 1, 0, N'e_sadio', N'Sadio', N'Mane', N'+3374', NULL, NULL, N'IT', 0, N'2018-03-19 17:59:23')");
            Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES (N'7b915177-4355-4680-b84a-de0a288da446', N'ally@gmail.com', 1, N'APr/jZ0Mv8pBffnpfw5Ml9/Lpid+vppaUBYKUo74gGOqa+t30YeNkWf0reecTV9+oA==', N'fcadcaf6-7311-4061-a210-9eafde0c6de2', N'07854114456', 0, 0, NULL, 1, 0, N'e_ally', N'Alison', N'Lawler', N'+3333', NULL, NULL, N'HR', 0, N'2018-03-19 18:05:03')");
            Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES (N'c1c91e96-dbc7-4a16-b624-f625ee7b9660', N'kayne@blueyonder.com', 1, N'ALEsClRnrDkRbA3j3m3qpj2XIulbaiByjI5F28+t2M/ZfYl7cE5MFibtWSeRnvtX4A==', N'feb6f156-1017-430b-8912-c73e8958e449', N'07458774563', 0, 0, NULL, 1, 0, N'a_kaynespa', N'Kayne', N'Spalding', N'+3332', NULL, NULL, NULL, 0, N'2018-03-19 18:10:59')");
            Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES (N'cc9b046c-2437-4d83-a714-11d5339a79dc', N'spencer@gmail.com', 1, N'AOslPgygd+KvjVZ65Ugy56C+jmdDFmfAonWfrShApgqmBfF7iICibsM/XKugthH/gw==', N'b4fa1239-f8de-4d9a-9428-a001e560c169', N'07845246587', 0, 0, NULL, 1, 0, N'c_spencer', N'Spencer', N'Jones', N'+6658', N'spencej', N'Execo Systems Ltd', N'IT Management', 0, N'2018-03-19 18:18:02')");
            Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES (N'd17bb74f-a9da-47b2-9089-f0fedfa06858', N'richard@gmail.com', 1, N'AG4/JZcO05vFOv7VhYa5sivclAz7e3qZeibwu4ClEhBwbqJZaZfd/+/9Yzo+RqqbvA==', N'00643169-1094-4b42-b2f7-57546fd89ecd', N'01166598857', 0, 0, NULL, 1, 0, N'e_richard', N'Richard', N'Jackson', NULL, NULL, NULL, N'IT Network', 0, N'2018-03-19 18:06:48')");
            Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [Extension], [OrganisationAlias], [Organisation], [Department], [Disabled], [CreatedTimestamp]) VALUES (N'f0b75deb-5d09-49ba-90be-8d6b1e0c0454', N'e_franco2217@gmail.com', 1, N'ACtj/wSa0EpgHkX68c5v1m4wduutxsSG1lwGotMUocFmqe6rNCuJawJxaF+uyNHSvw==', N'136a2976-3dda-436b-8c03-6018c4040e44', N'07845554481', 0, 0, NULL, 1, 0, N'e_franco', N'Frank', N'Butler', N'+4432', NULL, NULL, N'IT', 0, N'2018-03-19 17:56:11')");

            //Roles
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'534da83b-4a29-4c33-a5f1-39f68ef93c26', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'6d10a7fb-34da-442f-adce-90d10fd81406', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'7b915177-4355-4680-b84a-de0a288da446', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'c1c91e96-dbc7-4a16-b624-f625ee7b9660', N'02da0527-7540-4516-a887-993c471c7ce8')");
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'c1c91e96-dbc7-4a16-b624-f625ee7b9660', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'cc9b046c-2437-4d83-a714-11d5339a79dc', N'eaca6456-c8dc-4344-96d4-4e49a320ba7c')");
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'd17bb74f-a9da-47b2-9089-f0fedfa06858', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");
            Sql("INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'f0b75deb-5d09-49ba-90be-8d6b1e0c0454', N'bf8eca49-f90f-4fb3-9bf5-0bd99ad59905')");




            //Group Members
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (1, N'534da83b-4a29-4c33-a5f1-39f68ef93c26', 1)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (1, N'6d10a7fb-34da-442f-adce-90d10fd81406', 0)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (1, N'8aeafb25-1046-4790-9fae-bc153efedf7d', 1)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (1, N'ad037d14-40ab-4a9f-a474-aed08ca42a75', 0)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (1, N'd17bb74f-a9da-47b2-9089-f0fedfa06858', 0)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (1, N'f0b75deb-5d09-49ba-90be-8d6b1e0c0454', 0)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (2, N'7b915177-4355-4680-b84a-de0a288da446', 1)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (2, N'8aeafb25-1046-4790-9fae-bc153efedf7d', 0)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (2, N'ad037d14-40ab-4a9f-a474-aed08ca42a75', 0)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (3, N'8aeafb25-1046-4790-9fae-bc153efedf7d', 0)");
            Sql("INSERT INTO [dbo].[GroupMembers] ([Group_Id], [User_Id], [Owner]) VALUES (3, N'd17bb74f-a9da-47b2-9089-f0fedfa06858', 1)");

            //Managers
            Sql("INSERT INTO [dbo].[ManagerEmployees] ([ManagerUserId], [SubUserId]) VALUES (N'534da83b-4a29-4c33-a5f1-39f68ef93c26', N'6d10a7fb-34da-442f-adce-90d10fd81406')");
            Sql("INSERT INTO [dbo].[ManagerEmployees] ([ManagerUserId], [SubUserId]) VALUES (N'534da83b-4a29-4c33-a5f1-39f68ef93c26', N'f0b75deb-5d09-49ba-90be-8d6b1e0c0454')");

            //Knowledges
            Sql("SET IDENTITY_INSERT [dbo].[Knowledges] ON");
            Sql("INSERT INTO [dbo].[Knowledges] ([Id], [Created], [Group_Id], [Summary], [Description], [Updated], [LastUpdatedByUserId]) VALUES (1, N'2018-03-19 18:35:54', 1, N'''Printer not found e44785''', N'This error appears to be showing up on all pre-windows 7 devices when attempting to print to a valid printer, a fix is forthcoming but for now there is a quick work around detailed: " +
                "1) Open device and management shortcut on the desktop" +
                "2) Right click and disable the printer icon, then re-enable it." +
                "3) Restart the machine, it should be working now', N'2018-03-19 18:35:54', N'8aeafb25-1046-4790-9fae-bc153efedf7d')");
            Sql("INSERT INTO [dbo].[Knowledges] ([Id], [Created], [Group_Id], [Summary], [Description], [Updated], [LastUpdatedByUserId]) VALUES (2, N'2018-03-19 18:38:40', 1, N'Change Password on Windows XP issues', N'All XP devices are reacting badly to our latest group policy, this is being looked into, but in the mean time, you will need to change it on their behalf via Active Directory." +
            "', N'2018-03-19 18:38:40', N'8aeafb25-1046-4790-9fae-bc153efedf7d')");
            Sql("INSERT INTO [dbo].[Knowledges] ([Id], [Created], [Group_Id], [Summary], [Description], [Updated], [LastUpdatedByUserId]) VALUES (3, N'2018-03-19 18:41:12', 2, N'New employees as of w/c 19th March', N'Please update when you have actioned the below:" +
            "Richard Jones - completed" +
            "Alex Wyatt" +
            "Denise Phills', N'2018-03-19 18:41:12', N'8aeafb25-1046-4790-9fae-bc153efedf7d')");
            Sql("SET IDENTITY_INSERT [dbo].[Knowledges] OFF");

            //SLA
            Sql("SET IDENTITY_INSERT [dbo].[SLAPolicies] ON");
            Sql("INSERT INTO [dbo].[SLAPolicies] ([Id], [Name], [LowMins], [MedMins], [HighMins]) VALUES (1, N'IT', 10080, 4320, 1440)");
            Sql("INSERT INTO [dbo].[SLAPolicies] ([Id], [Name], [LowMins], [MedMins], [HighMins]) VALUES (2, N'HR', 40320, 20160, 10080)");
            Sql("SET IDENTITY_INSERT [dbo].[SLAPolicies] OFF");

            //Calls
            Sql("INSERT INTO [dbo].[Calls] ([Reference], [ResourceUserId], [ResourceGroupId], [SlaId], [SlaLevel], [Category], [Created], [Required_By], [SLAResetTime], [Summary], [Description], [ForUserId], [Closed], [Hidden], [LockedToUserId], [Email], [FirstName], [Lastname], [PhoneNumber], [Extension], [OrganisationAlias], [Organisation], [Department], [Regarding_Ref]) VALUES (N'76oyp44uwqvx', NULL, 1, 1, N'Low', N'Issue', N'2018-03-19 19:00:06', NULL, N'2018-03-19 19:00:06', N'Broken gauge mobile in L-Ops', N'User has reported the gauge mobile is experiencing issues. ', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'L-Ops', NULL)");
            Sql("INSERT INTO [dbo].[Calls] ([Reference], [ResourceUserId], [ResourceGroupId], [SlaId], [SlaLevel], [Category], [Created], [Required_By], [SLAResetTime], [Summary], [Description], [ForUserId], [Closed], [Hidden], [LockedToUserId], [Email], [FirstName], [Lastname], [PhoneNumber], [Extension], [OrganisationAlias], [Organisation], [Department], [Regarding_Ref]) VALUES (N'gcy4w9khy3lj', NULL, 1, 1, N'High', N'Issue', N'2018-03-19 19:10:34', NULL, N'2018-03-19 19:10:34', N'Dll-Services not loading', N'Execo Systems who rely on our dll services for business continuity report that errors are appearing when loading the associated software. ', N'cc9b046c-2437-4d83-a714-11d5339a79dc', 0, 0, NULL, N'spencer@gmail.com', N'Spencer', N'Jones', N'07845246587', N'+6658', N'spencej', N'Execo Systems Ltd', N'IT Management', NULL)");
            Sql("INSERT INTO [dbo].[Calls] ([Reference], [ResourceUserId], [ResourceGroupId], [SlaId], [SlaLevel], [Category], [Created], [Required_By], [SLAResetTime], [Summary], [Description], [ForUserId], [Closed], [Hidden], [LockedToUserId], [Email], [FirstName], [Lastname], [PhoneNumber], [Extension], [OrganisationAlias], [Organisation], [Department], [Regarding_Ref]) VALUES (N'k59t4sujtaou', N'8aeafb25-1046-4790-9fae-bc153efedf7d', NULL, 1, N'Task', N'Other', N'2018-03-19 18:52:28', NULL, NULL, N'Weekly Admin Report', N'Update on the admin report for the shareholders', NULL, 0, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
            Sql("INSERT INTO [dbo].[Calls] ([Reference], [ResourceUserId], [ResourceGroupId], [SlaId], [SlaLevel], [Category], [Created], [Required_By], [SLAResetTime], [Summary], [Description], [ForUserId], [Closed], [Hidden], [LockedToUserId], [Email], [FirstName], [Lastname], [PhoneNumber], [Extension], [OrganisationAlias], [Organisation], [Department], [Regarding_Ref]) VALUES (N'n36cxdixb817', N'6d10a7fb-34da-442f-adce-90d10fd81406', NULL, 1, N'On-Going', N'Other', N'2018-03-19 19:04:18', NULL, NULL, N'IT Health check of my PC', NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");

            //Actions
            Sql("SET IDENTITY_INSERT [dbo].[Actions] ON");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (1, N'k59t4sujtaou', N'8aeafb25-1046-4790-9fae-bc153efedf7d', N'2018-03-19 18:52:28', N'Opened Call', NULL, NULL, NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (2, N'k59t4sujtaou', N'8aeafb25-1046-4790-9fae-bc153efedf7d', N'2018-03-19 18:54:29', N'Progress Report', NULL, N'19th March, No issues identified.', N'C:\\Users\ab_p_.DESKTOP-OCHN93Q\\Documents\\p13223165\\ServiceDeskFYP\\ServiceDeskFYP\\Content\\actionfiles\\adminreport.docx')");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (3, N'76oyp44uwqvx', N'534da83b-4a29-4c33-a5f1-39f68ef93c26', N'2018-03-19 19:00:06', N'Opened Call', NULL, NULL, NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (4, N'76oyp44uwqvx', N'534da83b-4a29-4c33-a5f1-39f68ef93c26', N'2018-03-19 19:00:39', N'Assigned', N'IT Helpdesk', NULL, NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (5, N'76oyp44uwqvx', N'6d10a7fb-34da-442f-adce-90d10fd81406', N'2018-03-19 19:02:50', N'Identified Problem', NULL, N'CPU unit has burnt out, requested for a replacement to be bought.', NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (6, N'n36cxdixb817', N'6d10a7fb-34da-442f-adce-90d10fd81406', N'2018-03-19 19:04:18', N'Opened Call', NULL, NULL, NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (7, N'n36cxdixb817', N'6d10a7fb-34da-442f-adce-90d10fd81406', N'2018-03-19 19:04:54', N'Update', NULL, N'19th March no issues', NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (8, N'gcy4w9khy3lj', N'6d10a7fb-34da-442f-adce-90d10fd81406', N'2018-03-19 19:10:34', N'Opened Call', NULL, NULL, NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (9, N'gcy4w9khy3lj', N'6d10a7fb-34da-442f-adce-90d10fd81406', N'2018-03-19 19:10:42', N'Assigned', N'IT Helpdesk', NULL, NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (10, N'gcy4w9khy3lj', N'f0b75deb-5d09-49ba-90be-8d6b1e0c0454', N'2018-03-19 19:14:16', N'Advice Given', NULL, N'Advised client to reset the server and windows update it in an hour', NULL)");
            Sql("INSERT INTO [dbo].[Actions] ([Id], [CallReference], [ActionedByUserId], [Created], [Type], [TypeDetails], [Comments], [Attachment]) VALUES (11, N'gcy4w9khy3lj', N'f0b75deb-5d09-49ba-90be-8d6b1e0c0454', N'2018-03-19 19:15:03', N'Update', NULL, N'Client has advised the issue has not been resolved as a result of my previous advice', NULL)");
            Sql("SET IDENTITY_INSERT [dbo].[Actions] OFF");




        }

        public override void Down()
        {
            //Logs
            Sql("DELETE FROM[dbo].[Logs]");

            //Actions
            Sql("DELETE FROM[dbo].[Actions]");

            //Calls
            Sql("DELETE FROM[dbo].[Calls]");

            //SLA
            Sql("DELETE FROM[dbo].[SLAPolicies]");

            //Knowledges
            Sql("DELETE FROM[dbo].[Knowledges]");

            //Managers
            Sql("DELETE FROM[dbo].[ManagerEmployees]");

            //Group Members
            Sql("DELETE FROM[dbo].[GroupMembers]");

            //Employees & Clients
            Sql("DELETE FROM[dbo].[AspNetUsers]");

            //Roles
            Sql("DELETE FROM[dbo].[AspNetUserRoles]");

            //Groups
            Sql("DELETE FROM[dbo].[Groups]");
        }
    }
}
