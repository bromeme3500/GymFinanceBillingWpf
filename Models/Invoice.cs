using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFinanceBillingWpf.Models;

public enum InvoiceStatus
{
    Paid,
    Unpaid,
    Overdue
}

public class Invoice
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InvoiceNumber { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public Member? Member { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public string PaymentMethod { get; set; } = "None"; // Cash, Card, UPI, Bank Transfer, None
    public List<InvoiceItem> Items { get; set; } = new();

    // New Receipts & Split Payments Fields
    public DateTime? ServicePeriodStart { get; set; } = DateTime.Today;
    public DateTime? ServicePeriodEnd { get; set; } = DateTime.Today.AddMonths(1);
    public decimal AdmissionFee { get; set; } = 0.00m;
    public decimal AdmissionFeeDiscount { get; set; } = 0.00m;
    public string? Narration { get; set; }
    public decimal CashAmount { get; set; } = 0.00m;
    public decimal UpiAmount { get; set; } = 0.00m;
    public decimal CardAmount { get; set; } = 0.00m;

    // Legacy screen fields
    public string? PackageName { get; set; }
    public string? BankName { get; set; }
    public string? TransTypeLeft { get; set; }
    public decimal BalanceAdmnFee { get; set; } = 0.00m;
    public decimal BalanceFees { get; set; } = 0.00m;
    public bool PartialCollection { get; set; } = false;
    public string? TransactionTypeRight { get; set; } = "Subscription";
    public string? AccountType { get; set; } = "Cash";
    public decimal TotalAdmFee { get; set; } = 0.00m;
    public decimal TotalAmount { get; set; } = 0.00m;
    public decimal PendingAdmFee { get; set; } = 0.00m;
    public decimal CollectedAdmnFee { get; set; } = 0.00m;
    public decimal CollectedFee { get; set; } = 0.00m;

    public decimal Total => Items.Sum(i => i.TotalPrice);
    public decimal GrandTotal => Total + AdmissionFee - AdmissionFeeDiscount;
    public decimal AmountPaid => CashAmount + UpiAmount + CardAmount;
    public decimal PendingAmount => GrandTotal - AmountPaid;
}
