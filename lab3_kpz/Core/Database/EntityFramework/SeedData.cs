using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Data;

public static class SeedData
{
    public static void ConfigureStaticData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConditionState>().HasData(
            new ConditionState { ConditionStateId = 1, ConditionStateName = "Critical" },
            new ConditionState { ConditionStateId = 2, ConditionStateName = "Bad" },
            new ConditionState { ConditionStateId = 3, ConditionStateName = "Fair" },
            new ConditionState { ConditionStateId = 4, ConditionStateName = "Good" },
            new ConditionState { ConditionStateId = 5, ConditionStateName = "Excellent" }
        );

        modelBuilder.Entity<SeverityLevel>().HasData(
            new SeverityLevel { SeverityLevelId = 1, SeverityName = "Mild" },
            new SeverityLevel { SeverityLevelId = 2, SeverityName = "Moderate" },
            new SeverityLevel { SeverityLevelId = 3, SeverityName = "Severe" },
            new SeverityLevel { SeverityLevelId = 4, SeverityName = "Very Severe" }
        );

        modelBuilder.Entity<EventType>().HasData(
            new EventType { EventTypeId = 1, EventTypeName = "Medication Intake" },
            new EventType { EventTypeId = 2, EventTypeName = "Physical Exercise" },
            new EventType { EventTypeId = 3, EventTypeName = "Meal" },
            new EventType { EventTypeId = 4, EventTypeName = "Sleep" },
            new EventType { EventTypeId = 5, EventTypeName = "Emotional Stress" },
            new EventType { EventTypeId = 6, EventTypeName = "Rest"}
        );

        modelBuilder.Entity<Symptom>().HasData(
            new Symptom { SymptomId = 1, SymptomName = "Headache" },
            new Symptom { SymptomId = 2, SymptomName = "Fatigue" },
            new Symptom { SymptomId = 3, SymptomName = "Nausea" },
            new Symptom { SymptomId = 4, SymptomName = "Dizziness" },
            new Symptom { SymptomId = 5, SymptomName = "Dry Cough" },
            new Symptom { SymptomId = 6, SymptomName = "Fever" },
            new Symptom { SymptomId = 7, SymptomName = "Shortness of Breath" },
            new Symptom { SymptomId = 8, SymptomName = "Joint and Muscle Pain" }
        );

        modelBuilder.Entity<MetricUnit>().HasData(
            new MetricUnit { MetricUnitId = 1, UnitName = "Degrees Celsius (°C)", UnitMinValue = 34.00m, UnitMaxValue = 43.00m },
            new MetricUnit { MetricUnitId = 2, UnitName = "Beats Per Minute (bpm)", UnitMinValue = 30.00m, UnitMaxValue = 220.00m },
            new MetricUnit { MetricUnitId = 3, UnitName = "Millimeters of Mercury (mmHg)", UnitMinValue = 40.00m, UnitMaxValue = 260.00m },
            new MetricUnit { MetricUnitId = 4, UnitName = "Blood Oxygen Level (SpO2 %)", UnitMinValue = 50.00m, UnitMaxValue = 100.00m },
            new MetricUnit { MetricUnitId = 5, UnitName = "Blood Glucose (mmol/L)", UnitMinValue = 1.00m, UnitMaxValue = 35.00m }
        );
    }

    public static void ConfigureRepresentativeData(DbContext context)
    {
        var diaryContext = (SymptomDiaryDbContext)context;
        if (diaryContext.Users.Any())
        {
            return;
        }

        var users = TestData.GetSampleUsers();
        diaryContext.Users.AddRange(users);
        diaryContext.SaveChanges();

        var now = DateTime.UtcNow;

        var diaryEntries = TestData.GetSampleDiaryEntries(users, now);
        diaryContext.DiaryEntries.AddRange(diaryEntries);
        diaryContext.SaveChanges();

        var dayIntervals = TestData.GetSampleDayIntervals(diaryEntries);
        diaryContext.DayIntervals.AddRange(dayIntervals);
        diaryContext.SaveChanges();

        var intervalSymptoms = TestData.GetSampleIntervalSymptoms(dayIntervals);
        diaryContext.IntervalSymptoms.AddRange(intervalSymptoms);

        var dayEvents = TestData.GetSampleDayEvents(diaryEntries);
        diaryContext.DayEvents.AddRange(dayEvents);

        var measurements = TestData.GetSampleMeasurements(diaryEntries);
        diaryContext.Measurements.AddRange(measurements);
        diaryContext.SaveChanges();

        var metrics = TestData.GetSampleMetrics(measurements);
        diaryContext.Metrics.AddRange(metrics);

        var reports = TestData.GetSampleReports(users, now);
        diaryContext.Reports.AddRange(reports);

        diaryContext.SaveChanges();
    }

    public static async Task ConfigureRepresentativeDataAsync(DbContext context, CancellationToken ct = default)
    {
        var diaryContext = (SymptomDiaryDbContext)context;
        if (await diaryContext.Users.AnyAsync(ct))
        {
            return;
        }

        var users = TestData.GetSampleUsers();
        diaryContext.Users.AddRange(users);
        await diaryContext.SaveChangesAsync(ct);

        var now = DateTime.UtcNow;

        var diaryEntries = TestData.GetSampleDiaryEntries(users, now);
        diaryContext.DiaryEntries.AddRange(diaryEntries);
        await diaryContext.SaveChangesAsync(ct);

        var dayIntervals = TestData.GetSampleDayIntervals(diaryEntries);
        diaryContext.DayIntervals.AddRange(dayIntervals);
        await diaryContext.SaveChangesAsync(ct);

        var intervalSymptoms = TestData.GetSampleIntervalSymptoms(dayIntervals);
        diaryContext.IntervalSymptoms.AddRange(intervalSymptoms);

        var dayEvents = TestData.GetSampleDayEvents(diaryEntries);
        diaryContext.DayEvents.AddRange(dayEvents);

        var measurements = TestData.GetSampleMeasurements(diaryEntries);
        diaryContext.Measurements.AddRange(measurements);
        await diaryContext.SaveChangesAsync(ct);

        var metrics = TestData.GetSampleMetrics(measurements);
        diaryContext.Metrics.AddRange(metrics);

        var reports = TestData.GetSampleReports(users, now);
        diaryContext.Reports.AddRange(reports);

        await diaryContext.SaveChangesAsync(ct);
    }
}