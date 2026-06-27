using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var service = App.GymService;
            if (service == null) return;

            var isAdmin = App.CurrentUser?.Role == UserRole.Admin;

            // Set welcome message dynamically based on user's name
            TxtWelcome.Text = $"Welcome back, {App.CurrentUser?.FullName ?? "User"}";

            var members = await service.GetAllMembersAsync();
            var invoices = await service.GetAllInvoicesAsync();

            var activeMembers = members.Count(m => m.Status == MemberStatus.Active);

            // Active Members is visible to everyone
            TxtActiveMembers.Text = activeMembers.ToString();

            // Calculate unpaid dues for everyone to display (both Admin and Staff)
            var unpaidDues = invoices
                .Where(i => i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.Overdue)
                .Sum(i => i.Total);

            TxtUnpaidDues.Text = $"₹{unpaidDues:N2}";

            if (isAdmin)
            {
                // Full financial data for admins
                var now = DateTime.Today;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                var expenses = await service.GetAllExpensesAsync();

                var monthlyRevenue = invoices
                    .Where(i => i.Status == InvoiceStatus.Paid && i.IssueDate >= startOfMonth)
                    .Sum(i => i.Total);

                var monthlyExpenses = expenses
                    .Where(exp => exp.Date >= startOfMonth)
                    .Sum(exp => exp.Amount);

                var netProfit = monthlyRevenue - monthlyExpenses;

                TxtMonthlyRevenue.Text = $"₹{monthlyRevenue:N2}";
                TxtMonthlyExpenses.Text = $"₹{monthlyExpenses:N2}";
                TxtNetProfit.Text = netProfit < 0 ? $"-₹{Math.Abs(netProfit):N2}" : $"₹{netProfit:N2}";

                TxtNetProfit.Foreground = netProfit < 0
                    ? (SolidColorBrush)FindResource("StatusOverdue")
                    : (SolidColorBrush)FindResource("Secondary");
            }
            else
            {
                // Hide sensitive sales/financial info (revenue, expenses, net profit)
                BorderRevenue.Visibility = Visibility.Collapsed;
                BorderExpenses.Visibility = Visibility.Collapsed;
                BorderProfit.Visibility = Visibility.Collapsed;
                
                // Keep Active Members and Unpaid Dues visible in a 2-column grid
                BorderDues.Visibility = Visibility.Visible;
                KpiGrid.Columns = 2;

                // Hide the Recent Transactions table
                BorderTransactions.Visibility = Visibility.Collapsed;
                ColTransactions.Width = new GridLength(0);
                
                // Stretch the Upcoming Renewals table to full width
                Grid.SetColumnSpan(BorderRenewals, 2);
                BorderRenewals.Margin = new Thickness(0, 0, 0, 0);
            }

            var renewalsList = members
                .Where(m => m.Status != MemberStatus.Active || m.ActivePlanId == null)
                .Take(10)
                .ToList();
            GridRenewals.ItemsSource = renewalsList;

            var recentInvoices = invoices.Take(10).ToList();
            GridTransactions.ItemsSource = recentInvoices;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
