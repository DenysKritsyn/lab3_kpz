BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [UserPhoneNumber] nvarchar(25) NULL;

CREATE INDEX [IX_DiaryEntries_UserId_EntryUpdatedAt] ON [DiaryEntries] ([UserId], [EntryUpdatedAt]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261031183412_AddUserPhoneNumberAndDiaryEntriesIndex', N'10.0.14');

COMMIT;
GO

