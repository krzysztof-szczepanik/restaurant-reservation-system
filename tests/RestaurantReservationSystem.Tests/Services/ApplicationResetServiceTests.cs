using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.Services;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Tests.Fixtures;
using Xunit;

namespace RestaurantReservationSystem.Tests.Services
{
    public class ApplicationResetServiceTests : DatabaseTestBase
    {

        private readonly ApplicationResetService _resetService;

        public ApplicationResetServiceTests()
        {
            _resetService = new ApplicationResetService(Context);
        }

        [Fact]
        public async Task ResetAsync_WhenCalled_RemovesAllDataExceptSchema()
        {
            var table = new Table { Seats = 4, Number = 1 };
            var reservation = new Reservation { Guests = 2, Date = DateOnly.FromDateTime(DateTime.Now), Time = new TimeOnly(12, 0) };
            reservation.Tables.Add(table);

            Context.Tables.Add(table);
            Context.Reservations.Add(reservation);
            Context.InitialConfigurations.Add(new InitialConfiguration { Id = 1 });
            await Context.SaveChangesAsync();

            await _resetService.ResetAsync();

            var tablesCount = await Context.Tables.CountAsync();
            var reservationsCount = await Context.Reservations.CountAsync();
            var configsCount = await Context.InitialConfigurations.CountAsync();

            Assert.Equal(0, tablesCount);
            Assert.Equal(0, reservationsCount);
            Assert.Equal(0, configsCount);
        }
    }
}