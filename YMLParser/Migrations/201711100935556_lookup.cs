namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class lookup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OutputLinkCategories", "OutputLink_Id", "dbo.OutputLinks");
            DropForeignKey("dbo.OutputLinkCategories", "Category_Id", "dbo.Categories");
            DropForeignKey("dbo.Providers", "OutputLink_Id", "dbo.OutputLinks");
            DropIndex("dbo.Providers", new[] { "OutputLink_Id" });
            DropIndex("dbo.OutputLinkCategories", new[] { "OutputLink_Id" });
            DropIndex("dbo.OutputLinkCategories", new[] { "Category_Id" });
            DropColumn("dbo.Providers", "OutputLink_Id");
            DropTable("dbo.OutputLinkCategories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.OutputLinkCategories",
                c => new
                    {
                        OutputLink_Id = c.Int(nullable: false),
                        Category_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.OutputLink_Id, t.Category_Id });
            
            AddColumn("dbo.Providers", "OutputLink_Id", c => c.Int());
            CreateIndex("dbo.OutputLinkCategories", "Category_Id");
            CreateIndex("dbo.OutputLinkCategories", "OutputLink_Id");
            CreateIndex("dbo.Providers", "OutputLink_Id");
            AddForeignKey("dbo.Providers", "OutputLink_Id", "dbo.OutputLinks", "Id");
            AddForeignKey("dbo.OutputLinkCategories", "Category_Id", "dbo.Categories", "Id", cascadeDelete: true);
            AddForeignKey("dbo.OutputLinkCategories", "OutputLink_Id", "dbo.OutputLinks", "Id", cascadeDelete: true);
        }
    }
}
