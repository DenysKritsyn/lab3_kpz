using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab3_kpz.Entities;

public sealed class ConditionState
{
    public int ConditionStateId { get; set; }
    public string ConditionStateName { get; set; } = string.Empty;

    public ICollection<DayInterval> DayIntervals { get; set; } = new List<DayInterval>();
}

public sealed class ConditionStateConfiguration : IEntityTypeConfiguration<ConditionState>
{
    public void Configure(EntityTypeBuilder<ConditionState> builder)
    {
        builder.ToTable("ConditionStates");
        builder.HasKey(c => c.ConditionStateId);

        builder.Property(c => c.ConditionStateId)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.ConditionStateName)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode();

        builder.HasIndex(c => c.ConditionStateName)
            .IsUnique();
    }
}
