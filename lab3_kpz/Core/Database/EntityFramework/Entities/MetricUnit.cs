using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab3_kpz.Entities;

public sealed class MetricUnit
{
    public int MetricUnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal UnitMinValue { get; set; }
    public decimal UnitMaxValue { get; set; }

    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}

public sealed class MetricUnitConfiguration : IEntityTypeConfiguration<MetricUnit>
{
    public void Configure(EntityTypeBuilder<MetricUnit> builder)
    {
        builder.ToTable("MetricUnits");
        builder.HasKey(u => u.MetricUnitId);

        builder.Property(u => u.MetricUnitId)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UnitName)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode();

        builder.HasIndex(u => u.UnitName)
            .IsUnique();

        builder.Property(u => u.UnitMinValue)
            .HasColumnType("decimal(6,2)")
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(u => u.UnitMaxValue)
            .HasColumnType("decimal(6,2)")
            .HasPrecision(6, 2)
            .IsRequired();
    }
}
