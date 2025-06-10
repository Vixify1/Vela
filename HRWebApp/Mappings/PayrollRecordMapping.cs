using HRWebApp.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

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
