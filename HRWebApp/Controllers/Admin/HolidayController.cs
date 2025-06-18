using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRWebApp.Abstract;
using HRWebApp.Entities;
using HRWebApp.Models.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRWebApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class HolidayController : Controller
    {
        private readonly IEntitiesRepository<Holiday> _holidayRepository;

        public HolidayController(IEntitiesRepository<Holiday> holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        // Calendar is now the default view
        public IActionResult Index()
        {
            return RedirectToAction("Calendar");
        }

        // List view for detailed management
        public IActionResult List(int? year = null)
        {
            var selectedYear = year ?? DateTime.Now.Year;
            var holidays = _holidayRepository.GetAll()
                .Where(h => h.Date.Year == selectedYear)
                .OrderBy(h => h.Date)
                .Select(h => new HolidayViewModel
                {
                    Id = h.Id,
                    Date = h.Date,
                    Description = h.Description
                }).ToList();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.Years = GetYearSelectList();
            return View(holidays);
        }

        public IActionResult Create()
        {
            var model = new HolidayViewModel
            {
                Date = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HolidayViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if holiday already exists on this date
                var existingHoliday = _holidayRepository.GetAll()
                    .FirstOrDefault(h => h.Date.Date == model.Date.Date);

                if (existingHoliday != null)
                {
                    ModelState.AddModelError("Date", "A holiday already exists on this date.");
                    return View(model);
                }

                var holiday = new Holiday
                {
                    Date = model.Date.Date, // Ensure only date part, no time
                    Description = model.Description
                };

                _holidayRepository.Add(holiday);
                return RedirectToAction("Calendar", new { year = model.Date.Year });
            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var holiday = _holidayRepository.Get(id);
            if (holiday == null)
            {
                return NotFound();
            }

            var model = new HolidayViewModel
            {
                Id = holiday.Id,
                Date = holiday.Date,
                Description = holiday.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(HolidayViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if another holiday exists on this date (excluding current one)
                var existingHoliday = _holidayRepository.GetAll()
                    .FirstOrDefault(h => h.Date.Date == model.Date.Date && h.Id != model.Id);

                if (existingHoliday != null)
                {
                    ModelState.AddModelError("Date", "A holiday already exists on this date.");
                    return View(model);
                }

                var holiday = _holidayRepository.Get(model.Id);
                if (holiday == null)
                {
                    return NotFound();
                }

                holiday.Date = model.Date.Date;
                holiday.Description = model.Description;
                _holidayRepository.Update(holiday);

                return RedirectToAction("Calendar", new { year = model.Date.Year });
            }

            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var holiday = _holidayRepository.Get(id);
            if (holiday == null)
            {
                return NotFound();
            }

            var model = new HolidayViewModel
            {
                Id = holiday.Id,
                Date = holiday.Date,
                Description = holiday.Description
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var holiday = _holidayRepository.Get(id);
            var year = holiday?.Date.Year ?? DateTime.Now.Year;
            
            if (holiday != null)
            {
                _holidayRepository.Remove(holiday);
            }

            return RedirectToAction("Calendar", new { year = year });
        }

        // Calendar view - now supports year selection
        public IActionResult Calendar(int? year = null)
        {
            var selectedYear = year ?? DateTime.Now.Year;
            var holidays = _holidayRepository.GetAll()
                .Where(h => h.Date.Year == selectedYear)
                .OrderBy(h => h.Date)
                .Select(h => new HolidayViewModel
                {
                    Id = h.Id,
                    Date = h.Date,
                    Description = h.Description
                }).ToList();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.Years = GetYearSelectList();
            return View(holidays);
        }

        private List<SelectListItem> GetYearSelectList()
        {
            var years = new List<SelectListItem>();
            var currentYear = DateTime.Now.Year;
            
            // Add years from 2024 to current year + 2
            for (int year = 2024; year <= currentYear + 2; year++)
            {
                years.Add(new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                });
            }
            
            return years;
        }
    }
}