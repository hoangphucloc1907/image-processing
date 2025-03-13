namespace ImageProcessing
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string SecurityProtocol { get; set; } = "SaslSsl";
        public string SaslMechanism { get; set; } = "PLAIN";
        public string SaslUsername { get; set; } = string.Empty;
        public string SaslPassword { get; set; } = string.Empty;
    }
}
