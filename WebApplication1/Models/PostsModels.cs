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

    }

    public class PostsDbContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }   
    }

}