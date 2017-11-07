namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class linksName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OutputLinks", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OutputLinks", "Name");
        }
    }
}
