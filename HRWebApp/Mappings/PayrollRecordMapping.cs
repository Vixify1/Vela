using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRWebApp.Mappings
{
    public class PayrollRecordMapping : EntityTypeConfiguration<PayrollRecord>
    {
        public override void Configure(EntityTypeBuilder<PayrollRecord> builder)
        {
            base.Configure(builder);
        }
    }
}
