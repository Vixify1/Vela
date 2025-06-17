using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models.Admin
{
    public class HolidayViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Holiday date is required")]
        [Display(Name = "Holiday Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Holiday description is required")]
        [Display(Name = "Holiday Name")]
        [StringLength(100, ErrorMessage = "Holiday name cannot exceed 100 characters.")]
        public string Description { get; set; } = string.Empty;

        // For display formatting
        public string FormattedDate => Date.ToString("MMMM dd, yyyy");
        public string DayOfWeek => Date.ToString("dddd");
    }
}