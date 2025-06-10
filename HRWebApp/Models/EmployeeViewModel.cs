using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HRWebApp.Entities;


namespace HRWebApp.Models
{
    public class EmployeeViewModel
    {

        [Display(Name = "Active")]
        public bool IsActive;

        public int EmployeeId { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; }

        [Required]

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";


        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Address")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = false)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated On")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = false)]
        public DateTime? UpdatedAt { get; set; }

    }
}