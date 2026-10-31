# Завдання 11, 12 та 13. Еволюція схеми (друга міграція та SQL-скрипти), Reverse Engineering (`dbcontext scaffold`) і діагностика згенерованого SQL

## 1. Зміна моделі та друга міграція `AddUserPhoneNumberAndDiaryEntriesIndex` (Пункт 11)

Після створення та перевірки першого працездатного варіанта (міграція `InitialCreate`) модель предметної області було розширено двома змінами:
1. **Нова властивість у сутності `User`:** додано необов'язкове поле `UserPhoneNumber` (`string?`, у конфігурації `UserConfiguration` задано `.HasMaxLength(25)`), що відображається на стовпець `nvarchar(25) NULL` у таблиці `Users`.
2. **Новий складений індекс для швидкого пошуку в таблиці `DiaryEntries`:** у `DiaryEntryConfiguration` додано індекс `IX_DiaryEntries_UserId_EntryUpdatedAt` на поля `(UserId, EntryUpdatedAt)` для прискорення пошуку останніх змінених записів користувача та миттєвої перевірки актуальності звітів (`d.UserId == r.UserId && d.EntryUpdatedAt > r.ReportGeneratedAt`).

### Код другої міграції ([`20261031183412_AddUserPhoneNumberAndDiaryEntriesIndex.cs`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Migrations/20261031183412_AddUserPhoneNumberAndDiaryEntriesIndex.cs))

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "UserPhoneNumber",
        table: "Users",
        type: "nvarchar(25)",
        maxLength: 25,
        nullable: true);

    migrationBuilder.CreateIndex(
        name: "IX_DiaryEntries_UserId_EntryUpdatedAt",
        table: "DiaryEntries",
        columns: new[] { "UserId", "EntryUpdatedAt" });
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_DiaryEntries_UserId_EntryUpdatedAt",
        table: "DiaryEntries");

    migrationBuilder.DropColumn(
        name: "UserPhoneNumber",
        table: "Users");
}
```

### Згенеровані SQL-скрипти міграцій (збережено в папці [`sql_scripts/`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/sql_scripts))

1. [`01_all_migrations_full.sql`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/sql_scripts/01_all_migrations_full.sql) — повний DDL + seed-скрипт обох міграцій від порожньої бази (`dotnet ef migrations script`).
2. [`02_second_migration_delta.sql`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/sql_scripts/02_second_migration_delta.sql) — дельта-скрипт переходу від `InitialCreate` до `AddUserPhoneNumberAndDiaryEntriesIndex`:
   ```sql
   BEGIN TRANSACTION;
   ALTER TABLE [Users] ADD [UserPhoneNumber] nvarchar(25) NULL;

   CREATE INDEX [IX_DiaryEntries_UserId_EntryUpdatedAt] ON [DiaryEntries] ([UserId], [EntryUpdatedAt]);

   INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
   VALUES (N'20261031183412_AddUserPhoneNumberAndDiaryEntriesIndex', N'10.0.14');

   COMMIT;
   GO
   ```
3. [`03_idempotent_migrations.sql`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/sql_scripts/03_idempotent_migrations.sql) — ідемпотентний скрипт (`--idempotent`) із перевірками `IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] ...)` для безпечного розгортання на production-сервері.

---

## 2. Reverse Engineering (Scaffolding) існуючої БД (Пункт 12)

Виконано команду Reverse Engineering з параметром `--no-onconfiguring` в окрему папку [`Kpz.Lab2/Data/Scaffolded/`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Data/Scaffolded):

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:SymptomDiaryDb" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --context ScaffoldedSymptomDiaryDbContext \
  --output-dir Data/Scaffolded \
  --namespace Kpz.Lab2.Data.Scaffolded \
  --no-onconfiguring
```

**Результат Scaffolding:**
- Згенеровано контекст [`ScaffoldedSymptomDiaryDbContext.cs`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Data/Scaffolded/ScaffoldedSymptomDiaryDbContext.cs) (без вбудованого рядка підключення завдяки `--no-onconfiguring`) та 12 partial-класів сутностей (`User.cs`, `DiaryEntry.cs`, `DayInterval.cs`, `IntervalSymptom.cs`, `DayEvent.cs`, `Measurement.cs`, `Report.cs`, `Symptom.cs`, `EventType.cs`, `MetricType.cs`, `WellbeingLevel.cs`, `SeverityLevel.cs`).
- EF Core автоматично зчитав із системних каталогів SQL Server усі первинні ключі (зокрема складений ключ `entity.HasKey(e => new { e.DayIntervalId, e.SymptomId })` для `IntervalSymptom`), зовнішні ключі, унікальні та пошукові індекси (включно з новим `IX_DiaryEntries_UserId_EntryUpdatedAt` та стовпцем `UserPhoneNumber`), максимальні довжини рядків, точність `decimal(6, 2)` і токен оптимістичної конкуренції `.IsRowVersion().IsConcurrencyToken()`.

---

## 3. Аналіз згенерованого SQL для трьох LINQ-запитів та перевірка відсутності N+1 (Пункт 13)

За допомогою методу `.ToQueryString()` та логування `DbLoggerCategory.Database.Command` отримано й проаналізовано SQL-запити, які формує EF Core 10:

