using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class QuizReportConfiguration : AuditableEntityConfiguration<QuizReport>
{
    public override void Configure(EntityTypeBuilder<QuizReport> builder)
    {
        base.Configure(builder);

        builder.Property(r => r.QuizId).IsRequired();

        builder.Property(r => r.Reason)
            .HasConversion(new EnumerationValueConverter<ReportReason>())
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion(new EnumerationValueConverter<ReportStatus>())
            .IsRequired();

        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.ResolutionNote).HasMaxLength(500);

        builder.HasIndex(r => r.QuizId);
        builder.HasIndex(r => r.Status);
    }
}
