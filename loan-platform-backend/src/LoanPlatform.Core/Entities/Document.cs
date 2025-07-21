using LoanPlatform.Core.Enums;

namespace LoanPlatform.Core.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid LoanApplicationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentType Type { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedByUserId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual LoanApplication LoanApplication { get; set; } = null!;
    public virtual User UploadedByUser { get; set; } = null!;
}