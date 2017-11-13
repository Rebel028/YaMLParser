namespace YMLParser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class get : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OutputLinks", "Selected", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OutputLinks", "Selected");
        }
    }
}
