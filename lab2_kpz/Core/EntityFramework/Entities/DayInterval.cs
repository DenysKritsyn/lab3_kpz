using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab2_kpz.Entities;

public sealed class DayInterval
{
    public int DayIntervalId { get; set; }
    public int DiaryEntryId { get; set; }
    public TimeOnly IntervalStartTime { get; set; }
    public TimeOnly IntervalEndTime { get; set; }
    public int ConditionStateId { get; set; }
    public string? IntervalNote { get; set; }

    public DiaryEntry DiaryEntry { get; set; } = null!;
    public ConditionState ConditionState { get; set; } = null!;
    public ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}

public sealed class DayIntervalConfiguration : IEntityTypeConfiguration<DayInterval>
{
    public void Configure(EntityTypeBuilder<DayInterval> builder)
    {
        builder.ToTable("DayIntervals");
        builder.HasKey(i => i.DayIntervalId);

        builder.Property(i => i.DayIntervalId)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.IntervalStartTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.Property(i => i.IntervalEndTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.Property(i => i.IntervalNote)
            .HasMaxLength(500)
            .IsUnicode();

        builder.HasOne(i => i.ConditionState)
            .WithMany(c => c.DayIntervals)
            .HasForeignKey(i => i.ConditionStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.IntervalSymptoms)
            .WithOne(s => s.DayInterval)
            .HasForeignKey(s => s.DayIntervalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
