using System;
using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Entities
{
    public class PayrollRecord
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        public decimal TotalHours { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
    }
}