using Kpz.Lab2.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kpz.Lab2.Data.Configurations;

public sealed class WellbeingLevelConfiguration : IEntityTypeConfiguration<WellbeingLevel>
{
    public void Configure(EntityTypeBuilder<WellbeingLevel> entity)
    {
        entity.ToTable("WellbeingLevels");
        entity.HasKey(x => x.WellbeingLevelId);

        entity.Property(x => x.WellbeingName)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.WellbeingRank)
            .IsRequired();

        entity.HasIndex(x => x.WellbeingName).IsUnique();
        entity.HasIndex(x => x.WellbeingRank).IsUnique();

        entity.HasData(
            new WellbeingLevel { WellbeingLevelId = 1, WellbeingName = "Good", WellbeingRank = 1 },
            new WellbeingLevel { WellbeingLevelId = 2, WellbeingName = "Moderate", WellbeingRank = 2 },
            new WellbeingLevel { WellbeingLevelId = 3, WellbeingName = "Poor", WellbeingRank = 3 },
            new WellbeingLevel { WellbeingLevelId = 4, WellbeingName = "Very Poor", WellbeingRank = 4 }
        );
    }
}

public sealed class SeverityLevelConfiguration : IEntityTypeConfiguration<SeverityLevel>
{
    public void Configure(EntityTypeBuilder<SeverityLevel> entity)
    {
        entity.ToTable("SeverityLevels");
        entity.HasKey(x => x.SeverityLevelId);

        entity.Property(x => x.SeverityName)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.SeverityRank)
            .IsRequired();

        entity.HasIndex(x => x.SeverityName).IsUnique();
        entity.HasIndex(x => x.SeverityRank).IsUnique();

        entity.HasData(
            new SeverityLevel { SeverityLevelId = 1, SeverityName = "Moderate", SeverityRank = 1 },
            new SeverityLevel { SeverityLevelId = 2, SeverityName = "Severe", SeverityRank = 2 },
            new SeverityLevel { SeverityLevelId = 3, SeverityName = "Very Severe", SeverityRank = 3 }
        );
    }
}

public sealed class SymptomConfiguration : IEntityTypeConfiguration<Symptom>
{
    public void Configure(EntityTypeBuilder<Symptom> entity)
    {
        entity.ToTable("Symptoms");
        entity.HasKey(x => x.SymptomId);

        entity.Property(x => x.SymptomName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.SymptomCategory)
            .HasMaxLength(50);

        entity.HasIndex(x => x.SymptomName).IsUnique();
        entity.HasIndex(x => x.SymptomCategory);

        entity.HasData(
            new Symptom { SymptomId = 1, SymptomName = "Headache", SymptomCategory = "Neurological" },
            new Symptom { SymptomId = 2, SymptomName = "Dizziness", SymptomCategory = "Neurological" },
            new Symptom { SymptomId = 3, SymptomName = "Cough", SymptomCategory = "Respiratory" },
            new Symptom { SymptomId = 4, SymptomName = "Sore Throat", SymptomCategory = "Respiratory" },
            new Symptom { SymptomId = 5, SymptomName = "Runny Nose", SymptomCategory = "Respiratory" },
            new Symptom { SymptomId = 6, SymptomName = "Nausea", SymptomCategory = "Gastrointestinal" },
            new Symptom { SymptomId = 7, SymptomName = "Fatigue", SymptomCategory = "General" },
            new Symptom { SymptomId = 8, SymptomName = "Chills", SymptomCategory = "General" }
        );
    }
}

public sealed class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
{
    public void Configure(EntityTypeBuilder<EventType> entity)
    {
        entity.ToTable("EventTypes");
        entity.HasKey(x => x.EventTypeId);

        entity.Property(x => x.EventName)
            .HasMaxLength(100)
            .IsRequired();

        entity.HasIndex(x => x.EventName).IsUnique();

        entity.HasData(
            new EventType { EventTypeId = 1, EventName = "Stress" },
            new EventType { EventTypeId = 2, EventName = "Sleep Deprivation" },
            new EventType { EventTypeId = 3, EventName = "Medication Intake" },
            new EventType { EventTypeId = 4, EventName = "Physical Exercise" },
            new EventType { EventTypeId = 5, EventName = "Caffeine or Energy Drink" },
            new EventType { EventTypeId = 6, EventName = "Weather Change" }
        );
    }
}

