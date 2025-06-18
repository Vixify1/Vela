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

            // Calculate NET pay first (employee.HourlyRate is NET)
            var netStandardPay = standardHours * employee.HourlyRate;
            var netHolidayPay = holidayHours * employee.HourlyRate * 1.5m;
            var netSundayPay = sundayHours * employee.HourlyRate * 1.75m;
            var totalNetSalary = netStandardPay + netHolidayPay + netSundayPay;

            // Calculate the GROSS salary needed to achieve this NET salary
            var grossSalary = TaxHelper.CalculateGrossFromNet(totalNetSalary);
            
            // Calculate individual gross pay components proportionally
            var grossRatio = totalNetSalary > 0 ? grossSalary / totalNetSalary : 0;
            var grossStandardPay = Math.Round(netStandardPay * grossRatio, 2);
            var grossHolidayPay = Math.Round(netHolidayPay * grossRatio, 2);
            var grossSundayPay = Math.Round(netSundayPay * grossRatio, 2);

            // Calculate deductions from the gross salary
            var deductions = TaxHelper.CalculateAllDeductions(grossSalary);

            return new PayrollCalculation
            {
                EmployeeId = employeeId,
                EmployeeName = $"{employee.firstName} {employee.lastName}",
                Year = year,
                Month = month,
                HourlyRate = employee.HourlyRate, // This remains NET
                StandardHours = Math.Round(standardHours, 2),
                HolidayHours = Math.Round(holidayHours, 2),
                SundayHours = Math.Round(sundayHours, 2),
                TotalHours = Math.Round(standardHours + holidayHours + sundayHours, 2),
                StandardPay = grossStandardPay,
                HolidayPay = grossHolidayPay,
                SundayPay = grossSundayPay,
                GrossSalary = Math.Round(grossSalary, 2),
                SocialSecurityDeduction = deductions.socialSecurity,
                HealthInsuranceDeduction = deductions.healthInsurance,
                IncomeTaxDeduction = deductions.incomeTax,
                TotalDeductions = deductions.total,
                NetSalary = Math.Round(totalNetSalary, 2)
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
                existingPayroll.SocialSecurityDeduction = calculation.SocialSecurityDeduction;
                existingPayroll.HealthInsuranceDeduction = calculation.HealthInsuranceDeduction;
                existingPayroll.IncomeTaxDeduction = calculation.IncomeTaxDeduction;
                existingPayroll.TotalDeductions = calculation.TotalDeductions;
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
                    SocialSecurityDeduction = calculation.SocialSecurityDeduction,
                    HealthInsuranceDeduction = calculation.HealthInsuranceDeduction,
                    IncomeTaxDeduction = calculation.IncomeTaxDeduction,
                    TotalDeductions = calculation.TotalDeductions,
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