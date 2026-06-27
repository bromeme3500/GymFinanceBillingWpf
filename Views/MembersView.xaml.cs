using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf.Views;

public partial class MembersView : UserControl
{
    private List<Member> _allMembers = new();
    private List<MembershipPlan> _allPlans = new();
    private Member? _selectedMember;

    public MembersView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Populate Status dropdown
            ComboStatus.ItemsSource = Enum.GetValues(typeof(MemberStatus));
            ComboStatus.SelectedItem = MemberStatus.Active;

            // Load plans
            var service = App.GymService;
            _allPlans = await service.GetAllPlansAsync();

            // Setup plans dropdown (allow null / no plan)
            var planOptions = new List<object> { "None" };
            planOptions.AddRange(_allPlans);
            ComboPlans.ItemsSource = planOptions;
            ComboPlans.SelectedIndex = 0;

            await RefreshMembersListAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing members view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshMembersListAsync()
    {
        var service = App.GymService;
        _allMembers = await service.GetAllMembersAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = TxtSearch.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(query))
        {
            GridMembers.ItemsSource = _allMembers;
        }
        else
        {
            GridMembers.ItemsSource = _allMembers.Where(m =>
                m.FullName.ToLower().Contains(query) ||
                m.Email.ToLower().Contains(query) ||
                m.Phone.ToLower().Contains(query)).ToList();
        }
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void BtnResetSearch_Click(object sender, RoutedEventArgs e)
    {
        TxtSearch.Text = string.Empty;
        ApplyFilter();
    }

    private void ShowEditor()
    {
        ColEditor.Width = new GridLength(2.4, GridUnitType.Star);
        BorderEditor.Visibility = Visibility.Visible;
    }

    private void HideEditor()
    {
        ColEditor.Width = new GridLength(0);
        BorderEditor.Visibility = Visibility.Collapsed;
        GridMembers.SelectedItem = null;
    }

