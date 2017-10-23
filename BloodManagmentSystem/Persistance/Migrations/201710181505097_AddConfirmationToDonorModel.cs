namespace BloodManagmentSystem.Persistance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddConfirmationToDonorModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Donors", "Confirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Donors", "Confirmed");
        }
    }
}
