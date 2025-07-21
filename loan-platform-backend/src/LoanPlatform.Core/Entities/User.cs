using Microsoft.AspNetCore.Identity;

namespace LoanPlatform.Core.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}