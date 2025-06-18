using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRWebApp.Models.Admin;
using HRWebApp.Helper;

namespace HRWebApp.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly IEntitiesRepository<Department> _departmentRepository;
        private readonly IEntitiesRepository<PayrollRecord> _payrollRepository;
        private readonly SalaryLetterHelper _salaryLetterHelper;

        public UserProfileController(
            UserManager<ApplicationUser> userManager,
            IEntitiesRepository<Employee> employeeRepository,
            IEntitiesRepository<Department> departmentRepository,
            IEntitiesRepository<PayrollRecord> payrollRepository,
            SalaryLetterHelper salaryLetterHelper)
        {
            _userManager = userManager;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _payrollRepository = payrollRepository;
            _salaryLetterHelper = salaryLetterHelper;
        }

        public async Task<IActionResult> Index()
        {
            // Get current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Check if there's a employee profile
            var employee = _employeeRepository.GetAll()
                .FirstOrDefault(c => c.UserId == user.Id);

            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                HasEmployeeProfile = employee != null,
                IsAdmin = isAdmin
            };

            if (employee != null)
            {
                model.EmployeeId = employee.Id;
                var department = _departmentRepository.Get(employee.DepartmentId);
                model.DepartmentName = department?.Name ?? "Not Assigned";
                model.HourlyRate = employee.HourlyRate;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            // Get current user first to check admin status
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Check if user is admin
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            model.IsAdmin = isAdmin; // Ensure model has correct admin status

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Update user properties
                currentUser.FirstName = model.FirstName;
                currentUser.LastName = model.LastName;
                currentUser.UpdatedOnUtc = DateTime.UtcNow;

                // Update the user
                var result = await _userManager.UpdateAsync(currentUser);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("Index", model);
                }

                // For non-admin users, also update their employee profile
                if (!isAdmin)
                {
                    var employee = _employeeRepository.GetAll()
                        .FirstOrDefault(c => c.UserId == currentUser.Id);

                    if (employee != null)
                    {
                        employee.firstName = model.FirstName;
                        employee.lastName = model.LastName;
                        employee.updatedAt = DateTime.Now;
                        _employeeRepository.Update(employee);
                    }
                }

                TempData["StatusMessage"] = "Your profile has been updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating your profile.");
                return View("Index", model);
            }
        }

        public async Task<IActionResult> Payroll(int? month, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Authentication");

            var employee = _employeeRepository.GetAll()
                .FirstOrDefault(e => e.UserId == user.Id);

            if (employee == null)
            {
                TempData["Error"] = "Employee record not found.";
                return RedirectToAction("Index");
            }

            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            var model = new PayrollListViewModel
            {
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear,
                IsAdminView = false,
                Months = GetMonthSelectList(),
                Years = GetYearSelectList()
            };

            // Load payroll records for this employee only
            var payrollQuery = _payrollRepository.GetAll(p => p.Employee)
                .Where(p => p.EmployeeId == employee.Id && 
                       p.Year == selectedYear && 
                       p.Month == selectedMonth);

            // First get the data from database
            var payrollData = payrollQuery.ToList();

            // Then create view models in memory
            var payrollRecords = payrollData.Select(p => new PayrollViewModel
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
                NetSalary = p.NetSalary,
                IsCalculated = p.IsCalculated
            }).ToList();

            model.PayrollRecords = payrollRecords;
            return View(model);
        }
        // Download Salary Letter
        public async Task<IActionResult> DownloadSalaryLetter(int payrollId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Authentication");

            var employee = _employeeRepository.GetAll()
                .FirstOrDefault(e => e.UserId == user.Id);

            if (employee == null)
            {
                TempData["Error"] = "Employee record not found.";
                return RedirectToAction("Index");
            }

            var payrollRecord = _payrollRepository.GetAll(p => p.Employee)
                .FirstOrDefault(p => p.Id == payrollId && p.EmployeeId == employee.Id);

            if (payrollRecord == null)
            {
                TempData["Error"] = "Payroll record not found or access denied.";
                return RedirectToAction("Payroll");
            }

            var department = _departmentRepository.Get(employee.DepartmentId);

            var model = new SalaryLetterViewModel
            {
                EmployeeId = payrollRecord.EmployeeId,
                EmployeeName = $"{payrollRecord.Employee.firstName} {payrollRecord.Employee.lastName}",
                EmployeeEmail = user.Email ?? "",
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

        // Helper methods (add to UserProfileController)
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

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                
                if (result.Succeeded)
                {
                    TempData["StatusMessage"] = "Your password has been changed successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while changing your password. Please try again.");
            }

            return View(model);
        }
    }
}