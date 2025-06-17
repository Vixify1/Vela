using Microsoft.AspNetCore.Identity;
using HRWebApp.Entities;
using HRWebApp.Abstract;
using HRWebApp.Enums;

namespace HRWebApp.Data
{
    public class DbSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEntitiesRepository<Employee> _employeeRepository;
        private readonly IEntitiesRepository<Department> _departmentRepository;
        private readonly IEntitiesRepository<AttendanceLog> _attendanceRepository;
        private readonly IEntitiesRepository<Holiday> _holidayRepository;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IEntitiesRepository<Employee> employeeRepository,
            IEntitiesRepository<Department> departmentRepository,
            IEntitiesRepository<AttendanceLog> attendanceRepository,
            IEntitiesRepository<Holiday> holidayRepository,
            ILogger<DbSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _attendanceRepository = attendanceRepository;
            _holidayRepository = holidayRepository;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedRolesAsync();
                await SeedHolidaysAsync();
                await SeedTestUsersAsync();
                await SeedAttendanceDataAsync();
                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
                _logger.LogInformation("Admin role created.");
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });
                _logger.LogInformation("User role created.");
            }
        }

        private async Task SeedHolidaysAsync()
        {
            var holidays = new[]
            {
                new { Date = new DateTime(2024, 1, 1), Description = "New Year's Day" },
                new { Date = new DateTime(2024, 2, 14), Description = "Valentine's Day" },
                new { Date = new DateTime(2024, 3, 15), Description = "Special Holiday - March 15" },
                new { Date = new DateTime(2024, 4, 1), Description = "April Fool's Day" },
                new { Date = new DateTime(2024, 7, 4), Description = "Independence Day" },
                new { Date = new DateTime(2024, 12, 25), Description = "Christmas Day" }
            };

            foreach (var holidayData in holidays)
            {
                var existingHoliday = _holidayRepository.GetAll()
                    .FirstOrDefault(h => h.Date.Date == holidayData.Date.Date);

                if (existingHoliday == null)
                {
                    var holiday = new Holiday
                    {
                        Date = holidayData.Date,
                        Description = holidayData.Description
                    };
                    _holidayRepository.Add(holiday);
                }
            }
        }

        private async Task SeedTestUsersAsync()
        {
            // Create multiple test users for better testing
            var testUsers = new[]
            {
                new { Email = "test@example.com", FirstName = "Test", LastName = "User", HourlyRate = 25.00m },
                new { Email = "john.doe@example.com", FirstName = "John", LastName = "Doe", HourlyRate = 30.00m },
                new { Email = "jane.smith@example.com", FirstName = "Jane", LastName = "Smith", HourlyRate = 35.00m }
            };

            foreach (var userData in testUsers)
            {
                var existingUser = await _userManager.FindByEmailAsync(userData.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userData.Email,
                        Email = userData.Email,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        IsActive = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, "Test123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");

                        var department = _departmentRepository.GetAll().FirstOrDefault() ??
                            CreateDepartment("General");

                        var employee = new Employee
                        {
                            UserId = user.Id,
                            firstName = userData.FirstName,
                            lastName = userData.LastName,
                            HourlyRate = userData.HourlyRate,
                            DepartmentId = department.Id,
                            createdAt = DateTime.Now,
                            updatedAt = DateTime.Now
                        };
                        _employeeRepository.Add(employee);
                        _logger.LogInformation($"Created test user: {userData.Email}");
                    }
                }
            }
        }

        private Department CreateDepartment(string name)
        {
            var department = new Department { Name = name };
            _departmentRepository.Add(department);
            return department;
        }

        private async Task SeedAttendanceDataAsync()
        {
            var employees = _employeeRepository.GetAll().ToList();

            foreach (var employee in employees)
            {
                var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
                if (user != null)
                {
                    // Seed multiple months for each employee
                    await SeedMonthlyAttendance(employee, 2024, 1);  // January
                    await SeedMonthlyAttendance(employee, 2024, 2);  // February
                    await SeedMonthlyAttendance(employee, 2024, 3);  // March
                    await SeedMonthlyAttendance(employee, 2024, 4);  // April

                    _logger.LogInformation($"Seeded attendance data for {user.FirstName} {user.LastName}");
                }
            }
        }

        private async Task SeedMonthlyAttendance(Employee employee, int year, int month)
        {
            // Check if data already exists
            var existingRecords = _attendanceRepository.GetAll()
                .Where(a => a.EmployeeId == employee.Id &&
                           a.Date.Year == year &&
                           a.Date.Month == month)
                .Any();

            if (existingRecords)
                return;

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var random = new Random(employee.Id * month); // Seed for consistent but varied data

            for (int day = 1; day <= daysInMonth; day++)
            {
                var currentDate = new DateTime(year, month, day);
                var dayOfWeek = currentDate.DayOfWeek;

                // Skip weekends (except for special cases)
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    // Occasionally work on weekends (10% chance)
                    if (random.NextDouble() > 0.1)
                        continue;
                }

                // Occasionally skip weekdays (5% chance - sick days, vacation, etc.)
                if (dayOfWeek >= DayOfWeek.Monday && dayOfWeek <= DayOfWeek.Friday)
                {
                    if (random.NextDouble() < 0.05)
                        continue;
                }

                // Vary the work hours slightly for realism
                var startHour = 8 + random.Next(0, 2); // 8-9 AM start
                var workHours = 7.5 + (random.NextDouble() * 1.0); // 7.5-8.5 hours

                // Different patterns for different months
                switch (month)
                {
                    case 1: // January - Normal schedule
                        workHours = 8.0;
                        break;
                    case 2: // February - Shorter month, some longer days
                        workHours = 8.0 + (random.NextDouble() * 0.5);
                        break;
                    case 3: // March - Your original test month
                        workHours = 8.0;
                        // March 15th is special (Holiday)
                        if (day == 15)
                        {
                            workHours = 8.0; // Full day on holiday
                        }
                        break;
                    case 4: // April - Spring, some half days
                        if (random.NextDouble() < 0.1) // 10% chance of half day
                            workHours = 4.0;
                        else
                            workHours = 8.0;
                        break;
                }

                var clockIn = currentDate.AddHours(startHour);
                var clockOut = clockIn.AddHours(workHours);

                var attendanceLog = new AttendanceLog
                {
                    EmployeeId = employee.Id,
                    Date = currentDate,
                    ClockIn = clockIn,
                    ClockOut = clockOut
                };

                _attendanceRepository.Add(attendanceLog);
            }
        }
    }
}