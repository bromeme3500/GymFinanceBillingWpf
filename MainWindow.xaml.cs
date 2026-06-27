using System;
using System.Windows;
using System.Windows.Controls;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Views;

namespace GymFinanceBillingWpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        TxtDate.Text = DateTime.Today.ToString("dddd, MMMM dd, yyyy");

        ApplyRoleRestrictions();
        PopulateUserPanel();

        // Load Dashboard by default
        NavigateToDashboard();
    }

    /// <summary>Hides navigation items that the current user is not allowed to access.</summary>
    private void ApplyRoleRestrictions()
    {
        var user = App.CurrentUser;
        if (user == null || user.Role == UserRole.Receptionist)
        {
            // Receptionists cannot access Setup, Expenses or Payroll views
            BtnSetup.Visibility = Visibility.Collapsed;
            BtnExpenses.Visibility = Visibility.Collapsed;
            BtnPayroll.Visibility = Visibility.Collapsed;
        }
        else
        {
            BtnSetup.Visibility = Visibility.Visible;
            BtnExpenses.Visibility = Visibility.Visible;
            BtnPayroll.Visibility = Visibility.Visible;
        }
    }

    /// <summary>Populates the sidebar user panel with the logged-in user's info.</summary>
    private void PopulateUserPanel()
    {
        var user = App.CurrentUser;
        if (user != null)
        {
            TxtUserInitials.Text = user.Initials;
            TxtUserName.Text = user.FullName;
            TxtUserRole.Text = user.Role == UserRole.Admin ? "Administrator" : "Receptionist";
        }
    }

    public void Navigate(UserControl view)
    {
        ContentArea.Content = view;
        if (FindResource("FadeInTransition") is System.Windows.Media.Animation.Storyboard sb)
        {
            sb.Begin(this);
        }
    }

    public void NavigateToDashboard()
    {
        Navigate(new DashboardView());
        HighlightSidebarButton(BtnDashboard);
    }

    public void NavigateToMembers()
    {
        Navigate(new MembersView());
        HighlightSidebarButton(BtnMembers);
    }

    public void NavigateToSetup()
    {
        // Guard — receptionists cannot access setup menu
        if (App.CurrentUser?.Role != UserRole.Admin) return;
        Navigate(new SetupView());
        HighlightSidebarButton(BtnSetup);
    }

    public void NavigateToInvoices()
    {
        Navigate(new InvoicesView());
        HighlightSidebarButton(BtnInvoices);
    }

    public void NavigateToInvoiceEditor(string? invoiceId = null)
    {
        Navigate(new InvoiceEditorView(invoiceId));
        HighlightSidebarButton(BtnInvoices);
    }

    public void NavigateToExpenses()
    {
        // Guard — receptionists cannot access expenses
        if (App.CurrentUser?.Role != UserRole.Admin) return;
        Navigate(new ExpensesView());
        HighlightSidebarButton(BtnExpenses);
    }

    public void NavigateToPayroll()
    {
        // Guard — receptionists cannot access payroll
        if (App.CurrentUser?.Role != UserRole.Admin) return;
        Navigate(new PayrollView());
        HighlightSidebarButton(BtnPayroll);
    }

    public void NavigateToSettings()
    {
        Navigate(new SettingsView());
        HighlightSidebarButton(BtnSettings);
    }

    public void NavigateToAttendance()
    {
        Navigate(new AttendanceView());
        HighlightSidebarButton(BtnAttendance);
    }

    private void HighlightSidebarButton(Button activeBtn)
    {
        BtnDashboard.Opacity = 0.65;
        BtnMembers.Opacity = 0.65;
        BtnAttendance.Opacity = 0.65;
        BtnSetup.Opacity = 0.65;
        BtnInvoices.Opacity = 0.65;
        BtnExpenses.Opacity = 0.65;
        BtnPayroll.Opacity = 0.65;
        BtnSettings.Opacity = 0.65;

        activeBtn.Opacity = 1.0;
    }

    // --- Button Click Events ---
    private void NavigateDashboard(object sender, RoutedEventArgs e) => NavigateToDashboard();
    private void NavigateMembers(object sender, RoutedEventArgs e) => NavigateToMembers();
    private void NavigateAttendance(object sender, RoutedEventArgs e) => NavigateToAttendance();
    private void NavigateSetup(object sender, RoutedEventArgs e) => NavigateToSetup();
    private void NavigateInvoices(object sender, RoutedEventArgs e) => NavigateToInvoices();
    private void NavigateExpenses(object sender, RoutedEventArgs e) => NavigateToExpenses();
    private void NavigatePayroll(object sender, RoutedEventArgs e) => NavigateToPayroll();
    private void NavigateSettings(object sender, RoutedEventArgs e) => NavigateToSettings();

    private void CreateNewInvoiceClick(object sender, RoutedEventArgs e)
    {
        NavigateToInvoiceEditor(null);
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            // Clear the current user session
            App.CurrentUser = null;

            // Open the login window
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close this window
            this.Close();
        }
    }
}