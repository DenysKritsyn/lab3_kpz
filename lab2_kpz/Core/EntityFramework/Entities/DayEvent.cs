using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab2_kpz.Entities;

public sealed class DayEvent
{
    public int DayEventId { get; set; }
    public int DiaryEntryId { get; set; }
    public int EventTypeId { get; set; }
    public TimeOnly EventStartTime { get; set; }
    public TimeOnly EventEndTime { get; set; }
    public bool IsPreciseTime { get; set; }
    public string? EventNote { get; set; }

    public DiaryEntry DiaryEntry { get; set; } = null!;
    public EventType EventType { get; set; } = null!;
}

public sealed class DayEventConfiguration : IEntityTypeConfiguration<DayEvent>
{
    public void Configure(EntityTypeBuilder<DayEvent> builder)
    {
        builder.ToTable("DayEvents");
        builder.HasKey(e => e.DayEventId);

        builder.Property(e => e.DayEventId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.EventStartTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.Property(e => e.EventEndTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.Property(e => e.IsPreciseTime)
            .HasColumnType("bit")
            .IsRequired();

        builder.Property(e => e.EventNote)
            .HasMaxLength(500)
            .IsUnicode();

        builder.HasOne(e => e.EventType)
            .WithMany(t => t.DayEvents)
            .HasForeignKey(e => e.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
