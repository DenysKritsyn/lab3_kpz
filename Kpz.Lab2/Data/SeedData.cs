using Kpz.Lab2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kpz.Lab2.Data;

public static class SeedData
{
    public static void Seed(DbContext context)
    {
        if (context.Set<User>().Any())
            return;

        var users = BuildUsers();
        context.Set<User>().AddRange(users);
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<User>().AnyAsync(ct))
            return;

        var users = BuildUsers();
        await context.Set<User>().AddRangeAsync(users, ct);
        await context.SaveChangesAsync(ct);
    }

    private static List<User> BuildUsers()
    {
        var baseTime = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

        var user1 = new User
        {
            ExternalAuthId = "google-oauth2|100001",
            UserEmail = "denys.patient@example.com",
            UserName = "Denys Kovalenko",
            UserPhoneNumber = "+380671112233",
            UserBirthDate = new DateOnly(2005, 4, 15),
            UserRole = "User",
            UserCreatedAt = baseTime,
            DiaryEntries =
            [
                new DiaryEntry
                {
                    EntryDate = new DateOnly(2026, 10, 20),
                    EntryNote = "Cold onset after getting caught in the rain.",
                    EntryCreatedAt = baseTime.AddDays(19).AddHours(14),
                    EntryUpdatedAt = baseTime.AddDays(19).AddHours(14),
                    DayIntervals =
                    [
                        new DayInterval
                        {
                            IntervalStartHour = 2, // 08:00
                            IntervalEndHour = 8,   // 14:00
                            WellbeingLevelId = 2,  // Moderate
                            IntervalNote = "Mild throat irritation in the morning",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 4, SeverityLevelId = 1, SymptomNote = "Scratchy throat when swallowing" },
                                new IntervalSymptom { SymptomId = 7, SeverityLevelId = 1, SymptomNote = "Slight morning tiredness" }
                            ]
                        },
                        new DayInterval
                        {
                            IntervalStartHour = 10, // 16:00
                            IntervalEndHour = 17,   // 23:00
                            WellbeingLevelId = 3,   // Poor
                            IntervalNote = "Fever rose and dry cough started",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 3, SeverityLevelId = 2, SymptomNote = "Persistent dry cough" },
                                new IntervalSymptom { SymptomId = 4, SeverityLevelId = 2, SymptomNote = "Sharp sore throat" },
                                new IntervalSymptom { SymptomId = 8, SeverityLevelId = 2, SymptomNote = "Evening chills" }
                            ]
                        }
                    ],
                    DayEvents =
                    [
                        new DayEvent { EventTypeId = 6, EventHour = 1, EventNote = "Caught in cold rain on the way to university" },
                        new DayEvent { EventTypeId = 3, EventHour = 13, EventNote = "Took Paracetamol 500mg" }
                    ],
                    Measurements =
                    [
                        new Measurement { MetricTypeId = 1, MeasurementValue = 37.40m, MeasurementHour = 6, MeasurementNote = "Afternoon check" },
                        new Measurement { MetricTypeId = 1, MeasurementValue = 38.30m, MeasurementHour = 12, MeasurementNote = "Evening before sleep" },
                        new Measurement { MetricTypeId = 4, MeasurementValue = 88.00m, MeasurementHour = 12, MeasurementNote = "Elevated heart rate due to fever" }
                    ]
                },
                new DiaryEntry
                {
                    EntryDate = new DateOnly(2026, 10, 21),
                    EntryNote = "Second day of illness, staying in bed.",
                    EntryCreatedAt = baseTime.AddDays(20).AddHours(15),
                    EntryUpdatedAt = baseTime.AddDays(20).AddHours(15),
                    DayIntervals =
                    [
                        new DayInterval
                        {
                            IntervalStartHour = 0,  // 06:00
                            IntervalEndHour = 12,   // 18:00
                            WellbeingLevelId = 3,   // Poor
                            IntervalNote = "Fatigue and nasal congestion all day",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 3, SeverityLevelId = 2, SymptomNote = "Frequent cough" },
                                new IntervalSymptom { SymptomId = 5, SeverityLevelId = 2, SymptomNote = "Blocked nose" },
                                new IntervalSymptom { SymptomId = 1, SeverityLevelId = 2, SymptomNote = "Frontal headache pressure" }
                            ]
                        },
                        new DayInterval
                        {
                            IntervalStartHour = 12, // 18:00
                            IntervalEndHour = 20,   // 02:00
                            WellbeingLevelId = 2,   // Moderate
                            IntervalNote = "Felt better after warm tea and rest",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 5, SeverityLevelId = 1, SymptomNote = "Runny nose subsided" },
                                new IntervalSymptom { SymptomId = 7, SeverityLevelId = 1, SymptomNote = "Residual weakness" }
                            ]
                        }
                    ],
                    DayEvents =
                    [
                        new DayEvent { EventTypeId = 3, EventHour = 4, EventNote = "Throat spray and Vitamin C" }
                    ],
                    Measurements =
                    [
                        new Measurement { MetricTypeId = 1, MeasurementValue = 37.80m, MeasurementHour = 3, MeasurementNote = "Morning reading" },
                        new Measurement { MetricTypeId = 1, MeasurementValue = 37.10m, MeasurementHour = 14, MeasurementNote = "Evening reading" },
                        new Measurement { MetricTypeId = 5, MeasurementValue = 97.00m, MeasurementHour = 14, MeasurementNote = "Normal SpO2" }
                    ]
                },
                new DiaryEntry
                {
                    EntryDate = new DateOnly(2026, 10, 22),
                    EntryNote = "Recovering, but had a project deadline.",
                    EntryCreatedAt = baseTime.AddDays(21).AddHours(16),
                    EntryUpdatedAt = baseTime.AddDays(21).AddHours(16),
                    DayIntervals =
                    [
                        new DayInterval
                        {
                            IntervalStartHour = 3,  // 09:00
                            IntervalEndHour = 15,   // 21:00
                            WellbeingLevelId = 2,   // Moderate
                            IntervalNote = "Headache after long laptop session",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 1, SeverityLevelId = 2, SymptomNote = "Temporal headache" },
                                new IntervalSymptom { SymptomId = 3, SeverityLevelId = 1, SymptomNote = "Occasional cough" }
                            ]
                        }
                    ],
                    DayEvents =
                    [
                        new DayEvent { EventTypeId = 1, EventHour = 8, EventNote = "Stress before lab defense" },
                        new DayEvent { EventTypeId = 5, EventHour = 9, EventNote = "Double espresso" }
                    ],
                    Measurements =
                    [
                        new Measurement { MetricTypeId = 1, MeasurementValue = 36.60m, MeasurementHour = 4, MeasurementNote = "Temperature back to normal" },
                        new Measurement { MetricTypeId = 2, MeasurementValue = 132.00m, MeasurementHour = 11, MeasurementNote = "Systolic after coffee" },
                        new Measurement { MetricTypeId = 3, MeasurementValue = 86.00m, MeasurementHour = 11, MeasurementNote = "Diastolic after coffee" }
                    ]
                },
                new DiaryEntry
                {
                    EntryDate = new DateOnly(2026, 10, 23),
                    EntryNote = "Almost fully recovered.",
                    EntryCreatedAt = baseTime.AddDays(22).AddHours(16),
                    EntryUpdatedAt = baseTime.AddDays(22).AddHours(16),
                    DayIntervals =
                    [
                        new DayInterval
                        {
                            IntervalStartHour = 0,
                            IntervalEndHour = 24,
                            WellbeingLevelId = 1, // Good
                            IntervalNote = "Full-day summary interval — feeling good",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 7, SeverityLevelId = 1, SymptomNote = "Mild evening tiredness" }
                            ]
                        }
                    ],
                    DayEvents =
                    [
                        new DayEvent { EventTypeId = 4, EventHour = 12, EventNote = "5 km evening walk" }
                    ],
                    Measurements =
                    [
                        new Measurement { MetricTypeId = 1, MeasurementValue = 36.50m, MeasurementHour = 2, MeasurementNote = null },
                        new Measurement { MetricTypeId = 4, MeasurementValue = 72.00m, MeasurementHour = 13, MeasurementNote = "Resting heart rate" }
                    ]
                }
            ],
            Reports =
            [
                new Report
                {
                    ReportPeriodFrom = new DateOnly(2026, 10, 20),
                    ReportPeriodTo = new DateOnly(2026, 10, 23),
                    ReportGeneratedAt = baseTime.AddDays(23),
                    ReportContent = """{"patient":"Denys Kovalenko","period":"2026-10-20..2026-10-23","daysCount":4,"topSymptoms":["Cough","Headache","Fatigue"],"maxTemperature":38.3}"""
                }
            ]
        };

        var user2 = new User
        {
            ExternalAuthId = "google-oauth2|100002",
            UserEmail = "olena.savchuk@example.com",
            UserName = "Olena Savchuk",
            UserBirthDate = new DateOnly(1998, 11, 3),
            UserRole = "User",
            UserCreatedAt = baseTime.AddDays(1),
            DiaryEntries =
            [
                new DiaryEntry
                {
                    EntryDate = new DateOnly(2026, 10, 21),
                    EntryNote = "Migraine triggered by weather change and lack of sleep.",
                    EntryCreatedAt = baseTime.AddDays(20).AddHours(18),
                    EntryUpdatedAt = baseTime.AddDays(20).AddHours(18),
                    DayIntervals =
                    [
                        new DayInterval
                        {
                            IntervalStartHour = 1,  // 07:00
                            IntervalEndHour = 9,    // 15:00
                            WellbeingLevelId = 4,   // Very Poor
                            IntervalNote = "Acute morning migraine attack",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 1, SeverityLevelId = 3, SymptomNote = "Throbbing pain on the left side" },
                                new IntervalSymptom { SymptomId = 6, SeverityLevelId = 2, SymptomNote = "Nausea and photophobia" },
                                new IntervalSymptom { SymptomId = 2, SeverityLevelId = 2, SymptomNote = "Vertigo when standing up" }
                            ]
                        },
                        new DayInterval
                        {
                            IntervalStartHour = 9,  // 15:00
                            IntervalEndHour = 18,   // 00:00
                            WellbeingLevelId = 2,   // Moderate
                            IntervalNote = "Pain subsided after triptan intake",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 7, SeverityLevelId = 1, SymptomNote = "Post-migraine drowsiness" }
                            ]
                        }
                    ],
                    DayEvents =
                    [
                        new DayEvent { EventTypeId = 2, EventHour = null, EventNote = "Slept only 4.5 hours" },
                        new DayEvent { EventTypeId = 3, EventHour = 3, EventNote = "Took migraine medication" }
                    ],
                    Measurements =
                    [
                        new Measurement { MetricTypeId = 2, MeasurementValue = 138.00m, MeasurementHour = 2, MeasurementNote = "During migraine attack" },
                        new Measurement { MetricTypeId = 3, MeasurementValue = 90.00m, MeasurementHour = 2, MeasurementNote = "During migraine attack" },
                        new Measurement { MetricTypeId = 4, MeasurementValue = 84.00m, MeasurementHour = 2, MeasurementNote = null }
                    ]
                },
                new DiaryEntry
                {
                    EntryDate = new DateOnly(2026, 10, 22),
                    EntryNote = "Calm day after catching up on sleep.",
                    EntryCreatedAt = baseTime.AddDays(21).AddHours(19),
                    EntryUpdatedAt = baseTime.AddDays(21).AddHours(19),
                    DayIntervals =
                    [
                        new DayInterval
                        {
                            IntervalStartHour = 0,
                            IntervalEndHour = 24,
                            WellbeingLevelId = 2,
                            IntervalNote = "Mild background discomfort in the afternoon",
                            IntervalSymptoms =
                            [
                                new IntervalSymptom { SymptomId = 1, SeverityLevelId = 1, SymptomNote = "Mild tension headache" }
                            ]
                        }
                    ],
                    Measurements =
                    [
                        new Measurement { MetricTypeId = 2, MeasurementValue = 120.00m, MeasurementHour = 4, MeasurementNote = "Normal BP" },
                        new Measurement { MetricTypeId = 3, MeasurementValue = 78.00m, MeasurementHour = 4, MeasurementNote = "Normal BP" }
                    ]
                }
            ],
            Reports =
            [
                new Report
                {
                    ReportPeriodFrom = new DateOnly(2026, 10, 21),
                    ReportPeriodTo = new DateOnly(2026, 10, 22),
                    ReportGeneratedAt = baseTime.AddDays(22),
                    ReportContent = """{"patient":"Olena Savchuk","period":"2026-10-21..2026-10-22","daysCount":2,"topSymptoms":["Headache","Nausea"]}"""
                }
            ]
        };

        var user3 = new User
        {
            ExternalAuthId = "google-oauth2|100003",
            UserEmail = "dr.admin@example.com",
            UserName = "Dr. Ihor Melnyk",
            UserBirthDate = new DateOnly(1985, 7, 22),
            UserRole = "Admin",
            UserCreatedAt = baseTime
        };

        return [user1, user2, user3];
    }
}
