using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;
using System.IO;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Drawing;

namespace GymFinanceBillingWpf.Views;

public partial class AttendanceView : UserControl
{
    private List<AttendanceRecord> _activeSessions = new();
    private List<AttendanceRecord> _dailyHistory = new();
    private bool _isLoading = false;
    
    // Webcam Scanning Fields
    private FilterInfoCollection? _videoDevices;
    private VideoCaptureDevice? _videoSource;
    private BarcodeReader? _qrReader;
    private DateTime _lastScanTime = DateTime.MinValue;

    public AttendanceView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Initialize QR reader settings
        _qrReader = new BarcodeReader
        {
            AutoRotate = true,
            Options = new ZXing.Common.DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                TryHarder = true
            }
        };
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Enumerate webcam devices
        try
        {
            ComboCameras.Items.Clear();
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_videoDevices.Count > 0)
            {
                foreach (FilterInfo device in _videoDevices)
                {
                    ComboCameras.Items.Add(device.Name);
                }
                ComboCameras.SelectedIndex = 0;
            }
            else
            {
                ComboCameras.Items.Add("No Webcams Found");
                ComboCameras.SelectedIndex = 0;
                ComboCameras.IsEnabled = false;
                BtnToggleCamera.IsEnabled = false;
            }
        }
        catch
        {
            ComboCameras.Items.Add("Camera Load Failed");
            ComboCameras.SelectedIndex = 0;
            ComboCameras.IsEnabled = false;
            BtnToggleCamera.IsEnabled = false;
        }

        if (DpAttendanceDate != null)
        {
            DpAttendanceDate.SelectedDateChanged -= DpAttendanceDate_SelectedDateChanged;
            DpAttendanceDate.SelectedDate = DateTime.Today;
            DpAttendanceDate.SelectedDateChanged += DpAttendanceDate_SelectedDateChanged;
        }
        await RefreshListsAsync();
    }

    private async Task RefreshListsAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var service = App.GymService;
            
            // 1. Refresh active checked-in members list
            _activeSessions = await service.GetActiveSessionsAsync() ?? new List<AttendanceRecord>();
            GridActive.ItemsSource = null;
            GridActive.ItemsSource = _activeSessions;

            // 2. Refresh day history logs
            var date = DpAttendanceDate.SelectedDate ?? DateTime.Today;
            _dailyHistory = await service.GetAttendanceRecordsByDateAsync(date) ?? new List<AttendanceRecord>();
            GridHistory.ItemsSource = null;
            GridHistory.ItemsSource = _dailyHistory;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading attendance lists: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void BtnCheckIn_Click(object sender, RoutedEventArgs e)
    {
        await ProcessCheckInAsync();
    }

    private async void BtnCheckOut_Click(object sender, RoutedEventArgs e)
    {
        await ProcessCheckOutAsync();
    }

    private async void TxtRegNo_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // By default, try to Check-in if not already inside. If they are already checked in, check them out.
            var regNo = TxtRegNo.Text.Trim();
            if (string.IsNullOrEmpty(regNo)) return;

            var service = App.GymService;
            var activeSession = _activeSessions.FirstOrDefault(s => s.Member?.RegNo == regNo);

            if (activeSession != null)
            {
                await ProcessCheckOutAsync();
            }
            else
            {
                await ProcessCheckInAsync();
            }
        }
    }

    private async Task ProcessCheckInAsync()
    {
        ResetAlerts();
        var regNo = TxtRegNo.Text.Trim();
        if (string.IsNullOrEmpty(regNo))
        {
            MessageBox.Show("Please enter a valid Registration Number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var service = App.GymService;
            
            // Check if member exists and retrieve status first
            var members = await service.GetAllMembersAsync();
            var member = members.FirstOrDefault(m => m.RegNo == regNo);
            
            if (member == null)
            {
                ShowAlert("Member registration record not found.");
                return;
            }

            // Populate preview panel
            ShowPreview(member);

            // Verify membership status
            if (member.Status == MemberStatus.Suspended)
            {
                ShowAlert("Membership account has been SUSPENDED. Check-in denied.");
                return;
            }

            if (member.Status == MemberStatus.Expired || member.ActivePlan == null)
            {
                ShowAlert("Membership subscription plan is EXPIRED. Check-in denied.");
                return;
            }

            // Execute check-in
            await service.CheckInMemberAsync(regNo);
            
            ShowSuccess("Check-in registered successfully!");
            TxtRegNo.Text = string.Empty;
            await RefreshListsAsync();
        }
        catch (InvalidOperationException ex)
        {
            ShowAlert(ex.Message);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Check-in failed: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ProcessCheckOutAsync()
    {
        ResetAlerts();
        var regNo = TxtRegNo.Text.Trim();
        if (string.IsNullOrEmpty(regNo))
        {
            MessageBox.Show("Please enter a valid Registration Number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var service = App.GymService;
            var result = await service.CheckOutMemberAsync(regNo);
            
            if (result == null)
            {
                ShowAlert("Failed to check-out. Record not found.");
                return;
            }

            if (result.Member != null)
            {
                ShowPreview(result.Member);
            }
            
            ShowSuccess("Check-out registered successfully!");
            TxtRegNo.Text = string.Empty;
            await RefreshListsAsync();
        }
        catch (InvalidOperationException ex)
        {
            ShowAlert(ex.Message);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Check-out failed: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void GridCheckOut_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string regNo)
        {
            TxtRegNo.Text = regNo;
            await ProcessCheckOutAsync();
        }
    }

    private async void DpAttendanceDate_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DpAttendanceDate == null) return;
        await RefreshListsAsync();
    }

    #region Layout Styling and Preview Utilities

    private void ShowPreview(Member member)
    {
        TxtScanHelp.Visibility = Visibility.Collapsed;
        PanelScanPreview.Visibility = Visibility.Visible;

        TxtPreviewName.Text = member.FullName;
        TxtPreviewMeta.Text = $"Reg No: {member.RegNo} | Phone: {member.Phone}";

        // Configure subscription badging
        if (member.Status == MemberStatus.Active)
        {
            BadgeMemberStatus.Background = (SolidColorBrush)FindResource("StatusPaidBg");
            TxtMemberStatus.Foreground = (SolidColorBrush)FindResource("StatusPaid");
            TxtMemberStatus.Text = "ACTIVE";
        }
        else if (member.Status == MemberStatus.Expired)
        {
            BadgeMemberStatus.Background = (SolidColorBrush)FindResource("StatusOverdueBg");
            TxtMemberStatus.Foreground = (SolidColorBrush)FindResource("StatusOverdue");
            TxtMemberStatus.Text = "EXPIRED";
        }
        else
        {
            BadgeMemberStatus.Background = (SolidColorBrush)FindResource("StatusUnpaidBg");
            TxtMemberStatus.Foreground = (SolidColorBrush)FindResource("StatusUnpaid");
            TxtMemberStatus.Text = "SUSPENDED";
        }

        // Configure plan naming status
        if (member.ActivePlan != null)
        {
            TxtPlanStatus.Text = member.ActivePlan.Name;
        }
        else
        {
            TxtPlanStatus.Text = "No Active Plan";
        }
    }

    private void ShowAlert(string msg)
    {
        BorderAlert.Visibility = Visibility.Visible;
        TxtAlertMessage.Text = msg;
        BorderSuccess.Visibility = Visibility.Collapsed;
    }

    private void ShowSuccess(string msg)
    {
        BorderSuccess.Visibility = Visibility.Visible;
        TxtSuccessMessage.Text = msg;
        BorderAlert.Visibility = Visibility.Collapsed;
    }

    private void ResetAlerts()
    {
        BorderAlert.Visibility = Visibility.Collapsed;
        BorderSuccess.Visibility = Visibility.Collapsed;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopCamera();
    }

    private void BtnToggleCamera_Click(object sender, RoutedEventArgs e)
    {
        if (_videoSource != null && _videoSource.IsRunning)
        {
            StopCamera();
            BtnToggleCamera.Content = "Start";
            BorderCamFeed.Visibility = Visibility.Collapsed;
        }
        else
        {
            StartCamera();
        }
    }

    private void StartCamera()
    {
        if (_videoDevices == null || _videoDevices.Count == 0 || ComboCameras.SelectedIndex == -1) return;
        try
        {
            var selectedDevice = _videoDevices[ComboCameras.SelectedIndex].MonikerString;
            _videoSource = new VideoCaptureDevice(selectedDevice);
            
            // Set the highest available video resolution
            if (_videoSource.VideoCapabilities != null && _videoSource.VideoCapabilities.Length > 0)
            {
                var bestResolution = _videoSource.VideoCapabilities
                    .OrderByDescending(c => c.FrameSize.Width * c.FrameSize.Height)
                    .FirstOrDefault();
                
                if (bestResolution != null)
                {
                    _videoSource.VideoResolution = bestResolution;
                }
            }

            _videoSource.NewFrame += OnNewFrame;
            _videoSource.Start();
            BtnToggleCamera.Content = "Stop";
            BorderCamFeed.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start camera: {ex.Message}", "Camera Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopCamera()
    {
        try
        {
            if (_videoSource != null)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= OnNewFrame;
                _videoSource = null;
            }
        }
        catch { }
        ImgCamFeed.Source = null;
    }

    private void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        try
        {
            using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
            {
                // Render camera feed frame in WPF
                var bitmapImage = ConvertBitmapToBitmapImage(bitmap);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ImgCamFeed.Source = bitmapImage;
                }));

                // Rate-limit QR decoding scans to prevent accidental duplicate actions
                if (DateTime.Now - _lastScanTime < TimeSpan.FromSeconds(3.0)) return;

                var result = _qrReader?.Decode(bitmap);
                if (result == null)
                {
                    // Fallback: Webcams often mirror-flip the image. Flip horizontally and try again!
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    result = _qrReader?.Decode(bitmap);
                }

                if (result != null)
                {
                    var regNo = result.Text.Trim();
                    _lastScanTime = DateTime.Now;

                    Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        TxtRegNo.Text = regNo;
                        
                        // Play feedback beep sound
                        System.Media.SystemSounds.Asterisk.Play();

                        // Register check-in or check-out immediately
                        var activeSession = _activeSessions.FirstOrDefault(s => s.Member?.RegNo == regNo);
                        if (activeSession != null)
                        {
                            await ProcessCheckOutAsync();
                        }
                        else
                        {
                            await ProcessCheckInAsync();
                        }
                    }));
                }
            }
        }
        catch { }
    }

    private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
    #endregion
}
