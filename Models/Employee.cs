using System;

namespace GymFinanceBillingWpf.Models;

public class Employee
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string AddressLine3 { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string PhoneRes { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string GuardianName { get; set; } = string.Empty;
    public decimal BasicPay { get; set; } = 0.00m;
    public string EmployeeCategory { get; set; } = "Trainer"; // Trainer, Front Desk, Manager, Cleaner, etc.
    public DateTime? DOB { get; set; } = DateTime.Today.AddYears(-25);
    public int Age { get; set; } = 25;
    public DateTime? JoiningDate { get; set; } = DateTime.Today;
    public bool IsActive { get; set; } = true;
}
