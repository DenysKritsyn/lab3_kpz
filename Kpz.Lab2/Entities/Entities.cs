namespace Kpz.Lab2.Entities;

public sealed class WellbeingLevel
{
    public int WellbeingLevelId { get; set; }
    public required string WellbeingName { get; set; }
    public byte WellbeingRank { get; set; }

    public ICollection<DayInterval> DayIntervals { get; set; } = [];
}

public sealed class SeverityLevel
{
    public int SeverityLevelId { get; set; }
    public required string SeverityName { get; set; }
    public byte SeverityRank { get; set; }

    public ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = [];
}

public sealed class Symptom
{
    public int SymptomId { get; set; }
    public required string SymptomName { get; set; }
    public string? SymptomCategory { get; set; }

    public ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = [];
}

public sealed class EventType
{
    public int EventTypeId { get; set; }
    public required string EventName { get; set; }

    public ICollection<DayEvent> DayEvents { get; set; } = [];
}

public sealed class MetricType
{
    public int MetricTypeId { get; set; }
    public required string MetricName { get; set; }
    public required string MetricUnit { get; set; }
    public decimal MetricMinValue { get; set; }
    public decimal MetricMaxValue { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = [];
}

public sealed class User
{
    public int UserId { get; set; }
    public required string ExternalAuthId { get; set; }
    public required string UserEmail { get; set; }
    public required string UserName { get; set; }
    public DateOnly? UserBirthDate { get; set; }
    public required string UserRole { get; set; }
    public DateTime UserCreatedAt { get; set; }

    public ICollection<DiaryEntry> DiaryEntries { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
}

public sealed class DiaryEntry
{
    public int DiaryEntryId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly EntryDate { get; set; }
    public string? EntryNote { get; set; }
    public DateTime EntryCreatedAt { get; set; }
    public DateTime EntryUpdatedAt { get; set; }
    public byte[] EntryRowVersion { get; set; } = [];

    public ICollection<DayInterval> DayIntervals { get; set; } = [];
    public ICollection<DayEvent> DayEvents { get; set; } = [];
    public ICollection<Measurement> Measurements { get; set; } = [];
}

public sealed class DayInterval
{
    public int DayIntervalId { get; set; }
    public int DiaryEntryId { get; set; }
    public DiaryEntry DiaryEntry { get; set; } = null!;
    public byte IntervalStartHour { get; set; }
    public byte IntervalEndHour { get; set; }
    public int WellbeingLevelId { get; set; }
    public WellbeingLevel WellbeingLevel { get; set; } = null!;
    public string? IntervalNote { get; set; }

    public ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = [];
}

public sealed class IntervalSymptom
{
    public int DayIntervalId { get; set; }
    public DayInterval DayInterval { get; set; } = null!;
    public int SymptomId { get; set; }
    public Symptom Symptom { get; set; } = null!;
    public int SeverityLevelId { get; set; }
    public SeverityLevel SeverityLevel { get; set; } = null!;
    public string? SymptomNote { get; set; }
}

public sealed class DayEvent
{
    public int DayEventId { get; set; }
    public int DiaryEntryId { get; set; }
    public DiaryEntry DiaryEntry { get; set; } = null!;
    public int EventTypeId { get; set; }
    public EventType EventType { get; set; } = null!;
    public byte? EventHour { get; set; }
    public string? EventNote { get; set; }
}

public sealed class Measurement
{
    public int MeasurementId { get; set; }
    public int DiaryEntryId { get; set; }
    public DiaryEntry DiaryEntry { get; set; } = null!;
    public int MetricTypeId { get; set; }
    public MetricType MetricType { get; set; } = null!;
    public decimal MeasurementValue { get; set; }
    public byte? MeasurementHour { get; set; }
    public string? MeasurementNote { get; set; }
}

public sealed class Report
{
    public int ReportId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly ReportPeriodFrom { get; set; }
    public DateOnly ReportPeriodTo { get; set; }
    public DateTime ReportGeneratedAt { get; set; }
    public required string ReportContent { get; set; }
}

