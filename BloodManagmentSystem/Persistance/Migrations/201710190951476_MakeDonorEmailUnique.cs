namespace BloodManagmentSystem.Persistance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeDonorEmailUnique : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Donors", "Email", unique: true, name: "Email");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Donors", "Email");
        }
    }
}
