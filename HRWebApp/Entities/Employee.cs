using HRWebApp.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRWebApp.Entities
{

    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }

        public string? Address { get; set; }




        [Required]
        public decimal HourlyRate { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<AttendanceLog> AttendanceLogs { get; set; }
        public ICollection<PayrollRecord> PayrollRecords { get; set; }





        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

    }
}
