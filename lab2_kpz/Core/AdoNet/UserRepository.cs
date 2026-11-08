using System.Data;
using lab2_kpz.Entities;
using Microsoft.Data.SqlClient;

namespace lab2_kpz.Core.AdoNet;

public sealed class UserRepository(string connectionString)
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async Task<int> Create(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        const string sql = """
            INSERT INTO Users (UserEmail, UserName, UserBirthDate, UserPhoneNumber)
            OUTPUT INSERTED.UserId
            VALUES (@Email, @Name, @BirthDate, @PhoneNumber);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 120) { Value = user.UserEmail });
        command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = user.UserName });
        command.Parameters.Add(new SqlParameter("@BirthDate", SqlDbType.Date)
        {
            Value = user.UserBirthDate.HasValue ? user.UserBirthDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar, 20)
        {
            Value = string.IsNullOrWhiteSpace(user.UserPhoneNumber) ? DBNull.Value : user.UserPhoneNumber
        });

        var result = await command.ExecuteScalarAsync(ct);
        user.UserId = Convert.ToInt32(result);
        return user.UserId;
    }

    public async Task<User?> Read(int userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT UserId, UserEmail, UserName, UserBirthDate, UserPhoneNumber
            FROM Users
            WHERE UserId = @UserId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await reader.ReadAsync(ct))
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<bool> Update(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        const string sql = """
            UPDATE Users
            SET UserName = @Name,
                UserBirthDate = @BirthDate,
                UserPhoneNumber = @PhoneNumber
            WHERE UserId = @UserId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = user.UserId });
        command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = user.UserName });
        command.Parameters.Add(new SqlParameter("@BirthDate", SqlDbType.Date)
        {
            Value = user.UserBirthDate.HasValue ? user.UserBirthDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar, 20)
        {
            Value = string.IsNullOrWhiteSpace(user.UserPhoneNumber) ? DBNull.Value : user.UserPhoneNumber
        });

        int affected = await command.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<bool> Delete(int userId, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM Users
            WHERE UserId = @UserId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

        int affected = await command.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    private static User MapUser(SqlDataReader reader)
    {
        int birthDateOrdinal = reader.GetOrdinal("UserBirthDate");
        int phoneOrdinal = reader.GetOrdinal("UserPhoneNumber");

        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            UserEmail = reader.GetString(reader.GetOrdinal("UserEmail")),
            UserName = reader.GetString(reader.GetOrdinal("UserName")),
            UserBirthDate = reader.IsDBNull(birthDateOrdinal) ? null : DateOnly.FromDateTime(reader.GetDateTime(birthDateOrdinal)),
            UserPhoneNumber = reader.IsDBNull(phoneOrdinal) ? null : reader.GetString(phoneOrdinal)
        };
    }
}
