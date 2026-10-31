# Завдання 14 та 15. Порядок демонстрації роботи програми та висновки (порівняння прямого SQL в ADO.NET та ORM EF Core)

## 1. Сценарій демонстрації роботи програми (Пункт 14)

Консольний застосунок [`Kpz.Lab2`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Program.cs) через клас [`TaskManager`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Services/TaskManager.cs) послідовно та ідемпотентно демонструє всі обов'язкові пункти лабораторної роботи:

1. **Початковий стан даних (`RunMigrationAndVerifySeedDataAsync`):** автоматичне застосування міграцій (`MigrateAsync`) та виведення кількості рядків у довідниках (`HasData` — 26 записів) і транзакційних таблицях (`UseAsyncSeeding` — 59 записів).
2. **ADO.NET CRUD (`RunAdoNetCrudDemoAsync`):** асинхронні операції Create (`OUTPUT INSERTED`), Read (`SqlDataReader`), Update та Delete для пов'язаних таблиць `DiaryEntries` і `Measurements`.
3. **Складні SQL-запити ADO.NET (`RunAdoNetComplexQueriesDemoAsync`):**
   - `INNER JOIN` 6 таблиць із параметризованою фільтрацією та сортуванням;
   - `LEFT JOIN` + `GROUP BY` + `HAVING` із 4 агрегатними функціями (`COUNT`, `SUM`, `AVG`, `MAX`).
4. **Транзакції ADO.NET — Commit та Rollback (`RunAdoNetTransactionsDemoAsync`):**
   - успішний `Commit` вставки запису дня, інтервалів, симптомів та вимірювань у 4 таблиці;
   - негативний сценарій `Rollback` при спробі записати фізіологічно неможливу температуру `49.50 °C` із перевіркою, що жоден рядок не залишився в БД.
5. **EF Core CRUD, перевірка `DeleteBehavior.Restrict` та складна транзакційна операція (`RunEfCoreCrudAndGraphTransactionDemoAsync`):**
   - CRUD для `User`, `DiaryEntry` та `DayEvent`;
   - негативний сценарій видалення симптому з довідника, що перехоплює `DbUpdateException` завдяки `DeleteBehavior.Restrict`;
   - збереження повного графа сутностей дня разом із генерацією JSON-знімка звіту в таблиці `Reports` у межах явної транзакції.
6. **LINQ-запити, пагінація та SQL діагностика (`RunLinqQueriesAndSqlDiagnosticsDemoAsync`):**
   - 6 LINQ-запитів (динамічна фільтрація з `AsNoTracking`, сортування з DTO-проєкцією, `Include`/`ThenInclude`, `GroupBy` з агрегатами, пагінація `Skip`/`Take` з `TotalCount`, перевірка актуальності звітів);
   - виведення згенерованого SQL для трьох запитів через `ToQueryString()`;
   - запуск із прапорцем `dotnet run -- --sql-log` додатково вмикає логування всіх виконаних `DbCommand`.
7. **Оптимістична конкуренція (`RunOptimisticConcurrencyDemoAsync`):** імітація одночасного редагування одного `DiaryEntry` двома окремими `SymptomDiaryDbContext`, перехоплення `DbUpdateConcurrencyException`, читання актуальних даних із БД через `GetDatabaseValuesAsync` та узгоджене повторне збереження.
8. **Перевірка другої міграції та Reverse Engineering (`RunSecondMigrationAndScaffoldVerificationDemoAsync`):** виведення списку застосованих міграцій (`InitialCreate` та `AddUserPhoneNumberAndDiaryEntriesIndex`), перевірка нового поля `UserPhoneNumber` і виконання запиту через згенерований `ScaffoldedSymptomDiaryDbContext`.

---

## 2. Висновки: порівняння прямого SQL (ADO.NET) та ORM (Entity Framework Core) (Пункт 15)

Спираючись на власний код, реалізований у класах [`AdoNetDiaryService`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Services/AdoNetDiaryService.cs) та [`EfCoreDiaryService`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Services/EfCoreDiaryService.cs), можна зробити такі практичні висновки щодо доцільності кожного підходу:

