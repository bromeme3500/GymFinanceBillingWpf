using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class InvoiceEditorView : UserControl
{
    private readonly string? _invoiceId;
    private Invoice? _invoice;
    private List<Member> _members = new();
    private List<MembershipPlan> _plans = new();
    private bool _isUpdatingCalculations = false;

    public InvoiceEditorView(string? invoiceId = null)
    {
        InitializeComponent();
        _invoiceId = invoiceId;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var service = App.GymService;

            // Load plans list
            _plans = await service.GetAllPlansAsync();
            ComboPackages.ItemsSource = _plans;
            ComboPackages.DisplayMemberPath = "Name";

            // Load members list
            _members = await service.GetAllMembersAsync();
            ComboMembers.ItemsSource = _members;
            ComboMembers.DisplayMemberPath = "FullName";

            if (string.IsNullOrEmpty(_invoiceId))
            {
                // Initialize default values for NEW invoice
                TxtTitle.Text = "Create New Receipt";
                DpIssueDate.SelectedDate = DateTime.Today;
                DpServiceStart.SelectedDate = DateTime.Today;
                DpServiceEnd.SelectedDate = DateTime.Today.AddMonths(1);
                
                TxtAdmissionFee.Text = "0.00";
                TxtAdmissionDiscount.Text = "0.00";
                TxtTotalAdmFee.Text = "0.00";
                TxtFee.Text = "0.00";
                TxtTotalAmount.Text = "0.00";
                TxtPendingAdmFee.Text = "0.00";
                TxtPendingAmount.Text = "0.00";
                TxtGrandTotal.Text = "0.00";
                TxtCollectedAdmnFee.Text = "0.00";
                TxtCollectedFee.Text = "0.00";
                
                TxtCashAmount.Text = "0.00";
                TxtUpiAmount.Text = "0.00";
                TxtCardAmount.Text = "0.00";
                TxtNarration.Text = string.Empty;
                TxtBankName.Text = "";

                if (_plans.Any())
                {
                    ComboPackages.SelectedIndex = 0;
                }

                // Auto generate invoice number
                var count = (await service.GetAllInvoicesAsync()).Count;
                TxtInvoiceNum.Text = $"SLIP-{(count + 1):D4}";
            }
            else
            {
                // Load EXISTING invoice
                TxtTitle.Text = "Edit Receipt Details";
                _invoice = await service.GetInvoiceByIdAsync(_invoiceId);
                
                if (_invoice != null)
                {
                    TxtInvoiceNum.Text = _invoice.InvoiceNumber;
                    
                    var member = _members.FirstOrDefault(m => m.Id == _invoice.MemberId);
                    ComboMembers.SelectedItem = member;

                    DpIssueDate.SelectedDate = _invoice.IssueDate;
                    DpServiceStart.SelectedDate = _invoice.ServicePeriodStart ?? DateTime.Today;
                    DpServiceEnd.SelectedDate = _invoice.ServicePeriodEnd ?? DateTime.Today.AddMonths(1);
                    
                    TxtAdmissionFee.Text = _invoice.AdmissionFee.ToString("F2");
                    TxtAdmissionDiscount.Text = _invoice.AdmissionFeeDiscount.ToString("F2");
                    TxtTotalAdmFee.Text = _invoice.TotalAdmFee.ToString("F2");
                    TxtFee.Text = _invoice.Total.ToString("F2");
                    TxtTotalAmount.Text = _invoice.TotalAmount.ToString("F2");
                    TxtPendingAdmFee.Text = _invoice.PendingAdmFee.ToString("F2");
                    TxtPendingAmount.Text = _invoice.PendingAmount.ToString("F2");
                    TxtGrandTotal.Text = _invoice.GrandTotal.ToString("F2");
                    TxtCollectedAdmnFee.Text = _invoice.CollectedAdmnFee.ToString("F2");
                    TxtCollectedFee.Text = _invoice.CollectedFee.ToString("F2");
                    
                    TxtCashAmount.Text = _invoice.CashAmount.ToString("F2");
                    TxtUpiAmount.Text = _invoice.UpiAmount.ToString("F2");
                    TxtCardAmount.Text = _invoice.CardAmount.ToString("F2");
                    TxtNarration.Text = _invoice.Narration ?? "";
                    
                    TxtBankName.Text = _invoice.BankName ?? "";
                    ChkPartialCollection.IsChecked = _invoice.PartialCollection;

                    var plan = _plans.FirstOrDefault(p => p.Name == _invoice.PackageName);
                    if (plan != null)
                    {
                        ComboPackages.SelectedItem = plan;
                    }

                    // Restore combo selections
                    SetComboSelection(ComboTransTypeLeft, _invoice.TransTypeLeft ?? "Cash");
                    SetComboSelection(ComboTransTypeRight, _invoice.TransactionTypeRight ?? "Subscription");
                    SetComboSelection(ComboAccountType, _invoice.AccountType ?? "Cash");
                }
            }

            UpdateTotalSum();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading invoice editor: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetComboSelection(ComboBox combo, string value)
    {
        foreach (ComboBoxItem item in combo.Items)
        {
            if (item.Content.ToString() == value)
            {
                combo.SelectedItem = item;
                break;
            }
        }
    }

    private void ComboMembers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboMembers.SelectedItem is Member member)
        {
            TxtRegNo.Text = member.RegNo ?? string.Empty;
            TxtMobileNo.Text = member.Phone ?? string.Empty;
            TxtAddress.Text = string.Join(", ", new[] { member.AddressLine1, member.AddressLine2, member.AddressLine3 }
                .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

            if (member.ActivePlanId != null)
            {
                var plan = _plans.FirstOrDefault(p => p.Id == member.ActivePlanId);
                if (plan != null)
                {
                    ComboPackages.SelectedItem = plan;
                    TxtFee.Text = plan.Price.ToString("F2");
                }
            }
        }
        else
        {
            TxtRegNo.Text = string.Empty;
            TxtMobileNo.Text = string.Empty;
            TxtAddress.Text = string.Empty;
        }
    }

    private void ComboPackages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboPackages.SelectedItem is MembershipPlan plan)
        {
            TxtFee.Text = plan.Price.ToString("F2");
            UpdateTotalSum();
        }
    }

    private void UpdateTotalSum()
    {
        if (_isUpdatingCalculations) return;
        _isUpdatingCalculations = true;

        try
        {
            decimal.TryParse(TxtFee?.Text?.Trim() ?? "0.00", out decimal feeSubtotal);
            decimal.TryParse(TxtAdmissionFee?.Text?.Trim() ?? "0.00", out decimal admissionFee);
            decimal.TryParse(TxtAdmissionDiscount?.Text?.Trim() ?? "0.00", out decimal admissionDiscount);
            decimal.TryParse(TxtCollectedAdmnFee?.Text?.Trim() ?? "0.00", out decimal collectedAdmFee);
            decimal.TryParse(TxtCollectedFee?.Text?.Trim() ?? "0.00", out decimal collectedFee);
            
            decimal.TryParse(TxtCashAmount?.Text?.Trim() ?? "0.00", out decimal cash);
            decimal.TryParse(TxtUpiAmount?.Text?.Trim() ?? "0.00", out decimal upi);
            decimal.TryParse(TxtCardAmount?.Text?.Trim() ?? "0.00", out decimal card);

            // Calculations
            decimal totalAdmFee = Math.Max(0, admissionFee - admissionDiscount);
            decimal totalAmount = feeSubtotal + totalAdmFee;
            decimal pendingAdmFee = Math.Max(0, totalAdmFee - collectedAdmFee);
            decimal pendingAmount = Math.Max(0, feeSubtotal - collectedFee);
            decimal grandTotal = totalAmount;

            // Update UI elements
            if (TxtTotalAdmFee != null) TxtTotalAdmFee.Text = totalAdmFee.ToString("F2");
            if (TxtTotalAmount != null) TxtTotalAmount.Text = totalAmount.ToString("F2");
            if (TxtPendingAdmFee != null) TxtPendingAdmFee.Text = pendingAdmFee.ToString("F2");
            if (TxtPendingAmount != null) TxtPendingAmount.Text = pendingAmount.ToString("F2");
            if (TxtGrandTotal != null) TxtGrandTotal.Text = grandTotal.ToString("F2");
            
            if (TxtBalanceAdmnFee != null) TxtBalanceAdmnFee.Text = pendingAdmFee.ToString("F2");
            if (TxtBalanceFees != null) TxtBalanceFees.Text = pendingAmount.ToString("F2");
        }
        finally
        {
            _isUpdatingCalculations = false;
        }
    }

    private void LegacyCalculations_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTotalSum();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var selectedMember = ComboMembers.SelectedItem as Member;
        if (selectedMember == null)
        {
            MessageBox.Show("Please select a gym member for this receipt.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtInvoiceNum.Text))
        {
            MessageBox.Show("Please enter a slip number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var service = App.GymService;
            var invoiceToSave = _invoice ?? new Invoice();

            decimal.TryParse(TxtAdmissionFee.Text.Trim(), out decimal admissionFee);
            decimal.TryParse(TxtAdmissionDiscount.Text.Trim(), out decimal admissionDiscount);
            decimal.TryParse(TxtTotalAdmFee.Text.Trim(), out decimal totalAdmFee);
            decimal.TryParse(TxtFee.Text.Trim(), out decimal fee);
            decimal.TryParse(TxtTotalAmount.Text.Trim(), out decimal totalAmount);
            decimal.TryParse(TxtPendingAdmFee.Text.Trim(), out decimal pendingAdmFee);
            decimal.TryParse(TxtPendingAmount.Text.Trim(), out decimal pendingAmount);
            decimal.TryParse(TxtCollectedAdmnFee.Text.Trim(), out decimal collectedAdmFee);
            decimal.TryParse(TxtCollectedFee.Text.Trim(), out decimal collectedFee);
            
            decimal.TryParse(TxtCashAmount.Text.Trim(), out decimal cash);
            decimal.TryParse(TxtUpiAmount.Text.Trim(), out decimal upi);
            decimal.TryParse(TxtCardAmount.Text.Trim(), out decimal card);

            // Basic invoice fields
            invoiceToSave.InvoiceNumber = TxtInvoiceNum.Text.Trim();
            invoiceToSave.MemberId = selectedMember.Id;
            invoiceToSave.IssueDate = DpIssueDate.SelectedDate ?? DateTime.Today;
            invoiceToSave.DueDate = invoiceToSave.IssueDate.AddDays(7); // default due is 7 days after issue

            invoiceToSave.ServicePeriodStart = DpServiceStart.SelectedDate;
            invoiceToSave.ServicePeriodEnd = DpServiceEnd.SelectedDate;
            invoiceToSave.AdmissionFee = admissionFee;
            invoiceToSave.AdmissionFeeDiscount = admissionDiscount;
            invoiceToSave.Narration = TxtNarration.Text.Trim();
            invoiceToSave.CashAmount = cash;
            invoiceToSave.UpiAmount = upi;
            invoiceToSave.CardAmount = card;

            // Legacy receipt fields
            var selectedPlan = ComboPackages.SelectedItem as MembershipPlan;
            invoiceToSave.PackageName = selectedPlan?.Name ?? "General Membership";
            invoiceToSave.BankName = TxtBankName.Text.Trim();
            invoiceToSave.TransTypeLeft = (ComboTransTypeLeft.SelectedItem as ComboBoxItem)?.Content.ToString();
            invoiceToSave.BalanceAdmnFee = pendingAdmFee;
            invoiceToSave.BalanceFees = pendingAmount;
            invoiceToSave.PartialCollection = ChkPartialCollection.IsChecked ?? false;
            invoiceToSave.TransactionTypeRight = (ComboTransTypeRight.SelectedItem as ComboBoxItem)?.Content.ToString();
            invoiceToSave.AccountType = (ComboAccountType.SelectedItem as ComboBoxItem)?.Content.ToString();
            invoiceToSave.TotalAdmFee = totalAdmFee;
            invoiceToSave.TotalAmount = totalAmount;
            invoiceToSave.PendingAdmFee = pendingAdmFee;
            invoiceToSave.CollectedAdmnFee = collectedAdmFee;
            invoiceToSave.CollectedFee = collectedFee;

            // Determine status based on total pending balance (Fee balance + Adm Fee balance)
            decimal totalPending = pendingAdmFee + pendingAmount;
            if (totalPending <= 0)
            {
                invoiceToSave.Status = InvoiceStatus.Paid;
            }
            else if (invoiceToSave.IssueDate.AddDays(7) < DateTime.Today)
            {
                invoiceToSave.Status = InvoiceStatus.Overdue;
            }
            else
            {
                invoiceToSave.Status = InvoiceStatus.Unpaid;
            }

            // Sync payment method
            if (cash > 0 && upi == 0 && card == 0) invoiceToSave.PaymentMethod = "Cash";
            else if (upi > 0 && cash == 0 && card == 0) invoiceToSave.PaymentMethod = "UPI";
            else if (card > 0 && cash == 0 && upi == 0) invoiceToSave.PaymentMethod = "Card";
            else if (cash > 0 || upi > 0 || card > 0) invoiceToSave.PaymentMethod = "Split";
            else invoiceToSave.PaymentMethod = "None";

            // Sync a single default item representation for the DB/statistics logic
            invoiceToSave.Items = new List<InvoiceItem>
            {
                new InvoiceItem
                {
                    InvoiceId = invoiceToSave.Id,
                    Description = (string.IsNullOrWhiteSpace(invoiceToSave.PackageName) ? "General Membership" : invoiceToSave.PackageName) + " Fee",
                    Quantity = 1,
                    UnitPrice = fee
                }
            };

            await service.SaveInvoiceAsync(invoiceToSave);
            MessageBox.Show("Receipt saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Go back
            NavigateBack();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving receipt: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        NavigateBack();
    }

    private void NavigateBack()
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.NavigateToInvoices();
    }
}
