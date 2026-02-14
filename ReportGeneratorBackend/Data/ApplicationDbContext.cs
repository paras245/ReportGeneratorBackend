using Microsoft.EntityFrameworkCore;
using ReportGeneratorBackend.Models;
using System;

namespace ReportGeneratorBackend.Data
{
    // Represents the database session and maps objects to database tables
    public class ApplicationDbContext : DbContext
    {
        #region Constructor

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        #endregion

        #region DbSets

        // Table: ReportJobs
        public DbSet<ReportJob> ReportJobs => Set<ReportJob>();

        #endregion
    }

}
