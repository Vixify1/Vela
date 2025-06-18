using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRWebApp.Models.Admin
{
    public class AttendanceLogEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Clock In time is required")]
        [Display(Name = "Clock In Time")]
        [DataType(DataType.Time)]
        public TimeSpan ClockInTime { get; set; }

        [Display(Name = "Clock Out Time")]
        [DataType(DataType.Time)]
        public TimeSpan? ClockOutTime { get; set; }

        // For display and dropdown
        public string? EmployeeName { get; set; }
        public List<SelectListItem>? Employees { get; set; }

        // Filter parameters for maintaining state
        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }

        // Calculated properties
        public DateTime ClockInDateTime => Date.Add(ClockInTime);
        public DateTime? ClockOutDateTime => ClockOutTime.HasValue ? Date.Add(ClockOutTime.Value) : null;

        public double? WorkedHours
        {
            get
            {
                if (ClockOutTime.HasValue)
                {
                    return Math.Round((ClockOutTime.Value - ClockInTime).TotalHours, 2);
                }
                return null;
            }
        }
    }
}