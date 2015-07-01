using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Post
    {

        public int ID { get; set; }

        public string UserID { get; set; }

        public DateTime DateTime { get; set; }

        public string PostContent { get; set; }

        public byte[] PostFile { get; set; }

        public ICollection<Like> Likes { get; set; }

    }

    public class Like
    {

        public int ID { get; set; }

        public int postID { get; set; }

        public string userID { get; set; }

        public LikeStatus Status { get; set; }
        
    }

   public enum LikeStatus{

        nullLike = 0,
        Like = 1,
        Dislike = 2
    }

    public class PostsDbContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }

        public DbSet<Like> Likes { get; set; }
    }

}