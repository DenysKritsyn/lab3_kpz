using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab2_kpz.Entities;

public sealed class DiaryEntry
{
    public int DiaryEntryId { get; set; }
    public int UserId { get; set; }
    public DateOnly EntryDate { get; set; }
    public string? EntryNote { get; set; }
    public DateTime EntryCreatedAt { get; set; }
    public DateTime EntryUpdatedAt { get; set; }
    public byte[] EntryRowVersion { get; set; } = [];

    public User User { get; set; } = null!;
    public ICollection<DayInterval> DayIntervals { get; set; } = new List<DayInterval>();
    public ICollection<DayEvent> DayEvents { get; set; } = new List<DayEvent>();
    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}

public sealed class DiaryEntryConfiguration : IEntityTypeConfiguration<DiaryEntry>
{
    public void Configure(EntityTypeBuilder<DiaryEntry> builder)
    {
        builder.ToTable("DiaryEntries");
        builder.HasKey(d => d.DiaryEntryId);

        builder.Property(d => d.DiaryEntryId)
            .ValueGeneratedOnAdd();

        builder.Property(d => d.EntryDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(d => d.EntryNote)
            .HasMaxLength(1000)
            .IsUnicode();

        builder.Property(d => d.EntryCreatedAt)
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(d => d.EntryUpdatedAt)
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(d => d.EntryRowVersion)
            .IsRowVersion();

        // Business rule: a user cannot have two diary entries for the exact same date
        builder.HasIndex(d => new { d.UserId, d.EntryDate })
            .IsUnique();

        // Index on EntryDate to optimize searching
        builder.HasIndex(d => d.EntryDate)
            .HasDatabaseName("IX_DiaryEntries_EntryDate");

        builder.HasMany(d => d.DayIntervals)
            .WithOne(i => i.DiaryEntry)
            .HasForeignKey(i => i.DiaryEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.DayEvents)
            .WithOne(e => e.DiaryEntry)
            .HasForeignKey(e => e.DiaryEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Measurements)
            .WithOne(m => m.DiaryEntry)
            .HasForeignKey(m => m.DiaryEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
