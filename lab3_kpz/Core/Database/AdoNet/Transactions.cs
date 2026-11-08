using System.Data;
using Microsoft.Data.SqlClient;

namespace lab3_kpz.Core.AdoNet;

public sealed class Transactions(string connectionString)
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async Task<int> AddMeasurementWithAuditAsync(
        int diaryEntryId,
        TimeOnly measuringTime,
        int metricUnitId,
        string metricName,
        decimal metricValue,
        bool simulateMissingReport = false,
        CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(ct);

        try
        {
            var updatedAt = DateTime.UtcNow;

            // 1. Update DiaryEntries
            const string updateDiarySql = """
                UPDATE DiaryEntries
                SET EntryUpdatedAt = @UpdatedAt
                OUTPUT INSERTED.UserId, INSERTED.EntryDate
                WHERE DiaryEntryId = @DiaryEntryId;
                """;

            int userId;
            DateOnly entryDate;

            await using (var updateDiaryCmd = new SqlCommand(updateDiarySql, connection, transaction))
            {
                updateDiaryCmd.Parameters.Add(new SqlParameter("@DiaryEntryId", SqlDbType.Int) { Value = diaryEntryId });
                updateDiaryCmd.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = updatedAt });

                await using var reader = await updateDiaryCmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
                if (!await reader.ReadAsync(ct))
                {
                    throw new InvalidOperationException($"DiaryEntry with ID {diaryEntryId} was not found.");
                }

                userId = reader.GetInt32(0);
                entryDate = DateOnly.FromDateTime(reader.GetDateTime(1));
            }

            // 2. Insert new Measurement
            const string insertMeasurementSql = """
                INSERT INTO Measurements (DiaryEntryId, MeasuringTime)
                OUTPUT INSERTED.MeasurementId
                VALUES (@DiaryEntryId, @MeasuringTime);
                """;

            int measurementId;

            await using (var insertMeasurementCmd = new SqlCommand(insertMeasurementSql, connection, transaction))
            {
                insertMeasurementCmd.Parameters.Add(new SqlParameter("@DiaryEntryId", SqlDbType.Int) { Value = diaryEntryId });
                insertMeasurementCmd.Parameters.Add(new SqlParameter("@MeasuringTime", SqlDbType.Time) { Value = measuringTime.ToTimeSpan() });

                var result = await insertMeasurementCmd.ExecuteScalarAsync(ct);
                measurementId = Convert.ToInt32(result);
            }

            // 3. Insert new Metric
            const string insertMetricSql = """
                INSERT INTO Metrics (MeasurementId, MetricUnitId, MetricName, MetricValue)
                OUTPUT INSERTED.MetricId
                VALUES (@MeasurementId, @MetricUnitId, @MetricName, @MetricValue);
                """;

            await using (var insertMetricCmd = new SqlCommand(insertMetricSql, connection, transaction))
            {
                insertMetricCmd.Parameters.Add(new SqlParameter("@MeasurementId", SqlDbType.Int) { Value = measurementId });
                insertMetricCmd.Parameters.Add(new SqlParameter("@MetricUnitId", SqlDbType.Int) { Value = metricUnitId });
                insertMetricCmd.Parameters.Add(new SqlParameter("@MetricName", SqlDbType.NVarChar, 100) { Value = metricName });
                insertMetricCmd.Parameters.Add(new SqlParameter("@MetricValue", SqlDbType.Decimal)
                {
                    Precision = 6,
                    Scale = 2,
                    Value = metricValue
                });

                await insertMetricCmd.ExecuteScalarAsync(ct);
            }

            // Validate MetricValue
            const string checkUnitBoundsSql = """
                SELECT UnitName, UnitMinValue, UnitMaxValue
                FROM MetricUnits
                WHERE MetricUnitId = @MetricUnitId;
                """;

            await using (var checkBoundsCmd = new SqlCommand(checkUnitBoundsSql, connection, transaction))
            {
                checkBoundsCmd.Parameters.Add(new SqlParameter("@MetricUnitId", SqlDbType.Int) { Value = metricUnitId });

                await using var reader = await checkBoundsCmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
                if (!await reader.ReadAsync(ct))
                {
                    throw new InvalidOperationException($"MetricUnit with ID {metricUnitId} does not exist.");
                }

                string unitName = reader.GetString(0);
                decimal minValue = reader.GetDecimal(1);
                decimal maxValue = reader.GetDecimal(2);

                if (metricValue < minValue || metricValue > maxValue)
                {
                    throw new InvalidOperationException(
                        $"Business rule violation: '{metricName}' value ({metricValue}) is out of allowed physiological bounds [{minValue} .. {maxValue}] for unit '{unitName}'.");
                }
            }

            // 4. Update Reports covering the DiaryEntry
            const string updateReportSql = """
                UPDATE Reports
                SET ReportGeneratedAt = @UpdatedAt,
                    ReportSummary = CONCAT(ReportSummary, ' [Updated with ', @MetricName, ']')
                OUTPUT INSERTED.ReportId
                WHERE UserId = @UserId
                  AND ReportPeriodFrom <= @EntryDate
                  AND ReportPeriodTo >= @EntryDate
                  AND (@SimulateMissingReport = 0);
                """;

            await using (var updateReportCmd = new SqlCommand(updateReportSql, connection, transaction))
            {
                updateReportCmd.Parameters.Add(new SqlParameter("@MetricName", SqlDbType.NVarChar, 100) { Value = metricName });
                updateReportCmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                updateReportCmd.Parameters.Add(new SqlParameter("@EntryDate", SqlDbType.Date) { Value = entryDate.ToDateTime(TimeOnly.MinValue) });
                updateReportCmd.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = updatedAt });
                updateReportCmd.Parameters.Add(new SqlParameter("@SimulateMissingReport", SqlDbType.Bit) { Value = simulateMissingReport ? 1 : 0 });

                var updatedReportId = await updateReportCmd.ExecuteScalarAsync(ct);
                if (updatedReportId is null || updatedReportId == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        $"Transaction aborted: No active clinical Report found covering entry date {entryDate} for user {userId}. Changes rolled back.");
                }
            }

            // Commit all changes atomically
            await transaction.CommitAsync(ct);
            return measurementId;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
