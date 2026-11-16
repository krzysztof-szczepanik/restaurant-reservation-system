using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Application.Services;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Tests.Fixtures;

namespace RestaurantReservationSystem.Tests.Services {
    public class ConfigurationServiceTests : DatabaseTestBase {

        // FIELDS:
        private readonly ConfigurationService _configurationService;


        // CONSTRUCTORS:
        public ConfigurationServiceTests() {
            _configurationService = new ConfigurationService(_contextFactory);
        }


        // METHODS:
        [Fact]
        public async Task SaveInitialAsync_ValidConfiguration_SavesToDatabaseAndReturnsSuccess() {
            var config = new InitialConfiguration {
                FirstNameVisible = true,
                FirstNameRequired = true,
                TableNumberVisible = true,
                TableNumberRequired = true
            };

            var result = await _configurationService.SaveInitialAsync(config);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(1, await _context.InitialConfigurations.CountAsync());
        }

        [Fact]
        public async Task SaveInitialAsync_ConfigurationAlreadyExists_ReturnsFailure() {
            _context.InitialConfigurations.Add(new InitialConfiguration { Id = 1 });
            await _context.SaveChangesAsync();

            var newConfig = new InitialConfiguration {
                FirstNameVisible = true,
                FirstNameRequired = true,
                TableNumberVisible = true,
                TableNumberRequired = true
            };

            var result = await _configurationService.SaveInitialAsync(newConfig);

            Assert.False(result.IsSuccess);
            Assert.Equal(OperationErrorCode.ConfigurationAlreadyExists, result.ErrorCode);
        }

        [Fact]
        public async Task SaveInitialAsync_NoCustomerFieldsSelected_ReturnsValidationError() {
            var config = new InitialConfiguration {
                FirstNameVisible = false,
                LastNameVisible = false,
                PhoneNumberVisible = false,

                TableNumberVisible = true,
                TableNumberRequired = true
            };

            var result = await _configurationService.SaveInitialAsync(config);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.ValidationErrors, e => e.FieldName == "ReservationFields");
            Assert.Equal(0, await _context.InitialConfigurations.CountAsync());
        }

        [Fact]
        public async Task SaveInitialAsync_NoTableFieldsSelected_ReturnsValidationError() {
            var config = new InitialConfiguration {
                FirstNameVisible = true,
                FirstNameRequired = true,

                TableNumberVisible = true,
                TableNumberRequired = false,
                TableLabelVisible = false
            };
            var result = await _configurationService.SaveInitialAsync(config);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.ValidationErrors, e => e.FieldName == "TableFields");
            Assert.Equal(0, await _context.InitialConfigurations.CountAsync());
        }

        [Fact]
        public async Task GetAsync_WhenConfigurationExists_ReturnsConfiguration() {
            var config = new InitialConfiguration { Id = 1, FirstNameRequired = true };
            _context.InitialConfigurations.Add(config);
            await _context.SaveChangesAsync();

            var result = await _configurationService.GetAsync();

            Assert.NotNull(result);
            Assert.True(result.FirstNameRequired);
        }

        [Fact]
        public async Task GetAsync_WhenDatabaseIsEmpty_ReturnsNull() {
            var result = await _configurationService.GetAsync();

            Assert.Null(result);
        }
    }
}