public sealed class MetricTypeConfiguration : IEntityTypeConfiguration<MetricType>
{
    public void Configure(EntityTypeBuilder<MetricType> entity)
    {
        entity.ToTable("MetricTypes", t =>
            t.HasCheckConstraint("CK_MetricTypes_Range", "[MetricMaxValue] > [MetricMinValue]"));

        entity.HasKey(x => x.MetricTypeId);

        entity.Property(x => x.MetricName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.MetricUnit)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(x => x.MetricMinValue)
            .HasPrecision(6, 2)
            .IsRequired();

        entity.Property(x => x.MetricMaxValue)
            .HasPrecision(6, 2)
            .IsRequired();

        entity.HasIndex(x => x.MetricName).IsUnique();

        entity.HasData(
            new MetricType { MetricTypeId = 1, MetricName = "Body Temperature", MetricUnit = "°C", MetricMinValue = 34.00m, MetricMaxValue = 43.00m },
            new MetricType { MetricTypeId = 2, MetricName = "Systolic Blood Pressure", MetricUnit = "mmHg", MetricMinValue = 60.00m, MetricMaxValue = 250.00m },
            new MetricType { MetricTypeId = 3, MetricName = "Diastolic Blood Pressure", MetricUnit = "mmHg", MetricMinValue = 40.00m, MetricMaxValue = 160.00m },
            new MetricType { MetricTypeId = 4, MetricName = "Heart Rate", MetricUnit = "bpm", MetricMinValue = 35.00m, MetricMaxValue = 220.00m },
            new MetricType { MetricTypeId = 5, MetricName = "Oxygen Saturation", MetricUnit = "%", MetricMinValue = 70.00m, MetricMaxValue = 100.00m }
        );
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("Users");
        entity.HasKey(x => x.UserId);

        entity.Property(x => x.ExternalAuthId)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.UserEmail)
            .HasMaxLength(256)
            .IsRequired();

        entity.Property(x => x.UserName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.UserRole)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(x => x.UserCreatedAt)
            .IsRequired();

        entity.HasIndex(x => x.ExternalAuthId).IsUnique();
        entity.HasIndex(x => x.UserEmail).IsUnique();
    }
}

