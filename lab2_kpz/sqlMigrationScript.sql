IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [ConditionStates] (
    [ConditionStateId] int NOT NULL IDENTITY,
    [ConditionStateName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_ConditionStates] PRIMARY KEY ([ConditionStateId])
);

CREATE TABLE [EventTypes] (
    [EventTypeId] int NOT NULL IDENTITY,
    [EventTypeName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_EventTypes] PRIMARY KEY ([EventTypeId])
);

CREATE TABLE [MetricUnits] (
    [MetricUnitId] int NOT NULL IDENTITY,
    [UnitName] nvarchar(50) NOT NULL,
    [UnitMinValue] decimal(6,2) NOT NULL,
    [UnitMaxValue] decimal(6,2) NOT NULL,
    CONSTRAINT [PK_MetricUnits] PRIMARY KEY ([MetricUnitId])
);

CREATE TABLE [SeverityLevels] (
    [SeverityLevelId] int NOT NULL IDENTITY,
    [SeverityName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_SeverityLevels] PRIMARY KEY ([SeverityLevelId])
);

CREATE TABLE [Symptoms] (
    [SymptomId] int NOT NULL IDENTITY,
    [SymptomName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Symptoms] PRIMARY KEY ([SymptomId])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [UserEmail] nvarchar(120) NOT NULL,
    [UserName] nvarchar(100) NOT NULL,
    [UserBirthDate] date NULL,
    [UserPhoneNumber] varchar(20) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);

