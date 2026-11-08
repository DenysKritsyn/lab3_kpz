using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using lab2_kpz_rev_eng.entities;

namespace lab2_kpz_rev_eng.context;

public partial class ScaffoldedSymptomDiaryDbContext : DbContext
{
    public ScaffoldedSymptomDiaryDbContext(DbContextOptions<ScaffoldedSymptomDiaryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ConditionState> ConditionStates { get; set; }

    public virtual DbSet<DayEvent> DayEvents { get; set; }

    public virtual DbSet<DayInterval> DayIntervals { get; set; }

    public virtual DbSet<DiaryEntry> DiaryEntries { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<IntervalSymptom> IntervalSymptoms { get; set; }

    public virtual DbSet<Measurement> Measurements { get; set; }

    public virtual DbSet<Metric> Metrics { get; set; }

    public virtual DbSet<MetricUnit> MetricUnits { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SeverityLevel> SeverityLevels { get; set; }

    public virtual DbSet<Symptom> Symptoms { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConditionState>(entity =>
        {
            entity.HasIndex(e => e.ConditionStateName, "IX_ConditionStates_ConditionStateName").IsUnique();

            entity.Property(e => e.ConditionStateName).HasMaxLength(50);
        });

        modelBuilder.Entity<DayEvent>(entity =>
        {
            entity.HasIndex(e => e.DiaryEntryId, "IX_DayEvents_DiaryEntryId");

            entity.HasIndex(e => e.EventTypeId, "IX_DayEvents_EventTypeId");

            entity.Property(e => e.EventEndTime).HasPrecision(0);
            entity.Property(e => e.EventNote).HasMaxLength(500);
            entity.Property(e => e.EventStartTime).HasPrecision(0);

            entity.HasOne(d => d.DiaryEntry).WithMany(p => p.DayEvents).HasForeignKey(d => d.DiaryEntryId);

            entity.HasOne(d => d.EventType).WithMany(p => p.DayEvents)
                .HasForeignKey(d => d.EventTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DayInterval>(entity =>
        {
            entity.HasIndex(e => e.ConditionStateId, "IX_DayIntervals_ConditionStateId");

            entity.HasIndex(e => e.DiaryEntryId, "IX_DayIntervals_DiaryEntryId");

            entity.Property(e => e.IntervalEndTime).HasPrecision(0);
            entity.Property(e => e.IntervalNote).HasMaxLength(500);
            entity.Property(e => e.IntervalStartTime).HasPrecision(0);

            entity.HasOne(d => d.ConditionState).WithMany(p => p.DayIntervals)
                .HasForeignKey(d => d.ConditionStateId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DiaryEntry).WithMany(p => p.DayIntervals).HasForeignKey(d => d.DiaryEntryId);
        });

        modelBuilder.Entity<DiaryEntry>(entity =>
        {
            entity.HasIndex(e => e.EntryDate, "IX_DiaryEntries_EntryDate");

            entity.HasIndex(e => new { e.UserId, e.EntryDate }, "IX_DiaryEntries_UserId_EntryDate").IsUnique();

            entity.Property(e => e.EntryNote).HasMaxLength(1000);
            entity.Property(e => e.EntryRowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithMany(p => p.DiaryEntries).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasIndex(e => e.EventTypeName, "IX_EventTypes_EventTypeName").IsUnique();

            entity.Property(e => e.EventTypeName).HasMaxLength(100);
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
            entity.HasIndex(e => e.DiaryEntryId, "IX_Measurements_DiaryEntryId");

            entity.Property(e => e.MeasuringTime).HasPrecision(0);

            entity.HasOne(d => d.DiaryEntry).WithMany(p => p.Measurements).HasForeignKey(d => d.DiaryEntryId);
        });

        modelBuilder.Entity<Metric>(entity =>
        {
            entity.HasIndex(e => e.MeasurementId, "IX_Metrics_MeasurementId");

            entity.HasIndex(e => e.MetricUnitId, "IX_Metrics_MetricUnitId");

            entity.Property(e => e.MetricName).HasMaxLength(100);
            entity.Property(e => e.MetricValue).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.Measurement).WithMany(p => p.Metrics).HasForeignKey(d => d.MeasurementId);

            entity.HasOne(d => d.MetricUnit).WithMany(p => p.Metrics)
                .HasForeignKey(d => d.MetricUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MetricUnit>(entity =>
        {
            entity.HasIndex(e => e.UnitName, "IX_MetricUnits_UnitName").IsUnique();

            entity.Property(e => e.UnitMaxValue).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.UnitMinValue).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.UnitName).HasMaxLength(50);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Reports_UserId");

            entity.Property(e => e.ReportSummary).HasMaxLength(2000);

            entity.HasOne(d => d.User).WithMany(p => p.Reports).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SeverityLevel>(entity =>
        {
            entity.HasIndex(e => e.SeverityName, "IX_SeverityLevels_SeverityName").IsUnique();

            entity.Property(e => e.SeverityName).HasMaxLength(50);
        });

        modelBuilder.Entity<Symptom>(entity =>
        {
            entity.HasIndex(e => e.SymptomName, "IX_Symptoms_SymptomName").IsUnique();

            entity.Property(e => e.SymptomName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.UserEmail, "IX_Users_UserEmail").IsUnique();

            entity.Property(e => e.UserEmail).HasMaxLength(120);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.UserPhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
