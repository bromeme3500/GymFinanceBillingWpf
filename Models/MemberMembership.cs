using System;

namespace GymFinanceBillingWpf.Models;

public class MemberMembership
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MemberId { get; set; } = string.Empty;
    public Member? Member { get; set; }
    
    public string MembershipPlanId { get; set; } = string.Empty;
    public MembershipPlan? MembershipPlan { get; set; }
    
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);
    public decimal AmountPaid { get; set; }
    
    public bool IsActive => DateTime.Today >= StartDate && DateTime.Today <= EndDate;
}
