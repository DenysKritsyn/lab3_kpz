using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab3_kpz.Entities;

public sealed class IntervalSymptom
{
    public int DayIntervalId { get; set; }
    public int SymptomId { get; set; }
    public int SeverityLevelId { get; set; }
    public string? SymptomNote { get; set; }

    public DayInterval DayInterval { get; set; } = null!;
    public Symptom Symptom { get; set; } = null!;
    public SeverityLevel SeverityLevel { get; set; } = null!;
}

public sealed class IntervalSymptomConfiguration : IEntityTypeConfiguration<IntervalSymptom>
{
    public void Configure(EntityTypeBuilder<IntervalSymptom> builder)
    {
        builder.ToTable("IntervalSymptoms");
        builder.HasKey(s => new { s.DayIntervalId, s.SymptomId });

        builder.Property(s => s.SymptomNote)
            .HasMaxLength(500)
            .IsUnicode();

        builder.HasOne(s => s.Symptom)
            .WithMany(sym => sym.IntervalSymptoms)
            .HasForeignKey(s => s.SymptomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SeverityLevel)
            .WithMany(sev => sev.IntervalSymptoms)
            .HasForeignKey(s => s.SeverityLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
