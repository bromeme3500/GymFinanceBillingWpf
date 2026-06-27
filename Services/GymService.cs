using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GymFinanceBillingWpf.Models;

namespace GymFinanceBillingWpf.Services;

public class GymService : IGymService
{
    private readonly GymDbContext _context;

    public GymService(GymDbContext context)
    {
        _context = context;
    }

    public async Task InitializeDatabaseAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        // Migrate the database schema by adding columns if they don't exist
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN RegNo VARCHAR(100) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN AddressLine1 VARCHAR(255) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN AddressLine2 VARCHAR(255) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN AddressLine3 VARCHAR(255) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN PinCode VARCHAR(20) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN Gender VARCHAR(50) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN BloodGroup VARCHAR(20) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN Age INT NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN Height DOUBLE NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN Weight DOUBLE NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN OpeningAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Members ADD COLUMN RegDate DATETIME NULL;"); } catch { }

        // Invoices table updates
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN ServicePeriodStart DATETIME NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN ServicePeriodEnd DATETIME NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN AdmissionFee DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN AdmissionFeeDiscount DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN Narration VARCHAR(500) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN CashAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN UpiAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN CardAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }

        // Legacy receipt fields migration
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN PackageName VARCHAR(100) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN BankName VARCHAR(100) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN TransTypeLeft VARCHAR(50) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN BalanceAdmnFee DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN BalanceFees DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN PartialCollection TINYINT NOT NULL DEFAULT 0;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN TransactionTypeRight VARCHAR(50) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN AccountType VARCHAR(50) NULL;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN TotalAdmFee DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN PendingAdmFee DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN CollectedAdmnFee DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }
        try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Invoices ADD COLUMN CollectedFee DECIMAL(18,2) NOT NULL DEFAULT 0.00;"); } catch { }

        // Create PayrollRecords table if not exists
        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS PayrollRecords (
                    Id VARCHAR(255) PRIMARY KEY,
                    StaffId VARCHAR(255) NOT NULL,
                    StaffName VARCHAR(255) NOT NULL,
                    Role VARCHAR(100) NOT NULL,
                    Month VARCHAR(50) NOT NULL,
                    BasicSalary DECIMAL(18,2) NOT NULL,
                    Allowance DECIMAL(18,2) NOT NULL,
                    Deductions DECIMAL(18,2) NOT NULL,
                    PaymentDate DATETIME NOT NULL,
                    Status INT NOT NULL,
                    PaymentMethod VARCHAR(50) NOT NULL,
                    Narration VARCHAR(500) NULL
                );
            ");
        }
        catch { }

        // Create Employees table if not exists
        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS Employees (
                    Id VARCHAR(255) PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Designation VARCHAR(255) NOT NULL,
                    AddressLine1 VARCHAR(255) NULL,
                    AddressLine2 VARCHAR(255) NULL,
                    AddressLine3 VARCHAR(255) NULL,
                    PinCode VARCHAR(50) NULL,
                    PhoneRes VARCHAR(50) NULL,
                    MobileNo VARCHAR(50) NULL,
                    GuardianName VARCHAR(255) NULL,
                    BasicPay DECIMAL(18,2) NOT NULL,
                    EmployeeCategory VARCHAR(100) NOT NULL,
                    DOB DATETIME NULL,
                    Age INT NOT NULL,
                    JoiningDate DATETIME NULL,
                    IsActive TINYINT NOT NULL DEFAULT 1
                );
            ");
        }
        catch { }

        // Create AttendanceRecords table if not exists
        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS AttendanceRecords (
                    Id VARCHAR(255) PRIMARY KEY,
                    MemberId VARCHAR(255) NOT NULL,
                    Date DATETIME NOT NULL,
                    CheckInTime DATETIME NOT NULL,
                    CheckOutTime DATETIME NULL
                );
            ");
        }
        catch { }

        // Backfill existing members who don't have a RegNo
        try
        {
            var membersWithoutRegNo = await _context.Members.Where(m => m.RegNo == null || m.RegNo == "").ToListAsync();
            if (membersWithoutRegNo.Any())
            {
                int startingRegNo = 4500;
                foreach (var member in membersWithoutRegNo)
                {
                    member.RegNo = startingRegNo.ToString();
                    member.RegDate = member.JoinDate;
                    member.Gender = "Male";
                    startingRegNo++;
                }
                await _context.SaveChangesAsync();
            }
        }
        catch { }

        // Seed Plans if empty
        if (!await _context.MembershipPlans.AnyAsync())
        {
            var plans = new List<MembershipPlan>
            {
                new() { Name = "Monthly Basic", DurationMonths = 1, Price = 49.99m, Description = "Access to gym equipment only." },
                new() { Name = "Quarterly Standard", DurationMonths = 3, Price = 129.99m, Description = "Access to gym equipment and locker room." },
                new() { Name = "Annual VIP", DurationMonths = 12, Price = 399.99m, Description = "Full access, swimming pool, and 5 free personal training sessions." },
                new() { Name = "Cardio & Yoga Elite", DurationMonths = 1, Price = 79.99m, Description = "Access to cardio classes, yoga studio, and steam room." }
            };

            await _context.MembershipPlans.AddRangeAsync(plans);
            await _context.SaveChangesAsync();
        }

        // Seed Members if empty
        if (!await _context.Members.AnyAsync())
        {
            var plans = await _context.MembershipPlans.ToListAsync();
            var basicPlan = plans.FirstOrDefault(p => p.Name == "Monthly Basic");
            var annualPlan = plans.FirstOrDefault(p => p.Name == "Annual VIP");
            var cardioPlan = plans.FirstOrDefault(p => p.Name == "Cardio & Yoga Elite");

            var members = new List<Member>
            {
                new() { RegNo = "4500", FullName = "Liam Gallagher", Email = "liam@oasis.com", Phone = "555-0101", JoinDate = DateTime.Today.AddMonths(-6), RegDate = DateTime.Today.AddMonths(-6), Status = MemberStatus.Active, ActivePlanId = annualPlan?.Id, ActivePlan = annualPlan, AddressLine1 = "12 Oasis Rd", PinCode = "123456", Gender = "Male", BloodGroup = "O+", Age = 32, Height = 178, Weight = 75, OpeningAmount = 0.00m },
                new() { RegNo = "4501", FullName = "Noel Gallagher", Email = "noel@oasis.com", Phone = "555-0102", JoinDate = DateTime.Today.AddMonths(-3), RegDate = DateTime.Today.AddMonths(-3), Status = MemberStatus.Active, ActivePlanId = basicPlan?.Id, ActivePlan = basicPlan, AddressLine1 = "14 Oasis Rd", PinCode = "123456", Gender = "Male", BloodGroup = "A+", Age = 35, Height = 175, Weight = 72, OpeningAmount = 0.00m },
                new() { RegNo = "4502", FullName = "Damon Albarn", Email = "damon@gorillaz.com", Phone = "555-0202", JoinDate = DateTime.Today.AddMonths(-12), RegDate = DateTime.Today.AddMonths(-12), Status = MemberStatus.Expired, ActivePlanId = null, AddressLine1 = "22 Gorillaz Ave", PinCode = "654321", Gender = "Male", BloodGroup = "B-", Age = 33, Height = 180, Weight = 70, OpeningAmount = 0.00m },
                new() { RegNo = "4503", FullName = "Alex James", Email = "alex@blur.co.uk", Phone = "555-0303", JoinDate = DateTime.Today.AddMonths(-2), RegDate = DateTime.Today.AddMonths(-2), Status = MemberStatus.Suspended, ActivePlanId = null, AddressLine1 = "33 Blur Blvd", PinCode = "112233", Gender = "Male", BloodGroup = "AB+", Age = 30, Height = 182, Weight = 78, OpeningAmount = 0.00m },
                new() { RegNo = "4504", FullName = "Jarvis Cocker", Email = "jarvis@pulp.com", Phone = "555-0404", JoinDate = DateTime.Today.AddMonths(-1), RegDate = DateTime.Today.AddMonths(-1), Status = MemberStatus.Active, ActivePlanId = cardioPlan?.Id, ActivePlan = cardioPlan, AddressLine1 = "44 Pulp St", PinCode = "443322", Gender = "Male", BloodGroup = "O-", Age = 34, Height = 185, Weight = 68, OpeningAmount = 0.00m }
            };

            await _context.Members.AddRangeAsync(members);
            await _context.SaveChangesAsync();

            // Seed Member Memberships
            var memberMemberships = new List<MemberMembership>();
            if (basicPlan != null && members[1].Id != null)
            {
                memberMemberships.Add(new MemberMembership
                {
                    MemberId = members[1].Id,
                    MembershipPlanId = basicPlan.Id,
                    StartDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today,
                    AmountPaid = basicPlan.Price
                });
            }
            if (annualPlan != null && members[0].Id != null)
            {
                memberMemberships.Add(new MemberMembership
                {
                    MemberId = members[0].Id,
                    MembershipPlanId = annualPlan.Id,
                    StartDate = DateTime.Today.AddMonths(-3),
                    EndDate = DateTime.Today.AddMonths(9),
                    AmountPaid = annualPlan.Price
                });
            }
            if (cardioPlan != null && members[4].Id != null)
            {
                memberMemberships.Add(new MemberMembership
                {
                    MemberId = members[4].Id,
                    MembershipPlanId = cardioPlan.Id,
                    StartDate = DateTime.Today.AddDays(-15),
                    EndDate = DateTime.Today.AddDays(15),
                    AmountPaid = cardioPlan.Price
                });
            }
            await _context.MemberMemberships.AddRangeAsync(memberMemberships);
            await _context.SaveChangesAsync();
        }

        // Seed Invoices if empty
        if (!await _context.Invoices.AnyAsync())
        {
            var members = await _context.Members.ToListAsync();
            var plans = await _context.MembershipPlans.ToListAsync();
            var basicPlan = plans.FirstOrDefault(p => p.Name == "Monthly Basic");
            var annualPlan = plans.FirstOrDefault(p => p.Name == "Annual VIP");
            var cardioPlan = plans.FirstOrDefault(p => p.Name == "Cardio & Yoga Elite");

            var invoices = new List<Invoice>();

            if (members.Count >= 3)
            {
                // Paid Annual
                var inv1 = new Invoice
                {
                    InvoiceNumber = "INV-2026-001",
                    MemberId = members[0].Id,
                    IssueDate = DateTime.Today.AddMonths(-3),
                    DueDate = DateTime.Today.AddMonths(-3).AddDays(7),
                    Status = InvoiceStatus.Paid,
                    PaymentMethod = "UPI"
                };
                inv1.Items.Add(new InvoiceItem { Description = "Annual VIP Membership Renewal", Quantity = 1, UnitPrice = annualPlan?.Price ?? 399.99m });
                invoices.Add(inv1);

                // Unpaid Basic
                var inv2 = new Invoice
                {
                    InvoiceNumber = "INV-2026-002",
                    MemberId = members[1].Id,
                    IssueDate = DateTime.Today.AddDays(-5),
                    DueDate = DateTime.Today.AddDays(2),
                    Status = InvoiceStatus.Unpaid,
                    PaymentMethod = "None"
                };
                inv2.Items.Add(new InvoiceItem { Description = "Monthly Basic Membership Renewal", Quantity = 1, UnitPrice = basicPlan?.Price ?? 49.99m });
                invoices.Add(inv2);

                // Overdue
                var inv3 = new Invoice
                {
                    InvoiceNumber = "INV-2026-003",
                    MemberId = members[2].Id,
                    IssueDate = DateTime.Today.AddDays(-30),
                    DueDate = DateTime.Today.AddDays(-23),
                    Status = InvoiceStatus.Overdue,
                    PaymentMethod = "None"
                };
                inv3.Items.Add(new InvoiceItem { Description = "Cardio & Yoga Elite Membership Renewal", Quantity = 1, UnitPrice = cardioPlan?.Price ?? 79.99m });
                invoices.Add(inv3);
            }

            await _context.Invoices.AddRangeAsync(invoices);
            await _context.SaveChangesAsync();
        }

        // Seed Expenses if empty
        if (!await _context.Expenses.AnyAsync())
        {
            var expenses = new List<Expense>
            {
                new() { Description = "Rent Payment June", Amount = 1200m, Date = DateTime.Today.AddDays(-10), Category = "Rent", Notes = "Monthly space lease" },
                new() { Description = "Electricity Bill", Amount = 345.50m, Date = DateTime.Today.AddDays(-8), Category = "Utilities", Notes = "AC load heavy this month" },
                new() { Description = "Gym Instructor Salary", Amount = 800m, Date = DateTime.Today.AddDays(-12), Category = "Salaries", Notes = "Paid to John Trainer" },
                new() { Description = "Yoga Mat Replacement", Amount = 150m, Date = DateTime.Today.AddDays(-2), Category = "Equipment", Notes = "Replaced worn out mats" }
            };

            await _context.Expenses.AddRangeAsync(expenses);
            await _context.SaveChangesAsync();
        }

        // Seed Users if empty
        if (!await _context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new()
                {
                    Username = "admin",
                    PasswordHash = HashPassword("admin"),
                    FullName = "Gym Admin",
                    Role = UserRole.Admin,
                    IsActive = true
                },
                new()
                {
                    Username = "staff",
                    PasswordHash = HashPassword("staff"),
                    FullName = "Staff Member",
                    Role = UserRole.Receptionist,
                    IsActive = true
                }
            };
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }
    }

    // Members CRUD
    public async Task<List<Member>> GetAllMembersAsync()
    {
        return await _context.Members
            .Include(m => m.ActivePlan)
            .OrderBy(m => m.FullName)
            .ToListAsync();
    }

    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await _context.Members
            .Include(m => m.ActivePlan)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task SaveMemberAsync(Member member)
    {
        var existing = await _context.Members.FindAsync(member.Id);
        if (existing == null)
        {
            await _context.Members.AddAsync(member);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(member);
            existing.ActivePlanId = member.ActivePlanId;
            existing.ActivePlan = member.ActivePlan;
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMemberAsync(string id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member != null)
        {
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
        }
    }

    // Plans CRUD
    public async Task<List<MembershipPlan>> GetAllPlansAsync()
    {
        return await _context.MembershipPlans
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<MembershipPlan?> GetPlanByIdAsync(string id)
    {
        return await _context.MembershipPlans.FindAsync(id);
    }

    public async Task SavePlanAsync(MembershipPlan plan)
    {
        var existing = await _context.MembershipPlans.FindAsync(plan.Id);
        if (existing == null)
        {
            await _context.MembershipPlans.AddAsync(plan);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(plan);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeletePlanAsync(string id)
    {
        var plan = await _context.MembershipPlans.FindAsync(id);
        if (plan != null)
        {
            // Set member active plan IDs to null if they belong to this plan
            var membersWithPlan = await _context.Members.Where(m => m.ActivePlanId == id).ToListAsync();
            foreach (var m in membersWithPlan)
            {
                m.ActivePlanId = null;
                m.ActivePlan = null;
            }
            _context.MembershipPlans.Remove(plan);
            await _context.SaveChangesAsync();
        }
    }

    // Invoices CRUD
    public async Task<List<Invoice>> GetAllInvoicesAsync()
    {
        return await _context.Invoices
            .Include(i => i.Member)
            .Include(i => i.Items)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(string id)
    {
        return await _context.Invoices
            .Include(i => i.Member)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task SaveInvoiceAsync(Invoice invoice)
    {
        var existing = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);

        if (existing == null)
        {
            await _context.Invoices.AddAsync(invoice);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(invoice);
            
            // Remove deleted items
            foreach (var item in existing.Items.ToList())
            {
                if (!invoice.Items.Any(i => i.Id == item.Id))
                {
                    _context.InvoiceItems.Remove(item);
                }
            }

            // Update or add items
            foreach (var item in invoice.Items)
            {
                var existingItem = existing.Items.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    _context.Entry(existingItem).CurrentValues.SetValues(item);
                }
                else
                {
                    existing.Items.Add(item);
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInvoiceAsync(string id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice != null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }
    }

    // Expenses CRUD
    public async Task<List<Expense>> GetAllExpensesAsync()
    {
        return await _context.Expenses
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<Expense?> GetExpenseByIdAsync(string id)
    {
        return await _context.Expenses.FindAsync(id);
    }

    public async Task SaveExpenseAsync(Expense expense)
    {
        var existing = await _context.Expenses.FindAsync(expense.Id);
        if (existing == null)
        {
            await _context.Expenses.AddAsync(expense);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(expense);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpenseAsync(string id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense != null)
        {
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
        }
    }

    // Member Memberships
    public async Task AssignPlanToMemberAsync(string memberId, string planId, decimal amountPaid)
    {
        var member = await _context.Members.FindAsync(memberId);
        var plan = await _context.MembershipPlans.FindAsync(planId);

        if (member != null && plan != null)
        {
            member.ActivePlanId = plan.Id;
            member.ActivePlan = plan;
            member.Status = MemberStatus.Active;

            var startDate = DateTime.Today;
            var endDate = startDate.AddMonths(plan.DurationMonths);

            var mm = new MemberMembership
            {
                MemberId = memberId,
                MembershipPlanId = planId,
                StartDate = startDate,
                EndDate = endDate,
                AmountPaid = amountPaid
            };

            await _context.MemberMemberships.AddAsync(mm);

            // Generate an Invoice for this assignment
            var invoiceCount = await _context.Invoices.CountAsync();
            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-2026-{(invoiceCount + 1):D3}",
                MemberId = memberId,
                IssueDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7),
                Status = amountPaid >= plan.Price ? InvoiceStatus.Paid : InvoiceStatus.Unpaid,
                PaymentMethod = amountPaid >= plan.Price ? "UPI" : "None"
            };

            invoice.Items.Add(new InvoiceItem
            {
                Description = $"{plan.Name} Membership Plan Registration",
                Quantity = 1,
                UnitPrice = plan.Price
            });

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<MemberMembership>> GetMemberMembershipsAsync(string memberId)
    {
        return await _context.MemberMemberships
            .Include(mm => mm.MembershipPlan)
            .Where(mm => mm.MemberId == memberId)
            .OrderByDescending(mm => mm.StartDate)
            .ToListAsync();
    }

    // ── Auth ──────────────────────────────────────────────────────────────

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var hash = HashPassword(password);
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == username &&
                u.PasswordHash == hash &&
                u.IsActive);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync();
    }

    public async Task SaveUserAsync(User user)
    {
        var existing = await _context.Users.FindAsync(user.Id);
        if (existing == null)
            await _context.Users.AddAsync(user);
        else
            _context.Entry(existing).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    // ── Payroll ───────────────────────────────────────────────────────────

    public async Task<List<PayrollRecord>> GetAllPayrollRecordsAsync()
    {
        return await _context.PayrollRecords
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<PayrollRecord?> GetPayrollRecordByIdAsync(string id)
    {
        return await _context.PayrollRecords.FindAsync(id);
    }

    public async Task SavePayrollRecordAsync(PayrollRecord record)
    {
        var existing = await _context.PayrollRecords.FindAsync(record.Id);
        if (existing == null)
            await _context.PayrollRecords.AddAsync(record);
        else
            _context.Entry(existing).CurrentValues.SetValues(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePayrollRecordAsync(string id)
    {
        var record = await _context.PayrollRecords.FindAsync(id);
        if (record != null)
        {
            _context.PayrollRecords.Remove(record);
            await _context.SaveChangesAsync();
        }
    }

    // ── Employees ─────────────────────────────────────────────────────────

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(string id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task SaveEmployeeAsync(Employee employee)
    {
        var existing = await _context.Employees.FindAsync(employee.Id);
        if (existing == null)
            await _context.Employees.AddAsync(employee);
        else
            _context.Entry(existing).CurrentValues.SetValues(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEmployeeAsync(string id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
    }

    // ── Attendance ────────────────────────────────────────────────────────
    
    public async Task<List<AttendanceRecord>> GetAttendanceRecordsByDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return await _context.AttendanceRecords
            .Include(ar => ar.Member)
            .Where(ar => ar.Date == targetDate)
            .OrderByDescending(ar => ar.CheckInTime)
            .ToListAsync();
    }

    public async Task<List<AttendanceRecord>> GetActiveSessionsAsync()
    {
        return await _context.AttendanceRecords
            .Include(ar => ar.Member)
            .Where(ar => ar.CheckOutTime == null)
            .OrderByDescending(ar => ar.CheckInTime)
            .ToListAsync();
    }

    public async Task<Member?> CheckInMemberAsync(string regNo)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.RegNo == regNo);
        if (member == null) return null;

        // Check if already checked in today (and not checked out)
        var activeSession = await _context.AttendanceRecords
            .FirstOrDefaultAsync(ar => ar.MemberId == member.Id && ar.CheckOutTime == null);

        if (activeSession != null)
        {
            throw new InvalidOperationException("Member is already checked in.");
        }

        var record = new AttendanceRecord
        {
            MemberId = member.Id,
            Member = member,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now,
            CheckOutTime = null
        };

        await _context.AttendanceRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<AttendanceRecord?> CheckOutMemberAsync(string regNo)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.RegNo == regNo);
        if (member == null) return null;

        var activeSession = await _context.AttendanceRecords
            .Include(ar => ar.Member)
            .FirstOrDefaultAsync(ar => ar.MemberId == member.Id && ar.CheckOutTime == null);

        if (activeSession == null)
        {
            throw new InvalidOperationException("Member is not currently checked in.");
        }

        activeSession.CheckOutTime = DateTime.Now;
        await _context.SaveChangesAsync();
        return activeSession;
    }

    public async Task SaveAttendanceRecordAsync(AttendanceRecord record)
    {
        var existing = await _context.AttendanceRecords.FindAsync(record.Id);
        if (existing == null)
            await _context.AttendanceRecords.AddAsync(record);
        else
            _context.Entry(existing).CurrentValues.SetValues(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAttendanceRecordAsync(string id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record != null)
        {
            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();
        }
    }
}
