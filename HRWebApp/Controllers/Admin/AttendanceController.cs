using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models;
using HRWebApp.Models.Admin;

namespace HRWebApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AttendanceController : Controller
    {
        private readonly IEntitiesRepository<AttendanceLog> _attendanceRepository;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(
            IEntitiesRepository<AttendanceLog> attendanceRepository,
            IEntitiesRepository<Employee> employeeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
            _userManager = userManager;
        }

        // Admin-specific monthly details view
        public IActionResult Index()
        {
            var currentDate = DateTime.Now;
            var model = new MonthlyAttendanceViewModel
            {
                SelectedMonth = currentDate.Month,
                SelectedYear = currentDate.Year,
                IsAdminView = true,
                Months = GetMonthSelectList(),
                Years = GetYearSelectList(),
                Employees = GetEmployeeSelectList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(MonthlyAttendanceViewModel model)
        {
            model.IsAdminView = true;
            model.Months = GetMonthSelectList();
            model.Years = GetYearSelectList();
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

            return View(model);
        }

        public IActionResult Create(int? employeeId = null)
        {
            var model = new AttendanceLogEditViewModel
            {
                Date = DateTime.Today,
                ClockInTime = new TimeSpan(9, 0, 0), // Default 9:00 AM
                EmployeeId = employeeId ?? 0,
                Employees = GetEmployeeSelectListForAdmin()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AttendanceLogEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate times
                if (model.ClockOutTime.HasValue && model.ClockOutTime <= model.ClockInTime)
                {
                    ModelState.AddModelError("ClockOutTime", "Clock out time must be after clock in time.");
                    model.Employees = GetEmployeeSelectListForAdmin();
                    return View(model);
                }

                var attendanceLog = new AttendanceLog
                {
                    EmployeeId = model.EmployeeId,
                    Date = model.Date.Date,
                    ClockIn = model.ClockInDateTime,
                    ClockOut = model.ClockOutDateTime
                };

                _attendanceRepository.Add(attendanceLog);
                return RedirectToAction("MonthlyDetails", "TimeClock", new
                {
                    employeeId = model.EmployeeId,
                    selectedMonth = model.Date.Month,
                    selectedYear = model.Date.Year
                });
            }

            model.Employees = GetEmployeeSelectListForAdmin();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var attendanceLog = _attendanceRepository.Get(id);
            if (attendanceLog == null)
            {
                return NotFound();
            }

            var employee = _employeeRepository.Get(attendanceLog.EmployeeId);
            var model = new AttendanceLogEditViewModel
            {
                Id = attendanceLog.Id,
                EmployeeId = attendanceLog.EmployeeId,
                Date = attendanceLog.Date,
                ClockInTime = attendanceLog.ClockIn.TimeOfDay,
                ClockOutTime = attendanceLog.ClockOut?.TimeOfDay,
                EmployeeName = $"{employee?.firstName} {employee?.lastName}",
                Employees = GetEmployeeSelectListForAdmin()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AttendanceLogEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate times
                if (model.ClockOutTime.HasValue && model.ClockOutTime <= model.ClockInTime)
                {
                    ModelState.AddModelError("ClockOutTime", "Clock out time must be after clock in time.");
                    model.Employees = GetEmployeeSelectListForAdmin();
                    return View(model);
                }

                var attendanceLog = _attendanceRepository.Get(model.Id);
                if (attendanceLog == null)
                {
                    return NotFound();
                }

                attendanceLog.EmployeeId = model.EmployeeId;
                attendanceLog.Date = model.Date.Date;
                attendanceLog.ClockIn = model.ClockInDateTime;
                attendanceLog.ClockOut = model.ClockOutDateTime;

                _attendanceRepository.Update(attendanceLog);
                return RedirectToAction("MonthlyDetails", "TimeClock", new
                {
                    employeeId = model.EmployeeId,
                    selectedMonth = model.Date.Month,
                    selectedYear = model.Date.Year
                });
            }

            model.Employees = GetEmployeeSelectListForAdmin();
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var attendanceLog = _attendanceRepository.Get(id);
            if (attendanceLog == null)
            {
                return NotFound();
            }

            var employee = _employeeRepository.Get(attendanceLog.EmployeeId);
            var model = new AttendanceLogEditViewModel
            {
                Id = attendanceLog.Id,
                EmployeeId = attendanceLog.EmployeeId,
                Date = attendanceLog.Date,
                ClockInTime = attendanceLog.ClockIn.TimeOfDay,
                ClockOutTime = attendanceLog.ClockOut?.TimeOfDay,
                EmployeeName = $"{employee?.firstName} {employee?.lastName}"
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var attendanceLog = _attendanceRepository.Get(id);
            if (attendanceLog != null)
            {
                var employeeId = attendanceLog.EmployeeId;
                var date = attendanceLog.Date;

                _attendanceRepository.Remove(attendanceLog);

                return RedirectToAction("MonthlyDetails", "TimeClock", new
                {
                    employeeId = employeeId,
                    selectedMonth = date.Month,
                    selectedYear = date.Year
                });
            }

            return RedirectToAction("MonthlyDetails", "TimeClock");
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

        private List<SelectListItem> GetEmployeeSelectListForAdmin()
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