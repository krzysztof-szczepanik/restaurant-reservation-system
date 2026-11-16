using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.Application.Services {
    public class ConfigurationService : IConfigurationService {

        // FIELDS:
        private const int ConfigurationId = 1;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;


        // CONSTRUCTORS:
        public ConfigurationService(IDbContextFactory<AppDbContext> contextFactory) {
            _contextFactory = contextFactory;
        }


        // METHODS:
        public async Task<InitialConfiguration?> GetAsync() {
            await using var context = await _contextFactory.CreateDbContextAsync();

            if (!await context.Database.CanConnectAsync()) {
                return null;
            }

            return await context.InitialConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == ConfigurationId);
        }

        public async Task<OperationResult<InitialConfiguration>> SaveInitialAsync(InitialConfiguration configuration) {
            ArgumentNullException.ThrowIfNull(configuration);

            var validationErrors = ValidateConfiguration(configuration);

            if (validationErrors.Count > 0) {
                return OperationResult<InitialConfiguration>.Validation(validationErrors);
            }

            var existingConfig = await GetAsync();

            if (existingConfig is not null) {
                return OperationResult<InitialConfiguration>.Failure(OperationErrorCode.ConfigurationAlreadyExists, "Configuration already exists!");
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            await context.Database.MigrateAsync();

            configuration.Id = ConfigurationId;

            await context.InitialConfigurations.AddAsync(configuration);
            await context.SaveChangesAsync();

            return OperationResult<InitialConfiguration>.Success(configuration);
        }


        // HELPER METHODS:
        private List<ValidationError> ValidateConfiguration(InitialConfiguration configuration) {
            var errors = new List<ValidationError>();

            // Customer fields:
            if (!(configuration.FirstNameVisible && configuration.FirstNameRequired) &&
                !(configuration.LastNameVisible && configuration.LastNameRequired) &&
                !(configuration.PhoneNumberVisible && configuration.PhoneNumberRequired)) {

                errors.Add(new ValidationError("ReservationFields", "At least one customer field must be visible and required."));
            }

            // Table identifier fields:
            if (!(configuration.TableNumberVisible && configuration.TableNumberRequired) &&
                !(configuration.TableLabelVisible && configuration.TableLabelRequired)) {

                errors.Add(new ValidationError("TableFields", "At least one table identifier field (Number or Label) must be visible and required."));
            }

            // Table identifier order:
            if (!Enum.IsDefined(typeof(TableIdentifierOrder), configuration.TableIdentifierOrder)) {
                errors.Add(new ValidationError(nameof(configuration.TableIdentifierOrder), "Invalid table identifier order."));
            }

            return errors;
        }
    }
}