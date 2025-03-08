using ImageProcessing.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessing
{
    public class ImageDbContext : DbContext
    {
        public ImageDbContext(DbContextOptions<ImageDbContext> options)
            : base(options)
        {
        }
        public DbSet<ImageVariant> ImageVariants { get; set; }
        public DbSet<VariantType> VariantTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial VariantTypes
            modelBuilder.Entity<VariantType>().HasData(
                new VariantType { Id = 1, Name = "Original", MaxSize = 0, Quality = 100 },
                new VariantType { Id = 2, Name = "TwoK", MaxSize = 2048, Quality = 85 },
                new VariantType { Id = 3, Name = "Web", MaxSize = 1200, Quality = 75 },
                new VariantType { Id = 4, Name = "Mobile", MaxSize = 768, Quality = 70 },
                new VariantType { Id = 5, Name = "Thumbnail", MaxSize = 300, Quality = 65 }
            );
        }
    }
}
