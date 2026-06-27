using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class PayrollView : UserControl
{
    private List<PayrollRecord> _allRecords = new();
    private List<Employee> _employees = new();
    private PayrollRecord? _selectedRecord;
    private bool _isUpdatingCalculations = false;

    public PayrollView()
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

            // Load staff members from Employees table
            _employees = await service.GetAllEmployeesAsync() ?? new List<Employee>();
            if (ComboStaff != null)
            {
                ComboStaff.ItemsSource = _employees;
                ComboStaff.DisplayMemberPath = "Name";
            }

            // Load months dropdown
            var months = new List<string>();
            var today = DateTime.Today;
            for (int i = -3; i <= 1; i++)
            {
                months.Add(today.AddMonths(i).ToString("MMMM yyyy"));
            }
            
            if (ComboMonth != null)
            {
                ComboMonth.ItemsSource = months;
                ComboMonth.SelectedItem = today.ToString("MMMM yyyy");
            }

            // Month filter dropdown
            var filterMonths = new List<string> { "All Months" };
            filterMonths.AddRange(months);
            
            if (ComboMonthFilter != null)
            {
                ComboMonthFilter.ItemsSource = filterMonths;
                ComboMonthFilter.SelectedIndex = 0;
            }

            // Status dropdown
            if (ComboStatus != null)
            {
                ComboStatus.ItemsSource = Enum.GetValues(typeof(PayrollStatus));
                ComboStatus.SelectedItem = PayrollStatus.Paid;
            }

            await RefreshPayrollListAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing payroll screen: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshPayrollListAsync()
    {
        try
        {
            var service = App.GymService;
            if (service == null) return;

            _allRecords = await service.GetAllPayrollRecordsAsync() ?? new List<PayrollRecord>();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading payroll records: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (TxtSearch == null || ComboMonthFilter == null || GridPayroll == null) return;

        var searchText = TxtSearch.Text?.Trim().ToLower() ?? string.Empty;
        var selectedMonth = ComboMonthFilter.SelectedItem as string;

        var filtered = _allRecords.AsEnumerable();

        if (!string.IsNullOrEmpty(searchText))
        {
            filtered = filtered.Where(p => p.StaffName != null && p.StaffName.ToLower().Contains(searchText));
        }

        if (!string.IsNullOrEmpty(selectedMonth) && selectedMonth != "All Months")
        {
            filtered = filtered.Where(p => p.Month == selectedMonth);
        }

        GridPayroll.ItemsSource = filtered.ToList();
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ComboMonthFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        if (TxtSearch != null) TxtSearch.Text = string.Empty;
        if (ComboMonthFilter != null) ComboMonthFilter.SelectedIndex = 0;
        ApplyFilter();
    }

    private void Calculations_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateNetSalary();
    }

    private void UpdateNetSalary()
    {
        if (_isUpdatingCalculations) return;
        _isUpdatingCalculations = true;

        try
        {
            if (TxtBasicSalary == null || TxtAllowance == null || TxtDeduction == null || TxtNetSalary == null) return;

            decimal.TryParse(TxtBasicSalary.Text.Trim(), out decimal basic);
            decimal.TryParse(TxtAllowance.Text.Trim(), out decimal allowance);
            decimal.TryParse(TxtDeduction.Text.Trim(), out decimal deduction);

            decimal net = basic + allowance - deduction;
            TxtNetSalary.Text = net.ToString("F2");
        }
        finally
        {
            _isUpdatingCalculations = false;
        }
    }

    private void ComboStaff_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboStaff.SelectedItem is Employee emp)
        {
            TxtBasicSalary.Text = emp.BasicPay.ToString("F2");
            UpdateNetSalary();
        }
    }

    private void GridPayroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GridPayroll == null || TxtPanelTitle == null || ComboStaff == null || ComboMonth == null ||
            TxtBasicSalary == null || TxtAllowance == null || TxtDeduction == null || DpPaymentDate == null ||
            ComboStatus == null || TxtNarration == null || ComboPaymentMethod == null || BtnDelete == null) return;

        _selectedRecord = GridPayroll.SelectedItem as PayrollRecord;
        if (_selectedRecord != null)
        {
            TxtPanelTitle.Text = "Edit Staff Payment";
            
            var emp = _employees.FirstOrDefault(e => e.Id == _selectedRecord.StaffId);
            ComboStaff.SelectedItem = emp;

            ComboMonth.SelectedItem = _selectedRecord.Month;
            TxtBasicSalary.Text = _selectedRecord.BasicSalary.ToString("F2");
            TxtAllowance.Text = _selectedRecord.Allowance.ToString("F2");
            TxtDeduction.Text = _selectedRecord.Deductions.ToString("F2");
            DpPaymentDate.SelectedDate = _selectedRecord.PaymentDate;
            ComboStatus.SelectedItem = _selectedRecord.Status;
            TxtNarration.Text = _selectedRecord.Narration ?? "";

            // Select payment mode
            foreach (ComboBoxItem item in ComboPaymentMethod.Items)
            {
                if (item != null && item.Content != null && item.Content.ToString() == _selectedRecord.PaymentMethod)
                {
                    ComboPaymentMethod.SelectedItem = item;
                    break;
                }
            }

            BtnDelete.Visibility = Visibility.Visible;
            UpdateNetSalary();
        }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (ComboStaff == null || ComboMonth == null || TxtBasicSalary == null || TxtAllowance == null ||
            TxtDeduction == null || DpPaymentDate == null || ComboStatus == null || ComboPaymentMethod == null ||
            TxtNarration == null) return;

        var selectedEmp = ComboStaff.SelectedItem as Employee;
        if (selectedEmp == null)
        {
            MessageBox.Show("Please select an employee / staff member.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var month = ComboMonth.SelectedItem as string;
        if (string.IsNullOrEmpty(month))
        {
            MessageBox.Show("Please select a salary month.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var service = App.GymService;
            if (service == null) return;

            var recordToSave = _selectedRecord ?? new PayrollRecord();

            decimal.TryParse(TxtBasicSalary.Text.Trim(), out decimal basic);
            decimal.TryParse(TxtAllowance.Text.Trim(), out decimal allowance);
            decimal.TryParse(TxtDeduction.Text.Trim(), out decimal deduction);

            recordToSave.StaffId = selectedEmp.Id;
            recordToSave.StaffName = selectedEmp.Name;
            recordToSave.Role = selectedEmp.Designation;
            recordToSave.Month = month;
            recordToSave.BasicSalary = basic;
            recordToSave.Allowance = allowance;
            recordToSave.Deductions = deduction;
            recordToSave.PaymentDate = DpPaymentDate.SelectedDate ?? DateTime.Today;
            recordToSave.Status = (PayrollStatus)(ComboStatus.SelectedItem ?? PayrollStatus.Paid);
            recordToSave.PaymentMethod = (ComboPaymentMethod.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Cash";
            recordToSave.Narration = TxtNarration.Text.Trim();

            await service.SavePayrollRecordAsync(recordToSave);
            MessageBox.Show("Payroll record saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            await RefreshPayrollListAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving payroll record: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRecord == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete the payroll record for {_selectedRecord.StaffName} ({_selectedRecord.Month})?",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var service = App.GymService;
                if (service == null) return;

                await service.DeletePayrollRecordAsync(_selectedRecord.Id);
                MessageBox.Show("Payroll record deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                await RefreshPayrollListAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payroll record: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ClearForm()
    {
        _selectedRecord = null;
        if (TxtPanelTitle != null) TxtPanelTitle.Text = "Record Staff Payment";
        if (ComboStaff != null) ComboStaff.SelectedIndex = -1;
        if (ComboMonth != null) ComboMonth.SelectedItem = DateTime.Today.ToString("MMMM yyyy");
        if (TxtBasicSalary != null) TxtBasicSalary.Text = "0.00";
        if (TxtAllowance != null) TxtAllowance.Text = "0.00";
        if (TxtDeduction != null) TxtDeduction.Text = "0.00";
        if (TxtNetSalary != null) TxtNetSalary.Text = "0.00";
        if (DpPaymentDate != null) DpPaymentDate.SelectedDate = DateTime.Today;
        if (ComboPaymentMethod != null) ComboPaymentMethod.SelectedIndex = 0;
        if (ComboStatus != null) ComboStatus.SelectedItem = PayrollStatus.Paid;
        if (TxtNarration != null) TxtNarration.Text = string.Empty;
        if (BtnDelete != null) BtnDelete.Visibility = Visibility.Collapsed;
        if (GridPayroll != null) GridPayroll.SelectedItem = null;
    }
}
