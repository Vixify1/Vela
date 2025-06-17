using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models.Admin;
using HRWebApp.Enums;

namespace HRWebApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly IEntitiesRepository<Department> _departmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public EmployeeController(
            IEntitiesRepository<Employee> employeeRepository,
            IEntitiesRepository<Department> departmentRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var employees = _employeeRepository.GetAll(e => e.User)
                .ToList() // Execute query first
                .Select(e => new EmployeeViewModel
                {
                    Id = e.Id,
                    FirstName = e.firstName,
                    LastName = e.lastName,
                    Email = e.User?.Email ?? "No Email",
                    HourlyRate = e.HourlyRate,
                    DepartmentName = GetDepartmentName(e.DepartmentId)
                }).ToList();

            return View(employees);
        }

        public IActionResult Create()
        {
            var model = new EmployeeViewModel
            {
                Departments = GetDepartmentSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            // Custom validation for password on create
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required when creating a new employee.");
            }
            else if (model.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
            }

            if (ModelState.IsValid)
            {
                // Create the ApplicationUser first
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsActive = true,
                    CreatedOnUtc = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Create User role if it doesn't exist
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.User.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole()
                        { Name = UserTypeOptions.User.ToString() };

                        await _roleManager.CreateAsync(applicationRole);
                    }

                    // Add the new user to "User" Role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());

                    // Create the Employee record
                    var employee = new Employee
                    {
                        UserId = user.Id,
                        firstName = model.FirstName,
                        lastName = model.LastName,
                        HourlyRate = model.HourlyRate,
                        DepartmentId = model.DepartmentId,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now
                    };

                    _employeeRepository.Add(employee);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Reload departments for dropdown if validation fails
            model.Departments = GetDepartmentSelectList();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var employee = _employeeRepository.Get(e => e.Id == id, e => e.User);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.firstName,
                LastName = employee.lastName,
                Email = employee.User?.Email ?? "",
                HourlyRate = employee.HourlyRate,
                DepartmentId = employee.DepartmentId,
                Departments = GetDepartmentSelectList(employee.DepartmentId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeViewModel model)
        {
            // Remove password validation for edit - we don't require it
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                var employee = _employeeRepository.Get(e => e.Id == model.Id, e => e.User);
                if (employee == null)
                {
                    return NotFound();
                }

                // Get the user first
                var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
                if (user == null)
                {
                    ModelState.AddModelError("", "Associated user account not found.");
                    model.Departments = GetDepartmentSelectList(model.DepartmentId);
                    return View(model);
                }

                try
                {
                    // Update user information first (Identity system)
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.UserName = model.Email; // Important: Update username too
                    user.UpdatedOnUtc = DateTime.UtcNow;

                    var userUpdateResult = await _userManager.UpdateAsync(user);
                    if (!userUpdateResult.Succeeded)
                    {
                        foreach (var error in userUpdateResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        model.Departments = GetDepartmentSelectList(model.DepartmentId);
                        return View(model);
                    }

                    // Update employee information (EF system)
                    employee.firstName = model.FirstName;
                    employee.lastName = model.LastName;
                    employee.HourlyRate = model.HourlyRate;
                    employee.DepartmentId = model.DepartmentId;
                    employee.updatedAt = DateTime.Now;

                    _employeeRepository.Update(employee);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the employee: " + ex.Message);
                    model.Departments = GetDepartmentSelectList(model.DepartmentId);
                    return View(model);
                }
            }

            model.Departments = GetDepartmentSelectList(model.DepartmentId);
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var employee = _employeeRepository.Get(e => e.Id == id, e => e.User);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.firstName,
                LastName = employee.lastName,
                Email = employee.User?.Email ?? "",
                HourlyRate = employee.HourlyRate,
                DepartmentName = GetDepartmentName(employee.DepartmentId)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = _employeeRepository.Get(e => e.Id == id, e => e.User);
            if (employee != null)
            {
                // Get the associated user
                var user = await _userManager.FindByIdAsync(employee.UserId.ToString());

                // Remove the employee first
                _employeeRepository.Remove(employee);

                // Then remove the user account
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to get department select list
        private List<SelectListItem> GetDepartmentSelectList(int? selectedDepartmentId = null)
        {
            return _departmentRepository.GetAll()
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = selectedDepartmentId.HasValue && d.Id == selectedDepartmentId.Value
                }).ToList();
        }

        // Helper method to get department name
        private string GetDepartmentName(int departmentId)
        {
            var department = _departmentRepository.Get(departmentId);
            return department?.Name ?? "Unknown Department";
        }
    }
}