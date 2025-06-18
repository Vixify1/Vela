using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models.Admin;
public class DepartmentViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Department name is required")]
    [Display(Name = "Department Name")]
    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string Name { get; set; }

    public int EmployeeCount { get; set; }
}