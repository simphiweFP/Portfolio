using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanPlatform.Infrastructure.Data;
using LoanPlatform.Core.Entities;
using LoanPlatform.Core.Enums;
using System.Security.Claims;

namespace LoanPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoanApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LoanApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] string? status = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        IQueryable<LoanApplication> query = _context.LoanApplications
            .Include(la => la.User)
            .Include(la => la.LoanType)
            .Include(la => la.Documents);

        // Filter based on user role
        if (!userRoles.Contains("Admin") && !userRoles.Contains("LoanOfficer"))
        {
            query = query.Where(la => la.UserId == userId);
        }

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LoanStatus>(status, true, out var statusEnum))
        {
            query = query.Where(la => la.Status == statusEnum);
        }

        var applications = await query
            .OrderByDescending(la => la.CreatedAt)
            .Select(la => new
            {
                la.Id,
                la.ApplicationNumber,
                la.RequestedAmount,
                la.Status,
                la.CreatedAt,
                la.SubmittedAt,
                LoanType = la.LoanType.Name,
                ApplicantName = $"{la.User.FirstName} {la.User.LastName}",
                DocumentCount = la.Documents.Count
            })
            .ToListAsync();

        return Ok(applications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var application = await _context.LoanApplications
            .Include(la => la.User)
            .Include(la => la.LoanType)
            .Include(la => la.Documents)
            .Include(la => la.History)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(la => la.Id == id);

        if (application == null)
        {
            return NotFound();
        }

        // Check authorization
        if (!userRoles.Contains("Admin") && !userRoles.Contains("LoanOfficer") && application.UserId != userId)
        {
            return Forbid();
        }

        return Ok(application);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] CreateLoanApplicationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var application = new LoanApplication
        {
            Id = Guid.NewGuid(),
            ApplicationNumber = GenerateApplicationNumber(),
            UserId = userId!,
            LoanTypeId = request.LoanTypeId,
            RequestedAmount = request.RequestedAmount,
            LoanTermMonths = request.LoanTermMonths,
            Purpose = request.Purpose,
            MonthlyIncome = request.MonthlyIncome,
            MonthlyExpenses = request.MonthlyExpenses,
            EmploymentStatus = request.EmploymentStatus,
            EmployerName = request.EmployerName,
            YearsEmployed = request.YearsEmployed,
            Status = LoanStatus.Draft
        };

        _context.LoanApplications.Add(application);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] UpdateLoanApplicationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var application = await _context.LoanApplications.FindAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        // Only allow updates by the owner or admin/loan officer
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (application.UserId != userId && !userRoles.Contains("Admin") && !userRoles.Contains("LoanOfficer"))
        {
            return Forbid();
        }

        // Update fields
        application.RequestedAmount = request.RequestedAmount;
        application.LoanTermMonths = request.LoanTermMonths;
        application.Purpose = request.Purpose;
        application.MonthlyIncome = request.MonthlyIncome;
        application.MonthlyExpenses = request.MonthlyExpenses;
        application.EmploymentStatus = request.EmploymentStatus;
        application.EmployerName = request.EmployerName;
        application.YearsEmployed = request.YearsEmployed;

        await _context.SaveChangesAsync();
        return Ok(application);
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitApplication(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var application = await _context.LoanApplications.FindAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        if (application.UserId != userId)
        {
            return Forbid();
        }

        if (application.Status != LoanStatus.Draft)
        {
            return BadRequest("Application can only be submitted from Draft status");
        }

        application.Status = LoanStatus.Submitted;
        application.SubmittedAt = DateTime.UtcNow;

        // Add history entry
        var history = new ApplicationHistory
        {
            Id = Guid.NewGuid(),
            LoanApplicationId = id,
            FromStatus = LoanStatus.Draft,
            ToStatus = LoanStatus.Submitted,
            Notes = "Application submitted by applicant",
            ChangedByUserId = userId!
        };

        _context.ApplicationHistories.Add(history);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Application submitted successfully" });
    }

    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin,LoanOfficer,Reviewer")]
    public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewApplicationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var application = await _context.LoanApplications.FindAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var oldStatus = application.Status;
        application.Status = request.Status;
        application.ReviewerUserId = userId;
        application.ReviewedAt = DateTime.UtcNow;
        application.ReviewNotes = request.Notes;

        if (request.Status == LoanStatus.Approved)
        {
            application.ApprovedAt = DateTime.UtcNow;
            application.ApprovedAmount = request.ApprovedAmount;
            application.InterestRate = request.InterestRate;
            application.MonthlyPayment = CalculateMonthlyPayment(
                request.ApprovedAmount ?? application.RequestedAmount,
                request.InterestRate ?? 0,
                application.LoanTermMonths
            );
        }

        // Add history entry
        var history = new ApplicationHistory
        {
            Id = Guid.NewGuid(),
            LoanApplicationId = id,
            FromStatus = oldStatus,
            ToStatus = request.Status,
            Notes = request.Notes,
            ChangedByUserId = userId!
        };

        _context.ApplicationHistories.Add(history);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Application reviewed successfully" });
    }

    private static string GenerateApplicationNumber()
    {
        return $"LA{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
    }

    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths)
    {
        if (annualRate == 0) return principal / termMonths;
        
        var monthlyRate = annualRate / 100 / 12;
        var payment = principal * (monthlyRate * Math.Pow(1 + (double)monthlyRate, termMonths)) /
                     (Math.Pow(1 + (double)monthlyRate, termMonths) - 1);
        
        return Math.Round((decimal)payment, 2);
    }
}

public record CreateLoanApplicationRequest(
    Guid LoanTypeId,
    decimal RequestedAmount,
    int LoanTermMonths,
    string Purpose,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    string EmploymentStatus,
    string EmployerName,
    int YearsEmployed
);

public record UpdateLoanApplicationRequest(
    decimal RequestedAmount,
    int LoanTermMonths,
    string Purpose,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    string EmploymentStatus,
    string EmployerName,
    int YearsEmployed
);

public record ReviewApplicationRequest(
    LoanStatus Status,
    string? Notes,
    decimal? ApprovedAmount,
    decimal? InterestRate
);