using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class ExpensesView : UserControl
{
    private List<Expense> _allExpenses = new();
    private Expense? _selectedExpense;

    public ExpensesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Populate category filter dropdown
            var filters = new List<string> { "All Categories", "Utilities", "Rent", "Salaries", "Equipment", "Marketing", "Other" };
            ComboCategoryFilter.ItemsSource = filters;
            ComboCategoryFilter.SelectedIndex = 0;

            DpDate.SelectedDate = DateTime.Today;

            await RefreshExpensesListAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading expenses: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshExpensesListAsync()
    {
        var service = App.GymService;
        _allExpenses = await service.GetAllExpensesAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = TxtSearch.Text.Trim().ToLower();
        var selectedFilter = ComboCategoryFilter.SelectedItem as string ?? "All Categories";

        var filtered = _allExpenses.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            filtered = filtered.Where(e => e.Description.ToLower().Contains(query));
        }

        if (selectedFilter != "All Categories")
        {
            filtered = filtered.Where(e => e.Category == selectedFilter);
        }

        GridExpenses.ItemsSource = filtered.ToList();
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ComboCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        TxtSearch.Text = string.Empty;
        ComboCategoryFilter.SelectedIndex = 0;
        ApplyFilter();
    }

    private void GridExpenses_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedExpense = GridExpenses.SelectedItem as Expense;
        if (_selectedExpense != null)
        {
            TxtPanelTitle.Text = "Edit Expense Log";
            TxtDescription.Text = _selectedExpense.Description;
            TxtAmount.Text = _selectedExpense.Amount.ToString("F2");
            DpDate.SelectedDate = _selectedExpense.Date;
            TxtNotes.Text = _selectedExpense.Notes;

            // Match category
            foreach (ComboBoxItem item in ComboCategory.Items)
            {
                if (item.Content.ToString() == _selectedExpense.Category)
                {
                    ComboCategory.SelectedItem = item;
                    break;
                }
            }

            BtnDelete.Visibility = Visibility.Visible;
        }
        else
        {
            ClearForm();
        }
    }

    private void ClearForm()
    {
        _selectedExpense = null;
        TxtPanelTitle.Text = "Record New Expense";
        TxtDescription.Text = string.Empty;
        TxtAmount.Text = string.Empty;
        DpDate.SelectedDate = DateTime.Today;
        ComboCategory.SelectedIndex = 0; // Utilities
        TxtNotes.Text = string.Empty;
        BtnDelete.Visibility = Visibility.Collapsed;
        GridExpenses.SelectedItem = null;
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtDescription.Text))
        {
            MessageBox.Show("Please enter an expense description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtAmount.Text.Trim(), out decimal amount) || amount <= 0)
        {
            MessageBox.Show("Please enter a valid amount (positive number).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var service = App.GymService;
            var expenseToSave = _selectedExpense ?? new Expense();

            expenseToSave.Description = TxtDescription.Text.Trim();
            expenseToSave.Amount = amount;
            expenseToSave.Date = DpDate.SelectedDate ?? DateTime.Today;
            
            var selectedItem = ComboCategory.SelectedItem as ComboBoxItem;
            expenseToSave.Category = selectedItem?.Content as string ?? "Other";
            
            expenseToSave.Notes = TxtNotes.Text.Trim();

            await service.SaveExpenseAsync(expenseToSave);
            await RefreshExpensesListAsync();
            ClearForm();
            MessageBox.Show("Expense record saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving expense: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedExpense == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete expense record '{_selectedExpense.Description}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await App.GymService.DeleteExpenseAsync(_selectedExpense.Id);
                await RefreshExpensesListAsync();
                ClearForm();
                MessageBox.Show("Expense record deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting expense: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
