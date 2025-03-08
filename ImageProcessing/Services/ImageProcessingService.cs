using ImageProcessing.Models;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats;

namespace ImageProcessing.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly IMemoryCache _cache;
        private readonly ImageDbContext _dbContext;

        public ImageProcessingService(IMemoryCache cache, ImageDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<ImageVariant> GenerateVariantAsync(Stream imageStream, string fileName, VariantType type)
        {
            string cacheKey = $"{fileName}_{type.Name}";
            if (_cache.TryGetValue(cacheKey, out ImageVariant cachedVariant))
            {
                return cachedVariant;
            }

            using var image = await Image.LoadAsync(imageStream);
            var maxSize = type.MaxSize;
            var quality = type.Quality;

            if (maxSize > 0 && (image.Width > maxSize || image.Height > maxSize))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxSize, maxSize)
                }));
            }

            string format = type.Name == "Original" ? Path.GetExtension(fileName) : ".webp";
            string outputPath = Path.Combine("Assets", $"{Guid.NewGuid()}{format}");
            Directory.CreateDirectory("Assets");

            IImageEncoder encoder = type.Name == "Original"
            ? new JpegEncoder { Quality = quality }
            : new WebpEncoder { Quality = quality };

            await image.SaveAsync(outputPath, encoder);

            var variant = new ImageVariant
            {
                OriginalFileName = fileName,
                FilePath = outputPath,
                VariantTypeId = type.Id,
                Width = image.Width,
                Height = image.Height,
                Format = format,
                FileSize = new FileInfo(outputPath).Length,
                CreatedAt = DateTime.UtcNow,
                Quality = quality
            };

            _dbContext.ImageVariants.Add(variant);
            await _dbContext.SaveChangesAsync();

            _cache.Set(cacheKey, variant, TimeSpan.FromHours(24));
            return variant;
        }
    }
}
