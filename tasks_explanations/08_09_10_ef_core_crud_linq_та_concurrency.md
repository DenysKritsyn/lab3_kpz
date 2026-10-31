# Завдання 8, 9 та 10. Операції EF Core CRUD, граф сутностей у транзакції, LINQ-запити та оптимістична конкуренція (`rowversion`)

Реалізацію зосереджено у класі [`EfCoreDiaryService`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Services/EfCoreDiaryService.cs) та демонстраційному сценарії у [`Program.cs`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Program.cs).

---

## 1. EF Core CRUD для трьох сутностей та складна транзакційна бізнес-операція з Entity Graph (Пункт 8)

Реалізовано повний набір асинхронних операцій Create, Read, Update, Delete для трьох сутностей:
1. **`User`** (`CreateUserAsync`, `GetUserByIdAsNoTrackingAsync`, `UpdateUserNameAsync`, `DeleteUserAsync`);
2. **`DiaryEntry`** (`CreateDiaryEntryAsync`, `GetDiaryEntryWithGraphAsync`, `UpdateDiaryEntryNoteAsync`, `DeleteDiaryEntryAsync`);
3. **`DayEvent`** (`CreateDayEventAsync`, `GetDayEventsByEntryAsNoTrackingAsync`, `UpdateDayEventNoteAsync`, `DeleteDayEventAsync`).

**Складна бізнес-операція з Entity Graph у транзакції (`CreateFullEntryGraphAndGenerateReportAsync`):**
- Перед відкриттям транзакції перевіряє бізнес-правило неперетинності часових інтервалів.
- Відкриває явну транзакцію через `await db.Database.BeginTransactionAsync(ct)`.
- Будує повний граф сутностей: `DiaryEntry` → колекція `DayIntervals` → колекція `IntervalSymptoms`, а також `Measurements` та `DayEvents`, і зберігає весь граф одним викликом `SaveChangesAsync(ct)`.
- У межах тієї самої транзакції виконує агрегувальну вибірку всіх записів користувача за період `[reportFrom; reportTo]`, серіалізує результат у JSON-знімок і створює новий запис у таблиці `Reports`.
- Фіксує транзакцію через `await transaction.CommitAsync(ct)` (або виконує `RollbackAsync(CancellationToken.None)` у разі збою).

---

## 2. Шість LINQ-запитів та `AsNoTracking` (Пункт 9)

У `EfCoreDiaryService` реалізовано 6 різнотипних LINQ-запитів:
1. **Динамічна фільтрація за кількома параметрами (`BuildFilteredEntriesQuery`):**
   - Використовує **`AsNoTracking()`** і повертає сутності `DiaryEntry`.
   - Поступово накладає умови `Where` за `userId`, `fromDate`, `toDate` та наявністю конкретного симптому через `d.DayIntervals.Any(i => i.IntervalSymptoms.Any(s => s.SymptomId == symptomId))`.
2. **Сортування за кількома критеріями + DTO-проєкція (`BuildSymptomTimelineProjectionQuery`):**
   - Використовує `OrderByDescending(EntryDate).ThenBy(IntervalStartHour).ThenByDescending(SeverityRank)` і проєктує дані з 5 таблиць у `SymptomOccurrenceDto` через `Select` (вибираються лише необхідні стовпці без завантаження повних entity).
3. **Запит із пов'язаними даними (`GetDiaryEntryWithGraphAsync`):**
   - Використовує **`AsNoTracking()`** (другий read-only запит, що повертає сутності, відповідно до вимоги п. 9) разом із ланцюжками `Include` та `ThenInclude` для завантаження `User`, `DayIntervals` (відсортованих за `IntervalStartHour`), `WellbeingLevel`, `IntervalSymptoms`, `Symptom`, `SeverityLevel`, `DayEvents` та `Measurements` (плюс третій read-only запит із сутностями — `GetUserByIdAsNoTrackingAsync` та четвертий — `GetDayEventsByEntryAsNoTrackingAsync`).
4. **Групування `GroupBy` з агрегатами (`BuildMetricStatsGroupByQuery`):**
   - Групує вимірювання за `(MetricTypeId, MetricName, MetricUnit)`, застосовує фільтр груп `Where(g => g.Count() >= 1)` (транслюється у `HAVING`) і обчислює `Count()`, `Min()`, `Max()` та `Average()` у `MetricStatsDto`.
5. **Пагінація (`GetPagedDiaryEntriesAsync`):**
   - Обмежує `pageSize` через `Math.Clamp`, виконує `CountAsync` для загальної кількості записів, стабільне сортування `OrderByDescending(EntryDate).ThenByDescending(DiaryEntryId)`, застосовує `Skip((pageNumber - 1) * pageSize).Take(pageSize)` і повертає `PagedResultDto<DiaryEntryDto>`.
6. **Перевірка актуальності звітів (`GetReportsFreshnessStatusAsync`):**
   - Через корельований підзапит `db.DiaryEntries.Any(...)` у `Select` визначає для кожного звіту прапорець `IsOutdated`, якщо будь-який запис дня з періоду звіту був оновлений пізніше за `ReportGeneratedAt`.

---

## 3. Оптимістична конкуренція за допомогою `rowversion` (Пункт 10)

У сутності `DiaryEntry` властивість `byte[] EntryRowVersion` сконфігурована через `.IsRowVersion()`.
Демонстраційний сценарій у `Program.cs` використовує **два окремі екземпляри `SymptomDiaryDbContext`** (`db1` та `db2` у різних DI-scope):
1. Сесія A (`db1`) і Сесія B (`db2`) одночасно завантажують один і той самий запис дня (`DiaryEntryId = 1`) з однаковим початковим значенням `EntryRowVersion`.
2. Сесія A змінює `EntryNote` та `EntryUpdatedAt` і викликає `await db1.SaveChangesAsync(ct)`. SQL Server успішно оновлює рядок і автоматично генерує нове значення `EntryRowVersion`.
3. Сесія B намагається зберегти власну зміну примітки через `await db2.SaveChangesAsync(ct)`. EF Core формує `UPDATE DiaryEntries SET ... WHERE DiaryEntryId = @p0 AND EntryRowVersion = @p1` зі старим токеном версії. Оскільки рядок зі старою версією вже не існує, оновлюється `0` рядків і викидається **`DbUpdateConcurrencyException`**.
4. Програма перехоплює `DbUpdateConcurrencyException`, отримує актуальні значення з бази через `await conflictedEntry.GetDatabaseValuesAsync(ct)`, виводить поточний стан користувачу, узгоджує нову примітку, оновлює `OriginalValues.SetValues(databaseValues)` і успішно виконує повторне збереження.

