using System;
using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models
{
    public class TimeClockViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public bool IsCurrentlyClockedIn { get; set; }
        public DateTime? LastClockIn { get; set; }
        public DateTime? LastClockOut { get; set; }
    }
}