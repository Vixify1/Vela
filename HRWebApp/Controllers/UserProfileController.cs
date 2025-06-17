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
        private readonly IEntitiesRepository<Department> _departmentRepository;

        public UserProfileController(
            UserManager<ApplicationUser> userManager,
            IEntitiesRepository<Employee> employeeRepository,
            IEntitiesRepository<Department> departmentRepository)
        {
            _userManager = userManager;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
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

                // For admin users, update the username to reflect the new full name
                //if (isAdmin)
                //{
                //    var newUserName = $"{model.FirstName} {model.LastName}";
                //    currentUser.UserName = newUserName;
                //    currentUser.NormalizedUserName = newUserName.ToUpper();
                //}

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
    }
}

        //        [HttpGet]
        //        public async Task<IActionResult> Edit()
        //        {
        //            var user = await _userManager.GetUserAsync(User);
        //            if (user == null)
        //            {
        //                return NotFound();
        //            }

        //            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        //            if (!isAdmin)
        //            {
        //                return RedirectToAction(nameof(Index));
        //            }

        //            var model = new UserProfileViewModel
        //            {
        //                UserId = user.Id,
        //                UserName = $"{user.FirstName} {user.LastName}",
        //                Email = user.Email,
        //                FirstName = user.FirstName,
        //                LastName = user.LastName,
        //                IsAdmin = true
        //            };

        //            return View(model);
        //        }

        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<IActionResult> Edit(UserProfileViewModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                model.IsAdmin = true;
        //                return View(model);
        //            }

        //            var user = await _userManager.GetUserAsync(User);
        //            if (user == null)
        //            {
        //                return NotFound();
        //            }

        //            user.FirstName = model.FirstName;
        //            user.LastName = model.LastName;
        //            user.Email = model.Email;
        //            user.UpdatedOnUtc = DateTime.UtcNow;

        //            var result = await _userManager.UpdateAsync(user);
        //            if (!result.Succeeded)
        //            {
        //                foreach (var error in result.Errors)
        //                {
        //                    ModelState.AddModelError("", error.Description);
        //                }
        //                model.IsAdmin = true;
        //                return View(model);
        //            }

        //            TempData["StatusMessage"] = "Your profile has been updated";
        //            return RedirectToAction(nameof(Index));
        //        }
        //    }
    ///}