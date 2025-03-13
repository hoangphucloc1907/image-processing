namespace ImageProcessing.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string FileName { get; set; } 
        public string FileExtension { get; set; }
        public string OriginalPath { get; set; }
        public DateTime UploadedAt { get; set; }
        public List<ImageVariant> Variants { get; set; } = new();
    }
}
