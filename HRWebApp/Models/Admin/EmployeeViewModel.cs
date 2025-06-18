using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRWebApp.Models.Admin
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Hourly Rate is required")]
        [Display(Name = "Hourly Rate")]
        [Range(1, 100000, ErrorMessage = "Hourly Rate must be between 1 and 100,000 LEK")]
        public decimal HourlyRate { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        // For displaying in lists
        public string? DepartmentName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        // For dropdown lists
        public List<SelectListItem>? Departments { get; set; }

        // Helper property to determine if this is an edit operation
        public bool IsEdit => Id > 0;
    }
}