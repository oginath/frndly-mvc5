namespace WebApplication1.Migrations.PostsDbContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Like1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Likes", "Post_ID", "dbo.Posts");
            DropIndex("dbo.Likes", new[] { "Post_ID" });
            DropColumn("dbo.Likes", "postID");
            RenameColumn(table: "dbo.Likes", name: "Post_ID", newName: "postID");
            AlterColumn("dbo.Likes", "postID", c => c.Int(nullable: false));
            AlterColumn("dbo.Likes", "postID", c => c.Int(nullable: false));
            CreateIndex("dbo.Likes", "postID");
            AddForeignKey("dbo.Likes", "postID", "dbo.Posts", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Likes", "postID", "dbo.Posts");
            DropIndex("dbo.Likes", new[] { "postID" });
            AlterColumn("dbo.Likes", "postID", c => c.Int());
            AlterColumn("dbo.Likes", "postID", c => c.String());
            RenameColumn(table: "dbo.Likes", name: "postID", newName: "Post_ID");
            AddColumn("dbo.Likes", "postID", c => c.String());
            CreateIndex("dbo.Likes", "Post_ID");
            AddForeignKey("dbo.Likes", "Post_ID", "dbo.Posts", "ID");
        }
    }
}
