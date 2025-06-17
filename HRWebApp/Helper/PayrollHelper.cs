using HRWebApp.Abstract;
using HRWebApp.Entities;

namespace HRWebApp.Helper
{
    public class PayrollHelper
    {
        private readonly IEntitiesRepository<AttendanceLog> _attendanceRepository;
        private readonly IEntitiesRepository<Holiday> _holidayRepository;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly IEntitiesRepository<PayrollRecord> _payrollRepository;

        public PayrollHelper(
            IEntitiesRepository<AttendanceLog> attendanceRepository,
            IEntitiesRepository<Holiday> holidayRepository,
            IEntitiesRepository<Employee> employeeRepository,
            IEntitiesRepository<PayrollRecord> payrollRepository)
        {
            _attendanceRepository = attendanceRepository;
            _holidayRepository = holidayRepository;
            _employeeRepository = employeeRepository;
            _payrollRepository = payrollRepository;
        }

        public PayrollCalculation CalculatePayroll(int employeeId, int year, int month)
        {
            var employee = _employeeRepository.Get(employeeId);
            if (employee == null) return null;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get attendance records for the month
            var attendanceRecords = _attendanceRepository.GetAll()
                .Where(a => a.EmployeeId == employeeId && 
                           a.Date >= startDate && 
                           a.Date <= endDate &&
                           a.ClockOut.HasValue)
                .ToList();

            // Get holidays for the month
            var holidays = _holidayRepository.GetAll()
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .Select(h => h.Date.Date)
                .ToList();

            decimal standardHours = 0;
            decimal holidayHours = 0;
            decimal sundayHours = 0;

            foreach (var record in attendanceRecords)
            {
                var workedHours = (decimal)(record.ClockOut.Value - record.ClockIn).TotalHours;
                var workDate = record.Date.Date;

                if (holidays.Contains(workDate))
                {
                    holidayHours += workedHours;
                }
                else if (workDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    sundayHours += workedHours;
                }
                else
                {
                    standardHours += workedHours;
                }
            }

            // Calculate pay
            var standardPay = standardHours * employee.HourlyRate;
            var holidayPay = holidayHours * employee.HourlyRate * 1.5m;
            var sundayPay = sundayHours * employee.HourlyRate * 1.75m;
            var grossSalary = standardPay + holidayPay + sundayPay;
            var netSalary = grossSalary; // No deductions for now

            return new PayrollCalculation
            {
                EmployeeId = employeeId,
                EmployeeName = $"{employee.firstName} {employee.lastName}",
                Year = year,
                Month = month,
                HourlyRate = employee.HourlyRate,
                StandardHours = Math.Round(standardHours, 2),
                HolidayHours = Math.Round(holidayHours, 2),
                SundayHours = Math.Round(sundayHours, 2),
                TotalHours = Math.Round(standardHours + holidayHours + sundayHours, 2),
                StandardPay = Math.Round(standardPay, 2),
                HolidayPay = Math.Round(holidayPay, 2),
                SundayPay = Math.Round(sundayPay, 2),
                GrossSalary = Math.Round(grossSalary, 2),
                NetSalary = Math.Round(netSalary, 2)
            };
        }

        public void SavePayrollRecord(PayrollCalculation calculation)
        {
            // Check if payroll already exists
            var existingPayroll = _payrollRepository.GetAll()
                .FirstOrDefault(p => p.EmployeeId == calculation.EmployeeId && 
                               p.Year == calculation.Year && 
                               p.Month == calculation.Month);

            if (existingPayroll != null)
            {
                // Update existing record
                existingPayroll.StandardHours = calculation.StandardHours;
                existingPayroll.HolidayHours = calculation.HolidayHours;
                existingPayroll.SundayHours = calculation.SundayHours;
                existingPayroll.TotalHours = calculation.TotalHours;
                existingPayroll.HourlyRate = calculation.HourlyRate;
                existingPayroll.StandardPay = calculation.StandardPay;
                existingPayroll.HolidayPay = calculation.HolidayPay;
                existingPayroll.SundayPay = calculation.SundayPay;
                existingPayroll.GrossSalary = calculation.GrossSalary;
                existingPayroll.NetSalary = calculation.NetSalary;
                existingPayroll.UpdatedAt = DateTime.Now;
                existingPayroll.IsCalculated = true;
                
                _payrollRepository.Update(existingPayroll);
            }
            else
            {
                // Create new record
                var payrollRecord = new PayrollRecord
                {
                    EmployeeId = calculation.EmployeeId,
                    Year = calculation.Year,
                    Month = calculation.Month,
                    StandardHours = calculation.StandardHours,
                    HolidayHours = calculation.HolidayHours,
                    SundayHours = calculation.SundayHours,
                    TotalHours = calculation.TotalHours,
                    HourlyRate = calculation.HourlyRate,
                    StandardPay = calculation.StandardPay,
                    HolidayPay = calculation.HolidayPay,
                    SundayPay = calculation.SundayPay,
                    GrossSalary = calculation.GrossSalary,
                    NetSalary = calculation.NetSalary,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsCalculated = true
                };
                
                _payrollRepository.Add(payrollRecord);
            }
        }
    }
} 