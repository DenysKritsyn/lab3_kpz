# Завдання 2, 3 та 7. Реалізація моделі EF Core Code First, конфігурацій `IEntityTypeConfiguration<T>`, початкової міграції та наповнення даними (`HasData` / `UseSeeding`)

## 1. Контекст бази даних та `IEntityTypeConfiguration<T>` (Пункти 2 і 7)

Контекст бази даних [`SymptomDiaryDbContext`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Data/SymptomDiaryDbContext.cs) успадковується від `DbContext`, оголошує `DbSet<T>` для всіх 12 сутностей і в методі `OnModelCreating` автоматично підключає конфігурації з поточної збірки через `ApplyConfigurationsFromAssembly`.

У файлі [`EntityConfigurations.cs`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Data/Configurations/EntityConfigurations.cs) створено окремий клас `IEntityTypeConfiguration<T>` для кожної з 12 сутностей:
- **Типи, обов'язковість та максимальні довжини:** для всіх рядкових полів явно задано `.HasMaxLength(...)` та `.IsRequired()` (або залишено nullable для необов'язкових приміток `EntryNote`, `IntervalNote`, `SymptomNote`, `EventNote`, `MeasurementNote`).
- **Точність чисел із фіксованою комою (`decimal precision`):** для полів `MetricMinValue`, `MetricMaxValue` у `MetricTypes` та `MeasurementValue` у `Measurements` налаштовано `.HasPrecision(6, 2)`, що відповідає типу `decimal(6,2)` у SQL Server.
- **Складений первинний ключ (`Composite Key`) для проміжної сутності:** у `IntervalSymptomConfiguration` налаштовано `entity.HasKey(x => new { x.DayIntervalId, x.SymptomId })`, що реалізує зв'язок багато-до-багатьох між часовими інтервалами та симптомами.
- **Унікальні індекси (`Unique Index`) для бізнес-ідентифікаторів:**
  - `DiaryEntries`: унікальний складений індекс `HasIndex(x => new { x.UserId, x.EntryDate }).IsUnique()` — гарантує, що користувач має не більше одного запису щоденника на календарну добу;
  - `Users`: унікальні індекси на `UserEmail` та `ExternalAuthId`;
  - довідники: унікальні індекси на назви (`SymptomName`, `EventName`, `MetricName`, `WellbeingName`, `SeverityName`) та ранги (`WellbeingRank`, `SeverityRank`).
- **Свідомо обраний `DeleteBehavior`:**
  - `DeleteBehavior.Cascade` налаштовано для композиційних залежностей (`User → DiaryEntries/Reports`, `DiaryEntry → DayIntervals/DayEvents/Measurements`, `DayInterval → IntervalSymptoms`), оскільки дочірні записи не мають сенсу без батьківського запису дня або профілю пацієнта;
  - `DeleteBehavior.Restrict` свідомо налаштовано для всіх зовнішніх ключів, що ведуть до довідників (`Symptoms`, `SeverityLevels`, `WellbeingLevels`, `EventTypes`, `MetricTypes`), щоб унеможливити видалення класифікатора, який вже використовується в історії спостережень пацієнта.

---

## 2. Наповнення бази тестовими даними: `HasData` та `UseSeeding` / `UseAsyncSeeding` (Пункт 3)

Відповідно до вимог методички застосовано два різні механізми початкового наповнення:

1. **Сталі довідники через `HasData`** (у класах конфігурацій `EntityConfigurations.cs`, потрапляють безпосередньо у файл міграції `InitialCreate`):
   - `WellbeingLevels`: 4 записи («Добре», «Помірно», «Погано», «Дуже погано»);
   - `SeverityLevels`: 3 записи («Помірно», «Погано», «Дуже погано»);
   - `Symptoms`: 8 записів у 4 категоріях («Неврологічні», «Респіраторні», «ШКТ», «Загальні»);
   - `EventTypes`: 6 записів («Стрес», «Недосипання», «Прийом ліків», «Фізичне навантаження», «Кава або енергетик», «Зміна погоди»);
   - `MetricTypes`: 5 записів («Температура тіла», «Систолічний тиск», «Діастолічний тиск», «Пульс», «Сатурація»).
   - **Разом у довідникових таблицях:** **26 записів** (вимога: не менше 10).

2. **Репрезентативні транзакційні дані через `UseSeeding` та `UseAsyncSeeding`** (клас [`SeedData.cs`](file:///home/denys/Desktop/University/year_2026_2027/sem5/SWC/Lab2_KPZ/code/Kpz.Lab2/Data/SeedData.cs), підключений у `Program.cs`):
   - Метод перевіряє ідемпотентність (`!context.Set<User>().Any()`) і додає граф із **3 користувачів** (`Users`), **6 записів дня** (`DiaryEntries`), **9 часових інтервалів** (`DayIntervals`), **15 проявів симптомів в інтервалах** (`IntervalSymptoms`), **8 супутніх подій** (`DayEvents`), **16 вимірювань показників** (`Measurements`) та **2 звітів** (`Reports`).
   - **Разом у транзакційних/залежних таблицях:** **56 записів** (вимога: не менше 30).