CREATE TABLE [DiaryEntries] (
    [DiaryEntryId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EntryDate] date NOT NULL,
    [EntryNote] nvarchar(1000) NULL,
    [EntryCreatedAt] datetime2(7) NOT NULL,
    [EntryUpdatedAt] datetime2(7) NOT NULL,
    [EntryRowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DiaryEntries] PRIMARY KEY ([DiaryEntryId]),
    CONSTRAINT [FK_DiaryEntries_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Reports] (
    [ReportId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ReportPeriodFrom] date NOT NULL,
    [ReportPeriodTo] date NOT NULL,
    [ReportGeneratedAt] datetime2(7) NOT NULL,
    [ReportSummary] nvarchar(2000) NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportId]),
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [DayEvents] (
    [DayEventId] int NOT NULL IDENTITY,
    [DiaryEntryId] int NOT NULL,
    [EventTypeId] int NOT NULL,
    [EventStartTime] time(0) NOT NULL,
    [EventEndTime] time(0) NOT NULL,
    [IsPreciseTime] bit NOT NULL,
    [EventNote] nvarchar(500) NULL,
    CONSTRAINT [PK_DayEvents] PRIMARY KEY ([DayEventId]),
    CONSTRAINT [FK_DayEvents_DiaryEntries_DiaryEntryId] FOREIGN KEY ([DiaryEntryId]) REFERENCES [DiaryEntries] ([DiaryEntryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_DayEvents_EventTypes_EventTypeId] FOREIGN KEY ([EventTypeId]) REFERENCES [EventTypes] ([EventTypeId]) ON DELETE NO ACTION
);

CREATE TABLE [DayIntervals] (
    [DayIntervalId] int NOT NULL IDENTITY,
    [DiaryEntryId] int NOT NULL,
    [IntervalStartTime] time(0) NOT NULL,
    [IntervalEndTime] time(0) NOT NULL,
    [ConditionStateId] int NOT NULL,
    [IntervalNote] nvarchar(500) NULL,
    CONSTRAINT [PK_DayIntervals] PRIMARY KEY ([DayIntervalId]),
    CONSTRAINT [FK_DayIntervals_ConditionStates_ConditionStateId] FOREIGN KEY ([ConditionStateId]) REFERENCES [ConditionStates] ([ConditionStateId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DayIntervals_DiaryEntries_DiaryEntryId] FOREIGN KEY ([DiaryEntryId]) REFERENCES [DiaryEntries] ([DiaryEntryId]) ON DELETE CASCADE
);

CREATE TABLE [Measurements] (
    [MeasurementId] int NOT NULL IDENTITY,
    [DiaryEntryId] int NOT NULL,
    [MeasuringTime] time(0) NOT NULL,
    CONSTRAINT [PK_Measurements] PRIMARY KEY ([MeasurementId]),
    CONSTRAINT [FK_Measurements_DiaryEntries_DiaryEntryId] FOREIGN KEY ([DiaryEntryId]) REFERENCES [DiaryEntries] ([DiaryEntryId]) ON DELETE CASCADE
);

CREATE TABLE [IntervalSymptoms] (
    [DayIntervalId] int NOT NULL,
    [SymptomId] int NOT NULL,
    [SeverityLevelId] int NOT NULL,
    [SymptomNote] nvarchar(500) NULL,
    CONSTRAINT [PK_IntervalSymptoms] PRIMARY KEY ([DayIntervalId], [SymptomId]),
    CONSTRAINT [FK_IntervalSymptoms_DayIntervals_DayIntervalId] FOREIGN KEY ([DayIntervalId]) REFERENCES [DayIntervals] ([DayIntervalId]) ON DELETE CASCADE,
    CONSTRAINT [FK_IntervalSymptoms_SeverityLevels_SeverityLevelId] FOREIGN KEY ([SeverityLevelId]) REFERENCES [SeverityLevels] ([SeverityLevelId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IntervalSymptoms_Symptoms_SymptomId] FOREIGN KEY ([SymptomId]) REFERENCES [Symptoms] ([SymptomId]) ON DELETE NO ACTION
);

CREATE TABLE [Metrics] (
    [MetricId] int NOT NULL IDENTITY,
    [MeasurementId] int NOT NULL,
    [MetricUnitId] int NOT NULL,
    [MetricName] nvarchar(100) NOT NULL,
    [MetricValue] decimal(6,2) NOT NULL,
    CONSTRAINT [PK_Metrics] PRIMARY KEY ([MetricId]),
    CONSTRAINT [FK_Metrics_Measurements_MeasurementId] FOREIGN KEY ([MeasurementId]) REFERENCES [Measurements] ([MeasurementId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Metrics_MetricUnits_MetricUnitId] FOREIGN KEY ([MetricUnitId]) REFERENCES [MetricUnits] ([MetricUnitId]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ConditionStateId', N'ConditionStateName') AND [object_id] = OBJECT_ID(N'[ConditionStates]'))
    SET IDENTITY_INSERT [ConditionStates] ON;
INSERT INTO [ConditionStates] ([ConditionStateId], [ConditionStateName])
VALUES (1, N'Critical'),
(2, N'Bad'),
(3, N'Fair'),
(4, N'Good'),
(5, N'Excellent');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ConditionStateId', N'ConditionStateName') AND [object_id] = OBJECT_ID(N'[ConditionStates]'))
    SET IDENTITY_INSERT [ConditionStates] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EventTypeId', N'EventTypeName') AND [object_id] = OBJECT_ID(N'[EventTypes]'))
    SET IDENTITY_INSERT [EventTypes] ON;
INSERT INTO [EventTypes] ([EventTypeId], [EventTypeName])
VALUES (1, N'Medication Intake'),
(2, N'Physical Exercise'),
(3, N'Meal'),
(4, N'Sleep'),
(5, N'Emotional Stress'),
(6, N'Rest');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EventTypeId', N'EventTypeName') AND [object_id] = OBJECT_ID(N'[EventTypes]'))
    SET IDENTITY_INSERT [EventTypes] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MetricUnitId', N'UnitMaxValue', N'UnitMinValue', N'UnitName') AND [object_id] = OBJECT_ID(N'[MetricUnits]'))
    SET IDENTITY_INSERT [MetricUnits] ON;
INSERT INTO [MetricUnits] ([MetricUnitId], [UnitMaxValue], [UnitMinValue], [UnitName])
VALUES (1, 43.0, 34.0, N'Degrees Celsius (°C)'),
(2, 220.0, 30.0, N'Beats Per Minute (bpm)'),
(3, 260.0, 40.0, N'Millimeters of Mercury (mmHg)'),
(4, 100.0, 50.0, N'Blood Oxygen Level (SpO2 %)'),
(5, 35.0, 1.0, N'Blood Glucose (mmol/L)');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MetricUnitId', N'UnitMaxValue', N'UnitMinValue', N'UnitName') AND [object_id] = OBJECT_ID(N'[MetricUnits]'))
    SET IDENTITY_INSERT [MetricUnits] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SeverityLevelId', N'SeverityName') AND [object_id] = OBJECT_ID(N'[SeverityLevels]'))
    SET IDENTITY_INSERT [SeverityLevels] ON;
INSERT INTO [SeverityLevels] ([SeverityLevelId], [SeverityName])
VALUES (1, N'Mild'),
(2, N'Moderate'),
(3, N'Severe'),
(4, N'Very Severe');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SeverityLevelId', N'SeverityName') AND [object_id] = OBJECT_ID(N'[SeverityLevels]'))
    SET IDENTITY_INSERT [SeverityLevels] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SymptomId', N'SymptomName') AND [object_id] = OBJECT_ID(N'[Symptoms]'))
    SET IDENTITY_INSERT [Symptoms] ON;
INSERT INTO [Symptoms] ([SymptomId], [SymptomName])
VALUES (1, N'Headache'),
(2, N'Fatigue'),
(3, N'Nausea'),
(4, N'Dizziness'),
(5, N'Dry Cough'),
(6, N'Fever'),
(7, N'Shortness of Breath'),
(8, N'Joint and Muscle Pain');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SymptomId', N'SymptomName') AND [object_id] = OBJECT_ID(N'[Symptoms]'))
    SET IDENTITY_INSERT [Symptoms] OFF;

CREATE UNIQUE INDEX [IX_ConditionStates_ConditionStateName] ON [ConditionStates] ([ConditionStateName]);

CREATE INDEX [IX_DayEvents_DiaryEntryId] ON [DayEvents] ([DiaryEntryId]);

CREATE INDEX [IX_DayEvents_EventTypeId] ON [DayEvents] ([EventTypeId]);

CREATE INDEX [IX_DayIntervals_ConditionStateId] ON [DayIntervals] ([ConditionStateId]);

CREATE INDEX [IX_DayIntervals_DiaryEntryId] ON [DayIntervals] ([DiaryEntryId]);

CREATE UNIQUE INDEX [IX_DiaryEntries_UserId_EntryDate] ON [DiaryEntries] ([UserId], [EntryDate]);

CREATE UNIQUE INDEX [IX_EventTypes_EventTypeName] ON [EventTypes] ([EventTypeName]);

CREATE INDEX [IX_IntervalSymptoms_SeverityLevelId] ON [IntervalSymptoms] ([SeverityLevelId]);

CREATE INDEX [IX_IntervalSymptoms_SymptomId] ON [IntervalSymptoms] ([SymptomId]);

CREATE INDEX [IX_Measurements_DiaryEntryId] ON [Measurements] ([DiaryEntryId]);

CREATE INDEX [IX_Metrics_MeasurementId] ON [Metrics] ([MeasurementId]);

CREATE INDEX [IX_Metrics_MetricUnitId] ON [Metrics] ([MetricUnitId]);

CREATE UNIQUE INDEX [IX_MetricUnits_UnitName] ON [MetricUnits] ([UnitName]);

CREATE INDEX [IX_Reports_UserId] ON [Reports] ([UserId]);

CREATE UNIQUE INDEX [IX_SeverityLevels_SeverityName] ON [SeverityLevels] ([SeverityName]);

CREATE UNIQUE INDEX [IX_Symptoms_SymptomName] ON [Symptoms] ([SymptomName]);

CREATE UNIQUE INDEX [IX_Users_UserEmail] ON [Users] ([UserEmail]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261107181330_InitialCreate', N'10.0.14');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE INDEX [IX_DiaryEntries_EntryDate] ON [DiaryEntries] ([EntryDate]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261107230657_AddDiaryEntryDateIndex', N'10.0.14');

COMMIT;
GO

