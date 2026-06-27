using System;

namespace GymFinanceBillingWpf.Models;

public class Expense
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Category { get; set; } = "Other"; // Utilities, Rent, Salaries, Equipment, Marketing, Other
    public string Notes { get; set; } = string.Empty;
}
