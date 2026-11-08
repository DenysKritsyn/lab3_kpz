using lab2_kpz.Entities;

namespace lab2_kpz.Data;

public static class TestData
{
    public static List<User> GetSampleUsers()
    {
        return
        [
            new User
            {
                UserEmail = "alex.smith@example.com",
                UserName = "Alex Smith",
                UserBirthDate = new DateOnly(1992, 4, 15),
                UserPhoneNumber = "+380501112233"
            },
            new User
            {
                UserEmail = "olena.koval@example.com",
                UserName = "Olena Koval",
                UserBirthDate = new DateOnly(1987, 11, 23),
                UserPhoneNumber = "+380672223344"
            },
            new User
            {
                UserEmail = "dmytro.shevchenko@example.com",
                UserName = "Dmytro Shevchenko",
                UserBirthDate = new DateOnly(1995, 7, 8),
                UserPhoneNumber = "+380933334455"
            }
        ];
    }

    public static List<DiaryEntry> GetSampleDiaryEntries(IReadOnlyList<User> users, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;

        return
        [
            new DiaryEntry
            {
                UserId = users[0].UserId,
                EntryDate = new DateOnly(2026, 11, 1),
                EntryNote = "Felt mild fatigue in the morning, improved towards evening.",
                EntryCreatedAt = timestamp.AddDays(-5),
                EntryUpdatedAt = timestamp.AddDays(-5)
            },
            new DiaryEntry
            {
                UserId = users[0].UserId,
                EntryDate = new DateOnly(2026, 11, 2),
                EntryNote = "Severe headache after a stressful team meeting.",
                EntryCreatedAt = timestamp.AddDays(-4),
                EntryUpdatedAt = timestamp.AddDays(-4)
            },
            new DiaryEntry
            {
                UserId = users[1].UserId,
                EntryDate = new DateOnly(2026, 11, 1),
                EntryNote = "Post-workout muscle soreness.",
                EntryCreatedAt = timestamp.AddDays(-5),
                EntryUpdatedAt = timestamp.AddDays(-5)
            },
            new DiaryEntry
            {
                UserId = users[2].UserId,
                EntryDate = new DateOnly(2026, 11, 1),
                EntryNote = "Slight dizziness after quick position change.",
                EntryCreatedAt = timestamp.AddDays(-5),
                EntryUpdatedAt = timestamp.AddDays(-5)
            }
        ];
    }

    public static List<DayInterval> GetSampleDayIntervals(IReadOnlyList<DiaryEntry> diaryEntries)
    {
        return
        [
            new DayInterval
            {
                DiaryEntryId = diaryEntries[0].DiaryEntryId,
                IntervalStartTime = new TimeOnly(8, 0),
                IntervalEndTime = new TimeOnly(12, 0),
                ConditionStateId = 3,
                IntervalNote = "Morning work session"
            },
            new DayInterval
            {
                DiaryEntryId = diaryEntries[0].DiaryEntryId,
                IntervalStartTime = new TimeOnly(12, 0),
                IntervalEndTime = new TimeOnly(18, 0),
                ConditionStateId = 4,
                IntervalNote = "Afternoon walk and focus time"
            },
            new DayInterval
            {
                DiaryEntryId = diaryEntries[0].DiaryEntryId,
                IntervalStartTime = new TimeOnly(18, 0),
                IntervalEndTime = new TimeOnly(23, 0),
                ConditionStateId = 4,
                IntervalNote = "Evening relaxation"
            },
            new DayInterval
            {
                DiaryEntryId = diaryEntries[1].DiaryEntryId,
                IntervalStartTime = new TimeOnly(9, 0),
                IntervalEndTime = new TimeOnly(14, 0),
                ConditionStateId = 2,
                IntervalNote = "Intense work block"
            },
            new DayInterval
            {
                DiaryEntryId = diaryEntries[1].DiaryEntryId,
                IntervalStartTime = new TimeOnly(14, 0),
                IntervalEndTime = new TimeOnly(22, 0),
                ConditionStateId = 2,
                IntervalNote = "Resting in a dark room"
            },
            new DayInterval
            {
                DiaryEntryId = diaryEntries[2].DiaryEntryId,
                IntervalStartTime = new TimeOnly(7, 30),
                IntervalEndTime = new TimeOnly(13, 0),
                ConditionStateId = 4,
                IntervalNote = "Morning jog and breakfast"
            },
            new DayInterval
            {
                DiaryEntryId = diaryEntries[2].DiaryEntryId,
                IntervalStartTime = new TimeOnly(13, 0),
                IntervalEndTime = new TimeOnly(21, 0),
                ConditionStateId = 3,
                IntervalNote = "Moderate muscle fatigue"
            },
            new DayInterval
            {
                DiaryEntryId = diaryEntries[3].DiaryEntryId,
                IntervalStartTime = new TimeOnly(8, 30),
                IntervalEndTime = new TimeOnly(17, 30),
                ConditionStateId = 3,
                IntervalNote = "Office hours"
            }
        ];
    }

