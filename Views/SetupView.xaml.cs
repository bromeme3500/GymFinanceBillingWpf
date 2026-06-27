using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class SetupView : UserControl
{
    private List<MembershipPlan> _plans = new();
    private MembershipPlan? _selectedPlan;

    private List<Employee> _employees = new();
    private Employee? _selectedEmployee;

    public SetupView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await RefreshPlansListAsync();
        await RefreshEmployeesListAsync();
    }

    #region Membership Plans Logic

    private async Task RefreshPlansListAsync()
    {
        try
        {
            var service = App.GymService;
            _plans = await service.GetAllPlansAsync();
            GridPlans.ItemsSource = null;
            GridPlans.ItemsSource = _plans;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading plans: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GridPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GridPlans == null || TxtPlanTitle == null || TxtPlanName == null || 
            TxtPlanDuration == null || TxtPlanPrice == null || TxtPlanDescription == null || 
            BtnDeletePlan == null) return;

        _selectedPlan = GridPlans.SelectedItem as MembershipPlan;
        if (_selectedPlan != null)
        {
            TxtPlanTitle.Text = "Edit Plan Details";
            TxtPlanName.Text = _selectedPlan.Name;
            TxtPlanDuration.Text = _selectedPlan.DurationMonths.ToString();
            TxtPlanPrice.Text = _selectedPlan.Price.ToString("F2");
            TxtPlanDescription.Text = _selectedPlan.Description;
            BtnDeletePlan.Visibility = Visibility.Visible;
        }
        else
        {
            ClearPlanForm();
        }
    }

    private void ClearPlanForm()
    {
        _selectedPlan = null;
        if (GridPlans != null)
        {
            GridPlans.SelectionChanged -= GridPlans_SelectionChanged;
            GridPlans.SelectedItem = null;
        }

        if (TxtPlanTitle != null) TxtPlanTitle.Text = "Create New Plan";
        if (TxtPlanName != null) TxtPlanName.Text = string.Empty;
        if (TxtPlanDuration != null) TxtPlanDuration.Text = string.Empty;
        if (TxtPlanPrice != null) TxtPlanPrice.Text = string.Empty;
        if (TxtPlanDescription != null) TxtPlanDescription.Text = string.Empty;
        if (BtnDeletePlan != null) BtnDeletePlan.Visibility = Visibility.Collapsed;

        if (GridPlans != null)
        {
            GridPlans.SelectionChanged += GridPlans_SelectionChanged;
        }
    }

    private void BtnClearPlan_Click(object sender, RoutedEventArgs e)
    {
        ClearPlanForm();
    }

    private async void BtnSavePlan_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtPlanName.Text))
        {
            MessageBox.Show("Please enter a plan name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(TxtPlanDuration.Text.Trim(), out int duration) || duration <= 0)
        {
            MessageBox.Show("Please enter a valid duration in months (positive integer).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtPlanPrice.Text.Trim(), out decimal price) || price < 0)
        {
            MessageBox.Show("Please enter a valid price (positive number).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var service = App.GymService;
            var planToSave = _selectedPlan ?? new MembershipPlan();
            planToSave.Name = TxtPlanName.Text.Trim();
            planToSave.DurationMonths = duration;
            planToSave.Price = price;
            planToSave.Description = TxtPlanDescription.Text.Trim();

            await service.SavePlanAsync(planToSave);
            await RefreshPlansListAsync();
            ClearPlanForm();
            MessageBox.Show("Membership plan saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving plan: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnDeletePlan_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPlan == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete the plan '{_selectedPlan.Name}'? Any members currently assigned to this plan will have their plan set to 'None'.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await App.GymService.DeletePlanAsync(_selectedPlan.Id);
                await RefreshPlansListAsync();
                ClearPlanForm();
                MessageBox.Show("Membership plan deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting plan: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion

    #region Employee Setup Logic

    private async Task RefreshEmployeesListAsync()
    {
        try
        {
            var service = App.GymService;
            _employees = await service.GetAllEmployeesAsync();
            GridEmployees.ItemsSource = null;
            GridEmployees.ItemsSource = _employees;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading employees: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GridEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GridEmployees == null || TxtEmpTitle == null || TxtEmpName == null || 
            TxtEmpDesignation == null || TxtEmpAddr1 == null || TxtEmpAddr2 == null || 
            TxtEmpAddr3 == null || TxtEmpPinCode == null || TxtEmpMobile == null || 
            TxtEmpPhoneRes == null || TxtEmpGuardian == null || TxtEmpBasicPay == null || 
            ComboEmpCategory == null || DpEmpDOB == null || TxtEmpAge == null || 
            DpEmpJoiningDate == null || ChkEmpNotActive == null || BtnDeleteEmp == null) return;

        _selectedEmployee = GridEmployees.SelectedItem as Employee;
        if (_selectedEmployee != null)
        {
            TxtEmpTitle.Text = "Edit Employee Details";
            TxtEmpName.Text = _selectedEmployee.Name;
            TxtEmpDesignation.Text = _selectedEmployee.Designation;
            TxtEmpAddr1.Text = _selectedEmployee.AddressLine1;
            TxtEmpAddr2.Text = _selectedEmployee.AddressLine2;
            TxtEmpAddr3.Text = _selectedEmployee.AddressLine3;
            TxtEmpPinCode.Text = _selectedEmployee.PinCode;
            TxtEmpMobile.Text = _selectedEmployee.MobileNo;
            TxtEmpPhoneRes.Text = _selectedEmployee.PhoneRes;
            TxtEmpGuardian.Text = _selectedEmployee.GuardianName;
            TxtEmpBasicPay.Text = _selectedEmployee.BasicPay.ToString("F2");
            
            // Category mapping
            ComboEmpCategory.SelectedIndex = 0; // fallback
            for (int i = 0; i < ComboEmpCategory.Items.Count; i++)
            {
                if (ComboEmpCategory.Items[i] is ComboBoxItem item && 
                    item.Content.ToString() == _selectedEmployee.EmployeeCategory)
                {
                    ComboEmpCategory.SelectedIndex = i;
                    break;
                }
            }

            DpEmpDOB.SelectedDate = _selectedEmployee.DOB;
            TxtEmpAge.Text = _selectedEmployee.Age.ToString();
            DpEmpJoiningDate.SelectedDate = _selectedEmployee.JoiningDate;
            ChkEmpNotActive.IsChecked = !_selectedEmployee.IsActive; // If not active is checked, IsActive is false
            BtnDeleteEmp.Visibility = Visibility.Visible;
        }
        else
        {
            ClearEmployeeForm();
        }
    }

    private void ClearEmployeeForm()
    {
        _selectedEmployee = null;
        if (GridEmployees != null)
        {
            GridEmployees.SelectionChanged -= GridEmployees_SelectionChanged;
            GridEmployees.SelectedItem = null;
        }

        if (TxtEmpTitle != null) TxtEmpTitle.Text = "Record New Employee";
        if (TxtEmpName != null) TxtEmpName.Text = string.Empty;
        if (TxtEmpDesignation != null) TxtEmpDesignation.Text = string.Empty;
        if (TxtEmpAddr1 != null) TxtEmpAddr1.Text = string.Empty;
        if (TxtEmpAddr2 != null) TxtEmpAddr2.Text = string.Empty;
        if (TxtEmpAddr3 != null) TxtEmpAddr3.Text = string.Empty;
        if (TxtEmpPinCode != null) TxtEmpPinCode.Text = string.Empty;
        if (TxtEmpMobile != null) TxtEmpMobile.Text = string.Empty;
        if (TxtEmpPhoneRes != null) TxtEmpPhoneRes.Text = string.Empty;
        if (TxtEmpGuardian != null) TxtEmpGuardian.Text = string.Empty;
        if (TxtEmpBasicPay != null) TxtEmpBasicPay.Text = "0.00";
        if (ComboEmpCategory != null) ComboEmpCategory.SelectedIndex = 0;
        
        if (DpEmpDOB != null)
        {
            DpEmpDOB.SelectedDateChanged -= DpEmpDOB_SelectedDateChanged;
            DpEmpDOB.SelectedDate = DateTime.Today.AddYears(-25);
            DpEmpDOB.SelectedDateChanged += DpEmpDOB_SelectedDateChanged;
        }

        if (TxtEmpAge != null) TxtEmpAge.Text = "25";
        if (DpEmpJoiningDate != null) DpEmpJoiningDate.SelectedDate = DateTime.Today;
        if (ChkEmpNotActive != null) ChkEmpNotActive.IsChecked = false;
        if (BtnDeleteEmp != null) BtnDeleteEmp.Visibility = Visibility.Collapsed;

        if (GridEmployees != null)
        {
            GridEmployees.SelectionChanged += GridEmployees_SelectionChanged;
        }
    }

    private void BtnClearEmp_Click(object sender, RoutedEventArgs e)
    {
        ClearEmployeeForm();
    }

    private void DpEmpDOB_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DpEmpDOB == null || TxtEmpAge == null) return;
        if (DpEmpDOB.SelectedDate.HasValue)
        {
            int age = DateTime.Today.Year - DpEmpDOB.SelectedDate.Value.Year;
            if (DpEmpDOB.SelectedDate.Value.Date > DateTime.Today.AddYears(-age)) age--;
            TxtEmpAge.Text = age.ToString();
        }
        else
        {
            TxtEmpAge.Text = string.Empty;
        }
    }

    private async void BtnSaveEmp_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtEmpName.Text))
        {
            MessageBox.Show("Please enter employee name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtEmpDesignation.Text))
        {
            MessageBox.Show("Please enter employee designation.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtEmpBasicPay.Text.Trim(), out decimal basicPay) || basicPay < 0)
        {
            MessageBox.Show("Please enter a valid basic pay (positive number).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var service = App.GymService;
            var empToSave = _selectedEmployee ?? new Employee();
            empToSave.Name = TxtEmpName.Text.Trim();
            empToSave.Designation = TxtEmpDesignation.Text.Trim();
            empToSave.AddressLine1 = TxtEmpAddr1.Text.Trim();
            empToSave.AddressLine2 = TxtEmpAddr2.Text.Trim();
            empToSave.AddressLine3 = TxtEmpAddr3.Text.Trim();
            empToSave.PinCode = TxtEmpPinCode.Text.Trim();
            empToSave.MobileNo = TxtEmpMobile.Text.Trim();
            empToSave.PhoneRes = TxtEmpPhoneRes.Text.Trim();
            empToSave.GuardianName = TxtEmpGuardian.Text.Trim();
            empToSave.BasicPay = basicPay;
            
            if (ComboEmpCategory.SelectedItem is ComboBoxItem categoryItem)
            {
                empToSave.EmployeeCategory = categoryItem.Content.ToString() ?? "Trainer";
            }
            else
            {
                empToSave.EmployeeCategory = "Trainer";
            }

            empToSave.DOB = DpEmpDOB.SelectedDate;
            if (int.TryParse(TxtEmpAge.Text, out int age))
            {
                empToSave.Age = age;
            }
            empToSave.JoiningDate = DpEmpJoiningDate.SelectedDate;
            empToSave.IsActive = !(ChkEmpNotActive.IsChecked ?? false);

            await service.SaveEmployeeAsync(empToSave);
            await RefreshEmployeesListAsync();
            ClearEmployeeForm();
            MessageBox.Show("Employee profile saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving employee: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnDeleteEmp_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedEmployee == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete the employee profile '{_selectedEmployee.Name}'? This cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await App.GymService.DeleteEmployeeAsync(_selectedEmployee.Id);
                await RefreshEmployeesListAsync();
                ClearEmployeeForm();
                MessageBox.Show("Employee profile deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting employee: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}
