# Завдання 4, 5 та 6. Прямий доступ до даних через ADO.NET (`Microsoft.Data.SqlClient`), складні SQL-запити та транзакції

Усі операції ADO.NET реалізовано в класі [`AdoNetDiaryService`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Services/AdoNetDiaryService.cs) із використанням пакета `Microsoft.Data.SqlClient`. Кожен метод працює асинхронно (`OpenAsync`, `ExecuteScalarAsync`, `ExecuteReaderAsync`, `ExecuteNonQueryAsync`), приймає `CancellationToken`, використовує `await using` для гарантованого повернення з'єднань у пул і передає всі вхідні значення виключно через типізовані `SqlParameter` (`SqlDbType.Int`, `SqlDbType.Date`, `SqlDbType.NVarChar`, `SqlDbType.Decimal` із явною точністю `Precision = 6, Scale = 2`, `SqlDbType.TinyInt`, `DBNull.Value` для nullable-полів).

---

## 1. CRUD через ADO.NET для двох пов'язаних сутностей (Пункт 4)

Обрано дві безпосередньо пов'язані сутності (`1 : N`): **`DiaryEntries`** (запис дня) та **`Measurements`** (числові вимірювання показників за цей день):
1. **Create:**
   - `CreateDiaryEntryAsync` — виконує `INSERT INTO DiaryEntries ... OUTPUT INSERTED.DiaryEntryId` через `ExecuteScalarAsync(ct)` і повертає згенерований первинний ключ;
   - `CreateMeasurementAsync` — виконує `INSERT INTO Measurements ... OUTPUT INSERTED.MeasurementId` із прив'язкою до `DiaryEntryId` та `MetricTypeId`.
2. **Read (`SqlDataReader`):**
   - `GetDiaryEntryByIdAsync` та `GetMeasurementsByEntryAsync` — використовують `ExecuteReaderAsync(ct)`, попередньо отримують індекси стовпців через `reader.GetOrdinal(...)`, перевіряють `reader.IsDBNull(...)` для необов'язкових полів (`EntryNote`, `MeasurementHour`, `MeasurementNote`) і матеріалізують рядки в незмінні DTO (`DiaryEntryDto`, `MeasurementDto`).
3. **Update:**
   - `UpdateDiaryEntryNoteAsync` та `UpdateMeasurementAsync` — виконують параметризований `UPDATE ... WHERE ...` через `ExecuteNonQueryAsync(ct)` і перевіряють, чи кількість змінених рядків більша за `0`.
4. **Delete:**
   - `DeleteMeasurementAsync` та `DeleteDiaryEntryAsync` — видаляють записи за первинним ключем через `ExecuteNonQueryAsync(ct)`.

---

## 2. Складні SQL-запити через ADO.NET (Пункт 5)

1. **Запит із `JOIN` 6 таблиць, фільтрацією та сортуванням (`SearchSymptomOccurrencesAsync`):**
   - Поєднує таблиці `IntervalSymptoms`, `DayIntervals`, `DiaryEntries`, `Users`, `Symptoms` та `SeverityLevels` через `INNER JOIN`.
   - Фільтрує записи за мінімальним рангом тяжкості (`sl.SeverityRank >= @minSeverityRank`), опціональним користувачем (`@userId IS NULL OR d.UserId = @userId`) та початковою датою (`@fromDate IS NULL OR d.EntryDate >= @fromDate`).
   - Сортує результат за спаданням дати запису, зростанням години початку інтервалу та спаданням тяжкості симптому (`ORDER BY d.EntryDate DESC, di.IntervalStartHour ASC, sl.SeverityRank DESC`).

2. **Агрегований запит із `GROUP BY`, `HAVING` та 4 агрегатними функціями (`GetSymptomAggregatedStatsAsync`):**
   - Від таблиці `Symptoms` виконує `LEFT JOIN` до `IntervalSymptoms`, `DayIntervals` та `SeverityLevels`.
   - Групує за `s.SymptomId, s.SymptomName, s.SymptomCategory`.
   - Обчислює **4 агрегатні функції**:
     - `COUNT(isym.DayIntervalId)` — кількість зафіксованих випадків симптому;
     - `SUM(CAST(di.IntervalEndHour - di.IntervalStartHour AS int))` — сумарна тривалість симптому в годинах;
     - `AVG(CAST(sl.SeverityRank AS float))` — середній ранг тяжкості;
     - `MAX(sl.SeverityRank)` — піковий ранг тяжкості.
   - Фільтрує групи через `HAVING COUNT(isym.DayIntervalId) >= @minOccurrences`.

---

## 3. ADO.NET-транзакція, що змінює дані у 4 таблицях: `Commit` і `Rollback` (Пункт 6)

Метод `RecordFullDayEntryInTransactionAsync` відкриває явну транзакцію `(SqlTransaction)await connection.BeginTransactionAsync(ct)` і послідовно виконує зміни у **4 таблицях**:
1. Вставляє запис дня у **`DiaryEntries`** та отримує `DiaryEntryId`.
2. Перевіряє бізнес-правило неперетинності часових інтервалів (`current.IntervalStartHour < previous.IntervalEndHour`) і вставляє інтервали у **`DayIntervals`**.
3. Вставляє прояви симптомів для кожного інтервалу в проміжну таблицю **`IntervalSymptoms`**.
4. Для кожного фізичного вимірювання зчитує допустимі межі `[MetricMinValue; MetricMaxValue]` з `MetricTypes`, перевіряє бізнес-правило входження значення у фізіологічний діапазон і вставляє рядок у **`Measurements`**.

- **Сценарій `Commit`:** усі інтервали не перетинаються, а температура (`36.70 °C`) та пульс (`74.00 уд/хв`) входять у допустимі межі — викликається `await transaction.CommitAsync(ct)`, усі 4 таблиці успішно оновлюються.
- **Сценарій `Rollback`:** передається вимірювання температури `49.50 °C` (поза межами `[34.00; 43.00]`). Після того як у межах транзакції вже було вставлено рядки в `DiaryEntries`, `DayIntervals` та `IntervalSymptoms`, перевірка вимірювання генерує `InvalidOperationException`. У блоці `catch` виконується `await transaction.RollbackAsync(CancellationToken.None)`, і жоден із попередніх `INSERT` не залишається в базі даних (повна атомарність за ACID).

