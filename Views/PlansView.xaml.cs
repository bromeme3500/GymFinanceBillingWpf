using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class PlansView : UserControl
{
    private List<MembershipPlan> _plans = new();
    private MembershipPlan? _selectedPlan;

    public PlansView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await RefreshPlansListAsync();
    }

    private async Task RefreshPlansListAsync()
    {
        try
        {
            var service = App.GymService;
            _plans = await service.GetAllPlansAsync();
            GridPlans.ItemsSource = _plans;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading plans: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GridPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedPlan = GridPlans.SelectedItem as MembershipPlan;
        if (_selectedPlan != null)
        {
            TxtPanelTitle.Text = "Edit Plan Details";
            TxtPlanName.Text = _selectedPlan.Name;
            TxtDuration.Text = _selectedPlan.DurationMonths.ToString();
            TxtPrice.Text = _selectedPlan.Price.ToString("F2");
            TxtDescription.Text = _selectedPlan.Description;
            BtnDelete.Visibility = Visibility.Visible;
        }
        else
        {
            ClearForm();
        }
    }

    private void ClearForm()
    {
        _selectedPlan = null;
        TxtPanelTitle.Text = "Create New Plan";
        TxtPlanName.Text = string.Empty;
        TxtDuration.Text = string.Empty;
        TxtPrice.Text = string.Empty;
        TxtDescription.Text = string.Empty;
        BtnDelete.Visibility = Visibility.Collapsed;
        GridPlans.SelectedItem = null;
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtPlanName.Text))
        {
            MessageBox.Show("Please enter a plan name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(TxtDuration.Text.Trim(), out int duration) || duration <= 0)
        {
            MessageBox.Show("Please enter a valid duration in months (positive integer).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtPrice.Text.Trim(), out decimal price) || price < 0)
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
            planToSave.Description = TxtDescription.Text.Trim();

            await service.SavePlanAsync(planToSave);
            await RefreshPlansListAsync();
            ClearForm();
            MessageBox.Show("Membership plan saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving plan: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPlan == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete the plan '{_selectedPlan.Name}'? Any members currently assigned to this plan will have their plan set to 'None'.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await App.GymService.DeletePlanAsync(_selectedPlan.Id);
                await RefreshPlansListAsync();
                ClearForm();
                MessageBox.Show("Membership plan deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting plan: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
