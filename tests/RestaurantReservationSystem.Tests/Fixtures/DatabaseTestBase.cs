using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.Tests.Fixtures {
    public abstract class DatabaseTestBase : IDisposable {

        // FIELDS:
        private readonly SqliteConnection _connection;
        protected readonly AppDbContext _context;
        protected readonly IDbContextFactory<AppDbContext> _contextFactory;


        // CONSTRUCTORS:
        protected DatabaseTestBase() {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _contextFactory = new TestDbContextFactory(options);
            _context = _contextFactory.CreateDbContext();

            _context.Database.Migrate();
        }


        // METHODS:
        public void Dispose() {
            _context.Dispose();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }


        // HELPER CLASSES:
        private sealed class TestDbContextFactory : IDbContextFactory<AppDbContext> {

            private readonly DbContextOptions<AppDbContext> _options;

            public TestDbContextFactory(DbContextOptions<AppDbContext> options) {
                _options = options;
            }

            public AppDbContext CreateDbContext() {
                return new AppDbContext(_options);
            }
        }
    }
}