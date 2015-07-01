namespace WebApplication1.Migrations.PostsDbContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Like : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Likes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        postID = c.String(),
                        userID = c.String(),
                        Status = c.Int(nullable: false),
                        Post_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Posts", t => t.Post_ID)
                .Index(t => t.Post_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Likes", "Post_ID", "dbo.Posts");
            DropIndex("dbo.Likes", new[] { "Post_ID" });
            DropTable("dbo.Likes");
        }
    }
}
