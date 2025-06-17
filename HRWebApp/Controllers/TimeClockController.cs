using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRWebApp.Controllers
{
    [Authorize]
    public class TimeClockController : Controller
    {
        private readonly IEntitiesRepository<AttendanceLog> _attendanceRepository;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimeClockController(
            IEntitiesRepository<AttendanceLog> attendanceRepository,
            IEntitiesRepository<Employee> employeeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Authentication");

            var employee = _employeeRepository.GetAll()
                .FirstOrDefault(e => e.UserId == user.Id);

            if (employee == null)
                return RedirectToAction("Index", "Home");

            var today = DateTime.Today;
            var currentLog = _attendanceRepository.GetAll()
                .Where(a => a.EmployeeId == employee.Id && a.Date == today)
                .OrderByDescending(a => a.ClockIn)
                .FirstOrDefault();

            var viewModel = new TimeClockViewModel
            {
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.firstName} {employee.lastName}",
                IsCurrentlyClockedIn = currentLog != null && !currentLog.ClockOut.HasValue,
                LastClockIn = currentLog?.ClockIn,
                LastClockOut = currentLog?.ClockOut
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ClockIn()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Authentication");

            var employee = _employeeRepository.GetAll()
                .FirstOrDefault(e => e.UserId == user.Id);

            if (employee == null)
                return RedirectToAction("Index", "Home");

            var today = DateTime.Today;
            var now = DateTime.Now;

            var attendanceLog = new AttendanceLog
            {
                EmployeeId = employee.Id,
                Date = today,
                ClockIn = now
            };

            _attendanceRepository.Add(attendanceLog);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClockOut()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Authentication");

            var employee = _employeeRepository.GetAll()
                .FirstOrDefault(e => e.UserId == user.Id);

            if (employee == null)
                return RedirectToAction("Index", "Home");

            var today = DateTime.Today;
            var currentLog = _attendanceRepository.GetAll()
                .FirstOrDefault(a => a.EmployeeId == employee.Id &&
                                   a.Date == today &&
                                   !a.ClockOut.HasValue);

            if (currentLog != null)
            {
                currentLog.ClockOut = DateTime.Now;
                _attendanceRepository.Update(currentLog);
            }

            return RedirectToAction("Index");
        }

        // MonthlyAttendance - Updated to handle query parameters
        public async Task<IActionResult> MonthlyDetails(int? employeeId = null, int? selectedMonth = null, int? selectedYear = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Authentication");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var currentDate = DateTime.Now;

            var model = new MonthlyAttendanceViewModel
            {
                SelectedMonth = selectedMonth ?? currentDate.Month,
                SelectedYear = selectedYear ?? currentDate.Year,
                IsAdminView = isAdmin,
                Months = GetMonthSelectList(),
                Years = GetYearSelectList()
            };

            if (isAdmin)
            {
                // Admin can see all employees
                model.Employees = GetEmployeeSelectList();

                // If employeeId is provided from redirect, select that employee
                if (employeeId.HasValue && employeeId.Value > 0)
                {
                    model.EmployeeId = employeeId.Value;
                    var selectedEmployee = _employeeRepository.Get(employeeId.Value);
                    if (selectedEmployee != null)
                    {
                        model.EmployeeName = $"{selectedEmployee.firstName} {selectedEmployee.lastName}";
                        LoadAttendanceRecords(model);
                    }
                }
                else
                {
                    model.EmployeeId = 0; // No employee selected initially
                }
            }
            else
            {
                // Regular employee can only see their own records
                var employee = _employeeRepository.GetAll()
                    .FirstOrDefault(e => e.UserId == user.Id);

                if (employee == null)
                    return RedirectToAction("Index", "Home");

                model.EmployeeId = employee.Id;
                model.EmployeeName = $"{employee.firstName} {employee.lastName}";

                // Load attendance records for current month
                LoadAttendanceRecords(model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MonthlyDetails(MonthlyAttendanceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Authentication");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            model.IsAdminView = isAdmin;

            // Reload dropdown lists
            model.Months = GetMonthSelectList();
            model.Years = GetYearSelectList();

            if (isAdmin)
            {
                model.Employees = GetEmployeeSelectList();

                if (model.EmployeeId > 0)
                {
                    var selectedEmployee = _employeeRepository.Get(model.EmployeeId);
                    if (selectedEmployee != null)
                    {
                        model.EmployeeName = $"{selectedEmployee.firstName} {selectedEmployee.lastName}";
                        LoadAttendanceRecords(model);
                    }
                }
            }
            else
            {
                // Regular employee - ensure they can only see their own records
                var employee = _employeeRepository.GetAll()
                    .FirstOrDefault(e => e.UserId == user.Id);

                if (employee != null)
                {
                    model.EmployeeId = employee.Id;
                    model.EmployeeName = $"{employee.firstName} {employee.lastName}";
                    LoadAttendanceRecords(model);
                }
            }

            return View(model);
        }

        // Helper methods
        private void LoadAttendanceRecords(MonthlyAttendanceViewModel model)
        {
            if (model.EmployeeId > 0)
            {
                var startDate = new DateTime(model.SelectedYear, model.SelectedMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                model.AttendanceRecords = _attendanceRepository.GetAll()
                    .Where(a => a.EmployeeId == model.EmployeeId &&
                               a.Date >= startDate &&
                               a.Date <= endDate)
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.ClockIn)
                    .Select(a => new AttendanceDetailViewModel
                    {
                        Id = a.Id,
                        EmployeeId = a.EmployeeId,
                        EmployeeName = model.EmployeeName,
                        Date = a.Date,
                        ClockIn = a.ClockIn,
                        ClockOut = a.ClockOut
                    })
                    .ToList();
            }
        }

        private List<SelectListItem> GetMonthSelectList()
        {
            return Enumerable.Range(1, 12)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = new DateTime(2024, i, 1).ToString("MMMM")
                }).ToList();
        }

        private List<SelectListItem> GetYearSelectList()
        {
            var currentYear = DateTime.Now.Year;
            return Enumerable.Range(currentYear - 2, 5) // Current year +/- 2 years
                .Select(year => new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                }).ToList();
        }

        private List<SelectListItem> GetEmployeeSelectList()
        {
            return _employeeRepository.GetAll()
                .OrderBy(e => e.firstName)
                .ThenBy(e => e.lastName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.firstName} {e.lastName}"
                }).ToList();
        }
    }
}