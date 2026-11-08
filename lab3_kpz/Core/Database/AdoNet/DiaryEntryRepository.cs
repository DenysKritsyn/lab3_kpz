using System.Data;
using lab3_kpz.Entities;
using Microsoft.Data.SqlClient;

namespace lab3_kpz.Core.AdoNet;

public sealed class DiaryEntryRepository(string connectionString)
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async Task<int> Create(DiaryEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        const string sql = """
            INSERT INTO DiaryEntries (UserId, EntryDate, EntryNote, EntryCreatedAt, EntryUpdatedAt)
            OUTPUT INSERTED.DiaryEntryId, INSERTED.EntryCreatedAt, INSERTED.EntryUpdatedAt
            VALUES (@UserId, @EntryDate, @EntryNote, SYSUTCDATETIME(), SYSUTCDATETIME());
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = entry.UserId });
        command.Parameters.Add(new SqlParameter("@EntryDate", SqlDbType.Date) { Value = entry.EntryDate.ToDateTime(TimeOnly.MinValue) });
        command.Parameters.Add(new SqlParameter("@EntryNote", SqlDbType.NVarChar, 1000)
        {
            Value = string.IsNullOrWhiteSpace(entry.EntryNote) ? DBNull.Value : entry.EntryNote
        });

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await reader.ReadAsync(ct))
        {
            entry.DiaryEntryId = reader.GetInt32(0);
            entry.EntryCreatedAt = reader.GetDateTime(1);
            entry.EntryUpdatedAt = reader.GetDateTime(2);
        }

        return entry.DiaryEntryId;
    }

    public async Task<DiaryEntry?> Read(int diaryEntryId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT DiaryEntryId, UserId, EntryDate, EntryNote, EntryCreatedAt, EntryUpdatedAt, EntryRowVersion
            FROM DiaryEntries
            WHERE DiaryEntryId = @DiaryEntryId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@DiaryEntryId", SqlDbType.Int) { Value = diaryEntryId });

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await reader.ReadAsync(ct))
        {
            return MapDiaryEntry(reader);
        }

        return null;
    }

    public async Task<bool> Update(DiaryEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        const string sql = """
            UPDATE DiaryEntries
            SET EntryDate = @EntryDate,
                EntryNote = @Note,
                EntryUpdatedAt = SYSUTCDATETIME()
            WHERE DiaryEntryId = @DiaryEntryId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@DiaryEntryId", SqlDbType.Int) { Value = entry.DiaryEntryId });
        command.Parameters.Add(new SqlParameter("@EntryDate", SqlDbType.Date) { Value = entry.EntryDate.ToDateTime(TimeOnly.MinValue) });
        command.Parameters.Add(new SqlParameter("@Note", SqlDbType.NVarChar, 1000)
        {
            Value = string.IsNullOrWhiteSpace(entry.EntryNote) ? DBNull.Value : entry.EntryNote
        });

        int affected = await command.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<bool> Delete(int diaryEntryId, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM DiaryEntries
            WHERE DiaryEntryId = @DiaryEntryId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@DiaryEntryId", SqlDbType.Int) { Value = diaryEntryId });

        int affected = await command.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    private static DiaryEntry MapDiaryEntry(SqlDataReader reader)
    {
        int noteOrdinal = reader.GetOrdinal("EntryNote");

        return new DiaryEntry
        {
            DiaryEntryId = reader.GetInt32(reader.GetOrdinal("DiaryEntryId")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            EntryDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("EntryDate"))),
            EntryNote = reader.IsDBNull(noteOrdinal) ? null : reader.GetString(noteOrdinal),
            EntryCreatedAt = reader.GetDateTime(reader.GetOrdinal("EntryCreatedAt")),
            EntryUpdatedAt = reader.GetDateTime(reader.GetOrdinal("EntryUpdatedAt")),
            EntryRowVersion = (byte[])reader["EntryRowVersion"]
        };
    }
}