public sealed class DiaryEntryConfiguration : IEntityTypeConfiguration<DiaryEntry>
{
    public void Configure(EntityTypeBuilder<DiaryEntry> entity)
    {
        entity.ToTable("DiaryEntries");
        entity.HasKey(x => x.DiaryEntryId);

        entity.Property(x => x.EntryDate)
            .IsRequired();

        entity.Property(x => x.EntryNote)
            .HasMaxLength(2000);

        entity.Property(x => x.EntryCreatedAt)
            .IsRequired();

        entity.Property(x => x.EntryUpdatedAt)
            .IsRequired();

        entity.Property(x => x.EntryRowVersion)
            .IsRowVersion();

        // Unique business constraint: one diary entry per user per logical day
        entity.HasIndex(x => new { x.UserId, x.EntryDate })
            .IsUnique();

        entity.HasIndex(x => x.EntryDate);

        entity.HasOne(x => x.User)
            .WithMany(u => u.DiaryEntries)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class DayIntervalConfiguration : IEntityTypeConfiguration<DayInterval>
{
    public void Configure(EntityTypeBuilder<DayInterval> entity)
    {
        entity.ToTable("DayIntervals", t =>
            t.HasCheckConstraint(
                "CK_DayIntervals_Hours",
                "[IntervalStartHour] <= 23 AND [IntervalEndHour] >= 1 AND [IntervalEndHour] <= 24 AND [IntervalEndHour] > [IntervalStartHour]"));

        entity.HasKey(x => x.DayIntervalId);

        entity.Property(x => x.IntervalStartHour).IsRequired();
        entity.Property(x => x.IntervalEndHour).IsRequired();

        entity.Property(x => x.IntervalNote)
            .HasMaxLength(1000);

        entity.HasIndex(x => new { x.DiaryEntryId, x.IntervalStartHour });

        entity.HasOne(x => x.DiaryEntry)
            .WithMany(d => d.DayIntervals)
            .HasForeignKey(x => x.DiaryEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.WellbeingLevel)
            .WithMany(w => w.DayIntervals)
            .HasForeignKey(x => x.WellbeingLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class IntervalSymptomConfiguration : IEntityTypeConfiguration<IntervalSymptom>
{
    public void Configure(EntityTypeBuilder<IntervalSymptom> entity)
    {
        entity.ToTable("IntervalSymptoms");

        // Composite primary key for the explicit many-to-many join entity
        entity.HasKey(x => new { x.DayIntervalId, x.SymptomId });

        entity.Property(x => x.SymptomNote)
            .HasMaxLength(500);

        entity.HasIndex(x => x.SymptomId);
        entity.HasIndex(x => x.SeverityLevelId);

        entity.HasOne(x => x.DayInterval)
            .WithMany(i => i.IntervalSymptoms)
            .HasForeignKey(x => x.DayIntervalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Intentionally configured as Restrict: prevent deleting a symptom that is referenced in patient history
        entity.HasOne(x => x.Symptom)
            .WithMany(s => s.IntervalSymptoms)
            .HasForeignKey(x => x.SymptomId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.SeverityLevel)
            .WithMany(s => s.IntervalSymptoms)
            .HasForeignKey(x => x.SeverityLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DayEventConfiguration : IEntityTypeConfiguration<DayEvent>
{
    public void Configure(EntityTypeBuilder<DayEvent> entity)
    {
        entity.ToTable("DayEvents", t =>
            t.HasCheckConstraint("CK_DayEvents_Hour", "[EventHour] IS NULL OR [EventHour] <= 23"));

        entity.HasKey(x => x.DayEventId);

        entity.Property(x => x.EventNote)
            .HasMaxLength(500);

        entity.HasIndex(x => new { x.DiaryEntryId, x.EventTypeId });

        entity.HasOne(x => x.DiaryEntry)
            .WithMany(d => d.DayEvents)
            .HasForeignKey(x => x.DiaryEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.EventType)
            .WithMany(e => e.DayEvents)
            .HasForeignKey(x => x.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> entity)
    {
        entity.ToTable("Measurements", t =>
            t.HasCheckConstraint("CK_Measurements_Hour", "[MeasurementHour] IS NULL OR [MeasurementHour] <= 23"));

        entity.HasKey(x => x.MeasurementId);

        entity.Property(x => x.MeasurementValue)
            .HasPrecision(6, 2)
            .IsRequired();

        entity.Property(x => x.MeasurementNote)
            .HasMaxLength(500);

        entity.HasIndex(x => new { x.DiaryEntryId, x.MetricTypeId });

        entity.HasOne(x => x.DiaryEntry)
            .WithMany(d => d.Measurements)
            .HasForeignKey(x => x.DiaryEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.MetricType)
            .WithMany(m => m.Measurements)
            .HasForeignKey(x => x.MetricTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> entity)
    {
        entity.ToTable("Reports", t =>
            t.HasCheckConstraint("CK_Reports_Period", "[ReportPeriodTo] >= [ReportPeriodFrom]"));

        entity.HasKey(x => x.ReportId);

        entity.Property(x => x.ReportPeriodFrom).IsRequired();
        entity.Property(x => x.ReportPeriodTo).IsRequired();
        entity.Property(x => x.ReportGeneratedAt).IsRequired();

        entity.Property(x => x.ReportContent)
            .IsRequired();

        entity.HasIndex(x => new { x.UserId, x.ReportGeneratedAt });

        entity.HasOne(x => x.User)
            .WithMany(u => u.Reports)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
