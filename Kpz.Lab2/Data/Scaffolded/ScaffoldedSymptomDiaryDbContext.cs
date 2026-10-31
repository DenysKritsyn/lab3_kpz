using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class ScaffoldedSymptomDiaryDbContext : DbContext
{
    public ScaffoldedSymptomDiaryDbContext(DbContextOptions<ScaffoldedSymptomDiaryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DayEvent> DayEvents { get; set; }

    public virtual DbSet<DayInterval> DayIntervals { get; set; }

    public virtual DbSet<DiaryEntry> DiaryEntries { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<IntervalSymptom> IntervalSymptoms { get; set; }

    public virtual DbSet<Measurement> Measurements { get; set; }

    public virtual DbSet<MetricType> MetricTypes { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SeverityLevel> SeverityLevels { get; set; }

    public virtual DbSet<Symptom> Symptoms { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WellbeingLevel> WellbeingLevels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DayEvent>(entity =>
        {
            entity.HasIndex(e => new { e.DiaryEntryId, e.EventTypeId }, "IX_DayEvents_DiaryEntryId_EventTypeId");

            entity.HasIndex(e => e.EventTypeId, "IX_DayEvents_EventTypeId");

            entity.Property(e => e.EventNote).HasMaxLength(500);

            entity.HasOne(d => d.DiaryEntry).WithMany(p => p.DayEvents).HasForeignKey(d => d.DiaryEntryId);

            entity.HasOne(d => d.EventType).WithMany(p => p.DayEvents)
                .HasForeignKey(d => d.EventTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DayInterval>(entity =>
        {
            entity.HasIndex(e => new { e.DiaryEntryId, e.IntervalStartHour }, "IX_DayIntervals_DiaryEntryId_IntervalStartHour");

            entity.HasIndex(e => e.WellbeingLevelId, "IX_DayIntervals_WellbeingLevelId");

            entity.Property(e => e.IntervalNote).HasMaxLength(1000);

            entity.HasOne(d => d.DiaryEntry).WithMany(p => p.DayIntervals).HasForeignKey(d => d.DiaryEntryId);

            entity.HasOne(d => d.WellbeingLevel).WithMany(p => p.DayIntervals)
                .HasForeignKey(d => d.WellbeingLevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DiaryEntry>(entity =>
        {
            entity.HasIndex(e => e.EntryDate, "IX_DiaryEntries_EntryDate");

            entity.HasIndex(e => new { e.UserId, e.EntryDate }, "IX_DiaryEntries_UserId_EntryDate").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.EntryUpdatedAt }, "IX_DiaryEntries_UserId_EntryUpdatedAt");

            entity.Property(e => e.EntryNote).HasMaxLength(2000);
            entity.Property(e => e.EntryRowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithMany(p => p.DiaryEntries).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasIndex(e => e.EventName, "IX_EventTypes_EventName").IsUnique();

            entity.Property(e => e.EventName).HasMaxLength(100);
        });

        modelBuilder.Entity<IntervalSymptom>(entity =>
        {
            entity.HasKey(e => new { e.DayIntervalId, e.SymptomId });

            entity.HasIndex(e => e.SeverityLevelId, "IX_IntervalSymptoms_SeverityLevelId");

            entity.HasIndex(e => e.SymptomId, "IX_IntervalSymptoms_SymptomId");

            entity.Property(e => e.SymptomNote).HasMaxLength(500);

            entity.HasOne(d => d.DayInterval).WithMany(p => p.IntervalSymptoms).HasForeignKey(d => d.DayIntervalId);

            entity.HasOne(d => d.SeverityLevel).WithMany(p => p.IntervalSymptoms)
                .HasForeignKey(d => d.SeverityLevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Symptom).WithMany(p => p.IntervalSymptoms)
                .HasForeignKey(d => d.SymptomId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Measurement>(entity =>
        {
            entity.HasIndex(e => new { e.DiaryEntryId, e.MetricTypeId }, "IX_Measurements_DiaryEntryId_MetricTypeId");

            entity.HasIndex(e => e.MetricTypeId, "IX_Measurements_MetricTypeId");

            entity.Property(e => e.MeasurementNote).HasMaxLength(500);
            entity.Property(e => e.MeasurementValue).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.DiaryEntry).WithMany(p => p.Measurements).HasForeignKey(d => d.DiaryEntryId);

            entity.HasOne(d => d.MetricType).WithMany(p => p.Measurements)
                .HasForeignKey(d => d.MetricTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MetricType>(entity =>
        {
            entity.HasIndex(e => e.MetricName, "IX_MetricTypes_MetricName").IsUnique();

            entity.Property(e => e.MetricMaxValue).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.MetricMinValue).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.MetricName).HasMaxLength(100);
            entity.Property(e => e.MetricUnit).HasMaxLength(20);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ReportGeneratedAt }, "IX_Reports_UserId_ReportGeneratedAt");

            entity.HasOne(d => d.User).WithMany(p => p.Reports).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SeverityLevel>(entity =>
        {
            entity.HasIndex(e => e.SeverityName, "IX_SeverityLevels_SeverityName").IsUnique();

            entity.HasIndex(e => e.SeverityRank, "IX_SeverityLevels_SeverityRank").IsUnique();

            entity.Property(e => e.SeverityName).HasMaxLength(50);
        });

        modelBuilder.Entity<Symptom>(entity =>
        {
            entity.HasIndex(e => e.SymptomCategory, "IX_Symptoms_SymptomCategory");

            entity.HasIndex(e => e.SymptomName, "IX_Symptoms_SymptomName").IsUnique();

            entity.Property(e => e.SymptomCategory).HasMaxLength(50);
            entity.Property(e => e.SymptomName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.ExternalAuthId, "IX_Users_ExternalAuthId").IsUnique();

            entity.HasIndex(e => e.UserEmail, "IX_Users_UserEmail").IsUnique();

            entity.Property(e => e.ExternalAuthId).HasMaxLength(200);
            entity.Property(e => e.UserEmail).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.UserPhoneNumber).HasMaxLength(25);
            entity.Property(e => e.UserRole).HasMaxLength(20);
        });

        modelBuilder.Entity<WellbeingLevel>(entity =>
        {
            entity.HasIndex(e => e.WellbeingName, "IX_WellbeingLevels_WellbeingName").IsUnique();

            entity.HasIndex(e => e.WellbeingRank, "IX_WellbeingLevels_WellbeingRank").IsUnique();

            entity.Property(e => e.WellbeingName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
