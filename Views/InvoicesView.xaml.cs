using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class InvoicesView : UserControl
{
    private List<Invoice> _allInvoices = new();
    private Invoice? _selectedInvoice;

    public InvoicesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Populate filter combobox
            var filters = new List<string> { "All Invoices", "Paid", "Unpaid", "Overdue" };
            ComboStatusFilter.ItemsSource = filters;
            ComboStatusFilter.SelectedIndex = 0;

            await RefreshInvoicesListAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading invoices: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshInvoicesListAsync()
    {
        var service = App.GymService;
        _allInvoices = await service.GetAllInvoicesAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = TxtSearch.Text.Trim().ToLower();
        var selectedFilter = ComboStatusFilter.SelectedItem as string ?? "All Invoices";

        var filtered = _allInvoices.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            filtered = filtered.Where(i => i.Member != null && i.Member.FullName.ToLower().Contains(query));
        }

        if (selectedFilter != "All Invoices")
        {
            if (selectedFilter == "Paid")
                filtered = filtered.Where(i => i.Status == InvoiceStatus.Paid);
            else if (selectedFilter == "Unpaid")
                filtered = filtered.Where(i => i.Status == InvoiceStatus.Unpaid);
            else if (selectedFilter == "Overdue")
                filtered = filtered.Where(i => i.Status == InvoiceStatus.Overdue);
        }

        GridInvoices.ItemsSource = filtered.ToList();
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ComboStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        TxtSearch.Text = string.Empty;
        ComboStatusFilter.SelectedIndex = 0;
        ApplyFilter();
    }

    private void GridInvoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedInvoice = GridInvoices.SelectedItem as Invoice;
        if (_selectedInvoice != null)
        {
            TxtEmptyState.Visibility = Visibility.Collapsed;
            ScrollInvoiceDetails.Visibility = Visibility.Visible;

            TxtInvoiceNum.Text = _selectedInvoice.InvoiceNumber;
            TxtInvoiceDate.Text = $"Issued: {_selectedInvoice.IssueDate:MMMM dd, yyyy} | Due: {_selectedInvoice.DueDate:MMMM dd, yyyy}";
            TxtMemberName.Text = _selectedInvoice.Member?.FullName ?? "Unknown Member";
            TxtMemberContact.Text = $"Phone: {_selectedInvoice.Member?.Phone ?? "N/A"} | Email: {_selectedInvoice.Member?.Email ?? "N/A"}";
            
            TxtServicePeriod.Text = $"From: {_selectedInvoice.ServicePeriodStart?.ToString("yyyy-MM-dd") ?? "N/A"}  To: {_selectedInvoice.ServicePeriodEnd?.ToString("yyyy-MM-dd") ?? "N/A"}";

            if (!string.IsNullOrEmpty(_selectedInvoice.Narration))
            {
                LblNarration.Visibility = Visibility.Visible;
                TxtNarration.Visibility = Visibility.Visible;
                TxtNarration.Text = _selectedInvoice.Narration;
            }
            else
            {
                LblNarration.Visibility = Visibility.Collapsed;
                TxtNarration.Visibility = Visibility.Collapsed;
            }

            // Populate items
            ListItems.ItemsSource = _selectedInvoice.Items;

            // Totals and splits
            TxtSubtotal.Text = $"₹{_selectedInvoice.Total:N2}";
            TxtAdmission.Text = $"₹{_selectedInvoice.AdmissionFee:N2}";
            TxtDiscount.Text = $"-₹{_selectedInvoice.AdmissionFeeDiscount:N2}";
            TxtGrandTotal.Text = $"₹{_selectedInvoice.GrandTotal:N2}";
            TxtCashPaid.Text = $"₹{_selectedInvoice.CashAmount:N2}";
            TxtUpiPaid.Text = $"₹{_selectedInvoice.UpiAmount:N2}";
            TxtCardPaid.Text = $"₹{_selectedInvoice.CardAmount:N2}";
            TxtInvoiceTotal.Text = $"₹{_selectedInvoice.PendingAmount:N2}";

            // Record payment boxes defaults
            TxtRecordCash.Text = _selectedInvoice.PendingAmount > 0 ? _selectedInvoice.PendingAmount.ToString("F2") : "0.00";
            TxtRecordUpi.Text = "0.00";
            TxtRecordCard.Text = "0.00";

            // Update badge style
            UpdateStatusBadge(_selectedInvoice.Status);
        }
        else
        {
            TxtEmptyState.Visibility = Visibility.Visible;
            ScrollInvoiceDetails.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateStatusBadge(InvoiceStatus status)
    {
        if (status == InvoiceStatus.Paid)
        {
            BorderStatus.Background = (SolidColorBrush)FindResource("StatusPaidBg");
            TxtStatus.Foreground = (SolidColorBrush)FindResource("StatusPaid");
            TxtStatus.Text = "PAID";
            PanelPayment.Visibility = Visibility.Collapsed;
        }
        else if (status == InvoiceStatus.Unpaid)
        {
            BorderStatus.Background = (SolidColorBrush)FindResource("StatusUnpaidBg");
            TxtStatus.Foreground = (SolidColorBrush)FindResource("StatusUnpaid");
            TxtStatus.Text = "UNPAID";
            PanelPayment.Visibility = Visibility.Visible;
        }
        else
        {
            BorderStatus.Background = (SolidColorBrush)FindResource("StatusOverdueBg");
            TxtStatus.Foreground = (SolidColorBrush)FindResource("StatusOverdue");
            TxtStatus.Text = "OVERDUE";
            PanelPayment.Visibility = Visibility.Visible;
        }
    }

    private async void BtnConfirmPayment_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedInvoice == null) return;

        decimal.TryParse(TxtRecordCash.Text.Trim(), out decimal cash);
        decimal.TryParse(TxtRecordUpi.Text.Trim(), out decimal upi);
        decimal.TryParse(TxtRecordCard.Text.Trim(), out decimal card);

        try
        {
            _selectedInvoice.CashAmount += cash;
            _selectedInvoice.UpiAmount += upi;
            _selectedInvoice.CardAmount += card;

            // If the pending amount is fully covered (<= 0), mark it as Paid
            if (_selectedInvoice.PendingAmount <= 0)
            {
                _selectedInvoice.Status = InvoiceStatus.Paid;
            }

            // Determine payment method
            if (_selectedInvoice.CashAmount > 0 && _selectedInvoice.UpiAmount == 0 && _selectedInvoice.CardAmount == 0) _selectedInvoice.PaymentMethod = "Cash";
            else if (_selectedInvoice.UpiAmount > 0 && _selectedInvoice.CashAmount == 0 && _selectedInvoice.CardAmount == 0) _selectedInvoice.PaymentMethod = "UPI";
            else if (_selectedInvoice.CardAmount > 0 && _selectedInvoice.CashAmount == 0 && _selectedInvoice.UpiAmount == 0) _selectedInvoice.PaymentMethod = "Card";
            else if (_selectedInvoice.CashAmount > 0 || _selectedInvoice.UpiAmount > 0 || _selectedInvoice.CardAmount > 0) _selectedInvoice.PaymentMethod = "Split";

            var service = App.GymService;
            await service.SaveInvoiceAsync(_selectedInvoice);
            await RefreshInvoicesListAsync();

            // Refresh selected details
            GridInvoices.SelectedItem = null;
            GridInvoices.SelectedItem = _selectedInvoice;
            MessageBox.Show("Payment recorded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error recording payment: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnNewInvoice_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.NavigateToInvoiceEditor(null);
    }

    private void BtnEditInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedInvoice == null) return;
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.NavigateToInvoiceEditor(_selectedInvoice.Id);
    }

    private async void BtnDeleteInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedInvoice == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete invoice '{_selectedInvoice.InvoiceNumber}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await App.GymService.DeleteInvoiceAsync(_selectedInvoice.Id);
                await RefreshInvoicesListAsync();
                MessageBox.Show("Invoice deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting invoice: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
