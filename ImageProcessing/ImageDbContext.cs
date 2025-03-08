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
    }
}
