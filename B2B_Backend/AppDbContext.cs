using B2B_PRO.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace B2B_PRO
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
        }
        public DbSet<CompanyN> companyName { get; set; }
        public DbSet<LeadsData> leadsDatas { get; set; }
        public DbSet<User> Users { get; set;  }
        public DbSet<AuditLog> AuditLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId);
            modelBuilder.Entity<LeadsData>()
       .HasOne(x => x.Company)
       .WithMany()
       .HasForeignKey(x => x.CompanyId);
        }
    }
}

    