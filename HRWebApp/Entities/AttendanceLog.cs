using System;
using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Entities
{
    public class AttendanceLog
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
    }
}
