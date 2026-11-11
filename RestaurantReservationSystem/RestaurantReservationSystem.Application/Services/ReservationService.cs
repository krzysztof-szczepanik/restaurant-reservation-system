using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Infrastructure.Persistence;
using System.Text.RegularExpressions;

namespace RestaurantReservationSystem.Application.Services {
    public class ReservationService : IReservationService {

        // FIELDS:
        private readonly AppDbContext _context;
        private readonly IConfigurationService _configurationService;


        // CONSTRUCTORS:
        public ReservationService(AppDbContext context, IConfigurationService configurationService) {
            _context = context;
            _configurationService = configurationService;
        }


        // METHODS:
        public async Task<Reservation?> GetByIdAsync(int reservationId) {
            return await _context.Reservations
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task<IReadOnlyList<Reservation>> GetTodayAsync() {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var config = await _configurationService.GetAsync();

            var reservations = await _context.Reservations
                .Include(r => r.Tables)
                .Where(r => r.Date == today)
                .ToListAsync();

            return SortReservations(reservations, config, true);
        }

        public async Task<OperationResult> ValidateFiltersAsync(ReservationFilterCriteria criteria) {
            ArgumentNullException.ThrowIfNull(criteria);

            var errors = ValidateFilterCriteria(criteria, true);

            if (errors.Count > 0) {
                return OperationResult.Validation(errors);
            }

            var config = await _configurationService.GetAsync();

            if (config is null) {
                return OperationResult.Failure(OperationErrorCode.ConfigurationNotFound, "Initial configuration was not found.");
            }

            var selectionErrors = await ValidateFilterSelectionsAsync(criteria, config);

            if (selectionErrors.Count > 0) {
                return OperationResult.Validation(selectionErrors);
            }

            return OperationResult.Success();
        }

        public async Task<OperationResult<IReadOnlyList<Reservation>>> GetFilteredAsync(ReservationFilterCriteria criteria) {
            ArgumentNullException.ThrowIfNull(criteria);

            var errors = ValidateFilterCriteria(criteria, false);

            if (errors.Count > 0) {
                return OperationResult<IReadOnlyList<Reservation>>.Validation(errors);
            }

            var config = await _configurationService.GetAsync();

            if (config is null) {
                return OperationResult<IReadOnlyList<Reservation>>.Failure(OperationErrorCode.ConfigurationNotFound, "Initial configuration was not found.");
            }

            var selectionErrors = await ValidateFilterSelectionsAsync(criteria, config);

            if (selectionErrors.Count > 0) {
                return OperationResult<IReadOnlyList<Reservation>>.Validation(selectionErrors);
            }

            var selectedZones = config.TableZoneVisible ? criteria.Zones
                    .Select(NormalizeZone)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() : [];

            var selectedTableIds = criteria.TableIds
                .Distinct()
                .ToList();

            var query = _context.Reservations
                .AsNoTracking()
                .Include(r => r.Tables)
                .AsQueryable();

            // Date:
            if (criteria.Date.Mode == FilterValueMode.Exact) {
                if (criteria.Date.Exact.HasValue) {
                    query = query.Where(r => r.Date == criteria.Date.Exact.Value);
                }
            } else {
                if (criteria.Date.From.HasValue) {
                    query = query.Where(r => r.Date >= criteria.Date.From.Value);
                }

                if (criteria.Date.To.HasValue) {
                    query = query.Where(r => r.Date <= criteria.Date.To.Value);
                }
            }

            // Time:
            if (criteria.Time.Mode == FilterValueMode.Exact) {
                if (criteria.Time.Exact.HasValue) {
                    query = query.Where(r => r.Time == criteria.Time.Exact.Value);
                }
            } else {
                if (criteria.Time.From.HasValue) {
                    query = query.Where(r => r.Time >= criteria.Time.From.Value);
                }

                if (criteria.Time.To.HasValue) {
                    query = query.Where(r => r.Time <= criteria.Time.To.Value);
                }
            }

            // Guests:
            if (criteria.Guests.Mode == FilterValueMode.Exact) {
                if (criteria.Guests.Exact.HasValue) {
                    query = query.Where(r => r.Guests == criteria.Guests.Exact.Value);
                }
            } else {
                if (criteria.Guests.From.HasValue) {
                    query = query.Where(r => r.Guests >= criteria.Guests.From.Value);
                }

                if (criteria.Guests.To.HasValue) {
                    query = query.Where(r => r.Guests <= criteria.Guests.To.Value);
                }
            }

            var reservations = await query.ToListAsync();

            // Search:
            var phrases = (criteria.SearchText ?? "").Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (phrases.Length > 0) {
                reservations = reservations
                    .Where(r => phrases.All(p => MatchesSearch(r, p, config)))
                    .ToList();
            }

            // Zones:
            if (selectedZones.Count > 0) {
                reservations = reservations
                    .Where(r => {
                        var reservationZones = r.Tables
                            .Select(t => NormalizeZone(t.Zone))
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        return criteria.ZoneMatchMode == FilterMatchMode.And ? selectedZones.All(reservationZones.Contains) : selectedZones.Any(reservationZones.Contains);
                    })
                    .ToList();
            }

            // Tables:
            if (selectedTableIds.Count > 0) {
                reservations = reservations
                    .Where(r => {
                        var reservationTableIds = r.Tables
                            .Select(t => t.Id)
                            .ToHashSet();

                        return criteria.TableMatchMode == FilterMatchMode.And ? selectedTableIds.All(reservationTableIds.Contains) : selectedTableIds.Any(reservationTableIds.Contains);
                    })
                    .ToList();
            }

            var sorted = SortReservations(reservations, config, false);

            return OperationResult<IReadOnlyList<Reservation>>.Success(sorted);
        }

        public async Task<OperationResult<Reservation>> AddAsync(Reservation reservation) {
            ArgumentNullException.ThrowIfNull(reservation);

            var config = await _configurationService.GetAsync();

            if (config is null) {
                return OperationResult<Reservation>.Failure(OperationErrorCode.ConfigurationNotFound, "Initial configuration was not found.");
            }

            NormalizeReservation(reservation, config);

            var validationErrors = ValidateReservationData(reservation, config);

            if (validationErrors.Count > 0) {
                return OperationResult<Reservation>.Validation(validationErrors);
            }

            var tablesResult = await GetAssignedTablesAsync(reservation);

            if (tablesResult.Errors.Count > 0) {
                return OperationResult<Reservation>.Validation(tablesResult.Errors);
            }

            var availabilityErrors = await CheckTableAvailabilityAsync(reservation.Date, reservation.Time, tablesResult.Tables);

            if (availabilityErrors.Count > 0) {
                return OperationResult<Reservation>.Validation(availabilityErrors);
            }

            var warnings = GetCapacityWarnings(reservation.Guests, tablesResult.Tables);

            reservation.Id = 0;
            reservation.CreatedAt = DateTime.Now;
            reservation.Status = ReservationStatus.Expected;
            reservation.Tables = tablesResult.Tables;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return OperationResult<Reservation>.Success(reservation, warnings);
        }

        public async Task<OperationResult<Reservation>> UpdateAsync(Reservation reservation, bool unlockCancelledOrNoShow = false) {
            ArgumentNullException.ThrowIfNull(reservation);

            var existingReservation = await _context.Reservations
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == reservation.Id);

            if (existingReservation is null) {
                return OperationResult<Reservation>.Failure(OperationErrorCode.ReservationNotFound, "Reservation was not found.");
            }

            var isLocked = existingReservation.Status is ReservationStatus.Cancelled or ReservationStatus.NoShow;

            if (isLocked && !unlockCancelledOrNoShow) {
                return OperationResult<Reservation>.Failure(OperationErrorCode.InvalidStatusTransition, "This reservation must be unlocked before editing.");
            }

            var config = await _configurationService.GetAsync();

            if (config is null) {
                return OperationResult<Reservation>.Failure(OperationErrorCode.ConfigurationNotFound, "Initial configuration was not found.");
            }

            NormalizeReservation(reservation, config);

            var validationErrors = ValidateReservationData(reservation, config);

            if (validationErrors.Count > 0) {
                return OperationResult<Reservation>.Validation(validationErrors);
            }

            var tablesResult = await GetAssignedTablesAsync(reservation);

            if (tablesResult.Errors.Count > 0) {
                return OperationResult<Reservation>.Validation(tablesResult.Errors);
            }

            var availabilityErrors = await CheckTableAvailabilityAsync(reservation.Date, reservation.Time, tablesResult.Tables, existingReservation.Id);

            if (availabilityErrors.Count > 0) {
                return OperationResult<Reservation>.Validation(availabilityErrors);
            }

            var warnings = GetCapacityWarnings(reservation.Guests, tablesResult.Tables);

            existingReservation.Date = reservation.Date;
            existingReservation.Time = reservation.Time;
            existingReservation.Guests = reservation.Guests;
            existingReservation.Note = reservation.Note;

            if (config.FirstNameVisible) existingReservation.FirstName = reservation.FirstName;
            if (config.LastNameVisible) existingReservation.LastName = reservation.LastName;
            if (config.PhoneNumberVisible) existingReservation.PhoneNumber = reservation.PhoneNumber;
            existingReservation.Tables = tablesResult.Tables;

            if (isLocked) {
                existingReservation.Status = ReservationStatus.Expected;
            }

            await _context.SaveChangesAsync();

            return OperationResult<Reservation>.Success(existingReservation, warnings);
        }

