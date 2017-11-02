namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OutputLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Link = c.String(),
                        UserSelectionId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserSelections", t => t.UserSelectionId)
                .Index(t => t.UserSelectionId);
            
            CreateTable(
                "dbo.Providers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Link = c.String(),
                        OutputLink_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OutputLinks", t => t.OutputLink_Id)
                .Index(t => t.OutputLink_Id);
            
            CreateTable(
                "dbo.UserSelections",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OutputLinkCategories",
                c => new
                    {
                        OutputLink_Id = c.Int(nullable: false),
                        Category_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.OutputLink_Id, t.Category_Id })
                .ForeignKey("dbo.OutputLinks", t => t.OutputLink_Id, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.Category_Id, cascadeDelete: true)
                .Index(t => t.OutputLink_Id)
                .Index(t => t.Category_Id);
            
            CreateTable(
                "dbo.ProviderCategories",
                c => new
                    {
                        Provider_Id = c.Int(nullable: false),
                        Category_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Provider_Id, t.Category_Id })
                .ForeignKey("dbo.Providers", t => t.Provider_Id, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.Category_Id, cascadeDelete: true)
                .Index(t => t.Provider_Id)
                .Index(t => t.Category_Id);
            
            CreateTable(
                "dbo.UserSelectionProviders",
                c => new
                    {
                        UserSelection_Id = c.Int(nullable: false),
                        Provider_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserSelection_Id, t.Provider_Id })
                .ForeignKey("dbo.UserSelections", t => t.UserSelection_Id, cascadeDelete: true)
                .ForeignKey("dbo.Providers", t => t.Provider_Id, cascadeDelete: true)
                .Index(t => t.UserSelection_Id)
                .Index(t => t.Provider_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.OutputLinks", "UserSelectionId", "dbo.UserSelections");
            DropForeignKey("dbo.Providers", "OutputLink_Id", "dbo.OutputLinks");
            DropForeignKey("dbo.UserSelectionProviders", "Provider_Id", "dbo.Providers");
            DropForeignKey("dbo.UserSelectionProviders", "UserSelection_Id", "dbo.UserSelections");
            DropForeignKey("dbo.ProviderCategories", "Category_Id", "dbo.Categories");
            DropForeignKey("dbo.ProviderCategories", "Provider_Id", "dbo.Providers");
            DropForeignKey("dbo.OutputLinkCategories", "Category_Id", "dbo.Categories");
            DropForeignKey("dbo.OutputLinkCategories", "OutputLink_Id", "dbo.OutputLinks");
            DropIndex("dbo.UserSelectionProviders", new[] { "Provider_Id" });
            DropIndex("dbo.UserSelectionProviders", new[] { "UserSelection_Id" });
            DropIndex("dbo.ProviderCategories", new[] { "Category_Id" });
            DropIndex("dbo.ProviderCategories", new[] { "Provider_Id" });
            DropIndex("dbo.OutputLinkCategories", new[] { "Category_Id" });
            DropIndex("dbo.OutputLinkCategories", new[] { "OutputLink_Id" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Providers", new[] { "OutputLink_Id" });
            DropIndex("dbo.OutputLinks", new[] { "UserSelectionId" });
            DropTable("dbo.UserSelectionProviders");
            DropTable("dbo.ProviderCategories");
            DropTable("dbo.OutputLinkCategories");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.UserSelections");
            DropTable("dbo.Providers");
            DropTable("dbo.OutputLinks");
            DropTable("dbo.Categories");
        }
    }
}
