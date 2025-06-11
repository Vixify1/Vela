using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRWebApp.Mappings
{
    public class DepartmentMapping : EntityTypeConfiguration<Department>
    {
        public override void Configure(EntityTypeBuilder<Department> builder)
        {
            base.Configure(builder);
        }
    }
}
