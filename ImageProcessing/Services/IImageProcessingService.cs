using ImageProcessing.Models;

namespace ImageProcessing.Services
{
    public interface IImageProcessingService
    {
        Task<ImageVariant> GenerateVariantAsync(Stream imageStream, string fileName, VariantType type);
    }
}
