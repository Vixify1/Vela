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

        [HttpGet]

        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            var departments = _departmentRepository.GetAll().ToList();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName};
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //Check the status of the button
                    if (model.UserType == HRWebApp.Enums.UserTypeOptions.Admin)
                    {
                        //create admin role
                        if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                        {
                            // Create an IdentityRole instead of ApplicationRole
                            ApplicationRole applicationRole = new ApplicationRole()
                            { Name = UserTypeOptions.Admin.ToString() };

                            await _roleManager.CreateAsync(applicationRole);
                        }
                        //Add the new user into "Admin" Role
                        await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());
                    }

                    else
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
                            updatedAt = DateTime.Now,
                            DepartmentId = model.DepartmentId,
                            HourlyRate = model.HourlyRate
                        };

                        // add the new employee to the repository 
                        _employeeRepository.Add(employee);
                    }


                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }
            var departments = _departmentRepository.GetAll().ToList();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", model.DepartmentId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    //Admin using Areas
                    ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" }); //area is important , if not will go to the home/index of root
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
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }
    }
}