using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HRWebApp.Abstract;
using HRWebApp.Entities;

namespace HRWebApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly IEntitiesRepository<AttendanceLog> _attendanceRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IEntitiesRepository<Employee> employeeRepository,
            IEntitiesRepository<AttendanceLog> attendanceRepository,
            ILogger<DashboardController> logger)
        {
            _userManager = userManager;
            _employeeRepository = employeeRepository;
            _attendanceRepository = attendanceRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";
            ViewBag.UserEmail = user.Email;
            
            var today = DateTime.Today;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            if (User.IsInRole("Admin"))
            {
                // Admin stats - company-wide data
                var totalEmployees = _employeeRepository.GetAll().Count();
                ViewBag.TotalEmployees = totalEmployees;
                ViewBag.ActiveEmployees = totalEmployees;
                ViewBag.TodayAttendance = _attendanceRepository.GetAll()
                    .Where(a => a.ClockIn != default(DateTime) && a.ClockIn.Date == today).Count();
            }
            else
            {
                // Employee stats - personal data only
                var currentEmployee = _employeeRepository.GetAll()
                    .FirstOrDefault(e => e.UserId == user.Id);
                
                if (currentEmployee != null)
                {
                    // Days worked this month
                    ViewBag.DaysWorkedThisMonth = _attendanceRepository.GetAll()
                        .Where(a => a.EmployeeId == currentEmployee.Id && 
                               a.ClockIn.Month == currentMonth && 
                               a.ClockIn.Year == currentYear)
                        .Count();
                    
                    // Hours worked this week
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    var weeklyAttendance = _attendanceRepository.GetAll()
                        .Where(a => a.EmployeeId == currentEmployee.Id && 
                               a.ClockIn.Date >= startOfWeek && 
                               a.ClockIn.Date <= today &&
                               a.ClockOut.HasValue)
                        .ToList(); // Bring data to client side first
                    
                    var hoursThisWeek = weeklyAttendance
                        .Sum(a => (a.ClockOut.Value - a.ClockIn).TotalHours);
                    
                    ViewBag.HoursThisWeek = Math.Round(hoursThisWeek, 1);
                    
                    // Check if clocked in today
                    ViewBag.ClockedInToday = _attendanceRepository.GetAll()
                        .Any(a => a.EmployeeId == currentEmployee.Id && a.ClockIn.Date == today);
                }
                else
                {
                    ViewBag.DaysWorkedThisMonth = 0;
                    ViewBag.HoursThisWeek = 0.0;
                    ViewBag.ClockedInToday = false;
                }
                
                ViewBag.TodayAttendance = _attendanceRepository.GetAll()
                    .Where(a => a.ClockIn != default(DateTime) && a.ClockIn.Date == today).Count();
            }

            return View();
        }
    }
}