namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fileout : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FileOutputs");
        }
    }
}
