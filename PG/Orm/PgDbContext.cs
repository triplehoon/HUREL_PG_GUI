﻿using Microsoft.EntityFrameworkCore;
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
        public DbSet<NccLogData> NccLogDataList { get; set; }
        public DbSet<SessionInfo> SessionInfos { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string? dbConnectionString = ConfigurationManager.AppSettings["DbConnectionString"];
                if (dbConnectionString == null)
                {
                    throw new Exception("DbConnectionString is not set in App.config");
                }
                optionsBuilder.UseSqlServer(dbConnectionString);
            }
        }
    }
}