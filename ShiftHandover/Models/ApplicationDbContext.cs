using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ShiftHandover.Models
{
    public class ApplicationDbContext : DbContext



    {
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftLog> ShiftLogs { get; set; }

        public DbSet<User> Users { get; set; }
       


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }


    }
}
