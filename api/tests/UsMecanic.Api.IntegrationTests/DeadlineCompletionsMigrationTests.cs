using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Api.IntegrationTests;

/// <summary>
/// Migration de reprise des réalisations d'échéance en interventions, testée sur une base jetable :
/// schéma migré jusqu'à AddInterventions, données insérées en SQL, puis migration cible appliquée.
/// </summary>
public sealed class DeadlineCompletionsMigrationTests : IAsyncLifetime
{
    private const string BeforeMigration = "20261028100000_AddInterventions";
    private const string TargetMigration = "20261028110000_MigrateDeadlineCompletionsToInterventions";

    private readonly string _connectionString;

    public DeadlineCompletionsMigrationTests()
    {
        var server = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
            ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres";
        _connectionString = new NpgsqlConnectionStringBuilder(server) { Database = $"us_mecanic_test_{Guid.NewGuid():N}" }.ConnectionString;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var database = builder.Database;
        builder.Database = "postgres";
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed record Row(Guid VehicleId, DateOnly Date, int? Km, Guid? GarageId, string Work, DateTimeOffset RecordedAt, int Links);

    private async Task MigrateAsync(string target)
    {
        var options = new DbContextOptionsBuilder<UsMecanicDbContext>().UseNpgsql(_connectionString).Options;
        await using var context = new UsMecanicDbContext(options);
        await context.GetService<IMigrator>().MigrateAsync(target);
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql, params (string Name, object Value)[] parameters)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task Chaque_realisation_devient_une_intervention_liee_a_son_echeance_puis_la_table_est_supprimee()
    {
        await MigrateAsync(BeforeMigration);

        var vehicleId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var deadlineId = Guid.NewGuid();
        var withComment = Guid.NewGuid();
        var withoutComment = Guid.NewGuid();
        var blankComment = Guid.NewGuid();
        var recordedAt = new DateTimeOffset(2026, 10, 20, 8, 30, 0, TimeSpan.Zero);

        await using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await ExecuteAsync(
                connection,
                "INSERT INTO vehicles (id, license_plate, status, created_at, updated_at) VALUES (@id, 'AB-123-CD', 'Active', @now, @now)",
                ("id", vehicleId),
                ("now", recordedAt));
            await ExecuteAsync(
                connection,
                "INSERT INTO deadline_types (id, name, is_active) VALUES (@id, 'Type de test migration', true)",
                ("id", typeId));
            await ExecuteAsync(
                connection,
                "INSERT INTO vehicle_deadlines (id, vehicle_id, deadline_type_id, is_closed) VALUES (@id, @vehicle, @type, false)",
                ("id", deadlineId),
                ("vehicle", vehicleId),
                ("type", typeId));
            const string insertCompletion =
                "INSERT INTO deadline_completions (id, vehicle_deadline_id, date, kilometers, comment, recorded_at) VALUES (@id, @deadline, @date, @km, @comment, @recorded)";
            await ExecuteAsync(
                connection,
                insertCompletion,
                ("id", withComment),
                ("deadline", deadlineId),
                ("date", new DateOnly(2026, 9, 1)),
                ("km", 12_000),
                ("comment", " Plaquettes changées "),
                ("recorded", recordedAt));
            await ExecuteAsync(
                connection,
                insertCompletion,
                ("id", withoutComment),
                ("deadline", deadlineId),
                ("date", new DateOnly(2026, 8, 1)),
                ("km", DBNull.Value),
                ("comment", DBNull.Value),
                ("recorded", recordedAt));
            await ExecuteAsync(
                connection,
                insertCompletion,
                ("id", blankComment),
                ("deadline", deadlineId),
                ("date", new DateOnly(2026, 7, 1)),
                ("km", 9_000),
                ("comment", "   "),
                ("recorded", recordedAt));
        }

        await MigrateAsync(TargetMigration);

        await using var check = new NpgsqlConnection(_connectionString);
        await check.OpenAsync();
        var rows = new Dictionary<Guid, Row>();
        await using (var command = new NpgsqlCommand(
            """
            SELECT i.id, i.vehicle_id, i.date, i.kilometers, i.garage_id, i.work, i.recorded_at,
                   (SELECT COUNT(*) FROM intervention_deadlines l WHERE l.intervention_id = i.id AND l.vehicle_deadline_id = @deadline)
            FROM interventions i
            """,
            check))
        {
            command.Parameters.AddWithValue("deadline", deadlineId);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows[reader.GetGuid(0)] = new Row(
                    reader.GetGuid(1),
                    reader.GetFieldValue<DateOnly>(2),
                    reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetGuid(4),
                    reader.GetString(5),
                    reader.GetFieldValue<DateTimeOffset>(6),
                    (int)reader.GetInt64(7));
            }
        }

        Assert.Equal(3, rows.Count);
        Assert.Equal(new Row(vehicleId, new DateOnly(2026, 9, 1), 12_000, null, "Plaquettes changées", recordedAt, 1), rows[withComment]);
        Assert.Equal(new Row(vehicleId, new DateOnly(2026, 8, 1), null, null, "Type de test migration", recordedAt, 1), rows[withoutComment]);
        Assert.Equal(new Row(vehicleId, new DateOnly(2026, 7, 1), 9_000, null, "Type de test migration", recordedAt, 1), rows[blankComment]);

        await using var exists = new NpgsqlCommand("SELECT to_regclass('public.deadline_completions') IS NULL", check);
        Assert.True((bool)(await exists.ExecuteScalarAsync())!);
    }
}
