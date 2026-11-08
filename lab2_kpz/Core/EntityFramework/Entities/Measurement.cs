using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab2_kpz.Entities;

public sealed class Measurement
{
    public int MeasurementId { get; set; }
    public int DiaryEntryId { get; set; }
    public TimeOnly MeasuringTime { get; set; }

    public DiaryEntry DiaryEntry { get; set; } = null!;
    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}

public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.ToTable("Measurements");
        builder.HasKey(m => m.MeasurementId);

        builder.Property(m => m.MeasurementId)
            .ValueGeneratedOnAdd();

        builder.Property(m => m.MeasuringTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.HasMany(m => m.Metrics)
            .WithOne(met => met.Measurement)
            .HasForeignKey(met => met.MeasurementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
