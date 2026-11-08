using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab3_kpz.Entities;

public sealed class EventType
{
    public int EventTypeId { get; set; }
    public string EventTypeName { get; set; } = string.Empty;

    public ICollection<DayEvent> DayEvents { get; set; } = new List<DayEvent>();
}

public sealed class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
{
    public void Configure(EntityTypeBuilder<EventType> builder)
    {
        builder.ToTable("EventTypes");
        builder.HasKey(t => t.EventTypeId);

        builder.Property(t => t.EventTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.EventTypeName)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.HasIndex(t => t.EventTypeName)
            .IsUnique();
    }
}
