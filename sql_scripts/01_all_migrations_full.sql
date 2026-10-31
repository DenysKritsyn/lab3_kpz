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
CREATE TABLE [EventTypes] (
    [EventTypeId] int NOT NULL IDENTITY,
    [EventName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_EventTypes] PRIMARY KEY ([EventTypeId])
);

CREATE TABLE [MetricTypes] (
    [MetricTypeId] int NOT NULL IDENTITY,
    [MetricName] nvarchar(100) NOT NULL,
    [MetricUnit] nvarchar(20) NOT NULL,
    [MetricMinValue] decimal(6,2) NOT NULL,
    [MetricMaxValue] decimal(6,2) NOT NULL,
    CONSTRAINT [PK_MetricTypes] PRIMARY KEY ([MetricTypeId]),
    CONSTRAINT [CK_MetricTypes_Range] CHECK ([MetricMaxValue] > [MetricMinValue])
);

CREATE TABLE [SeverityLevels] (
    [SeverityLevelId] int NOT NULL IDENTITY,
    [SeverityName] nvarchar(50) NOT NULL,
    [SeverityRank] tinyint NOT NULL,
    CONSTRAINT [PK_SeverityLevels] PRIMARY KEY ([SeverityLevelId])
);

CREATE TABLE [Symptoms] (
    [SymptomId] int NOT NULL IDENTITY,
    [SymptomName] nvarchar(100) NOT NULL,
    [SymptomCategory] nvarchar(50) NULL,
    CONSTRAINT [PK_Symptoms] PRIMARY KEY ([SymptomId])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [ExternalAuthId] nvarchar(200) NOT NULL,
    [UserEmail] nvarchar(256) NOT NULL,
    [UserName] nvarchar(100) NOT NULL,
    [UserBirthDate] date NULL,
    [UserRole] nvarchar(20) NOT NULL,
    [UserCreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);

CREATE TABLE [WellbeingLevels] (
    [WellbeingLevelId] int NOT NULL IDENTITY,
    [WellbeingName] nvarchar(50) NOT NULL,
    [WellbeingRank] tinyint NOT NULL,
    CONSTRAINT [PK_WellbeingLevels] PRIMARY KEY ([WellbeingLevelId])
);

CREATE TABLE [DiaryEntries] (
    [DiaryEntryId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EntryDate] date NOT NULL,
    [EntryNote] nvarchar(2000) NULL,
    [EntryCreatedAt] datetime2 NOT NULL,
    [EntryUpdatedAt] datetime2 NOT NULL,
    [EntryRowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DiaryEntries] PRIMARY KEY ([DiaryEntryId]),
    CONSTRAINT [FK_DiaryEntries_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Reports] (
    [ReportId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ReportPeriodFrom] date NOT NULL,
    [ReportPeriodTo] date NOT NULL,
    [ReportGeneratedAt] datetime2 NOT NULL,
    [ReportContent] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportId]),
    CONSTRAINT [CK_Reports_Period] CHECK ([ReportPeriodTo] >= [ReportPeriodFrom]),
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [DayEvents] (
    [DayEventId] int NOT NULL IDENTITY,
    [DiaryEntryId] int NOT NULL,
    [EventTypeId] int NOT NULL,
    [EventHour] tinyint NULL,
    [EventNote] nvarchar(500) NULL,
    CONSTRAINT [PK_DayEvents] PRIMARY KEY ([DayEventId]),
    CONSTRAINT [CK_DayEvents_Hour] CHECK ([EventHour] IS NULL OR [EventHour] <= 23),
    CONSTRAINT [FK_DayEvents_DiaryEntries_DiaryEntryId] FOREIGN KEY ([DiaryEntryId]) REFERENCES [DiaryEntries] ([DiaryEntryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_DayEvents_EventTypes_EventTypeId] FOREIGN KEY ([EventTypeId]) REFERENCES [EventTypes] ([EventTypeId]) ON DELETE NO ACTION
);

CREATE TABLE [DayIntervals] (
    [DayIntervalId] int NOT NULL IDENTITY,
    [DiaryEntryId] int NOT NULL,
    [IntervalStartHour] tinyint NOT NULL,
    [IntervalEndHour] tinyint NOT NULL,
    [WellbeingLevelId] int NOT NULL,
    [IntervalNote] nvarchar(1000) NULL,
    CONSTRAINT [PK_DayIntervals] PRIMARY KEY ([DayIntervalId]),
    CONSTRAINT [CK_DayIntervals_Hours] CHECK ([IntervalStartHour] <= 23 AND [IntervalEndHour] >= 1 AND [IntervalEndHour] <= 24 AND [IntervalEndHour] > [IntervalStartHour]),
    CONSTRAINT [FK_DayIntervals_DiaryEntries_DiaryEntryId] FOREIGN KEY ([DiaryEntryId]) REFERENCES [DiaryEntries] ([DiaryEntryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_DayIntervals_WellbeingLevels_WellbeingLevelId] FOREIGN KEY ([WellbeingLevelId]) REFERENCES [WellbeingLevels] ([WellbeingLevelId]) ON DELETE NO ACTION
);

CREATE TABLE [Measurements] (
    [MeasurementId] int NOT NULL IDENTITY,
    [DiaryEntryId] int NOT NULL,
    [MetricTypeId] int NOT NULL,
    [MeasurementValue] decimal(6,2) NOT NULL,
    [MeasurementHour] tinyint NULL,
    [MeasurementNote] nvarchar(500) NULL,
    CONSTRAINT [PK_Measurements] PRIMARY KEY ([MeasurementId]),
    CONSTRAINT [CK_Measurements_Hour] CHECK ([MeasurementHour] IS NULL OR [MeasurementHour] <= 23),
    CONSTRAINT [FK_Measurements_DiaryEntries_DiaryEntryId] FOREIGN KEY ([DiaryEntryId]) REFERENCES [DiaryEntries] ([DiaryEntryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Measurements_MetricTypes_MetricTypeId] FOREIGN KEY ([MetricTypeId]) REFERENCES [MetricTypes] ([MetricTypeId]) ON DELETE NO ACTION
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

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EventTypeId', N'EventName') AND [object_id] = OBJECT_ID(N'[EventTypes]'))
    SET IDENTITY_INSERT [EventTypes] ON;
INSERT INTO [EventTypes] ([EventTypeId], [EventName])
VALUES (1, N'Stress'),
(2, N'Sleep Deprivation'),
(3, N'Medication Intake'),
(4, N'Physical Exercise'),
(5, N'Caffeine or Energy Drink'),
(6, N'Weather Change');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EventTypeId', N'EventName') AND [object_id] = OBJECT_ID(N'[EventTypes]'))
    SET IDENTITY_INSERT [EventTypes] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MetricTypeId', N'MetricMaxValue', N'MetricMinValue', N'MetricName', N'MetricUnit') AND [object_id] = OBJECT_ID(N'[MetricTypes]'))
    SET IDENTITY_INSERT [MetricTypes] ON;
INSERT INTO [MetricTypes] ([MetricTypeId], [MetricMaxValue], [MetricMinValue], [MetricName], [MetricUnit])
VALUES (1, 43.0, 34.0, N'Body Temperature', N'°C'),
(2, 250.0, 60.0, N'Systolic Blood Pressure', N'mmHg'),
(3, 160.0, 40.0, N'Diastolic Blood Pressure', N'mmHg'),
(4, 220.0, 35.0, N'Heart Rate', N'bpm'),
(5, 100.0, 70.0, N'Oxygen Saturation', N'%');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MetricTypeId', N'MetricMaxValue', N'MetricMinValue', N'MetricName', N'MetricUnit') AND [object_id] = OBJECT_ID(N'[MetricTypes]'))
    SET IDENTITY_INSERT [MetricTypes] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SeverityLevelId', N'SeverityName', N'SeverityRank') AND [object_id] = OBJECT_ID(N'[SeverityLevels]'))
    SET IDENTITY_INSERT [SeverityLevels] ON;
INSERT INTO [SeverityLevels] ([SeverityLevelId], [SeverityName], [SeverityRank])
VALUES (1, N'Moderate', CAST(1 AS tinyint)),
(2, N'Severe', CAST(2 AS tinyint)),
(3, N'Very Severe', CAST(3 AS tinyint));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SeverityLevelId', N'SeverityName', N'SeverityRank') AND [object_id] = OBJECT_ID(N'[SeverityLevels]'))
    SET IDENTITY_INSERT [SeverityLevels] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SymptomId', N'SymptomCategory', N'SymptomName') AND [object_id] = OBJECT_ID(N'[Symptoms]'))
    SET IDENTITY_INSERT [Symptoms] ON;
INSERT INTO [Symptoms] ([SymptomId], [SymptomCategory], [SymptomName])
VALUES (1, N'Neurological', N'Headache'),
(2, N'Neurological', N'Dizziness'),
(3, N'Respiratory', N'Cough'),
(4, N'Respiratory', N'Sore Throat'),
(5, N'Respiratory', N'Runny Nose'),
(6, N'Gastrointestinal', N'Nausea'),
(7, N'General', N'Fatigue'),
(8, N'General', N'Chills');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SymptomId', N'SymptomCategory', N'SymptomName') AND [object_id] = OBJECT_ID(N'[Symptoms]'))
    SET IDENTITY_INSERT [Symptoms] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'WellbeingLevelId', N'WellbeingName', N'WellbeingRank') AND [object_id] = OBJECT_ID(N'[WellbeingLevels]'))
    SET IDENTITY_INSERT [WellbeingLevels] ON;
INSERT INTO [WellbeingLevels] ([WellbeingLevelId], [WellbeingName], [WellbeingRank])
VALUES (1, N'Good', CAST(1 AS tinyint)),
(2, N'Moderate', CAST(2 AS tinyint)),
(3, N'Poor', CAST(3 AS tinyint)),
(4, N'Very Poor', CAST(4 AS tinyint));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'WellbeingLevelId', N'WellbeingName', N'WellbeingRank') AND [object_id] = OBJECT_ID(N'[WellbeingLevels]'))
    SET IDENTITY_INSERT [WellbeingLevels] OFF;

CREATE INDEX [IX_DayEvents_DiaryEntryId_EventTypeId] ON [DayEvents] ([DiaryEntryId], [EventTypeId]);

CREATE INDEX [IX_DayEvents_EventTypeId] ON [DayEvents] ([EventTypeId]);

CREATE INDEX [IX_DayIntervals_DiaryEntryId_IntervalStartHour] ON [DayIntervals] ([DiaryEntryId], [IntervalStartHour]);

CREATE INDEX [IX_DayIntervals_WellbeingLevelId] ON [DayIntervals] ([WellbeingLevelId]);

CREATE INDEX [IX_DiaryEntries_EntryDate] ON [DiaryEntries] ([EntryDate]);

CREATE UNIQUE INDEX [IX_DiaryEntries_UserId_EntryDate] ON [DiaryEntries] ([UserId], [EntryDate]);

CREATE UNIQUE INDEX [IX_EventTypes_EventName] ON [EventTypes] ([EventName]);

CREATE INDEX [IX_IntervalSymptoms_SeverityLevelId] ON [IntervalSymptoms] ([SeverityLevelId]);

CREATE INDEX [IX_IntervalSymptoms_SymptomId] ON [IntervalSymptoms] ([SymptomId]);

CREATE INDEX [IX_Measurements_DiaryEntryId_MetricTypeId] ON [Measurements] ([DiaryEntryId], [MetricTypeId]);

CREATE INDEX [IX_Measurements_MetricTypeId] ON [Measurements] ([MetricTypeId]);

CREATE UNIQUE INDEX [IX_MetricTypes_MetricName] ON [MetricTypes] ([MetricName]);

CREATE INDEX [IX_Reports_UserId_ReportGeneratedAt] ON [Reports] ([UserId], [ReportGeneratedAt]);

CREATE UNIQUE INDEX [IX_SeverityLevels_SeverityName] ON [SeverityLevels] ([SeverityName]);

CREATE UNIQUE INDEX [IX_SeverityLevels_SeverityRank] ON [SeverityLevels] ([SeverityRank]);

CREATE INDEX [IX_Symptoms_SymptomCategory] ON [Symptoms] ([SymptomCategory]);

CREATE UNIQUE INDEX [IX_Symptoms_SymptomName] ON [Symptoms] ([SymptomName]);

CREATE UNIQUE INDEX [IX_Users_ExternalAuthId] ON [Users] ([ExternalAuthId]);

CREATE UNIQUE INDEX [IX_Users_UserEmail] ON [Users] ([UserEmail]);

CREATE UNIQUE INDEX [IX_WellbeingLevels_WellbeingName] ON [WellbeingLevels] ([WellbeingName]);

CREATE UNIQUE INDEX [IX_WellbeingLevels_WellbeingRank] ON [WellbeingLevels] ([WellbeingRank]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261031154954_InitialCreate', N'10.0.14');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [UserPhoneNumber] nvarchar(25) NULL;

CREATE INDEX [IX_DiaryEntries_UserId_EntryUpdatedAt] ON [DiaryEntries] ([UserId], [EntryUpdatedAt]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261031183412_AddUserPhoneNumberAndDiaryEntriesIndex', N'10.0.14');

COMMIT;
GO

