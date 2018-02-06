namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mainOutputFile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Providers", "MainOutputFile_Id", c => c.Int());
            CreateIndex("dbo.Providers", "MainOutputFile_Id");
            AddForeignKey("dbo.Providers", "MainOutputFile_Id", "dbo.FileOutputs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Providers", "MainOutputFile_Id", "dbo.FileOutputs");
            DropIndex("dbo.Providers", new[] { "MainOutputFile_Id" });
            DropColumn("dbo.Providers", "MainOutputFile_Id");
        }
    }
}
