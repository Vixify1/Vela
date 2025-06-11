using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRWebApp.Mappings
{
    public class EmployeeMapping : EntityTypeConfiguration<Employee>
    {
        public override void Configure(EntityTypeBuilder<Employee> builder)
        {
            base.Configure(builder);
        }
    }
}
