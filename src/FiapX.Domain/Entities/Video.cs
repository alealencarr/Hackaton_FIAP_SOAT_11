using FiapX.Domain.Enums;

namespace FiapX.Domain.Entities
{
    public class Video
    {
        public Video(Guid id, Guid userId, string originalFileName, string storagePath, 
            VideoStatus status, int? frameCount, string? zipPath, string? errorMessage, 
            DateTime createdAt, DateTime? processedAt)
        {
            Id = id;
            UserId = userId;
            OriginalFileName = originalFileName;
            StoragePath = storagePath;
            Status = status;
            FrameCount = frameCount;
            ZipPath = zipPath;
            ErrorMessage = errorMessage;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
        }

        public Video(Guid userId, string originalFileName, string storagePath)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException("UserId é obrigatório para criar um vídeo.");

            if (string.IsNullOrWhiteSpace(originalFileName))
                throw new ArgumentNullException("O nome do arquivo é obrigatório.");

            if (string.IsNullOrWhiteSpace(storagePath))
                throw new ArgumentNullException("O caminho de armazenamento é obrigatório.");

            Id = Guid.NewGuid();
            UserId = userId;
            OriginalFileName = originalFileName;
            StoragePath = storagePath;
            Status = VideoStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public Video() { }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public VideoStatus Status { get; set; }
        public int? FrameCount { get; set; }
        public string? ZipPath { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public void StartProcessing()
        {
            Status = VideoStatus.Processing;
        }

        public void CompleteProcessing(int frameCount, string zipPath)
        {
            Status = VideoStatus.Completed;
            FrameCount = frameCount;
            ZipPath = zipPath;
            ProcessedAt = DateTime.UtcNow;
        }

        public void FailProcessing(string errorMessage)
        {
            Status = VideoStatus.Failed;
            ErrorMessage = errorMessage.Length > 2000
                ? errorMessage.Substring(0, 2000)
                : errorMessage;
            ProcessedAt = DateTime.UtcNow;
        }
 
    }
}
