using System.Data;
using Microsoft.Data.SqlClient;

namespace lab3_kpz.Core.AdoNet;

public sealed class ComplexRequestsRepository(string connectionString)
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async Task<List<MeasurementLogReport>> GetMeasurementLogReport(
        int userId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                d.EntryDate,
                m.MeasuringTime,
                met.MetricName,
                met.MetricValue,
                u.UnitName
            FROM DiaryEntries AS d
            JOIN Measurements AS m ON d.DiaryEntryId = m.DiaryEntryId
            JOIN Metrics AS met ON m.MeasurementId = met.MeasurementId
            JOIN MetricUnits AS u ON met.MetricUnitId = u.MetricUnitId
            WHERE d.UserId = @UserId
              AND d.EntryDate >= @FromDate
              AND d.EntryDate <= @ToDate
            ORDER BY d.EntryDate DESC, m.MeasuringTime DESC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        command.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.Date) { Value = fromDate.ToDateTime(TimeOnly.MinValue) });
        command.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.Date) { Value = toDate.ToDateTime(TimeOnly.MinValue) });

        await using var reader = await command.ExecuteReaderAsync(ct);

        var results = new List<MeasurementLogReport>();

        int dateOrdinal = reader.GetOrdinal("EntryDate");
        int timeOrdinal = reader.GetOrdinal("MeasuringTime");
        int metricNameOrdinal = reader.GetOrdinal("MetricName");
        int metricValueOrdinal = reader.GetOrdinal("MetricValue");
        int unitNameOrdinal = reader.GetOrdinal("UnitName");

        while (await reader.ReadAsync(ct))
        {
            results.Add(new MeasurementLogReport(
                EntryDate: DateOnly.FromDateTime(reader.GetDateTime(dateOrdinal)),
                MeasuringTime: TimeOnly.FromTimeSpan(reader.GetTimeSpan(timeOrdinal)),
                MetricName: reader.GetString(metricNameOrdinal),
                MetricValue: reader.GetDecimal(metricValueOrdinal),
                UnitName: reader.GetString(unitNameOrdinal)
            ));
        }

        return results;
    }

    public async Task<List<MetricSummaryReport>> GetMetricSummaryReport(
        int userId,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                met.MetricName,
                u.UnitName,
                COUNT(met.MetricId) AS TotalMeasurements,
                CAST(ROUND(AVG(met.MetricValue), 2) AS DECIMAL(6, 2)) AS AverageValue,
                MIN(met.MetricValue) AS MinValue,
                MAX(met.MetricValue) AS MaxValue
            FROM Metrics AS met
            JOIN MetricUnits AS u ON met.MetricUnitId = u.MetricUnitId
            JOIN Measurements AS m ON met.MeasurementId = m.MeasurementId
            JOIN DiaryEntries AS d ON m.DiaryEntryId = d.DiaryEntryId
            WHERE d.UserId = @UserId
            GROUP BY met.MetricName, u.UnitName
            HAVING COUNT(met.MetricId) > 0
            ORDER BY TotalMeasurements DESC, met.MetricName ASC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

        await using var reader = await command.ExecuteReaderAsync(ct);

        var results = new List<MetricSummaryReport>();

        int metricNameOrdinal = reader.GetOrdinal("MetricName");
        int unitNameOrdinal = reader.GetOrdinal("UnitName");
        int countOrdinal = reader.GetOrdinal("TotalMeasurements");
        int avgOrdinal = reader.GetOrdinal("AverageValue");
        int minOrdinal = reader.GetOrdinal("MinValue");
        int maxOrdinal = reader.GetOrdinal("MaxValue");

        while (await reader.ReadAsync(ct))
        {
            results.Add(new MetricSummaryReport(
                MetricName: reader.GetString(metricNameOrdinal),
                UnitName: reader.GetString(unitNameOrdinal),
                TotalMeasurements: reader.GetInt32(countOrdinal),
                AverageValue: reader.GetDecimal(avgOrdinal),
                MinValue: reader.GetDecimal(minOrdinal),
                MaxValue: reader.GetDecimal(maxOrdinal)
            ));
        }

        return results;
    }
}

public sealed record MeasurementLogReport(
    DateOnly EntryDate,
    TimeOnly MeasuringTime,
    string MetricName,
    decimal MetricValue,
    string UnitName);

public sealed record MetricSummaryReport(
    string MetricName,
    string UnitName,
    int TotalMeasurements,
    decimal AverageValue,
    decimal MinValue,
    decimal MaxValue);

