using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net.WebSockets;
using HRWebApp.Entities;
using System.Reflection;
using HRWebApp.Enums;
using HRWebApp.Abstract;
using HRWebApp.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRWebApp.Controllers
{
    public class AuthenticationController : Controller
    {
        //  TOGGLE THIS TO ENABLE/DISABLE ADMIN CREATION
        private const bool ENABLE_ADMIN_CREATION = true;

        // Make this accessible to views
        public static bool IsAdminCreationEnabled => ENABLE_ADMIN_CREATION;

        public readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly IEntitiesRepository<Department> _departmentRepository;

        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IEntitiesRepository<Employee> employeeRepository, IEntitiesRepository<Department> departmentRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
        }

        // Existing Register method for regular users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
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

                    // Add the new user into "User" Role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());

                    var employee = new Employee
                    {
                        UserId = user.Id,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now
                    };

                    // add the new employee to the repository 
                    _employeeRepository.Add(employee);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // NEW: Admin Registration Methods
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateAdmin()
        {
            if (!ENABLE_ADMIN_CREATION)
            {
                return NotFound(); // Hide the functionality when disabled
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(AdminRegisterViewModel model)
        {
            if (!ENABLE_ADMIN_CREATION)
            {
                return NotFound(); // Hide the functionality when disabled
            }

            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsActive = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailConfirmed = true // Admins don't need email confirmation
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Ensure Admin role exists
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                    {
                        ApplicationRole adminRole = new ApplicationRole()
                        { Name = UserTypeOptions.Admin.ToString() };

                        await _roleManager.CreateAsync(adminRole);
                    }

                    // Add the new user to "Admin" Role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());

                    TempData["Success"] = $"Admin user '{model.Email}' has been created successfully!";
                    return RedirectToAction("CreateAdmin");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // Existing Login method
// ... existing code ...
        // Existing Login method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return LocalRedirect(ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            // FIX: Change this line from return View(model); to redirect back to Home
            TempData["LoginError"] = "Invalid login attempt. Please check your email and password.";
            return RedirectToAction("Index", "Home");
        }
// ... existing code ...

        // Existing Logout method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}