        public async Task<OperationResult> DeleteAsync(int reservationId) {
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation is null) {
                return OperationResult.Failure(OperationErrorCode.ReservationNotFound, "Reservation was not found.");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return OperationResult.Success();
        }

        public async Task<OperationResult> ChangeStatusAsync(int reservationId, ReservationStatus status) {

            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation is null) {
                return OperationResult.Failure(OperationErrorCode.ReservationNotFound, "Reservation was not found.");
            }

            if (reservation.Status == status) {
                return OperationResult.Success();
            }

            bool currentIsActive = reservation.Status is ReservationStatus.Expected or ReservationStatus.Late or ReservationStatus.Arrived;
            bool newIsActive = status is ReservationStatus.Expected or ReservationStatus.Late or ReservationStatus.Arrived;
            bool currentIsInactive = reservation.Status is ReservationStatus.Cancelled or ReservationStatus.NoShow;
            bool newIsInactive = status is ReservationStatus.Cancelled or ReservationStatus.NoShow;
            bool isValidTransition = (currentIsActive && (newIsActive || newIsInactive)) || (currentIsInactive && newIsInactive);

            if (!isValidTransition) {
                return OperationResult.Failure(OperationErrorCode.InvalidStatusTransition, "Invalid reservation status transition.");
            }

