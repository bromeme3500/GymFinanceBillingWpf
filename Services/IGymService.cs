using System.Collections.Generic;
using System.Threading.Tasks;
using GymFinanceBillingWpf.Models;

namespace GymFinanceBillingWpf.Services;

public interface IGymService
{
    Task InitializeDatabaseAsync();

    // Members
    Task<List<Member>> GetAllMembersAsync();
    Task<Member?> GetMemberByIdAsync(string id);
    Task SaveMemberAsync(Member member);
    Task DeleteMemberAsync(string id);

    // Plans
    Task<List<MembershipPlan>> GetAllPlansAsync();
    Task<MembershipPlan?> GetPlanByIdAsync(string id);
    Task SavePlanAsync(MembershipPlan plan);
    Task DeletePlanAsync(string id);

    // Invoices
    Task<List<Invoice>> GetAllInvoicesAsync();
    Task<Invoice?> GetInvoiceByIdAsync(string id);
    Task SaveInvoiceAsync(Invoice invoice);
    Task DeleteInvoiceAsync(string id);

    // Expenses
    Task<List<Expense>> GetAllExpensesAsync();
    Task<Expense?> GetExpenseByIdAsync(string id);
    Task SaveExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(string id);

    // Member Memberships
    Task AssignPlanToMemberAsync(string memberId, string planId, decimal amountPaid);
    Task<List<MemberMembership>> GetMemberMembershipsAsync(string memberId);

    // Authentication
    Task<User?> AuthenticateAsync(string username, string password);
    Task<List<User>> GetAllUsersAsync();
    Task SaveUserAsync(User user);
    Task DeleteUserAsync(string id);

    // Payroll
    Task<List<PayrollRecord>> GetAllPayrollRecordsAsync();
    Task<PayrollRecord?> GetPayrollRecordByIdAsync(string id);
    Task SavePayrollRecordAsync(PayrollRecord record);
    Task DeletePayrollRecordAsync(string id);

    // Employees
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(string id);
    Task SaveEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(string id);

    // Attendance
    Task<List<AttendanceRecord>> GetAttendanceRecordsByDateAsync(DateTime date);
    Task<List<AttendanceRecord>> GetActiveSessionsAsync();
    Task<Member?> CheckInMemberAsync(string regNo);
    Task<AttendanceRecord?> CheckOutMemberAsync(string regNo);
    Task SaveAttendanceRecordAsync(AttendanceRecord record);
    Task DeleteAttendanceRecordAsync(string id);
}
