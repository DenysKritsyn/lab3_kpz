using System.Data;
using Kpz.Lab2.Dtos;
using Microsoft.Data.SqlClient;

namespace Kpz.Lab2.Services;

public sealed record IntervalInputDto(
    byte IntervalStartHour,
    byte IntervalEndHour,
    int WellbeingLevelId,
    string? IntervalNote,
    IReadOnlyList<IntervalSymptomInputDto> Symptoms);

public sealed record IntervalSymptomInputDto(
    int SymptomId,
    int SeverityLevelId,
    string? SymptomNote);

public sealed record MeasurementInputDto(
    int MetricTypeId,
    decimal MeasurementValue,
    byte? MeasurementHour,
    string? MeasurementNote);

public sealed class AdoNetDiaryService(string connectionString)
{
    // =========================================================================
    // Task 4. CRUD via Microsoft.Data.SqlClient for two related entities:
    //         1) DiaryEntries (parent transactional entity)
    //         2) Measurements (dependent entity)
    // =========================================================================

    public async Task<int> CreateDiaryEntryAsync(
        int userId,
        DateOnly entryDate,
        string? entryNote,
        CancellationToken ct)
    {
        const string sql = """
            INSERT INTO DiaryEntries (UserId, EntryDate, EntryNote, EntryCreatedAt, EntryUpdatedAt)
            OUTPUT INSERTED.DiaryEntryId
            VALUES (@userId, @entryDate, @entryNote, @now, @now);
            """;

        var now = DateTime.UtcNow;
        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@entryDate", SqlDbType.Date).Value = entryDate.ToDateTime(TimeOnly.MinValue);
        command.Parameters.Add("@entryNote", SqlDbType.NVarChar, 2000).Value = (object?)entryNote ?? DBNull.Value;
        command.Parameters.Add("@now", SqlDbType.DateTime2).Value = now;

        await connection.OpenAsync(ct);
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<DiaryEntryDto?> GetDiaryEntryByIdAsync(int diaryEntryId, CancellationToken ct)
    {
        const string sql = """
            SELECT d.DiaryEntryId, d.UserId, u.UserName, d.EntryDate, d.EntryNote, d.EntryCreatedAt, d.EntryUpdatedAt
            FROM DiaryEntries AS d
            INNER JOIN Users AS u ON u.UserId = d.UserId
            WHERE d.DiaryEntryId = @diaryEntryId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@diaryEntryId", SqlDbType.Int).Value = diaryEntryId;

        await connection.OpenAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        var idOrd = reader.GetOrdinal("DiaryEntryId");
        var userIdOrd = reader.GetOrdinal("UserId");
        var userNameOrd = reader.GetOrdinal("UserName");
        var dateOrd = reader.GetOrdinal("EntryDate");
        var noteOrd = reader.GetOrdinal("EntryNote");
        var createdOrd = reader.GetOrdinal("EntryCreatedAt");
        var updatedOrd = reader.GetOrdinal("EntryUpdatedAt");

        return new DiaryEntryDto(
            reader.GetInt32(idOrd),
            reader.GetInt32(userIdOrd),
            reader.GetString(userNameOrd),
            DateOnly.FromDateTime(reader.GetDateTime(dateOrd)),
            reader.IsDBNull(noteOrd) ? null : reader.GetString(noteOrd),
            reader.GetDateTime(createdOrd),
            reader.GetDateTime(updatedOrd));
    }

    public async Task<bool> UpdateDiaryEntryNoteAsync(int diaryEntryId, string? newNote, CancellationToken ct)
    {
        const string sql = """
            UPDATE DiaryEntries
            SET EntryNote = @entryNote,
                EntryUpdatedAt = @updatedAt
            WHERE DiaryEntryId = @diaryEntryId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@diaryEntryId", SqlDbType.Int).Value = diaryEntryId;
        command.Parameters.Add("@entryNote", SqlDbType.NVarChar, 2000).Value = (object?)newNote ?? DBNull.Value;
        command.Parameters.Add("@updatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;

        await connection.OpenAsync(ct);
        var affected = await command.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<bool> DeleteDiaryEntryAsync(int diaryEntryId, CancellationToken ct)
    {
        const string sql = "DELETE FROM DiaryEntries WHERE DiaryEntryId = @diaryEntryId;";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@diaryEntryId", SqlDbType.Int).Value = diaryEntryId;

        await connection.OpenAsync(ct);
        var affected = await command.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    // CRUD for the second related entity: Measurements

    public async Task<int> CreateMeasurementAsync(
        int diaryEntryId,
        int metricTypeId,
        decimal measurementValue,
        byte? measurementHour,
        string? measurementNote,
        CancellationToken ct)
    {
        const string sql = """
            INSERT INTO Measurements (DiaryEntryId, MetricTypeId, MeasurementValue, MeasurementHour, MeasurementNote)
            OUTPUT INSERTED.MeasurementId
            VALUES (@diaryEntryId, @metricTypeId, @measurementValue, @measurementHour, @measurementNote);
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@diaryEntryId", SqlDbType.Int).Value = diaryEntryId;
        command.Parameters.Add("@metricTypeId", SqlDbType.Int).Value = metricTypeId;

        var valParam = command.Parameters.Add("@measurementValue", SqlDbType.Decimal);
        valParam.Precision = 6;
        valParam.Scale = 2;
        valParam.Value = measurementValue;

        command.Parameters.Add("@measurementHour", SqlDbType.TinyInt).Value =
            measurementHour.HasValue ? measurementHour.Value : DBNull.Value;
        command.Parameters.Add("@measurementNote", SqlDbType.NVarChar, 500).Value =
            (object?)measurementNote ?? DBNull.Value;

        await connection.OpenAsync(ct);
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<MeasurementDto>> GetMeasurementsByEntryAsync(int diaryEntryId, CancellationToken ct)
    {
        const string sql = """
            SELECT m.MeasurementId, m.DiaryEntryId, m.MetricTypeId,
                   mt.MetricName, mt.MetricUnit,
                   m.MeasurementValue, m.MeasurementHour, m.MeasurementNote
            FROM Measurements AS m
            INNER JOIN MetricTypes AS mt ON mt.MetricTypeId = m.MetricTypeId
            WHERE m.DiaryEntryId = @diaryEntryId
            ORDER BY m.MeasurementHour, m.MeasurementId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@diaryEntryId", SqlDbType.Int).Value = diaryEntryId;

        await connection.OpenAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var idOrd = reader.GetOrdinal("MeasurementId");
        var entryOrd = reader.GetOrdinal("DiaryEntryId");
        var metricIdOrd = reader.GetOrdinal("MetricTypeId");
        var metricNameOrd = reader.GetOrdinal("MetricName");
        var unitOrd = reader.GetOrdinal("MetricUnit");
        var valOrd = reader.GetOrdinal("MeasurementValue");
        var hourOrd = reader.GetOrdinal("MeasurementHour");
        var noteOrd = reader.GetOrdinal("MeasurementNote");

        var list = new List<MeasurementDto>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new MeasurementDto(
                reader.GetInt32(idOrd),
                reader.GetInt32(entryOrd),
                reader.GetInt32(metricIdOrd),
                reader.GetString(metricNameOrd),
                reader.GetString(unitOrd),
                reader.GetDecimal(valOrd),
                reader.IsDBNull(hourOrd) ? null : reader.GetByte(hourOrd),
                reader.IsDBNull(noteOrd) ? null : reader.GetString(noteOrd)));
        }

        return list;
    }

    public async Task<bool> UpdateMeasurementAsync(
        int measurementId,
        decimal newValue,
        byte? newHour,
        string? newNote,
        CancellationToken ct)
    {
        const string sql = """
            UPDATE Measurements
            SET MeasurementValue = @val,
                MeasurementHour = @hour,
                MeasurementNote = @note
            WHERE MeasurementId = @id;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = measurementId;
        var valParam = command.Parameters.Add("@val", SqlDbType.Decimal);
        valParam.Precision = 6;
        valParam.Scale = 2;
        valParam.Value = newValue;

        command.Parameters.Add("@hour", SqlDbType.TinyInt).Value =
            newHour.HasValue ? newHour.Value : DBNull.Value;
        command.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value =
            (object?)newNote ?? DBNull.Value;

        await connection.OpenAsync(ct);
        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> DeleteMeasurementAsync(int measurementId, CancellationToken ct)
    {
        const string sql = "DELETE FROM Measurements WHERE MeasurementId = @id;";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@id", SqlDbType.Int).Value = measurementId;

        await connection.OpenAsync(ct);
        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    // =========================================================================
    // Task 5. Two complex queries via ADO.NET:
    //         1) Multi-table JOIN (6 tables) with filtering and sorting
    //         2) Aggregated GROUP BY query with 4 aggregate functions
    // =========================================================================

    public async Task<IReadOnlyList<SymptomOccurrenceDto>> SearchSymptomOccurrencesAsync(
        int? userId,
        byte minSeverityRank,
        DateOnly? fromDate,
        CancellationToken ct)
    {
        const string sql = """
            SELECT d.DiaryEntryId,
                   d.EntryDate,
                   u.UserName,
                   di.IntervalStartHour,
                   di.IntervalEndHour,
                   s.SymptomName,
                   s.SymptomCategory,
                   sl.SeverityName,
                   sl.SeverityRank,
                   isym.SymptomNote
            FROM IntervalSymptoms AS isym
            INNER JOIN DayIntervals AS di ON di.DayIntervalId = isym.DayIntervalId
            INNER JOIN DiaryEntries AS d ON d.DiaryEntryId = di.DiaryEntryId
            INNER JOIN Users AS u ON u.UserId = d.UserId
            INNER JOIN Symptoms AS s ON s.SymptomId = isym.SymptomId
            INNER JOIN SeverityLevels AS sl ON sl.SeverityLevelId = isym.SeverityLevelId
            WHERE sl.SeverityRank >= @minSeverityRank
              AND (@userId IS NULL OR d.UserId = @userId)
              AND (@fromDate IS NULL OR d.EntryDate >= @fromDate)
            ORDER BY d.EntryDate DESC, di.IntervalStartHour ASC, sl.SeverityRank DESC;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@minSeverityRank", SqlDbType.TinyInt).Value = minSeverityRank;
        command.Parameters.Add("@userId", SqlDbType.Int).Value =
            userId.HasValue ? userId.Value : DBNull.Value;
        command.Parameters.Add("@fromDate", SqlDbType.Date).Value =
            fromDate.HasValue ? fromDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;

        await connection.OpenAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var entryIdOrd = reader.GetOrdinal("DiaryEntryId");
        var dateOrd = reader.GetOrdinal("EntryDate");
        var userOrd = reader.GetOrdinal("UserName");
        var startOrd = reader.GetOrdinal("IntervalStartHour");
        var endOrd = reader.GetOrdinal("IntervalEndHour");
        var symNameOrd = reader.GetOrdinal("SymptomName");
        var symCatOrd = reader.GetOrdinal("SymptomCategory");
        var sevNameOrd = reader.GetOrdinal("SeverityName");
        var sevRankOrd = reader.GetOrdinal("SeverityRank");
        var noteOrd = reader.GetOrdinal("SymptomNote");

        var items = new List<SymptomOccurrenceDto>();
        while (await reader.ReadAsync(ct))
        {
            items.Add(new SymptomOccurrenceDto(
                reader.GetInt32(entryIdOrd),
                DateOnly.FromDateTime(reader.GetDateTime(dateOrd)),
                reader.GetString(userOrd),
                reader.GetByte(startOrd),
                reader.GetByte(endOrd),
                reader.GetString(symNameOrd),
                reader.IsDBNull(symCatOrd) ? null : reader.GetString(symCatOrd),
                reader.GetString(sevNameOrd),
                reader.GetByte(sevRankOrd),
                reader.IsDBNull(noteOrd) ? null : reader.GetString(noteOrd)));
        }

        return items;
    }

    public async Task<IReadOnlyList<SymptomStatsDto>> GetSymptomAggregatedStatsAsync(
        int minOccurrences,
        CancellationToken ct)
    {
        const string sql = """
            SELECT s.SymptomId,
                   s.SymptomName,
                   s.SymptomCategory,
                   COUNT(isym.DayIntervalId) AS OccurrencesCount,
                   COALESCE(SUM(CAST(di.IntervalEndHour - di.IntervalStartHour AS int)), 0) AS TotalHoursDuration,
                   COALESCE(AVG(CAST(sl.SeverityRank AS float)), 0.0) AS AverageSeverityRank,
                   COALESCE(MAX(sl.SeverityRank), CAST(0 AS tinyint)) AS MaxSeverityRank
            FROM Symptoms AS s
            LEFT JOIN IntervalSymptoms AS isym ON isym.SymptomId = s.SymptomId
            LEFT JOIN DayIntervals AS di ON di.DayIntervalId = isym.DayIntervalId
            LEFT JOIN SeverityLevels AS sl ON sl.SeverityLevelId = isym.SeverityLevelId
            GROUP BY s.SymptomId, s.SymptomName, s.SymptomCategory
            HAVING COUNT(isym.DayIntervalId) >= @minOccurrences
            ORDER BY OccurrencesCount DESC, TotalHoursDuration DESC;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@minOccurrences", SqlDbType.Int).Value = minOccurrences;

        await connection.OpenAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var idOrd = reader.GetOrdinal("SymptomId");
        var nameOrd = reader.GetOrdinal("SymptomName");
        var catOrd = reader.GetOrdinal("SymptomCategory");
        var countOrd = reader.GetOrdinal("OccurrencesCount");
        var hoursOrd = reader.GetOrdinal("TotalHoursDuration");
        var avgOrd = reader.GetOrdinal("AverageSeverityRank");
        var maxOrd = reader.GetOrdinal("MaxSeverityRank");

        var stats = new List<SymptomStatsDto>();
        while (await reader.ReadAsync(ct))
        {
            stats.Add(new SymptomStatsDto(
                reader.GetInt32(idOrd),
                reader.GetString(nameOrd),
                reader.IsDBNull(catOrd) ? null : reader.GetString(catOrd),
                reader.GetInt32(countOrd),
                reader.GetInt32(hoursOrd),
                Math.Round(reader.GetDouble(avgOrd), 2),
                reader.GetByte(maxOrd)));
        }

        return stats;
    }

    // =========================================================================
    // Task 6. ADO.NET Transaction modifying 4 tables:
    //         DiaryEntries, DayIntervals, IntervalSymptoms, Measurements.
    //         Validates business rules:
    //         1) Day intervals must not overlap;
    //         2) Measurement values must be within [MetricMinValue; MetricMaxValue].
    // =========================================================================

    public async Task<int> RecordFullDayEntryInTransactionAsync(
        int userId,
        DateOnly entryDate,
        string? entryNote,
        IReadOnlyList<IntervalInputDto> intervals,
        IReadOnlyList<MeasurementInputDto> measurements,
        CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(ct);

        try
        {
            var now = DateTime.UtcNow;

            // Insert day entry (Table 1: DiaryEntries)
            const string insertEntrySql = """
                INSERT INTO DiaryEntries (UserId, EntryDate, EntryNote, EntryCreatedAt, EntryUpdatedAt)
                OUTPUT INSERTED.DiaryEntryId
                VALUES (@userId, @entryDate, @entryNote, @now, @now);
                """;

            int diaryEntryId;
            await using (var cmd = new SqlCommand(insertEntrySql, connection, transaction))
            {
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@entryDate", SqlDbType.Date).Value = entryDate.ToDateTime(TimeOnly.MinValue);
                cmd.Parameters.Add("@entryNote", SqlDbType.NVarChar, 2000).Value = (object?)entryNote ?? DBNull.Value;
                cmd.Parameters.Add("@now", SqlDbType.DateTime2).Value = now;
                diaryEntryId = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }

            // Validate Business Rule 1: Intervals within the same day must not overlap
            var sortedIntervals = intervals.OrderBy(x => x.IntervalStartHour).ToList();
            for (int i = 0; i < sortedIntervals.Count; i++)
            {
                var current = sortedIntervals[i];
                if (current.IntervalEndHour <= current.IntervalStartHour || current.IntervalEndHour > 24)
                    throw new InvalidOperationException(
                        $"Invalid interval range [{current.IntervalStartHour}..{current.IntervalEndHour}].");

                if (i > 0 && current.IntervalStartHour < sortedIntervals[i - 1].IntervalEndHour)
                {
                    throw new InvalidOperationException(
                        $"Business rule violation: interval [{current.IntervalStartHour}..{current.IntervalEndHour}] " +
                        $"overlaps with interval [{sortedIntervals[i - 1].IntervalStartHour}..{sortedIntervals[i - 1].IntervalEndHour}].");
                }
            }

            // Insert intervals (Table 2: DayIntervals) and symptoms (Table 3: IntervalSymptoms)
            const string insertIntervalSql = """
                INSERT INTO DayIntervals (DiaryEntryId, IntervalStartHour, IntervalEndHour, WellbeingLevelId, IntervalNote)
                OUTPUT INSERTED.DayIntervalId
                VALUES (@diaryEntryId, @startHour, @endHour, @wellbeingId, @note);
                """;

            const string insertSymptomSql = """
                INSERT INTO IntervalSymptoms (DayIntervalId, SymptomId, SeverityLevelId, SymptomNote)
                VALUES (@intervalId, @symptomId, @severityId, @note);
                """;

            foreach (var interval in sortedIntervals)
            {
                int intervalId;
                await using (var intCmd = new SqlCommand(insertIntervalSql, connection, transaction))
                {
                    intCmd.Parameters.Add("@diaryEntryId", SqlDbType.Int).Value = diaryEntryId;
                    intCmd.Parameters.Add("@startHour", SqlDbType.TinyInt).Value = interval.IntervalStartHour;
                    intCmd.Parameters.Add("@endHour", SqlDbType.TinyInt).Value = interval.IntervalEndHour;
                    intCmd.Parameters.Add("@wellbeingId", SqlDbType.Int).Value = interval.WellbeingLevelId;
                    intCmd.Parameters.Add("@note", SqlDbType.NVarChar, 1000).Value =
                        (object?)interval.IntervalNote ?? DBNull.Value;

                    intervalId = Convert.ToInt32(await intCmd.ExecuteScalarAsync(ct));
                }

                foreach (var sym in interval.Symptoms)
                {
                    await using var symCmd = new SqlCommand(insertSymptomSql, connection, transaction);
                    symCmd.Parameters.Add("@intervalId", SqlDbType.Int).Value = intervalId;
                    symCmd.Parameters.Add("@symptomId", SqlDbType.Int).Value = sym.SymptomId;
                    symCmd.Parameters.Add("@severityId", SqlDbType.Int).Value = sym.SeverityLevelId;
                    symCmd.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value =
                        (object?)sym.SymptomNote ?? DBNull.Value;

                    await symCmd.ExecuteNonQueryAsync(ct);
                }
            }

            // Validate physiological bounds and insert measurements (Table 4: Measurements)
            const string checkMetricSql = """
                SELECT MetricName, MetricMinValue, MetricMaxValue
                FROM MetricTypes
                WHERE MetricTypeId = @metricTypeId;
                """;

            const string insertMeasurementSql = """
                INSERT INTO Measurements (DiaryEntryId, MetricTypeId, MeasurementValue, MeasurementHour, MeasurementNote)
                VALUES (@diaryEntryId, @metricTypeId, @val, @hour, @note);
                """;

            foreach (var m in measurements)
            {
                await using (var checkCmd = new SqlCommand(checkMetricSql, connection, transaction))
                {
                    checkCmd.Parameters.Add("@metricTypeId", SqlDbType.Int).Value = m.MetricTypeId;
                    await using var reader = await checkCmd.ExecuteReaderAsync(ct);
                    if (!await reader.ReadAsync(ct))
                        throw new InvalidOperationException($"MetricType ID={m.MetricTypeId} was not found.");

                    var metricName = reader.GetString(0);
                    var minVal = reader.GetDecimal(1);
                    var maxVal = reader.GetDecimal(2);

                    if (m.MeasurementValue < minVal || m.MeasurementValue > maxVal)
                    {
                        throw new InvalidOperationException(
                            $"Business rule violation: value {m.MeasurementValue} for '{metricName}' " +
                            $"is outside allowed bounds [{minVal}; {maxVal}].");
                    }
                }

                await using (var mCmd = new SqlCommand(insertMeasurementSql, connection, transaction))
                {
                    mCmd.Parameters.Add("@diaryEntryId", SqlDbType.Int).Value = diaryEntryId;
                    mCmd.Parameters.Add("@metricTypeId", SqlDbType.Int).Value = m.MetricTypeId;
                    var valParam = mCmd.Parameters.Add("@val", SqlDbType.Decimal);
                    valParam.Precision = 6;
                    valParam.Scale = 2;
                    valParam.Value = m.MeasurementValue;
                    mCmd.Parameters.Add("@hour", SqlDbType.TinyInt).Value =
                        m.MeasurementHour.HasValue ? m.MeasurementHour.Value : DBNull.Value;
                    mCmd.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value =
                        (object?)m.MeasurementNote ?? DBNull.Value;

                    await mCmd.ExecuteNonQueryAsync(ct);
                }
            }

            await transaction.CommitAsync(ct);
            return diaryEntryId;
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                // If connection is lost, SQL Server will roll back the transaction automatically
            }
            throw;
        }
    }
}
