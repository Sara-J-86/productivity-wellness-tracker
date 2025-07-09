using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ProductivityApp.Models
{
    public class AppdbContext : DbContext
    {
        public AppdbContext() : base("name=ProductivityDB") { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserTask> Tasks { get; set; }

        public DbSet<Mood> Moods { get; set; }
    }
}
