using System;

namespace GymFinanceBillingWpf.Models;

public enum MemberStatus
{
    Active,
    Expired,
    Suspended
}

public class Member
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? RegNo { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; } = DateTime.Today;
    public DateTime? RegDate { get; set; }
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public string Notes { get; set; } = string.Empty;
    
    // New fields from the classic registration system
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? PinCode { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public int? Age { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public decimal OpeningAmount { get; set; } = 0.00m;
    
    public string? ActivePlanId { get; set; }
    public MembershipPlan? ActivePlan { get; set; }
}
