using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab2_kpz.Entities;

public sealed class Symptom
{
    public int SymptomId { get; set; }
    public string SymptomName { get; set; } = string.Empty;

    public ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}

public sealed class SymptomConfiguration : IEntityTypeConfiguration<Symptom>
{
    public void Configure(EntityTypeBuilder<Symptom> builder)
    {
        builder.ToTable("Symptoms");
        builder.HasKey(s => s.SymptomId);

        builder.Property(s => s.SymptomId)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SymptomName)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.HasIndex(s => s.SymptomName)
            .IsUnique();
    }
}
