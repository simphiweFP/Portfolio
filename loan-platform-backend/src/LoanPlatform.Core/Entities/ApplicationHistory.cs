using LoanPlatform.Core.Enums;

namespace LoanPlatform.Core.Entities;

public class ApplicationHistory
{
    public Guid Id { get; set; }
    public Guid LoanApplicationId { get; set; }
    public LoanStatus FromStatus { get; set; }
    public LoanStatus ToStatus { get; set; }
    public string? Notes { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual LoanApplication LoanApplication { get; set; } = null!;
    public virtual User ChangedByUser { get; set; } = null!;
}