using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRWebApp.Models
{
    public class MonthlyAttendanceViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        [Display(Name = "Select Month")]
        public int SelectedMonth { get; set; }

        [Display(Name = "Select Year")]
        public int SelectedYear { get; set; }

        public List<AttendanceDetailViewModel> AttendanceRecords { get; set; } = new List<AttendanceDetailViewModel>();

        // For dropdowns
        public List<SelectListItem> Months { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Years { get; set; } = new List<SelectListItem>();

        // Summary information
        public int TotalDaysWorked => AttendanceRecords.Count(a => a.ClockOut.HasValue);
        public double TotalHoursWorked => AttendanceRecords.Where(a => a.WorkedHours.HasValue).Sum(a => a.WorkedHours.Value);
        public string MonthYearDisplay => new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM yyyy");

        // For admin view - to select different employees
        public bool IsAdminView { get; set; }
        public List<SelectListItem>? Employees { get; set; }
    }
}