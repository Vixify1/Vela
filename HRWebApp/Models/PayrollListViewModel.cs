using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRWebApp.Models.Admin;

namespace HRWebApp.Models
{
    public class PayrollListViewModel
    {
        [Display(Name = "Select Month")]
        public int SelectedMonth { get; set; }
        
        [Display(Name = "Select Year")]
        public int SelectedYear { get; set; }
        
        [Display(Name = "Select Employee")]
        public int? SelectedEmployeeId { get; set; }
        
        public List<PayrollViewModel> PayrollRecords { get; set; } = new List<PayrollViewModel>();
        
        // For dropdowns
        public List<SelectListItem> Months { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Years { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
        
        // Summary
        public string MonthYearDisplay => new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM yyyy");
        public bool IsAdminView { get; set; }
    }
} 