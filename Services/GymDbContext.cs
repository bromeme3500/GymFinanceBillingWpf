using Microsoft.EntityFrameworkCore;
using GymFinanceBillingWpf.Models;

namespace GymFinanceBillingWpf.Services;

public class GymDbContext : DbContext
{
    private readonly string _connectionString;

    public GymDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<MembershipPlan> MembershipPlans { get; set; } = null!;
    public DbSet<MemberMembership> MemberMemberships { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PayrollRecord> PayrollRecords { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use a static server version to allow database creation if the database doesn't exist yet.
        optionsBuilder.UseMySql(_connectionString, new MySqlServerVersion(new Version(8, 0, 30)));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Member)
            .WithMany()
            .HasForeignKey(i => i.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Member>()
            .HasOne(m => m.ActivePlan)
            .WithMany()
            .HasForeignKey(m => m.ActivePlanId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MemberMembership>()
            .HasOne(mm => mm.Member)
            .WithMany()
            .HasForeignKey(mm => mm.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemberMembership>()
            .HasOne(mm => mm.MembershipPlan)
            .WithMany()
            .HasForeignKey(mm => mm.MembershipPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
