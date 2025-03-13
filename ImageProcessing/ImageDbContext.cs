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
        public DbSet<Models.Image> Images { get; set; }
        public DbSet<ImageVariant> ImageVariants { get; set; }
        public DbSet<VariantType> VariantTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VariantType>().HasData(
                new VariantType { Id = 1, Name = "Original", MaxSize = 0, Description = "Unmodified original image" },
                new VariantType { Id = 2, Name = "TwoK", MaxSize = 2048, Description = "2K resolution image" },
                new VariantType { Id = 3, Name = "Web", MaxSize = 1200, Description = "Optimized for web use" },
                new VariantType { Id = 4, Name = "Mobile", MaxSize = 768, Description = "Optimized for mobile screens" },
                new VariantType { Id = 5, Name = "Thumbnail", MaxSize = 300, Description = "Small preview image" }
            );
        }

    }
}
