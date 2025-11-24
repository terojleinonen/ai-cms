namespace Cms.Domain.Entities;

public class MediaFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
