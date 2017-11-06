namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aliases : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "Aliases", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "Aliases");
        }
    }
}