### 1. Обсяг коду та підтримка CRUD-операцій
- **EF Core є значно зручнішим для стандартного CRUD та збереження ієрархічних графів сутностей.** Наприклад, у методі `CreateFullEntryGraphAndGenerateReportAsync` достатньо сформувати об'єкт `DiaryEntry` із вкладеними колекціями `DayIntervals`, `IntervalSymptoms`, `Measurements` та `DayEvents` і викликати один `await db.SaveChangesAsync(ct)`. EF Core автоматично визначає порядок `INSERT`, підставляє згенеровані `IDENTITY`-ключі (`DiaryEntryId`, `DayIntervalId`) у залежні рядки та загортає пакет у транзакцію.
- **У прямому ADO.NET (`RecordFullDayEntryInTransactionAsync`)** для тієї самої операції довелося вручну писати 4 окремі SQL-тексти, явно отримувати кожен згенерований ключ через `OUTPUT INSERTED.DiaryEntryId` / `OUTPUT INSERTED.DayIntervalId`, створювати десятки `SqlParameter` із ручною перевіркою `DBNull.Value` та вкладати цикли команд. Це збільшує обсяг шаблонного коду в 3–4 рази і підвищує ризик помилки в іменах стовпців.

### 2. Динамічні фільтри, пагінація та DTO-проєкції
- **EF Core (`IQueryable<T>`) суттєво переважає при побудові динамічних фільтрів та пагінації.** У методах `BuildFilteredEntriesQuery` та `GetPagedDiaryEntriesAsync` умови `Where` компонуються типобезпечно у C#, а `Skip`/`Take` і проєкція `Select` автоматично перетворюються на оптимальний SQL без ризику SQL-ін'єкцій.
- **В ADO.NET** для необов'язкових фільтрів доводиться або будувати запит через `StringBuilder`, або використовувати конструкції `(@userId IS NULL OR d.UserId = @userId)`, а при читанні через `SqlDataReader` вручну контролювати порядкові номери стовпців (`GetOrdinal`, `IsDBNull`).

### 3. Складні аналітичні запити та агрегації
- **Прямий SQL (ADO.NET або `Database.SqlQuery<T>` в EF Core) є зручнішим і прозорішим для складної аналітики та нестандартних агрегацій.** Наприклад, у запиті `GetSymptomAggregatedStatsAsync` потрібно було виконати `LEFT JOIN` від довідника симптомів до проміжної таблиці та інтервалів і одночасно обчислити `COUNT(isym.DayIntervalId)`, сумарну тривалість інтервалів `COALESCE(SUM(CAST(di.IntervalEndHour - di.IntervalStartHour AS int)), 0)`, середній і максимальний ранг тяжкості з `GROUP BY` та `HAVING`. У чистому SQL цей запит записується прямолінійно і дозволяє точно контролювати план виконання та приведення типів (`CAST`, `COALESCE`).

### 4. Керування транзакціями, конкуренцією та еволюцією схеми
- **Оптимістична конкуренція (`rowversion`) та міграції схеми:** EF Core бере на себе перевірку `EntryRowVersion` у `WHERE` під час `UPDATE`, генерацію `DbUpdateConcurrencyException`, а також версіонування схеми через міграції (`InitialCreate`, `AddUserPhoneNumberAndDiaryEntriesIndex`) і генерацію ідемпотентних SQL-скриптів.
- **Точковий контроль транзакцій та продуктивності:** ADO.NET дає повний низькорівневий контроль над кожним виконаним батчем і мережевим викликом без накладних витрат `ChangeTracker`.

**Підсумок:** у реальній архітектурі додатку «Щоденник симптомів» оптимальним є комбінований підхід: використання **EF Core** як основного інструменту для керування схемою (міграції), роботи з графами сутностей, перевірки оптимістичної конкуренції, динамічної фільтрації та пагінації (із обов'язковим `AsNoTracking()` та DTO-проєкціями для читання), а **прямого параметризованого SQL** — для важких аналітичних звітів і високопродуктивних пакетних операцій.