            reservation.Status = status;
            await _context.SaveChangesAsync();

            return OperationResult.Success();
        }

        public async Task RemovePastReservationsAsync() {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var pastReservations = await _context.Reservations
                .Where(r => r.Date < today)
                .ToListAsync();

            if (pastReservations.Count == 0) {
                return;
            }

            _context.Reservations.RemoveRange(pastReservations);
            await _context.SaveChangesAsync();
        }


        // HELPER METHODS:
        private void NormalizeReservation(Reservation reservation, InitialConfiguration config) {
            if (config.FirstNameVisible) {
                reservation.FirstName = NormalizeText(reservation.FirstName);
            } else {
                reservation.FirstName = null;
            }

            if (config.LastNameVisible) {
                reservation.LastName = NormalizeText(reservation.LastName);
            } else {
                reservation.LastName = null;
            }

            if (config.PhoneNumberVisible) {
                reservation.PhoneNumber = string.IsNullOrWhiteSpace(reservation.PhoneNumber) ? null : reservation.PhoneNumber.Replace(" ", "");
            } else {
                reservation.PhoneNumber = null;
            }

            reservation.Note = NormalizeText(reservation.Note);
        }

        private static string? NormalizeText(string? value) {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? NormalizeZone(string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return null;
            }

            var zone = Regex.Replace(value.Trim(), @"\s+", " ");

            return zone.Equals("No zone", StringComparison.OrdinalIgnoreCase) ? null : zone;
        }

