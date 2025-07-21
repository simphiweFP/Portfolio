namespace LoanPlatform.Core.Entities;

public class LoanType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal BaseInterestRate { get; set; }
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}