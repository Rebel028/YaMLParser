namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fileout2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileOutputs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        FilePath = c.String(),
                        Vendor = c.String(),
                        FileType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.OutputLinks", "File_Id", c => c.Int());
            CreateIndex("dbo.OutputLinks", "File_Id");
            AddForeignKey("dbo.OutputLinks", "File_Id", "dbo.FileOutputs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OutputLinks", "File_Id", "dbo.FileOutputs");
            DropIndex("dbo.OutputLinks", new[] { "File_Id" });
            DropColumn("dbo.OutputLinks", "File_Id");
            DropTable("dbo.FileOutputs");
        }
    }
}
