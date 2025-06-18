using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models.Admin;
using HRWebApp.Models;
using HRWebApp.Helper;

namespace HRWebApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class PayrollController : Controller
    {
        private readonly IEntitiesRepository<PayrollRecord> _payrollRepository;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly IEntitiesRepository<Department> _departmentRepository;
        private readonly PayrollHelper _payrollHelper;
        private readonly SalaryLetterHelper _salaryLetterHelper;

        public PayrollController(
            IEntitiesRepository<PayrollRecord> payrollRepository,
            IEntitiesRepository<Employee> employeeRepository,
            IEntitiesRepository<Department> departmentRepository,
            PayrollHelper payrollHelper,
            SalaryLetterHelper salaryLetterHelper)
        {
            _payrollRepository = payrollRepository;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _payrollHelper = payrollHelper;
            _salaryLetterHelper = salaryLetterHelper;
        }

        public IActionResult Index(int? selectedMonth, int? selectedYear, int? selectedEmployeeId)
        {
            var model = new PayrollListViewModel
            {
                SelectedMonth = selectedMonth ?? DateTime.Now.Month,
                SelectedYear = selectedYear ?? DateTime.Now.Year,
                SelectedEmployeeId = selectedEmployeeId,
                IsAdminView = true,
                Months = GetMonthSelectList(),
                Years = GetYearSelectList(),
                Employees = GetEmployeeSelectList()
            };

            LoadPayrollRecords(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(PayrollListViewModel model)
        {
            model.IsAdminView = true;
            model.Months = GetMonthSelectList();
            model.Years = GetYearSelectList();
            model.Employees = GetEmployeeSelectList();

            LoadPayrollRecords(model);
            return View(model);
        }

        public IActionResult Calculate(int? employeeId, int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            if (employeeId.HasValue)
            {
                // Calculate for specific employee
                var calculation = _payrollHelper.CalculatePayroll(employeeId.Value, selectedYear, selectedMonth);
                if (calculation != null)
                {
                    _payrollHelper.SavePayrollRecord(calculation);
                    TempData["Success"] = $"Payroll calculated for {calculation.EmployeeName}";
                }
                else
                {
                    TempData["Error"] = "No attendance data found for the selected employee and period.";
                }
            }
            else
            {
                // Calculate for all employees
                var employees = _employeeRepository.GetAll().ToList();
                int calculatedCount = 0;

                foreach (var employee in employees)
                {
                    var calculation = _payrollHelper.CalculatePayroll(employee.Id, selectedYear, selectedMonth);
                    if (calculation != null)
                    {
                        _payrollHelper.SavePayrollRecord(calculation);
                        calculatedCount++;
                    }
                }

                if (calculatedCount > 0)
                {
                    TempData["Success"] = $"Payroll calculated for {calculatedCount} employees for {new DateTime(selectedYear, selectedMonth, 1):MMMM yyyy}";
                }
                else
                {
                    TempData["Warning"] = $"No attendance data found for {new DateTime(selectedYear, selectedMonth, 1):MMMM yyyy}. Please check if employees have attendance records for this period.";
                }
            }

            // Redirect back to Index with the same month/year parameters
            return RedirectToAction("Index", new
            {
                selectedMonth = selectedMonth,
                selectedYear = selectedYear,
                selectedEmployeeId = employeeId
            });
        }

        private void LoadPayrollRecords(PayrollListViewModel model)
        {
            var query = _payrollRepository.GetAll(p => p.Employee)
                .Where(p => p.Year == model.SelectedYear && p.Month == model.SelectedMonth);

            if (model.SelectedEmployeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeId == model.SelectedEmployeeId.Value);
            }

            // First, get the data from database
            var payrollData = query.ToList();

            // Then, create the view models in memory (where string interpolation works)
            model.PayrollRecords = payrollData.Select(p => new PayrollViewModel
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeName = $"{p.Employee.firstName} {p.Employee.lastName}",
                Month = p.Month,
                Year = p.Year,
                HourlyRate = p.HourlyRate,
                StandardHours = p.StandardHours,
                HolidayHours = p.HolidayHours,
                SundayHours = p.SundayHours,
                TotalHours = p.TotalHours,
                StandardPay = p.StandardPay,
                HolidayPay = p.HolidayPay,
                SundayPay = p.SundayPay,
                GrossSalary = p.GrossSalary,
                SocialSecurityDeduction = p.SocialSecurityDeduction,
                HealthInsuranceDeduction = p.HealthInsuranceDeduction,
                IncomeTaxDeduction = p.IncomeTaxDeduction,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                IsCalculated = p.IsCalculated
            }).OrderBy(p => p.EmployeeName).ToList();
        }

        private List<SelectListItem> GetEmployeeSelectList()
        {
            var employees = _employeeRepository.GetAll(e => e.User).ToList();
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Employees" }
            };

            selectList.AddRange(employees.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.firstName} {e.lastName}"
            }));

            return selectList;
        }

        private List<SelectListItem> GetMonthSelectList()
        {
            return Enumerable.Range(1, 12).Select(month => new SelectListItem
            {
                Value = month.ToString(),
                Text = new DateTime(2024, month, 1).ToString("MMMM")
            }).ToList();
        }

        private List<SelectListItem> GetYearSelectList()
        {
            var currentYear = DateTime.Now.Year;
            return Enumerable.Range(currentYear - 2, 5).Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            }).ToList();
        }

        public IActionResult DownloadSalaryLetter(int payrollId)
        {
            var payrollRecord = _payrollRepository.GetAll(p => p.Employee)
                .FirstOrDefault(p => p.Id == payrollId);

            if (payrollRecord == null)
            {
                TempData["Error"] = "Payroll record not found.";
                return RedirectToAction("Index");
            }

            var department = _departmentRepository.Get(payrollRecord.Employee.DepartmentId);

            var model = new SalaryLetterViewModel
            {
                EmployeeId = payrollRecord.EmployeeId,
                EmployeeName = $"{payrollRecord.Employee.firstName} {payrollRecord.Employee.lastName}",
                EmployeeEmail = payrollRecord.Employee.User?.Email ?? "",
                DepartmentName = department?.Name ?? "Not Assigned",
                Month = payrollRecord.Month,
                Year = payrollRecord.Year,
                HourlyRate = payrollRecord.HourlyRate,
                StandardHours = payrollRecord.StandardHours,
                HolidayHours = payrollRecord.HolidayHours,
                SundayHours = payrollRecord.SundayHours,
                TotalHours = payrollRecord.TotalHours,
                StandardPay = payrollRecord.StandardPay,
                HolidayPay = payrollRecord.HolidayPay,
                SundayPay = payrollRecord.SundayPay,
                GrossSalary = payrollRecord.GrossSalary,
                SocialSecurityDeduction = payrollRecord.SocialSecurityDeduction,
                HealthInsuranceDeduction = payrollRecord.HealthInsuranceDeduction,
                IncomeTaxDeduction = payrollRecord.IncomeTaxDeduction,
                TotalDeductions = payrollRecord.TotalDeductions,
                NetSalary = payrollRecord.NetSalary
            };

            var pdfBytes = _salaryLetterHelper.GenerateSalaryLetterPdf(model);
            var fileName = $"SalaryLetter_{model.EmployeeName.Replace(" ", "_")}_{model.MonthYearDisplay.Replace(" ", "_")}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}