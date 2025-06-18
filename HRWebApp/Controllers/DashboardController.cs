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
            
            // Get some basic stats for the dashboard
            var totalEmployees = _employeeRepository.GetAll().Count();
            ViewBag.TotalEmployees = totalEmployees;
            
            // For now, let's consider all employees as active
            // You can add more complex logic later if needed (e.g., based on employment status)
            ViewBag.ActiveEmployees = totalEmployees;
            
            var today = DateTime.Today;
            ViewBag.TodayAttendance = _attendanceRepository.GetAll()
                .Where(a => a.ClockIn != default(DateTime) && a.ClockIn.Date == today).Count();

            return View();
        }
    }
}