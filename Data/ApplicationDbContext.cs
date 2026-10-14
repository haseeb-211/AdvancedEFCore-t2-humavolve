using AdvancedEFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCore.Data
{



    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics"
                },
                new Category
                {
                    Id = 2,
                    Name = "Books"
                }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Keyboard",
                    Price = 1500m,
                    CategoryId = 1,
                    IsDeleted = false
                },
                new Product
                {
                    Id = 2,
                    Name = "C# Fundamentals",
                    Price = 450m,
                    CategoryId = 2,
                    IsDeleted = false
                }
            );
        }
    }

}
