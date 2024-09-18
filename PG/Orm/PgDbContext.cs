using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Orm
{
    internal class PgDbContext : DbContext
    {
        public PgDbContext ()
        {


        }

        public MyDbContext(DbContextOptions<MyDbContext> options)
       : base(options)
        {
        }

        public virtual DbSet<MyUsers> MyUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConfigurationManager.AppSettings["DbConnectionString"]);
            }
        }
    }
}
