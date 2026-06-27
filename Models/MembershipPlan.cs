using System;

namespace GymFinanceBillingWpf.Models;

public class MembershipPlan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;

    public override string ToString() => $"{Name} (₹{Price:F2} / {DurationMonths} mo)";
}
