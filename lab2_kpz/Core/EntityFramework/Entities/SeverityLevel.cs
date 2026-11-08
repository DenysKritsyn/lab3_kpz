using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab2_kpz.Entities;

public sealed class SeverityLevel
{
    public int SeverityLevelId { get; set; }
    public string SeverityName { get; set; } = string.Empty;

    public ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}

public sealed class SeverityLevelConfiguration : IEntityTypeConfiguration<SeverityLevel>
{
    public void Configure(EntityTypeBuilder<SeverityLevel> builder)
    {
        builder.ToTable("SeverityLevels");
        builder.HasKey(s => s.SeverityLevelId);

        builder.Property(s => s.SeverityLevelId)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SeverityName)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode();

        builder.HasIndex(s => s.SeverityName)
            .IsUnique();
    }
}
