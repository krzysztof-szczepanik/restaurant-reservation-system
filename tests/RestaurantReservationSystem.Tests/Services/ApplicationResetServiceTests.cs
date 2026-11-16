using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.Services;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Tests.Fixtures;

namespace RestaurantReservationSystem.Tests.Services {
    public class ApplicationResetServiceTests : DatabaseTestBase {

        // FIELDS:
        private readonly ApplicationResetService _resetService;


        // CONSTRUCTORS:
        public ApplicationResetServiceTests() {
            _resetService = new ApplicationResetService(_contextFactory);
        }


        // METHODS:
        [Fact]
        public async Task ResetAsync_WhenCalled_RemovesAllDataAndAllowsReconfiguration() {
            var table = new Table { Seats = 4, Number = 1 };
            var reservation = new Reservation { Guests = 2, Date = DateOnly.FromDateTime(DateTime.Now), Time = new TimeOnly(12, 0) };
            reservation.Tables.Add(table);

            _context.Tables.Add(table);
            _context.Reservations.Add(reservation);
            _context.InitialConfigurations.Add(new InitialConfiguration { Id = 1 });
            await _context.SaveChangesAsync();

            await _resetService.ResetAsync();

            var tablesCount = await _context.Tables.CountAsync();
            var reservationsCount = await _context.Reservations.CountAsync();
            var configsCount = await _context.InitialConfigurations.CountAsync();

            Assert.Equal(0, tablesCount);
            Assert.Equal(0, reservationsCount);
            Assert.Equal(0, configsCount);

            // Verify that the application can be configured again after reset:
            var configurationService = new ConfigurationService(_contextFactory);

            var newConfig = new InitialConfiguration {
                FirstNameVisible = true,
                FirstNameRequired = true,
                TableNumberVisible = true,
                TableNumberRequired = true
            };

            var result = await configurationService.SaveInitialAsync(newConfig);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, await _context.InitialConfigurations.CountAsync());
        }
    }
}