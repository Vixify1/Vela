using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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
    }
}