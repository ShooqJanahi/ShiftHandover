using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ShiftHandover.Models
{
    
    // It handles the connection to the database and manages entity sets (tables).
    public class ApplicationDbContext : DbContext
    {
        // Table for storing Shift records
        public DbSet<Shift> Shifts { get; set; } 

        // Table for storing Shift Logs (like incidents, manpower updates, etc.)
        public DbSet<ShiftLog> ShiftLogs { get; set; }

        // Table for storing Users (Supervisors, Admins, etc.)
        public DbSet<User> Users { get; set; }

        // Table for storing Departments
        public DbSet<Department> Departments { get; set; }

        // Constructor - Configure the database options (connection string, etc.)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
        {
        }

        // Override to configure database schema details using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Always call base.OnModelCreating(modelBuilder) to ensure EF Core setups its own configurations properly
            base.OnModelCreating(modelBuilder);

            // Enforce a unique constraint on the Username field in the Users table
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }


    }
}
