using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRWebApp.Mappings
{
    public class HolidayMapping : EntityTypeConfiguration<Holiday>
    {
        public override void Configure(EntityTypeBuilder<Holiday> builder)
        {
            base.Configure(builder);
        }
    }
}