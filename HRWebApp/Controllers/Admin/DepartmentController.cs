using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models.Admin;

namespace HRWebApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly IEntitiesRepository<Department> _departmentRepository;
        private readonly IEntitiesRepository<Employee> _employeeRepository;

        public DepartmentController(IEntitiesRepository<Department> departmentRepository, IEntitiesRepository<Employee> employeeRepository)
        {
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
        }

        public IActionResult Index()
        {
            var departments = _departmentRepository.GetAll()
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    EmployeeCount = _employeeRepository.GetAll().Count(e => e.DepartmentId == d.Id)
                }).ToList();

            return View(departments);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var department = new Department
                {
                    Name = model.Name
                };

                _departmentRepository.Add(department);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var department = _departmentRepository.Get(id);
            if (department == null)
            {
                return NotFound();
            }

            var model = new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                EmployeeCount = _employeeRepository.GetAll().Count(e => e.DepartmentId == department.Id)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var department = _departmentRepository.Get(model.Id);
                if (department == null)
                {
                    return NotFound();
                }

                department.Name = model.Name;
                _departmentRepository.Update(department);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var department = _departmentRepository.Get(id);
            if (department == null)
            {
                return NotFound();
            }

            var model = new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                EmployeeCount = _employeeRepository.GetAll().Count(e => e.DepartmentId == department.Id)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var department = _departmentRepository.Get(id);
            if (department != null)
            {
                _departmentRepository.Remove(department);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