    public static List<IntervalSymptom> GetSampleIntervalSymptoms(IReadOnlyList<DayInterval> dayIntervals)
    {
        return
        [
            new IntervalSymptom { DayIntervalId = dayIntervals[0].DayIntervalId, SymptomId = 2, SeverityLevelId = 2, SymptomNote = "Drowsy and slow start" },
            new IntervalSymptom { DayIntervalId = dayIntervals[0].DayIntervalId, SymptomId = 1, SeverityLevelId = 1, SymptomNote = "Faint temple pressure" },
            new IntervalSymptom { DayIntervalId = dayIntervals[3].DayIntervalId, SymptomId = 1, SeverityLevelId = 3, SymptomNote = "Pulsating pain behind eyes" },
            new IntervalSymptom { DayIntervalId = dayIntervals[3].DayIntervalId, SymptomId = 4, SeverityLevelId = 2, SymptomNote = "Unsteadiness upon standing" },
            new IntervalSymptom { DayIntervalId = dayIntervals[4].DayIntervalId, SymptomId = 1, SeverityLevelId = 4, SymptomNote = "Photophobia and severe headache" },
            new IntervalSymptom { DayIntervalId = dayIntervals[4].DayIntervalId, SymptomId = 3, SeverityLevelId = 2, SymptomNote = "Mild nausea from pain" },
            new IntervalSymptom { DayIntervalId = dayIntervals[5].DayIntervalId, SymptomId = 8, SeverityLevelId = 1, SymptomNote = "Mild calf tension" },
            new IntervalSymptom { DayIntervalId = dayIntervals[6].DayIntervalId, SymptomId = 8, SeverityLevelId = 2, SymptomNote = "Hamstring soreness" },
            new IntervalSymptom { DayIntervalId = dayIntervals[6].DayIntervalId, SymptomId = 2, SeverityLevelId = 1, SymptomNote = "Physical exhaustion" },
            new IntervalSymptom { DayIntervalId = dayIntervals[7].DayIntervalId, SymptomId = 4, SeverityLevelId = 2, SymptomNote = "Brief vertigo when standing up" }
        ];
    }

    public static List<DayEvent> GetSampleDayEvents(IReadOnlyList<DiaryEntry> diaryEntries)
    {
        return
        [
            new DayEvent { DiaryEntryId = diaryEntries[0].DiaryEntryId, EventTypeId = 3, EventStartTime = new TimeOnly(8, 30), EventEndTime = new TimeOnly(9, 0), IsPreciseTime = true, EventNote = "Oatmeal and black tea" },
            new DayEvent { DiaryEntryId = diaryEntries[0].DiaryEntryId, EventTypeId = 2, EventStartTime = new TimeOnly(17, 30), EventEndTime = new TimeOnly(18, 30), IsPreciseTime = false, EventNote = "Evening park walk" },
            new DayEvent { DiaryEntryId = diaryEntries[1].DiaryEntryId, EventTypeId = 5, EventStartTime = new TimeOnly(10, 0), EventEndTime = new TimeOnly(12, 0), IsPreciseTime = true, EventNote = "Client escalation meeting" },
            new DayEvent { DiaryEntryId = diaryEntries[1].DiaryEntryId, EventTypeId = 1, EventStartTime = new TimeOnly(14, 15), EventEndTime = new TimeOnly(14, 20), IsPreciseTime = true, EventNote = "Ibuprofen 400mg" },
            new DayEvent { DiaryEntryId = diaryEntries[2].DiaryEntryId, EventTypeId = 2, EventStartTime = new TimeOnly(7, 30), EventEndTime = new TimeOnly(8, 30), IsPreciseTime = true, EventNote = "5km outdoor run" },
            new DayEvent { DiaryEntryId = diaryEntries[3].DiaryEntryId, EventTypeId = 4, EventStartTime = new TimeOnly(23, 0), EventEndTime = new TimeOnly(7, 0), IsPreciseTime = false, EventNote = "Restful deep sleep" }
        ];
    }

