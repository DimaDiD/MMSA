using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MMSA.DAL.Entities
{
    public class RepositoryContext: IdentityDbContext<User>
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options): base(options)
        {
            Database.EnsureCreated();
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MMSADB;Trusted_Connection=True;");            
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        UserName = "first1",
                        Password = "pass"
                    }
                );

            base.OnModelCreating(modelBuilder);
        }       
        public DbSet<User> Users { get; set; }
        public DbSet<UserCalculation> UserCalculations { get; set; }
        public DbSet<Calculation> Calculations { get; set; }
    }
}
