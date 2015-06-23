using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace WebApplication1.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public byte[] UserImage { get; set; }

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; }

        public ICollection<Interest> Interests { get; set; }

        public  ICollection<string> Friends { get; set; }

    }

    public class Interest
    {
        public int ID { get; set; }

        public string UserID { get; set; }

        public string InterestString { get; set; } 
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Interest> Interest { get; set; } 

    }
}