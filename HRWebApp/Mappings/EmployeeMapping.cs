using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

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
