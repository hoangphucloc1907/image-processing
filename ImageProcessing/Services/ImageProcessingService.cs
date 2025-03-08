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
        private readonly Dictionary<VariantType, (int maxSize, int quality)> VariantSettings = new()
    {
        { VariantType.Original, (0, 100) },
        { VariantType.TwoK, (2048, 85) },
        { VariantType.WebOptimized, (1200, 75) },
        { VariantType.MobileOptimized, (768, 70) },
        { VariantType.Thumbnail, (300, 65) }
    };

        public ImageProcessingService(IMemoryCache cache, ImageDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<ImageVariant> GenerateVariantAsync(Stream imageStream, string fileName, VariantType type)
        {
            string cacheKey = $"{fileName}_{type}";
            if (_cache.TryGetValue(cacheKey, out ImageVariant cachedVariant))
            {
                return cachedVariant;
            }

            using var image = await Image.LoadAsync(imageStream);
            var (maxSize, quality) = VariantSettings[type];

            if (maxSize > 0 && (image.Width > maxSize || image.Height > maxSize))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxSize, maxSize)
                }));
            }

            string format = type == VariantType.Original ? Path.GetExtension(fileName) : ".webp";
            string outputPath = Path.Combine("Assets", $"{Guid.NewGuid()}{format}");
            Directory.CreateDirectory("Assets");

            IImageEncoder encoder = type == VariantType.Original
            ? new JpegEncoder { Quality = quality }
            : new WebpEncoder { Quality = quality };

            await image.SaveAsync(outputPath, encoder);

            await image.SaveAsync(outputPath, encoder);

            var variant = new ImageVariant
            {
                OriginalFileName = fileName,
                FilePath = outputPath,
                Type = type,
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
