using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.Services;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.ManualTests {
    internal class Program {
        static async Task Main(string[] args) {

            // DATABASE:
            const string databasePath = "initial-setup-test.db";

            if (File.Exists(databasePath)) {
                File.Delete(databasePath);
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;

            using var context = new AppDbContext(options);

            // SERVICE:
            var configurationService = new ConfigurationService(context);

            // TEST 1: Database does not exist before Initial Setup
            Console.WriteLine(
                !File.Exists(databasePath)
                    ? "PASS: Database does not exist before Initial Setup."
                    : "FAIL: Database already exists.");

            // TEST 2: Simulate Initial Setup Save
            // Replace this configuration with valid Initial Setup values.
            var configuration = new InitialConfiguration();

            await context.Database.EnsureCreatedAsync();

            var result = await configurationService.SaveInitialAsync(configuration);

            Console.WriteLine(
                result.IsSuccess
                    ? "PASS: Initial Setup saved successfully."
                    : "FAIL: Initial Setup could not be saved.");

            // TEST 3: Database file exists
            Console.WriteLine(
                File.Exists(databasePath)
                    ? "PASS: Database file was created."
                    : "FAIL: Database file was not created.");

            // TEST 4: Required tables exist
            var requiredTables = new[] {
                "InitialConfigurations",
                "Reservations",
                "Tables",
                "ReservationTables"
            };

            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            foreach (var tableName in requiredTables) {
                using var command = connection.CreateCommand();

                command.CommandText = """
                    SELECT COUNT(*)
                    FROM sqlite_master
                    WHERE type = 'table' AND name = $name;
                    """;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "$name";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);

                var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                Console.WriteLine(
                    exists
                        ? $"PASS: Table '{tableName}' exists."
                        : $"FAIL: Table '{tableName}' does not exist.");
            }

            // TEST 5: Configuration was persisted
            var savedConfiguration = await configurationService.GetAsync();

            Console.WriteLine(
                savedConfiguration is not null
                    ? "PASS: Initial configuration exists."
                    : "FAIL: Initial configuration does not exist.");
        }
    }
}