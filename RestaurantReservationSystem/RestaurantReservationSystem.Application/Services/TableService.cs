using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Infrastructure.Persistence;
using System.Text.RegularExpressions;

namespace RestaurantReservationSystem.Application.Services {
    public class TableService : ITableService {

        // FIELDS:
        private readonly AppDbContext _context;
        private readonly IConfigurationService _configurationService;


        // CONSTRUCTORS:
        public TableService(AppDbContext context, IConfigurationService configurationService) {
            _context = context;
            _configurationService = configurationService;
        }


        // METHODS:
        public async Task<IReadOnlyList<Table>> GetAllAsync() {
            return await _context.Tables.ToListAsync();
        }

        public async Task<Table?> GetByIdAsync(int tableId) {
            return await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == tableId);
        }

        public async Task<OperationResult<Table>> AddAsync(Table table) {
            ArgumentNullException.ThrowIfNull(table);

            var config = await _configurationService.GetAsync();

            if (config is null) {
                return OperationResult<Table>.Failure(OperationErrorCode.ConfigurationNotFound, "Initial configuration was not found.");
            }

            NormalizeTable(table, config);

            // Clear hidden fields:
            if (!config.TableNumberVisible) table.Number = null;
            if (!config.TableLabelVisible) table.Label = null;
            if (!config.TableZoneVisible) table.Zone = null;

            var validationErrors = ValidateTable(table, config);

            if (validationErrors.Any()) {
                return OperationResult<Table>.Validation(validationErrors);
            }

            if (!await IsTableUniqueAsync(table, config)) {
                return OperationResult<Table>.Validation([ new ValidationError("TableIdentifier", "A table with the same identifier already exists.") ]);
            }

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return OperationResult<Table>.Success(table);
        }

        public async Task<OperationResult<Table>> UpdateAsync(Table table) {
            ArgumentNullException.ThrowIfNull(table);

            var existingTable = await _context.Tables.FindAsync(table.Id);

            if (existingTable is null) {
                return OperationResult<Table>.Failure(OperationErrorCode.TableNotFound, "Table was not found.");
            }

            var config = await _configurationService.GetAsync();

            if (config is null) {
                return OperationResult<Table>.Failure(OperationErrorCode.ConfigurationNotFound, "Initial configuration was not found.");
            }

            NormalizeTable(table, config);

            var validationErrors = ValidateTable(table, config);

            if (validationErrors.Any()) {
                return OperationResult<Table>.Validation(validationErrors);
            }

            if (!await IsTableUniqueAsync(table, config)) {
                return OperationResult<Table>.Validation([ new ValidationError("TableIdentifier", "A table with the same identifier already exists.") ]);
            }

            if (config.TableNumberVisible) existingTable.Number = table.Number;
            if (config.TableLabelVisible) existingTable.Label = table.Label;
            if (config.TableZoneVisible) existingTable.Zone = table.Zone;
            existingTable.Seats = table.Seats;

            await _context.SaveChangesAsync();

            return OperationResult<Table>.Success(existingTable);
        }

        public async Task<OperationResult> DeleteAsync(int tableId) {
            var table = await _context.Tables.FindAsync(tableId);

            if (table is null) {
                return OperationResult.Failure(OperationErrorCode.TableNotFound, "Table was not found.");
            }

            var hasReservations = await _context.Reservations
                .AnyAsync(r => r.Tables.Any(t => t.Id == tableId));

            if (hasReservations) {
                return OperationResult.Failure(OperationErrorCode.TableHasReservations, "Cannot delete a table associated with reservations.");
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return OperationResult.Success();
        }


        // HELPER METHODS:
        private void NormalizeTable(Table table, InitialConfiguration config) {

            // Label:
            if (config.TableLabelVisible) {
                if (!string.IsNullOrWhiteSpace(table.Label)) {
                    table.Label = table.Label.Trim().ToUpperInvariant();
                } else {
                    table.Label = null;
                }
            }

            // Zone:
            if (config.TableZoneVisible) {
                if (!string.IsNullOrWhiteSpace(table.Zone)) {
                    var zone = table.Zone.Trim();
                    zone = Regex.Replace(zone, @"\s+", " ");

                    if (zone.Equals("No zone", StringComparison.OrdinalIgnoreCase)) {
                        table.Zone = null;
                    } else {
                        table.Zone = zone;
                    }
                } else {
                    table.Zone = null;
                }
            }
        }

        private List<ValidationError> ValidateTable(Table table, InitialConfiguration config) {
            var errors = new List<ValidationError>();

            // Seats:
            if (table.Seats <= 0) {
                errors.Add(new ValidationError(nameof(table.Seats), "The number of seats must be a positive integer."));
            }

            // Number:
            if (config.TableNumberVisible) {
                if (config.TableNumberRequired && table.Number is null) {
                    errors.Add(new ValidationError(nameof(table.Number), "Table number is required."));
                }

                if (table.Number is <= 0) {
                    errors.Add(new ValidationError(nameof(table.Number), "Table number must be a positive integer."));
                }
            }

            // Label:
            if (config.TableLabelVisible) {
                if (config.TableLabelRequired && string.IsNullOrEmpty(table.Label)) {
                    errors.Add(new ValidationError(nameof(table.Label), "Table label is required."));
                }

                if (!string.IsNullOrEmpty(table.Label) &&
                    !Regex.IsMatch(table.Label, @"^[A-Za-z]+$")) {

                    errors.Add(new ValidationError(nameof(table.Label), "Table label may contain only Latin letters."));
                }
            }

            // Zone:
            if (config.TableZoneVisible) {
                if (config.TableZoneRequired && string.IsNullOrEmpty(table.Zone)) {
                    errors.Add(new ValidationError(nameof(table.Zone), "Table zone is required."));
                }
            }

            return errors;
        }

        private async Task<bool> IsTableUniqueAsync(Table table, InitialConfiguration config) {
            var otherTables = await _context.Tables
                .Where(t => t.Id != table.Id)
                .ToListAsync();

            foreach (var existing in otherTables) {
                bool numberMatches = !config.TableNumberVisible || existing.Number == table.Number;
                bool labelMatches = !config.TableLabelVisible || string.Equals(existing.Label, table.Label, StringComparison.OrdinalIgnoreCase);
                bool zoneMatches = !config.TableZoneVisible || string.Equals(existing.Zone, table.Zone, StringComparison.OrdinalIgnoreCase);

                if (numberMatches && labelMatches && zoneMatches) {
                    return false;
                }
            }

            return true;
        }
    }
}