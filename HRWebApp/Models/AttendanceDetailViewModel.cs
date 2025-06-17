using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models
{
    public class AttendanceDetailViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Clock In")]
        public DateTime ClockIn { get; set; }

        [Display(Name = "Clock Out")]
        public DateTime? ClockOut { get; set; }

        // Calculated properties
        public string FormattedDate => Date.ToString("dddd, MMMM dd, yyyy");
        public string ClockInTime => ClockIn.ToString("HH:mm");
        public string ClockOutTime => ClockOut?.ToString("HH:mm") ?? "Not clocked out";
        public string DayOfWeek => Date.ToString("dddd");

        public double? WorkedHours
        {
            get
            {
                if (ClockOut.HasValue)
                {
                    return Math.Round((ClockOut.Value - ClockIn).TotalHours, 2);
                }
                return null;
            }
        }

        public string Status => ClockOut.HasValue ? "Complete" : "In Progress";
    }
}