    public static List<Measurement> GetSampleMeasurements(IReadOnlyList<DiaryEntry> diaryEntries)
    {
        return
        [
            new Measurement { DiaryEntryId = diaryEntries[0].DiaryEntryId, MeasuringTime = new TimeOnly(8, 15) },
            new Measurement { DiaryEntryId = diaryEntries[0].DiaryEntryId, MeasuringTime = new TimeOnly(18, 45) },
            new Measurement { DiaryEntryId = diaryEntries[1].DiaryEntryId, MeasuringTime = new TimeOnly(14, 0) },
            new Measurement { DiaryEntryId = diaryEntries[1].DiaryEntryId, MeasuringTime = new TimeOnly(20, 0) },
            new Measurement { DiaryEntryId = diaryEntries[2].DiaryEntryId, MeasuringTime = new TimeOnly(8, 45) },
            new Measurement { DiaryEntryId = diaryEntries[3].DiaryEntryId, MeasuringTime = new TimeOnly(9, 0) }
        ];
    }

    public static List<Metric> GetSampleMetrics(IReadOnlyList<Measurement> measurements)
    {
        return
        [
            new Metric { MeasurementId = measurements[0].MeasurementId, MetricUnitId = 1, MetricName = "Body Temperature", MetricValue = 36.60m },
            new Metric { MeasurementId = measurements[0].MeasurementId, MetricUnitId = 2, MetricName = "Resting Heart Rate", MetricValue = 68.00m },
            new Metric { MeasurementId = measurements[1].MeasurementId, MetricUnitId = 1, MetricName = "Body Temperature", MetricValue = 36.80m },
            new Metric { MeasurementId = measurements[1].MeasurementId, MetricUnitId = 3, MetricName = "Systolic Blood Pressure", MetricValue = 120.00m },

            new Metric { MeasurementId = measurements[2].MeasurementId, MetricUnitId = 1, MetricName = "Body Temperature", MetricValue = 37.20m },
            new Metric { MeasurementId = measurements[2].MeasurementId, MetricUnitId = 2, MetricName = "Heart Rate", MetricValue = 92.00m },
            new Metric { MeasurementId = measurements[2].MeasurementId, MetricUnitId = 3, MetricName = "Systolic Blood Pressure", MetricValue = 135.00m },
            new Metric { MeasurementId = measurements[3].MeasurementId, MetricUnitId = 1, MetricName = "Body Temperature", MetricValue = 36.90m },

            new Metric { MeasurementId = measurements[4].MeasurementId, MetricUnitId = 2, MetricName = "Post-Run Heart Rate", MetricValue = 118.00m },
            new Metric { MeasurementId = measurements[4].MeasurementId, MetricUnitId = 4, MetricName = "Blood Oxygen Saturation", MetricValue = 98.00m },

            new Metric { MeasurementId = measurements[5].MeasurementId, MetricUnitId = 3, MetricName = "Systolic Blood Pressure", MetricValue = 110.00m },
            new Metric { MeasurementId = measurements[5].MeasurementId, MetricUnitId = 5, MetricName = "Fasting Blood Glucose", MetricValue = 5.20m }
        ];
    }

    public static List<Report> GetSampleReports(IReadOnlyList<User> users, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;

        return
        [
            new Report
            {
                UserId = users[0].UserId,
                ReportPeriodFrom = new DateOnly(2026, 11, 1),
                ReportPeriodTo = new DateOnly(2026, 11, 2),
                ReportGeneratedAt = timestamp,
                ReportSummary = "Patient exhibited moderate tension headache correlating with work stress. Medication provided relief."
            },
            new Report
            {
                UserId = users[1].UserId,
                ReportPeriodFrom = new DateOnly(2026, 11, 1),
                ReportPeriodTo = new DateOnly(2026, 11, 1),
                ReportGeneratedAt = timestamp,
                ReportSummary = "Stable cardiovascular metrics following physical exertion. No pathological symptoms observed."
            },
            new Report
            {
                UserId = users[2].UserId,
                ReportPeriodFrom = new DateOnly(2026, 11, 1),
                ReportPeriodTo = new DateOnly(2026, 11, 1),
                ReportGeneratedAt = timestamp,
                ReportSummary = "Isolated orthostatic dizziness in the morning. Glucose and blood pressure parameters remain within normal physiological ranges."
            }
        ];
    }
}

