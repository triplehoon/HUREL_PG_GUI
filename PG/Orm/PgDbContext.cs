using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Orm
{
    public class PgDbContext : DbContext
    {
        public PgDbContext()
        {
        }

        public PgDbContext(DbContextOptions<PgDbContext> options) : base(options)
        {
        }

        public DbSet<FpgaData> RawDataList { get; set; }
        public DbSet<SessionInfo> SessionInfos { get; set; }
        public DbSet<SessionAggSpots> SessionAggSpots { get; set; }
        public DbSet<SessionLogSpots> SessionLogSpots { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string? dbConnectionString = ConfigurationManager.AppSettings["DbConnectionString"];
                if (dbConnectionString == null)
                {
                    throw new Exception("DbConnectionString is not set in App.config");
                }
                optionsBuilder.UseNpgsql(dbConnectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // mapping for SessionInfo table set table name
            modelBuilder.Entity<SessionInfo>().ToTable("sessioninfo");
            // mapping for FpgaData table set table name
            modelBuilder.Entity<FpgaData>().ToTable("fpgadata");
            modelBuilder.Entity<SessionAggSpots>().ToTable("sessionaggspots");
            modelBuilder.Entity<SessionLogSpots>().ToTable("sessionlogspots");
        }
    }
}
