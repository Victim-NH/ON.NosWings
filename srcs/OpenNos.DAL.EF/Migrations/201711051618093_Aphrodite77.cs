using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite77 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "IsDaily", c => c.Boolean(false));
            AddColumn("dbo.Quest", "EndDialogId", c => c.Int());
        }

        public override void Down()
        {
            DropColumn("dbo.Quest", "EndDialogId");
            DropColumn("dbo.Quest", "IsDaily");
        }
    }
}