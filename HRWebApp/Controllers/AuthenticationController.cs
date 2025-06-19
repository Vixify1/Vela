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

        // Improved Login method with specific error messages
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                // First check if user exists
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Email doesn't exist - add specific error message
                    ModelState.AddModelError("Email", "No account found with this email address.");
                    return View("~/Views/Home/Index.cshtml", model);
                }

                // Check if user account is active (if you have this property)
                if (user.IsActive == false)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact your administrator.");
                    return View("~/Views/Home/Index.cshtml", model);
                }

                // Try to sign in
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                
                if (result.Succeeded)
                {
                    // Login successful - redirect based on role
                    if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return LocalRedirect(ReturnUrl);
                    }
                    
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Your account is temporarily locked due to multiple failed login attempts. Please try again later.");
                    return View("~/Views/Home/Index.cshtml", model);
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Login is not allowed. Please contact your administrator.");
                    return View("~/Views/Home/Index.cshtml", model);
                }
                else
                {
                    // Password is wrong since user exists but login failed
                    ModelState.AddModelError("Password", "The password you entered is incorrect.");
                    return View("~/Views/Home/Index.cshtml", model);
                }
            }
            
            // If we got here, model validation failed - return to login form
            return View("~/Views/Home/Index.cshtml", model);
        }

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