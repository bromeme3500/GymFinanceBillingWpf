using System;

namespace GymFinanceBillingWpf.Models;

public enum PayrollStatus
{
    Paid,
    Pending,
    Draft
}

public class PayrollRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty; // e.g. "June 2026"
    public decimal BasicSalary { get; set; } = 0.00m;
    public decimal Allowance { get; set; } = 0.00m;
    public decimal Deductions { get; set; } = 0.00m;
    public decimal NetSalary => BasicSalary + Allowance - Deductions;
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public PayrollStatus Status { get; set; } = PayrollStatus.Paid;
    public string PaymentMethod { get; set; } = "Cash"; // Cash, UPI, Card, Bank Transfer
    public string? Narration { get; set; }
}