    private void BtnAddNewMember_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
        ShowEditor();
    }

    private void BtnCloseEditor_Click(object sender, RoutedEventArgs e)
    {
        HideEditor();
    }

    private void GridMembers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedMember = GridMembers.SelectedItem as Member;
        if (_selectedMember != null)
        {
            TxtPanelTitle.Text = "Edit Member Details";
            TxtRegNo.Text = _selectedMember.RegNo ?? "";
            TxtFullName.Text = _selectedMember.FullName ?? "";
            TxtEmail.Text = _selectedMember.Email ?? "";
            TxtPhone.Text = _selectedMember.Phone ?? "";
            ComboStatus.SelectedItem = _selectedMember.Status;
            TxtNotes.Text = _selectedMember.Notes ?? "";

            TxtAddress1.Text = _selectedMember.AddressLine1 ?? "";
            TxtAddress2.Text = _selectedMember.AddressLine2 ?? "";
            TxtAddress3.Text = _selectedMember.AddressLine3 ?? "";
            TxtPinCode.Text = _selectedMember.PinCode ?? "";
            TxtAge.Text = _selectedMember.Age?.ToString() ?? "";
            TxtHeight.Text = _selectedMember.Height?.ToString() ?? "";
            TxtWeight.Text = _selectedMember.Weight?.ToString() ?? "";
            TxtOpeningAmount.Text = _selectedMember.OpeningAmount.ToString("F2");
            DpRegDate.SelectedDate = _selectedMember.RegDate;
            DpJoinDate.SelectedDate = _selectedMember.JoinDate;

            // Select Gender
            ComboGender.SelectedIndex = -1;
            foreach (ComboBoxItem item in ComboGender.Items)
            {
                if (item.Content.ToString() == (_selectedMember.Gender ?? "Male"))
                {
                    ComboGender.SelectedItem = item;
                    break;
                }
            }
            if (ComboGender.SelectedIndex == -1) ComboGender.SelectedIndex = 0;

            // Select Blood Group
            ComboBloodGroup.SelectedIndex = -1;
            foreach (ComboBoxItem item in ComboBloodGroup.Items)
            {
                if (item.Content.ToString() == (_selectedMember.BloodGroup ?? "Unknown"))
                {
                    ComboBloodGroup.SelectedItem = item;
                    break;
                }
            }
            if (ComboBloodGroup.SelectedIndex == -1) ComboBloodGroup.SelectedIndex = 8; // Unknown

            if (_selectedMember.ActivePlanId == null)
            {
                ComboPlans.SelectedIndex = 0; // "None"
            }
            else
            {
                var plan = _allPlans.FirstOrDefault(p => p.Id == _selectedMember.ActivePlanId);
                if (plan != null)
                {
                    ComboPlans.SelectedItem = plan;
                }
                else
                {
                    ComboPlans.SelectedIndex = 0;
                }
            }

            BtnDelete.Visibility = Visibility.Visible;
            ShowEditor();
        }
        else
        {
            ClearForm();
        }
    }

    private void ClearForm()
    {
        _selectedMember = null;
        TxtPanelTitle.Text = "Register New Member";
        TxtFullName.Text = string.Empty;
        TxtEmail.Text = string.Empty;
        TxtPhone.Text = string.Empty;
        ComboStatus.SelectedItem = MemberStatus.Active;
        ComboPlans.SelectedIndex = 0;
        TxtNotes.Text = string.Empty;

        TxtAddress1.Text = string.Empty;
        TxtAddress2.Text = string.Empty;
        TxtAddress3.Text = string.Empty;
        TxtPinCode.Text = string.Empty;
        ComboGender.SelectedIndex = 0; // Male
        ComboBloodGroup.SelectedIndex = 8; // Unknown
        TxtAge.Text = string.Empty;
        TxtHeight.Text = string.Empty;
        TxtWeight.Text = string.Empty;
        TxtOpeningAmount.Text = "0.00";
        DpRegDate.SelectedDate = DateTime.Today;
        DpJoinDate.SelectedDate = DateTime.Today;

        // Auto generate RegNo
        int maxRegNo = 4500;
        if (_allMembers != null && _allMembers.Any())
        {
            foreach (var m in _allMembers)
            {
                if (int.TryParse(m.RegNo, out int num))
                {
                    if (num >= maxRegNo)
                    {
                        maxRegNo = num + 1;
                    }
                }
            }
        }
        TxtRegNo.Text = maxRegNo.ToString();

        BtnDelete.Visibility = Visibility.Collapsed;
        GridMembers.SelectedItem = null;
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtFullName.Text))
        {
            MessageBox.Show("Please enter a member name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Validate numeric formats
        int? age = null;
        if (!string.IsNullOrWhiteSpace(TxtAge.Text))
        {
            if (int.TryParse(TxtAge.Text.Trim(), out int val)) age = val;
            else { MessageBox.Show("Please enter a valid number for Age.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        }

        double? height = null;
        if (!string.IsNullOrWhiteSpace(TxtHeight.Text))
        {
            if (double.TryParse(TxtHeight.Text.Trim(), out double val)) height = val;
            else { MessageBox.Show("Please enter a valid number for Height.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        }

        double? weight = null;
        if (!string.IsNullOrWhiteSpace(TxtWeight.Text))
        {
            if (double.TryParse(TxtWeight.Text.Trim(), out double val)) weight = val;
            else { MessageBox.Show("Please enter a valid number for Weight.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        }

        decimal openingAmount = 0.00m;
        if (!string.IsNullOrWhiteSpace(TxtOpeningAmount.Text))
        {
            if (decimal.TryParse(TxtOpeningAmount.Text.Trim(), out decimal val)) openingAmount = val;
            else { MessageBox.Show("Please enter a valid number for Opening Amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        }

        try
        {
            var service = App.GymService;
            var planSelected = ComboPlans.SelectedItem as MembershipPlan;
            var isNew = _selectedMember == null;

            var memberToSave = _selectedMember ?? new Member();
            memberToSave.RegNo = TxtRegNo.Text.Trim();
            memberToSave.FullName = TxtFullName.Text.Trim();
            memberToSave.Email = TxtEmail.Text.Trim();
            memberToSave.Phone = TxtPhone.Text.Trim();
            memberToSave.Status = (MemberStatus)ComboStatus.SelectedItem;
            memberToSave.Notes = TxtNotes.Text.Trim();

            memberToSave.AddressLine1 = TxtAddress1.Text.Trim();
            memberToSave.AddressLine2 = TxtAddress2.Text.Trim();
            memberToSave.AddressLine3 = TxtAddress3.Text.Trim();
            memberToSave.PinCode = TxtPinCode.Text.Trim();
            memberToSave.Gender = (ComboGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Male";
            memberToSave.BloodGroup = (ComboBloodGroup.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            memberToSave.Age = age;
            memberToSave.Height = height;
            memberToSave.Weight = weight;
            memberToSave.OpeningAmount = openingAmount;
            memberToSave.RegDate = DpRegDate.SelectedDate ?? DateTime.Today;
            memberToSave.JoinDate = DpJoinDate.SelectedDate ?? DateTime.Today;

            if (planSelected != null)
            {
                // If it's a new registration or plan changed, we assign it
                if (memberToSave.ActivePlanId != planSelected.Id)
                {
                    // If member is new, save them first to get a valid DB entry
                    if (isNew)
                    {
                        await service.SaveMemberAsync(memberToSave);
                    }
                    // Assign plan and auto-generate invoice
                    await service.AssignPlanToMemberAsync(memberToSave.Id, planSelected.Id, planSelected.Price);
                    
                    // Reload member details from DB
                    var reloaded = await service.GetMemberByIdAsync(memberToSave.Id);
                    if (reloaded != null) memberToSave = reloaded;
                }
            }
            else
            {
                memberToSave.ActivePlanId = null;
                memberToSave.ActivePlan = null;
                await service.SaveMemberAsync(memberToSave);
            }

            await RefreshMembersListAsync();
            ClearForm();
            HideEditor();
            MessageBox.Show("Member saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving member: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMember == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete member '{_selectedMember.FullName}'? All invoices associated with this member will remain, but the member record will be deleted.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await App.GymService.DeleteMemberAsync(_selectedMember.Id);
                await RefreshMembersListAsync();
                ClearForm();
                HideEditor();
                MessageBox.Show("Member deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting member: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
