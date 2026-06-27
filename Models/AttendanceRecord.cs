using System;

namespace GymFinanceBillingWpf.Models;

public class AttendanceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MemberId { get; set; } = string.Empty;
    public Member? Member { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public DateTime CheckInTime { get; set; } = DateTime.Now;
    public DateTime? CheckOutTime { get; set; }

    public string DurationText
    {
        get
        {
            if (CheckOutTime == null) return "Active Session";
            var span = CheckOutTime.Value - CheckInTime;
            if (span.TotalHours >= 1)
            {
                return $"{(int)span.TotalHours}h {span.Minutes}m";
            }
            return $"{span.Minutes}m";
        }
    }
}
