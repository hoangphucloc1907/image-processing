namespace ImageProcessing.Models
{
    public class ImageVariant
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public int VariantTypeId { get; set; } // Links to VariantType
        public string Path { get; set; } // Stores the file path of the variant
        public string Format { get; set; } // jpg, png, webp
        public int Width { get; set; }
        public int Height { get; set; }
        public int Quality { get; set; } // Compression quality percentage

        public Image Image { get; set; }
        public VariantType VariantType { get; set; }
    }
}
