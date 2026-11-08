using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RestaurantReservationSystem.Application.Services
{
    public class ConfigurationService:IConfigurationService
    {
        private readonly IConfigurationRepository _repository;
        public ConfigurationService(IConfigurationRepository repository) {  _repository = repository; }
        public async Task<InitialConfiguration?> GetAsync()
        {
            return await _repository.GetConfigurationAsync();
        }
        public async Task<OperationResult<InitialConfiguration>> SaveInitialAsync(InitialConfiguration configuration)
        {
            var existingConfig = await _repository.GetConfigurationAsync();
            if (existingConfig is not null)
            {
                return OperationResult<InitialConfiguration>.Failure(OperationErrorCode.ConfigurationAlreadyExists, "Konfiguracja już istnieje");
            }

            NormalizeConfiguration(configuration);

            var validationErrors = ValidateConfiguration(configuration);
            if (validationErrors.Any())
            {
                return OperationResult<InitialConfiguration>.Validation(validationErrors);
            }

            configuration.Id = 1;

            await _repository.AddOrUpdateAsync(configuration);
            await _repository.SaveChangesAsync();

            return OperationResult<InitialConfiguration>.Success(configuration);
        }
        private void NormalizeConfiguration(InitialConfiguration configuration)
        {
            if (!configuration.FirstNameVisible) configuration.FirstNameRequired = false;
            if (!configuration.LastNameVisible) configuration.LastNameRequired = false;
            if (!configuration.PhoneNumberVisible) configuration.PhoneNumberRequired = false;

            if (!configuration.TableNumberVisible) configuration.TableNumberRequired = false;
            if (!configuration.TableLabelVisible) configuration.TableLabelRequired = false;
            if (!configuration.TableZoneVisible) configuration.TableZoneRequired = false;
        }

        private List<ValidationError> ValidateConfiguration(InitialConfiguration configuration)
        {
            var errors = new List<ValidationError>();

            if (!Enum.IsDefined(typeof(TableIdentifierOrder), configuration.TableIdentifierOrder))
            {
                errors.Add(new ValidationError(
                    nameof(configuration.TableIdentifierOrder),
                    $"Nieprawidłowa wartość dla {nameof(configuration.TableIdentifierOrder)}."
                ));
            }
            return errors;
        }
    }
}
