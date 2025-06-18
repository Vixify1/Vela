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

        // Break down hours by type
        public decimal StandardHours { get; set; }
        public decimal HolidayHours { get; set; }
        public decimal SundayHours { get; set; }
        public decimal TotalHours { get; set; }

        // Pay calculations - HourlyRate remains NET
        public decimal HourlyRate { get; set; }
        public decimal StandardPay { get; set; }
        public decimal HolidayPay { get; set; }  // 1.5x rate
        public decimal SundayPay { get; set; }   // 1.75x rate
        public decimal GrossSalary { get; set; }
        
        // Albanian tax deductions
        public decimal SocialSecurityDeduction { get; set; } // 9.5%
        public decimal HealthInsuranceDeduction { get; set; } // 1.7%
        public decimal IncomeTaxDeduction { get; set; }
        public decimal TotalDeductions { get; set; }
        
        public decimal NetSalary { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsCalculated { get; set; }   // Flag to track if payroll is calculated
    }
}