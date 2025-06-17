using System;

namespace HRWebApp.Entities
{
    public class PayrollCalculation
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal HourlyRate { get; set; }
        
        public decimal StandardHours { get; set; }
        public decimal HolidayHours { get; set; }
        public decimal SundayHours { get; set; }
        public decimal TotalHours { get; set; }
        
        public decimal StandardPay { get; set; }
        public decimal HolidayPay { get; set; }
        public decimal SundayPay { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
    }
} 