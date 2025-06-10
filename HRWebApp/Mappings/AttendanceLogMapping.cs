using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

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
