using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRWebApp.Models.Admin
{
    public class PayrollViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        
        [Display(Name = "Month")]
        public int Month { get; set; }
        
        [Display(Name = "Year")]
        public int Year { get; set; }
        
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }
        
        [Display(Name = "Standard Hours")]
        public decimal StandardHours { get; set; }
        
        [Display(Name = "Holiday Hours (1.5x)")]
        public decimal HolidayHours { get; set; }
        
        [Display(Name = "Sunday Hours (1.75x)")]
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
        
        public bool IsCalculated { get; set; }
        
        // For dropdowns
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Months { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Years { get; set; } = new List<SelectListItem>();
        
        // Display helpers
        public string MonthYearDisplay => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }
} 