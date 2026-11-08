using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab3_kpz.Entities;

public sealed class Report
{
    public int ReportId { get; set; }
    public int UserId { get; set; }
    public DateOnly ReportPeriodFrom { get; set; }
    public DateOnly ReportPeriodTo { get; set; }
    public DateTime ReportGeneratedAt { get; set; }
    public string ReportSummary { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(r => r.ReportId);

        builder.Property(r => r.ReportId)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.ReportPeriodFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(r => r.ReportPeriodTo)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(r => r.ReportGeneratedAt)
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(r => r.ReportSummary)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();
    }
}