        private List<ValidationError> ValidateReservationData(Reservation reservation, InitialConfiguration config) {
            var errors = new List<ValidationError>();

            // Date and Time:
            var now = DateTime.Now;
            var currentMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            var reservationDateTime = reservation.Date.ToDateTime(reservation.Time);

            if (reservationDateTime < currentMinute) {
                errors.Add(new ValidationError(nameof(reservation.Date), "Reservation date and time cannot be in the past."));
            }

            // Guests:
            if (reservation.Guests <= 0) {
                errors.Add(new ValidationError(nameof(reservation.Guests), "The number of guests must be a positive integer."));
            }

            // Tables:
            if (reservation.Tables is null || reservation.Tables.Count == 0) {
                errors.Add(new ValidationError(nameof(reservation.Tables), "At least one table must be assigned to the reservation."));
            }

            // FirstName:
            if (config.FirstNameVisible) {
                if (config.FirstNameRequired && string.IsNullOrEmpty(reservation.FirstName)) {
                    errors.Add(new ValidationError(nameof(reservation.FirstName), "First name is required."));
                }

                if (!string.IsNullOrEmpty(reservation.FirstName) &&
                    !IsValidLatinName(reservation.FirstName, false)) {
                    errors.Add(new ValidationError(nameof(reservation.FirstName), "First name may contain only Latin letters."));
                }
            }

            // LastName:
            if (config.LastNameVisible) {
                if (config.LastNameRequired && string.IsNullOrEmpty(reservation.LastName)) {
                    errors.Add(new ValidationError(nameof(reservation.LastName), "Last name may contain only Latin letters and single hyphens between name parts."));
                }

                if (!string.IsNullOrEmpty(reservation.LastName) &&
                    !IsValidLatinName(reservation.LastName, true)) {
                    errors.Add(new ValidationError(nameof(reservation.LastName), "Last name may contain only Latin letters and single hyphens between name parts."));
                }
            }

            // PhoneNumber:
            if (config.PhoneNumberVisible) {
                if (config.PhoneNumberRequired && string.IsNullOrEmpty(reservation.PhoneNumber)) {
                    errors.Add(new ValidationError(nameof(reservation.PhoneNumber), "Phone number is required."));
                }

                if (!string.IsNullOrEmpty(reservation.PhoneNumber) &&
                    !Regex.IsMatch(reservation.PhoneNumber, @"^[0-9]{9}$")) {
                    errors.Add(new ValidationError(nameof(reservation.PhoneNumber), "Phone number must contain exactly 9 digits."));
                }
            }

            return errors;
        }

        private static bool IsValidLatinName(string value, bool allowHyphens) {
            if (string.IsNullOrEmpty(value)) {
                return false;
            }

            var previousWasHyphen = false;

            for (var i = 0; i < value.Length; i++) {
                var character = value[i];

                if (character == '-' && allowHyphens) {
                    if (i == 0 || i == value.Length - 1 || previousWasHyphen) {
                        return false;
                    }

                    previousWasHyphen = true;
                    continue;
                }

                if (!char.IsLetter(character) ||
                    !Regex.IsMatch(character.ToString(), @"\p{IsBasicLatin}|\p{IsLatin-1Supplement}|\p{IsLatinExtended-A}|\p{IsLatinExtended-B}")) {
                    return false;
                }

                previousWasHyphen = false;
            }

            return true;
        }

