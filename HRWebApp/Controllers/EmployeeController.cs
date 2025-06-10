using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models;

namespace HRWebApp.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeController(
            IEntitiesRepository<Employee> employeeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _employeeRepository = employeeRepository;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string FirstNameFilter, string LastNameFilter)
        {
            // First get the base query from the repository
            var employeesQuery = _employeeRepository.GetAll();

            // Materialize the data first, then apply filters in memory
            var employees = employeesQuery.ToList();

            // Now filter in memory with your original logic
            if (!string.IsNullOrEmpty(FirstNameFilter))
            {
                employees = employees.Where(c => c.firstName != null &&
                    c.firstName.Contains(FirstNameFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(LastNameFilter))
            {
                employees = employees.Where(c => c.lastName != null &&
                    c.lastName.Contains(LastNameFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }


            var employeeViewModels = new List<EmployeeViewModel>();
            foreach (var employee in employees)
            {
                var employeeViewModel = new EmployeeViewModel
                {
                    EmployeeId = employee.Id,
                    UserId = employee.UserId,
                    FirstName = employee.firstName,
                    LastName = employee.lastName,
                    Address = employee.Address,
                    CreatedAt = employee.createdAt,
                    UpdatedAt = employee.updatedAt
                };

                var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
                if (user != null)
                {
                    employeeViewModel.Email = user.Email;
                    employeeViewModel.IsActive = user.IsActive;
                }

                employeeViewModels.Add(employeeViewModel);
            }

            var listViewModel = new EmployeeListViewModel
            {
                FirstNameFilter = FirstNameFilter,
                LastNameFilter = LastNameFilter,
                Employees = employeeViewModels
            };

            return View(listViewModel);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            // Get users who don't already have a employee profile
            var existingEmployeeUserIds = _employeeRepository.GetAll().Select(c => c.UserId).ToList();
            var availableUsers = await _userManager.Users
                .Where(u => !existingEmployeeUserIds.Contains(u.Id))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.LastName}, {u.FirstName} ({u.Email})"
                })
                .ToListAsync();

            ViewBag.AvailableUsers = availableUsers;

            return View(new EmployeeViewModel());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var existingEmployeeUserIds = _employeeRepository.GetAll().Select(c => c.UserId).ToList();
                var availableUsers = await _userManager.Users
                    .Where(u => !existingEmployeeUserIds.Contains(u.Id))
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.LastName}, {u.FirstName} ({u.Email})"
                    })
                    .ToListAsync();

                ViewBag.AvailableUsers = availableUsers;
                return View(model);
            }

            // Check if this user already has a employee profile
            var existingEmployee = _employeeRepository.GetAll()
                .FirstOrDefault(c => c.UserId == model.UserId);

            if (existingEmployee != null)
            {
                ModelState.AddModelError("UserId", "This user already has a employee profile");
                return View(model);
            }

            // Get user info to sync with employee
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                ModelState.AddModelError("UserId", "User not found");
                return View(model);
            }

            var employee = new Employee
            {
                UserId = model.UserId,
                firstName = model.FirstName ?? user.FirstName, // Use model data or fallback to user data
                lastName = model.LastName ?? user.LastName,
                Address = model.Address,
                createdAt = DateTime.Now,
                updatedAt = DateTime.Now
            };

            _employeeRepository.Add(employee);
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var employee = _employeeRepository.Get(id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeViewModel
            {
                EmployeeId = employee.Id,
                UserId = employee.UserId,
                FirstName = employee.firstName,
                LastName = employee.lastName,
                Address = employee.Address,
                CreatedAt = employee.createdAt,
                UpdatedAt = employee.updatedAt
            };

            // Get user information
            var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
            if (user != null)
            {
                model.Email = user.Email;
                model.IsActive = user.IsActive;
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = _employeeRepository.Get(model.EmployeeId);
            if (employee == null)
            {
                return NotFound();
            }

            // Update employee properties
            employee.firstName = model.FirstName;
            employee.lastName = model.LastName;
            employee.Address = model.Address;
            employee.updatedAt = DateTime.Now;

            _employeeRepository.Update(employee);

            // Optionally sync some data with user
            var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
            if (user != null)
            {
                bool userChanged = false;

                // Only update user if data is different
                if (user.FirstName != model.FirstName)
                {
                    user.FirstName = model.FirstName;
                    userChanged = true;
                }

                if (user.LastName != model.LastName)
                {
                    user.LastName = model.LastName;
                    userChanged = true;
                }

                if (userChanged)
                {
                    user.UpdatedOnUtc = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
            }

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            // Get the employee with the specified ID
            var employee = _employeeRepository.Get(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Get associated user information
            var user = await _userManager.FindByIdAsync(employee.UserId.ToString());

            var model = new EmployeeViewModel
            {
                EmployeeId = employee.Id,
                UserId = employee.UserId,
                FirstName = employee.firstName,
                LastName = employee.lastName,
                Address = employee.Address,
                CreatedAt = employee.createdAt,
                UpdatedAt = employee.updatedAt,
            };

            if (user != null)
            {
                model.Email = user.Email;
                model.IsActive = user.IsActive;
            }

            return View(model);
        }
        [Authorize(Roles = "Admin")]

        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            var employee = _employeeRepository.Get(id);
            if (employee == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(employee.UserId.ToString());

            var model = new EmployeeViewModel
            {
                EmployeeId = employee.Id,
                UserId = employee.UserId,
                FirstName = employee.firstName,
                LastName = employee.lastName,
                Address = employee.Address,
                CreatedAt = employee.createdAt,
                UpdatedAt = employee.updatedAt,
            };

            if (user != null)
            {
                model.Email = user.Email;
                model.IsActive = user.IsActive;
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var employee = _employeeRepository.Get(id);
            if (employee == null)
            {
                return NotFound();
            }
            _employeeRepository.Remove(employee);
            return RedirectToAction("Index");
        }
    }
}