### Запит 1: Фільтрація записів щоденника за користувачем, періодом дат і наявністю симптому
```sql
SELECT [d].[DiaryEntryId], [d].[EntryCreatedAt], [d].[EntryDate], [d].[EntryNote], [d].[EntryRowVersion], [d].[EntryUpdatedAt], [d].[UserId]
FROM [DiaryEntries] AS [d]
WHERE [d].[UserId] = @userId_Value
  AND [d].[EntryDate] >= @fromDate_Value
  AND [d].[EntryDate] <= @toDate_Value
  AND EXISTS (
      SELECT 1
      FROM [DayIntervals] AS [d0]
      WHERE [d].[DiaryEntryId] = [d0].[DiaryEntryId] AND EXISTS (
          SELECT 1
          FROM [IntervalSymptoms] AS [i]
          WHERE [d0].[DayIntervalId] = [i].[DayIntervalId] AND [i].[SymptomId] = @symptomId_Value))
ORDER BY [d].[EntryDate] DESC, [d].[DiaryEntryId]
```
- **Пояснення:** LINQ-предикат `d.DayIntervals.Any(i => i.IntervalSymptoms.Any(s => s.SymptomId == symptomId))` перетворено ORM у два вкладені підзапити `EXISTS (...)`. Завдяки цьому рядки таблиці `DiaryEntries` не розмножуються (як це сталося б при звичайному `INNER JOIN` без `DISTINCT`), а SQL Server використовує індекси `IX_DiaryEntries_UserId_EntryDate`, `IX_DayIntervals_DiaryEntryId_IntervalStartHour` та первинний ключ `PK_IntervalSymptoms`.

### Запит 2: Хронологія проявів симптомів (DTO-проєкція `Select` + сортування)
```sql
SELECT [d].[DiaryEntryId], [d0].[EntryDate], [u].[UserName], [d].[IntervalStartHour], [d].[IntervalEndHour],
       [s0].[SymptomName], [s0].[SymptomCategory], [s].[SeverityName], [s].[SeverityRank], [i].[SymptomNote]
FROM [IntervalSymptoms] AS [i]
INNER JOIN [DayIntervals] AS [d] ON [i].[DayIntervalId] = [d].[DayIntervalId]
INNER JOIN [DiaryEntries] AS [d0] ON [d].[DiaryEntryId] = [d0].[DiaryEntryId]
INNER JOIN [SeverityLevels] AS [s] ON [i].[SeverityLevelId] = [s].[SeverityLevelId]
INNER JOIN [Users] AS [u] ON [d0].[UserId] = [u].[UserId]
INNER JOIN [Symptoms] AS [s0] ON [i].[SymptomId] = [s0].[SymptomId]
WHERE [d0].[UserId] = @userId AND [s].[SeverityRank] >= @minSeverityRank
ORDER BY [d0].[EntryDate] DESC, [d].[IntervalStartHour], [s].[SeverityRank] DESC
```
- **Пояснення:** звернення до навігаційних властивостей у `Where`, `OrderBy` та `Select` (`s.DayInterval.DiaryEntry.User.UserName`, `s.Symptom.SymptomName`, `s.SeverityLevel.SeverityName`) EF Core транслював у 5 операторів `INNER JOIN` (оскільки всі ці зовнішні ключі є обов'язковими `NOT NULL`). Завдяки DTO-проєкції `Select` у блоці `SELECT` вибираються лише 10 потрібних стовпців, без великих полів `EntryNote`, `IntervalNote` чи `EntryRowVersion`.

### Запит 3: Агрегована статистика фізичних вимірювань (`GroupBy`)
```sql
SELECT [m].[MetricTypeId], [m0].[MetricName], [m0].[MetricUnit],
       COUNT(*), MIN([m].[MeasurementValue]), MAX([m].[MeasurementValue]), ROUND(AVG([m].[MeasurementValue]), 2)
FROM [Measurements] AS [m]
INNER JOIN [DiaryEntries] AS [d] ON [m].[DiaryEntryId] = [d].[DiaryEntryId]
INNER JOIN [MetricTypes] AS [m0] ON [m].[MetricTypeId] = [m0].[MetricTypeId]
WHERE [d].[UserId] = @userId_Value
GROUP BY [m].[MetricTypeId], [m0].[MetricName], [m0].[MetricUnit]
HAVING COUNT(*) >= 1
ORDER BY COUNT(*) DESC
```
- **Пояснення:** фільтр до групування став секцією `WHERE [d].[UserId] = @userId_Value`, ключ `GroupBy` став `GROUP BY [m].[MetricTypeId], [m0].[MetricName], [m0].[MetricUnit]`, умова після групування `Where(g => g.Count() >= 1)` перетворилася на `HAVING COUNT(*) >= 1`, а агрегації `Count()`, `Min()`, `Max()`, `Math.Round(Average(), 2)` виконані безпосередньо на стороні SQL Server одним запитом.

### Перевірка відсутності проблеми N+1
У демонстраційних сценаріях проблема N+1 повністю відсутня:
- При читанні списків для відображення (`BuildSymptomTimelineProjectionQuery`, `GetPagedDiaryEntriesAsync`, `GetReportsFreshnessStatusAsync`) використовуються DTO-проєкції `Select`, які виконуються за **один SQL-запит** на вибірку.
- При читанні повного графа одного дня (`GetDiaryEntryWithGraphAsync`) всі пов'язані колекції завантажуються наперед через `Include` / `ThenInclude` без лінивого довантаження у циклах C#.