        private List<ValidationError> ValidateFilterCriteria(ReservationFilterCriteria criteria, bool validatePastDates) {

            var errors = new List<ValidationError>();
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Date:
            if (criteria.Date.Mode == FilterValueMode.Range) {
                if (criteria.Date.From.HasValue && criteria.Date.To.HasValue &&
                    criteria.Date.From.Value > criteria.Date.To.Value) {

                    errors.Add(new ValidationError(nameof(criteria.Date), "Start date cannot be later than end date."));
                }
            }

            if (validatePastDates) {
                if (criteria.Date.Mode == FilterValueMode.Exact) {
                    if (criteria.Date.Exact.HasValue && criteria.Date.Exact.Value < today) {
                        errors.Add(new ValidationError(nameof(criteria.Date), "Selected date cannot be in the past."));
                    }
                } else {
                    if (criteria.Date.From.HasValue && criteria.Date.From.Value < today) {
                        errors.Add(new ValidationError(nameof(criteria.Date), "Start date cannot be in the past."));
                    }

                    if (criteria.Date.To.HasValue && criteria.Date.To.Value < today) {
                        errors.Add(new ValidationError(nameof(criteria.Date), "End date cannot be in the past."));
                    }
                }
            }

            // Time:
            if (criteria.Time.Mode == FilterValueMode.Range && criteria.Time.From.HasValue && criteria.Time.To.HasValue && criteria.Time.From.Value > criteria.Time.To.Value) {
                errors.Add(new ValidationError(nameof(criteria.Time), "Start time cannot be later than end time."));
            }

            // Guests:
            if (criteria.Guests.Mode == FilterValueMode.Exact) {
                if (criteria.Guests.Exact is <= 0) {
                    errors.Add(new ValidationError(nameof(criteria.Guests), "The number of guests must be positive."));
                }
            } else {
                if (criteria.Guests.From is <= 0 || criteria.Guests.To is <= 0) {
                    errors.Add(new ValidationError(nameof(criteria.Guests), "Guest range values must be positive."));
                }

                if (criteria.Guests.From.HasValue && criteria.Guests.To.HasValue && criteria.Guests.From.Value > criteria.Guests.To.Value) {
                    errors.Add(new ValidationError(nameof(criteria.Guests), "Minimum guests cannot exceed maximum guests."));
                }
            }

            // TableIds:
            if (criteria.TableIds.Any(id => id <= 0)) {
                errors.Add(new ValidationError(nameof(criteria.TableIds), "Selected table identifiers must be positive."));
            }

            return errors;
        }

        private async Task<List<ValidationError>> ValidateFilterSelectionsAsync(ReservationFilterCriteria criteria, InitialConfiguration config) {

            var errors = new List<ValidationError>();

            // Zones:
            if (config.TableZoneVisible && criteria.Zones.Count > 0) {
                var selectedZones = criteria.Zones
                    .Select(NormalizeZone)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var availableZones = await _context.Tables
                    .AsNoTracking()
                    .Select(t => t.Zone)
                    .ToListAsync();

                var normalizedZones = availableZones
                    .Select(NormalizeZone)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (selectedZones.Any(z => !normalizedZones.Contains(z))) {
                    errors.Add(new ValidationError(nameof(criteria.Zones), "One or more selected zones do not exist."));
                }
            }

            // Tables:
            if (criteria.TableIds.Count > 0) {
                var selectedTableIds = criteria.TableIds
                    .Distinct()
                    .ToList();

                if (selectedTableIds.Any(id => id <= 0)) {
                    errors.Add(new ValidationError(nameof(criteria.TableIds), "Selected table identifiers must be positive."));

                    return errors;
                }

                var existingIds = await _context.Tables
                    .AsNoTracking()
                    .Where(t => selectedTableIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                if (existingIds.Count != selectedTableIds.Count) {
                    errors.Add(new ValidationError(nameof(criteria.TableIds), "One or more selected tables do not exist."));
                }
            }

            return errors;
        }

        private async Task<(List<Table> Tables, List<ValidationError> Errors)> GetAssignedTablesAsync(Reservation reservation) {

            var errors = new List<ValidationError>();
            var requestedIds = reservation.Tables?
                .Select(t => t.Id)
                .ToList() ?? [];

            if (requestedIds.Count == 0) {
                errors.Add(new ValidationError(nameof(reservation.Tables), "At least one table must be assigned to the reservation."));

                return ([], errors);
            }

            if (requestedIds.Any(id => id <= 0) || requestedIds.Count != requestedIds.Distinct().Count()) {
                errors.Add(new ValidationError(nameof(reservation.Tables), "Assigned tables contain invalid or duplicate identifiers."));

                return ([], errors);
            }

            var tables = await _context.Tables
                .Where(t => requestedIds.Contains(t.Id))
                .ToListAsync();

            if (tables.Count != requestedIds.Count) {
                errors.Add(new ValidationError(nameof(reservation.Tables), "One or more assigned tables do not exist."));

                return ([], errors);
            }

            return (tables, errors);
        }

        private async Task<List<ValidationError>> CheckTableAvailabilityAsync(DateOnly date, TimeOnly time, IReadOnlyList<Table> tables, int? excludedReservationId = null) {

            var errors = new List<ValidationError>();
            var tableIds = tables.Select(t => t.Id).ToList();

            var conflictingTableIds = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Date == date &&
                    r.Time <= time &&
                    r.Id != excludedReservationId &&
                    (r.Status == ReservationStatus.Expected ||
                     r.Status == ReservationStatus.Late ||
                     r.Status == ReservationStatus.Arrived))
                .SelectMany(r => r.Tables)
                .Where(t => tableIds.Contains(t.Id))
                .Select(t => t.Id)
                .Distinct()
                .ToListAsync();

            foreach (var tableId in conflictingTableIds) {
                errors.Add(new ValidationError(nameof(Reservation.Tables), $"Table (ID: {tableId}) is unavailable at the selected time."));
            }

            return errors;
        }

