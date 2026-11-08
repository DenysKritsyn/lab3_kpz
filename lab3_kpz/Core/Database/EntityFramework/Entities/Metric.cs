using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab3_kpz.Entities;

public sealed class Metric
{
    public int MetricId { get; set; }
    public int MeasurementId { get; set; }
    public int MetricUnitId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }

    public Measurement Measurement { get; set; } = null!;
    public MetricUnit MetricUnit { get; set; } = null!;
}

public sealed class MetricConfiguration : IEntityTypeConfiguration<Metric>
{
    public void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.ToTable("Metrics");
        builder.HasKey(m => m.MetricId);

        builder.Property(m => m.MetricId)
            .ValueGeneratedOnAdd();

        builder.Property(m => m.MetricName)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(m => m.MetricValue)
            .HasColumnType("decimal(6,2)")
            .HasPrecision(6, 2)
            .IsRequired();

        builder.HasOne(m => m.MetricUnit)
            .WithMany(u => u.Metrics)
            .HasForeignKey(m => m.MetricUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
