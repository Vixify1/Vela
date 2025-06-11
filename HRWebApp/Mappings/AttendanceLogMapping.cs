using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRWebApp.Mappings
{
    public class AttendanceLogMapping : EntityTypeConfiguration<AttendanceLog>
    {
        public override void Configure(EntityTypeBuilder<AttendanceLog> builder)
        {
            base.Configure(builder);
        }
    }
}
