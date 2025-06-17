using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models
{
    public class SalaryLetterViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "Month")]
        public int Month { get; set; }

        [Display(Name = "Year")]
        public int Year { get; set; }

        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Standard Hours")]
        public decimal StandardHours { get; set; }

        [Display(Name = "Holiday Hours")]
        public decimal HolidayHours { get; set; }

        [Display(Name = "Sunday Hours")]
        public decimal SundayHours { get; set; }

        [Display(Name = "Total Hours")]
        public decimal TotalHours { get; set; }

        [Display(Name = "Standard Pay")]
        public decimal StandardPay { get; set; }

        [Display(Name = "Holiday Pay")]
        public decimal HolidayPay { get; set; }

        [Display(Name = "Sunday Pay")]
        public decimal SundayPay { get; set; }

        [Display(Name = "Gross Salary")]
        public decimal GrossSalary { get; set; }

        [Display(Name = "Net Salary")]
        public decimal NetSalary { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        // Display helpers
        public string MonthYearDisplay => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
        public string CompanyName => "HR Web Application Company";
        public string CompanyAddress => "123 Business Street, City, State 12345";
    }
}