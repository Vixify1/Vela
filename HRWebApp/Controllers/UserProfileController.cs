using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HRWebApp.Controllers
{
    [Authorize] // Ensure user is logged in
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEntitiesRepository<Employee> _employeeRepository;

        public UserProfileController(
            UserManager<ApplicationUser> userManager,
            IEntitiesRepository<Employee> employeeRepository)
        {
            _userManager = userManager;
            _employeeRepository = employeeRepository;
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
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                HasEmployeeProfile = employee != null,
                IsAdmin = isAdmin
            };

            if (employee != null)
            {
                model.EmployeeId = employee.Id;
                model.Address = employee.Address;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Get current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Update user
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdatedOnUtc = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Index", model);
            }

            // Find or create employee profile
            var employee = _employeeRepository.GetAll()
                .FirstOrDefault(c => c.UserId == user.Id);

            if (employee == null)
            {
                // Create new employee profile
                employee = new Employee
                {
                    UserId = user.Id,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    Address = model.Address,
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now
                };
                _employeeRepository.Add(employee);
            }
            else
            {
                // Update existing employee
                employee.firstName = model.FirstName;
                employee.lastName = model.LastName;
                employee.Address = model.Address;
                employee.updatedAt = DateTime.Now;
                _employeeRepository.Update(employee);
            }

            TempData["StatusMessage"] = "Your profile has been updated";
            return RedirectToAction(nameof(Index));
        }
    }
}