        private static List<ValidationError> GetCapacityWarnings(int guests, IReadOnlyList<Table> tables) {

            var warnings = new List<ValidationError>();
            var totalSeats = tables.Sum(t => t.Seats);

            if (guests > totalSeats) {
                warnings.Add(new ValidationError(nameof(Reservation.Guests), "The number of guests exceeds the total capacity of the assigned tables."));
            }

            return warnings;
        }

        private static bool MatchesSearch(Reservation reservation, string phrase, InitialConfiguration config) {

            var comparison = StringComparison.OrdinalIgnoreCase;

            if (config.FirstNameVisible && reservation.FirstName?.Contains(phrase, comparison) == true) {
                return true;
            }

            if (config.LastNameVisible && reservation.LastName?.Contains(phrase, comparison) == true) {
                return true;
            }

            if (config.PhoneNumberVisible && reservation.PhoneNumber is not null) {
                var phonePhrase = phrase.Replace(" ", "");

                if (phonePhrase.Length > 0 && reservation.PhoneNumber.StartsWith(phonePhrase, comparison)) {
                    return true;
                }
            }

            if (config.FirstNameVisible && config.LastNameVisible) {
                var parts = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2 &&
                    Regex.IsMatch(parts[0], @"^\p{L}+$") &&
                    Regex.IsMatch(parts[1], @"^\p{L}+$") &&
                    reservation.FirstName?.StartsWith(parts[0], comparison) == true &&
                    reservation.LastName?.StartsWith(parts[1], comparison) == true) {

                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyList<Reservation> SortReservations(IEnumerable<Reservation> reservations, InitialConfiguration? config, bool includeStatus) {
            IOrderedEnumerable<Reservation> ordered;

            if (includeStatus) {
                ordered = reservations
                    .OrderBy(r => r.Status switch {
                        ReservationStatus.Expected or ReservationStatus.Late => 0,
                        ReservationStatus.Arrived => 1,
                        ReservationStatus.Cancelled or ReservationStatus.NoShow => 2,
                        _ => 3
                    })
                    .ThenBy(r => r.Date)
                    .ThenBy(r => r.Time)
                    .ThenBy(r => r.Status switch {
                        ReservationStatus.Expected => 0,
                        ReservationStatus.Late => 1,
                        ReservationStatus.Arrived => 2,
                        ReservationStatus.Cancelled => 3,
                        ReservationStatus.NoShow => 4,
                        _ => 5
                    });
            } else {
                ordered = reservations
                    .OrderBy(r => r.Date)
                    .ThenBy(r => r.Time);
            }

            if (config?.FirstNameVisible == true) {
                ordered = ordered
                    .ThenBy(r => r.FirstName is null)
                    .ThenBy(r => r.FirstName, StringComparer.OrdinalIgnoreCase);
            }

            if (config?.LastNameVisible == true) {
                ordered = ordered
                    .ThenBy(r => r.LastName is null)
                    .ThenBy(r => r.LastName, StringComparer.OrdinalIgnoreCase);
            }

            return ordered
                .ThenByDescending(r => r.Guests)
                .ThenBy(r => r.CreatedAt)
                .ThenBy(r => r.Id)
                .ToList();
        }
    }
}