using HRWebApp.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models
{
    public class EmployeeListViewModel
    {
        [Display(Name = "First Name")]
        public string FirstNameFilter { get; set; }

        [Display(Name = "Last Name")]
        public string LastNameFilter { get; set; }

        public List<EmployeeViewModel> Employees { get; set; } = new List<EmployeeViewModel>();

        public int TotalEmployees => Employees.Count;

        public bool HasFilters => !string.IsNullOrEmpty(FirstNameFilter) || !string.IsNullOrEmpty(LastNameFilter);
    }
}