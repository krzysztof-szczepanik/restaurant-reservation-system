using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Core.Repositories;
using System.Text.RegularExpressions;

namespace RestaurantReservationSystem.Application.Services
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;
        private readonly IConfigurationService _configurationService;

        public TableService(ITableRepository tableRepository, IConfigurationService configurationService)
        {
            _tableRepository = tableRepository;
            _configurationService = configurationService;
        }

        public async Task<IReadOnlyList<Table>> GetAllAsync()
        {
            return await _tableRepository.GetAllAsync();
        }

        public async Task<Table?> GetByIdAsync(int tableId)
        {
            return await _tableRepository.GetByIdAsync(tableId);
        }

        public async Task<OperationResult<Table>> AddAsync(Table table)
        {
            var config = await _configurationService.GetAsync();
            if (config is null)
            {
                return OperationResult<Table>.Failure(OperationErrorCode.TableNotFound, "Konfiguracja początkowa nie została znaleziona."); // Or a generic error
            }

            NormalizeTable(table);

            var validationErrors = ValidateTable(table, config);
            if (validationErrors.Any())
            {
                return OperationResult<Table>.Validation(validationErrors);
            }

            if (!await IsTableUniqueAsync(table, config))
            {
                return OperationResult<Table>.Failure(OperationErrorCode.TableHasReservations, "Kombinacja numeru, oznaczenia i strefy stolika musi być unikalna."); // You might want a dedicated ErrorCode for Uniqueness in the future.
            }

            await _tableRepository.AddAsync(table);
            await _tableRepository.SaveChangesAsync();

            return OperationResult<Table>.Success(table);
        }

        public async Task<OperationResult<Table>> UpdateAsync(Table table)
        {
            var config = await _configurationService.GetAsync();
            if (config is null)
            {
                return OperationResult<Table>.Failure(OperationErrorCode.TableNotFound, "Brak konfiguracji początkowej.");
            }

            NormalizeTable(table);

            var validationErrors = ValidateTable(table, config);
            if (validationErrors.Any())
            {
                return OperationResult<Table>.Validation(validationErrors);
            }

            if (!await IsTableUniqueAsync(table, config))
            {
                return OperationResult<Table>.Failure(OperationErrorCode.TableHasReservations, "Kombinacja numeru, oznaczenia i strefy stolika po edycji musi być unikalna.");
            }

            _tableRepository.Update(table);
            await _tableRepository.SaveChangesAsync();

            return OperationResult<Table>.Success(table);
        }

        public async Task<OperationResult> DeleteAsync(int tableId)
        {
            var table = await _tableRepository.GetByIdAsync(tableId);

            if (table is null)
            {
                return OperationResult.Failure(OperationErrorCode.TableNotFound, "Stolik nie istnieje.");
            }
            if (table.Reservations != null && table.Reservations.Any())
            {
                return OperationResult.Failure(OperationErrorCode.TableHasReservations, "Nie można usunąć stolika powiązanego z rezerwacjami.");
            }

            _tableRepository.Delete(table);
            await _tableRepository.SaveChangesAsync();
            return OperationResult.Success();
        }

        private void NormalizeTable(Table table)
        {
            if (!string.IsNullOrWhiteSpace(table.Label))
            {
                table.Label = table.Label.Trim().ToUpper();
            }
            else
            {
                table.Label = null;
            }
            if (!string.IsNullOrWhiteSpace(table.Zone))
            {
                var zone = table.Zone.Trim();
                zone = Regex.Replace(zone, @"\s+", " ");

                if (zone.Equals("No zone", StringComparison.OrdinalIgnoreCase))
                {
                    table.Zone = null;
                }
                else
                {
                    table.Zone = zone;
                }
            }
            else
            {
                table.Zone = null;
            }
        }

        private List<ValidationError> ValidateTable(Table table, InitialConfiguration config)
        {
            var errors = new List<ValidationError>();

            if (table.Seats <= 0)
            {
                errors.Add(new ValidationError(nameof(table.Seats), "Liczba miejsc (Seats) musi być dodatnią liczbą całkowitą."));
            }
            if (config.TableNumberVisible)
            {
                if (config.TableNumberRequired && table.Number == null)
                {
                    errors.Add(new ValidationError(nameof(table.Number), "Numer stolika jest wymagany."));
                }

                if (table.Number.HasValue && table.Number.Value <= 0)
                {
                    errors.Add(new ValidationError(nameof(table.Number), "Numer stolika musi być dodatnią liczbą całkowitą."));
                }
            }
            if (config.TableLabelVisible)
            {
                if (config.TableLabelRequired && string.IsNullOrEmpty(table.Label))
                {
                    errors.Add(new ValidationError(nameof(table.Label), "Oznaczenie stolika jest wymagane."));
                }

                if (!string.IsNullOrEmpty(table.Label) && !Regex.IsMatch(table.Label, @"^[A-Za-z]+$"))
                {
                    errors.Add(new ValidationError(nameof(table.Label), "Oznaczenie stolika może zawierać wyłącznie litery alfabetu łacińskiego (bez cyfr, spacji i znaków specjalnych)."));
                }
            }
            if (config.TableZoneVisible)
            {
                if (config.TableZoneRequired && string.IsNullOrEmpty(table.Zone))
                {
                    errors.Add(new ValidationError(nameof(table.Zone), "Strefa stolika jest wymagana."));
                }
            }

            return errors;
        }

        private async Task<bool> IsTableUniqueAsync(Table table, InitialConfiguration config)
        {
            var allTables = await _tableRepository.GetAllAsync();
            var otherTables = allTables.Where(t => t.Id != table.Id);

            foreach (var existing in otherTables)
            {
                bool numberMatches = !config.TableNumberVisible || existing.Number == table.Number;

                bool labelMatches = !config.TableLabelVisible || string.Equals(existing.Label, table.Label, StringComparison.OrdinalIgnoreCase);

                bool zoneMatches = !config.TableZoneVisible || string.Equals(existing.Zone, table.Zone, StringComparison.OrdinalIgnoreCase);

                if (numberMatches && labelMatches && zoneMatches)
                {
                    return false;
                }
            }

            return true;
        }
    }
}