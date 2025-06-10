using HRWebApp.Entities;
using HRWebApp.Models;


namespace HRWebApp.Services
{
    public static class EmployeeHelper
    {
        /// <summary>
        /// Converts a Employee entity to a EmployeeViewModel.
        /// </summary>
        /// <param name="employee">The Employee entity to convert.</param>
        /// <returns>A EmployeeViewModel populated with the properties of the Employee entity.</returns>
        public static EmployeeViewModel ConvertFromEmployeeToEmployeeViewModel(Employee employee)
        {
            return new EmployeeViewModel
            {
                EmployeeId = employee.Id,
                UserId = employee.User.Id,
                FirstName = employee.firstName,
                LastName = employee.lastName,
                Address = employee.Address,
                CreatedAt = employee.createdAt

            };
        }
    }
}