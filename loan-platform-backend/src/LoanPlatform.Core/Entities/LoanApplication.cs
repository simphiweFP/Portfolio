using LoanPlatform.Core.Enums;

namespace LoanPlatform.Core.Entities;

public class LoanApplication
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Guid LoanTypeId { get; set; }
    
    // Personal Information
    public decimal RequestedAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public int YearsEmployed { get; set; }
    
    // Application Status
    public LoanStatus Status { get; set; } = LoanStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ReviewerUserId { get; set; }
    public string? ReviewNotes { get; set; }
    
    // Calculated fields
    public decimal? ApprovedAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public decimal? MonthlyPayment { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual User? ReviewerUser { get; set; }
    public virtual LoanType LoanType { get; set; } = null!;
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<ApplicationHistory> History { get; set; } = new List<ApplicationHistory>();
}