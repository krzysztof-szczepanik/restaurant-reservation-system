using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Application.Services;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Infrastructure.Persistence;
using RestaurantReservationSystem.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantReservationSystem.Tests.Services
{
    public class ReservationServiceTests : DatabaseTestBase
    {
        private readonly ReservationService _reservationService;
        private readonly ConfigurationService _configurationService;
        private class TestDbContextFactory : IDbContextFactory<AppDbContext>
        {
            private readonly DbContextOptions<AppDbContext> _options;
            public TestDbContextFactory(DbContextOptions<AppDbContext> options) => _options = options;
            public AppDbContext CreateDbContext() => new AppDbContext(_options);
        }

        public ReservationServiceTests()
        {
            _configurationService = new ConfigurationService(Context);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(Context.Database.GetDbConnection())
                .Options;

            var factory = new TestDbContextFactory(options);

            _reservationService = new ReservationService(Context, _configurationService, factory);
        }

        private async Task SeedInitialConfigurationAsync()
        {
            var config = new InitialConfiguration
            {
                FirstNameVisible = true,
                FirstNameRequired = true,
                TableNumberVisible = true,
                TableNumberRequired = true,
                PhoneNumberVisible = false,
                LastNameVisible = false
            };
            await _configurationService.SaveInitialAsync(config);
        }

        [Fact]
        public async Task AddAsync_ValidReservation_SavesToDatabaseAndReturnsSuccess()
        {
            await SeedInitialConfigurationAsync();

            var table = new Table { Number = 1, Seats = 4 };
            Context.Tables.Add(table);
            await Context.SaveChangesAsync();

            var reservation = new Reservation
            {
                FirstName = "Jan",
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Time = new TimeOnly(18, 0),
                Guests = 2,
                Tables = new List<Table> { table }
            };

            var result = await _reservationService.AddAsync(reservation);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, await Context.Reservations.CountAsync());
            Assert.Equal(ReservationStatus.Expected, result.Data!.Status);
        }

        [Fact]
        public async Task AddAsync_TableAlreadyBooked_ReturnsAvailabilityError()
        {
            await SeedInitialConfigurationAsync();

            var table = new Table { Number = 2, Seats = 4 };
            Context.Tables.Add(table);

            var existingRes = new Reservation
            {
                FirstName = "Anna",
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Time = new TimeOnly(18, 0),
                Guests = 2,
                Status = ReservationStatus.Expected,
                Tables = new List<Table> { table }
            };
            Context.Reservations.Add(existingRes);
            await Context.SaveChangesAsync();

            var conflictingRes = new Reservation
            {
                FirstName = "Piotr",
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Time = new TimeOnly(18, 0),
                Guests = 2,
                Tables = new List<Table> { table }
            };

            var result = await _reservationService.AddAsync(conflictingRes);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.ValidationErrors, e => e.FieldName == "Tables");
            Assert.Equal(1, await Context.Reservations.CountAsync());
        }

        [Fact]
        public async Task GetFilteredAsync_UsesDbContextFactory_AndFiltersByDate()
        {
            await SeedInitialConfigurationAsync();

            var table = new Table { Number = 3, Seats = 2 };
            Context.Tables.Add(table);

            var targetDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));

            Context.Reservations.Add(new Reservation
            {
                FirstName = "Test1",
                Date = targetDate,
                Time = new TimeOnly(12, 0),
                Guests = 1,
                Tables = new List<Table> { table }
            });

            Context.Reservations.Add(new Reservation
            {
                FirstName = "Test2",
                Date = targetDate.AddDays(1),
                Time = new TimeOnly(12, 0),
                Guests = 1,
                Tables = new List<Table> { table }
            });

            await Context.SaveChangesAsync();

            var criteria = new ReservationFilterCriteria
            {
                Date = new ValueFilter<DateOnly> { Mode = FilterValueMode.Exact, Exact = targetDate },
                Time = new ValueFilter<TimeOnly>(),
                Guests = new ValueFilter<int>()
            };

            var result = await _reservationService.GetFilteredAsync(criteria);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data!);
            Assert.Equal("Test1", result.Data![0].FirstName);
        }
        [Fact]
        public async Task ChangeStatusAsync_ValidTransition_ReturnsSuccess()
        {
            var table = new Table { Number = 4, Seats = 2 };
            Context.Tables.Add(table);

            var reservation = new Reservation
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Time = new TimeOnly(12, 0),
                Guests = 2,
                Status = ReservationStatus.Expected,
                Tables = new List<Table> { table }
            };
            Context.Reservations.Add(reservation);
            await Context.SaveChangesAsync();

            var result = await _reservationService.ChangeStatusAsync(reservation.Id, ReservationStatus.Arrived);

            Assert.True(result.IsSuccess);
            var updatedReservation = await Context.Reservations.FindAsync(reservation.Id);
            Assert.Equal(ReservationStatus.Arrived, updatedReservation!.Status);
        }

        [Fact]
        public async Task ChangeStatusAsync_InvalidTransition_ReturnsFailure()
        {
            var table = new Table { Number = 5, Seats = 2 };
            Context.Tables.Add(table);

            var reservation = new Reservation
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Time = new TimeOnly(12, 0),
                Guests = 2,
                Status = ReservationStatus.Cancelled,
                Tables = new List<Table> { table }
            };
            Context.Reservations.Add(reservation);
            await Context.SaveChangesAsync();

            var result = await _reservationService.ChangeStatusAsync(reservation.Id, ReservationStatus.Expected);

            Assert.False(result.IsSuccess);
            Assert.Equal(OperationErrorCode.InvalidStatusTransition, result.ErrorCode);
        }

        [Fact]
        public async Task RemovePastReservationsAsync_RemovesOnlyPastDates()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var pastDate = today.AddDays(-1);
            var futureDate = today.AddDays(1);

            Context.Reservations.Add(new Reservation { Date = pastDate, Time = new TimeOnly(12, 0), Guests = 1 });
            Context.Reservations.Add(new Reservation { Date = today, Time = new TimeOnly(12, 0), Guests = 1 });
            Context.Reservations.Add(new Reservation { Date = futureDate, Time = new TimeOnly(12, 0), Guests = 1 });
            await Context.SaveChangesAsync();

            await _reservationService.RemovePastReservationsAsync();

            var remainingReservations = await Context.Reservations.ToListAsync();
            Assert.Equal(2, remainingReservations.Count);
            Assert.DoesNotContain(remainingReservations, r => r.Date == pastDate);
        }
        [Fact]
        public async Task UpdateAsync_ValidReservation_UpdatesData()
        {
            await SeedInitialConfigurationAsync();
            var table = new Table { Number = 6, Seats = 4 };
            Context.Tables.Add(table);

            var reservation = new Reservation
            {
                FirstName = "Adam",
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                Time = new TimeOnly(15, 0),
                Guests = 2,
                Tables = new List<Table> { table }
            };
            Context.Reservations.Add(reservation);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var updatedReservation = new Reservation
            {
                Id = reservation.Id,
                FirstName = "AdamZaktualizowany",
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                Time = new TimeOnly(16, 0),
                Guests = 3,
                Tables = new List<Table> { table }
            };

            var result = await _reservationService.UpdateAsync(updatedReservation);

            Assert.True(result.IsSuccess);
            var dbReservation = await Context.Reservations.FindAsync(reservation.Id);
            Assert.Equal("AdamZaktualizowany", dbReservation!.FirstName);
            Assert.Equal(3, dbReservation.Guests);
        }

        [Fact]
        public async Task UpdateAsync_LockedReservationWithoutUnlockFlag_ReturnsFailure()
        {
            await SeedInitialConfigurationAsync();
            var table = new Table { Number = 7, Seats = 2 };
            Context.Tables.Add(table);

            var reservation = new Reservation
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Time = new TimeOnly(12, 0),
                Guests = 2,
                Status = ReservationStatus.Cancelled,
                Tables = new List<Table> { table }
            };
            Context.Reservations.Add(reservation);
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();

            var result = await _reservationService.UpdateAsync(reservation, unlockCancelledOrNoShow: false);

            Assert.False(result.IsSuccess);
            Assert.Equal(OperationErrorCode.InvalidStatusTransition, result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_ExistingReservation_RemovesFromDatabase()
        {
            var reservation = new Reservation { Date = DateOnly.FromDateTime(DateTime.Now), Time = new TimeOnly(12, 0), Guests = 1 };
            Context.Reservations.Add(reservation);
            await Context.SaveChangesAsync();

            var result = await _reservationService.DeleteAsync(reservation.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, await Context.Reservations.CountAsync());
        }

        [Fact]
        public async Task AddAsync_PastDate_ReturnsValidationError()
        {
            await SeedInitialConfigurationAsync();
            var table = new Table { Number = 8, Seats = 2 };
            Context.Tables.Add(table);

            var reservation = new Reservation
            {
                FirstName = "Test",
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                Time = new TimeOnly(12, 0),
                Guests = 2,
                Tables = new List<Table> { table }
            };

            var result = await _reservationService.AddAsync(reservation);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.ValidationErrors, e => e.FieldName == "Date");
            Assert.Equal(0, await Context.Reservations.CountAsync());
        }
    }
}