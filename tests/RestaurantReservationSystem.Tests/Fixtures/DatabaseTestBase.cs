using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.Tests.Fixtures
{
    public abstract class DatabaseTestBase : IDisposable
    {

        private readonly SqliteConnection _connection;
        protected readonly AppDbContext Context;

        protected DatabaseTestBase()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new AppDbContext(options);
            Context.Database.Migrate();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}