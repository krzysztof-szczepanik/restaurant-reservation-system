using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Core.Repositories;
using System.Text.RegularExpressions;
namespace RestaurantReservationSystem.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IConfigurationService _configurationService;
        private readonly ITableService _tableService;

        public ReservationService(IReservationRepository reservationRepository, IConfigurationService configurationService, ITableService tableService)
        {
            _reservationRepository = reservationRepository;
            _configurationService = configurationService;
            _tableService = tableService;
        }
        public async Task<Reservation?> GetByIdAsync(int reservationId)
        {
            return await _reservationRepository.GetByIdAsync(reservationId);
        }
        public async Task<IReadOnlyList<Reservation>> GetTodayAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var criteria = new ReservationFilterCriteria
            {
                Date = new ValueFilter<DateOnly>
                {
                    Mode = FilterValueMode.Exact,
                    Exact = today
                }
            };
            var result = await GetFilteredAsync(criteria);
            return result.IsSuccess ? result.Data! : new List<Reservation>();
        }
        public async Task<OperationResult<IReadOnlyList<Reservation>>> GetFilteredAsync(ReservationFilterCriteria criteria){
            DateTime? searchDate = null;
            if (criteria.Date.Mode == FilterValueMode.Exact && criteria.Date.Exact.HasValue)
            {
                searchDate = criteria.Date.Exact.Value.ToDateTime(TimeOnly.MinValue);
            }
            var reservations = await _reservationRepository.SearchAndFilterAsync(
                date: searchDate,
                status: null,
                searchTerm: criteria.SearchText);
            return OperationResult<IReadOnlyList<Reservation>>.Success(reservations);
        }
        public Task<OperationResult> ValidateFiltersAsync(ReservationFilterCriteria criteria)
        {
            var errors = new List<ValidationError>();
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (criteria.Date.Mode == FilterValueMode.Exact)
            {
                if (criteria.Date.Exact.HasValue && criteria.Date.Exact.Value < today)
                {
                    errors.Add(new ValidationError(nameof(criteria.Date), "Wybrana data nie może być z przeszłości."));
                }
            }
            else if (criteria.Date.Mode == FilterValueMode.Range)
            {
                if (criteria.Date.From.HasValue && criteria.Date.From.Value < today)
                {
                    errors.Add(new ValidationError(nameof(criteria.Date), "Data początkowa nie może być z przeszłości."));
                }

                if (criteria.Date.To.HasValue && criteria.Date.To.Value < today)
                {
                    errors.Add(new ValidationError(nameof(criteria.Date), "Data końcowa nie może być z przeszłości."));
                }

                if (criteria.Date.From.HasValue && criteria.Date.To.HasValue && criteria.Date.From.Value > criteria.Date.To.Value)
                {
                    errors.Add(new ValidationError(nameof(criteria.Date), "Data początkowa nie może być późniejsza niż data końcowa."));
                }
            }
            if (criteria.Time.Mode == FilterValueMode.Range && criteria.Time.From.HasValue && criteria.Time.To.HasValue)
            {
                if (criteria.Time.From.Value > criteria.Time.To.Value)
                {
                    errors.Add(new ValidationError(nameof(criteria.Time), "Godzina początkowa nie może być późniejsza niż godzina końcowa."));
                }
            }
            if (criteria.Guests.Mode == FilterValueMode.Range && criteria.Guests.From.HasValue && criteria.Guests.To.HasValue)
            {
                if (criteria.Guests.From.Value > criteria.Guests.To.Value)
                {
                    errors.Add(new ValidationError(nameof(criteria.Guests), "Minimalna liczba gości nie może być większa niż maksymalna."));
                }
            }
            if (errors.Count > 0)
            {
                return Task.FromResult(OperationResult.Validation(errors));
            }
            return Task.FromResult(OperationResult.Success());
        }
        public async Task<OperationResult<Reservation>> AddAsync(Reservation reservation)
        {
            var config = await _configurationService.GetAsync();
            if (config is null)
            {
                return OperationResult<Reservation>.Validation(new List<ValidationError>
        {
            new ValidationError("Configuration", "Brak konfiguracji początkowej systemu.")
        });
            }

            NormalizeReservation(reservation);

            var validationErrors = ValidateReservationData(reservation, config);
            if (validationErrors.Any())
            {
                return OperationResult<Reservation>.Validation(validationErrors);
            }

            var availabilityErrors = await CheckTableAvailabilityAsync(reservation);
            if (availabilityErrors.Any())
            {
                return OperationResult<Reservation>.Validation(availabilityErrors);
            }

            reservation.Status = ReservationStatus.Expected;
            reservation.CreatedAt = DateTime.Now;

            await _reservationRepository.AddAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            var result = OperationResult<Reservation>.Success(reservation);

            if (await HasCapacityWarningAsync(reservation))
            {
                if (result.Warnings is IList<ValidationError> validationWarnings)
                {
                    validationWarnings.Add(new ValidationError("Guests", "Liczba gości przekracza łączną pojemność przypisanych stolików."));
                }
                else if (result.Warnings is IList<string> stringWarnings)
                {
                    stringWarnings.Add("Liczba gości przekracza łączną pojemność przypisanych stolików.");
                }
            }

            return result;
        }
        public async Task<OperationResult<Reservation>> UpdateAsync(Reservation reservation, bool unlockCancelledOrNoShow = false)
        {
            var config = await _configurationService.GetAsync();
            if (config is null)
            {
                return OperationResult<Reservation>.Validation(new List<ValidationError>
        {
            new ValidationError("Configuration", "Brak konfiguracji początkowej systemu.")
        });
            }

            var existingReservation = await _reservationRepository.GetByIdAsync(reservation.Id);
            if (existingReservation is null)
            {
                return OperationResult<Reservation>.Failure(OperationErrorCode.ReservationNotFound, "Nie znaleziono rezerwacji.");
            }

            if (existingReservation.Status == ReservationStatus.Cancelled || existingReservation.Status == ReservationStatus.NoShow)
            {
                if (!unlockCancelledOrNoShow)
                {
                    return OperationResult<Reservation>.Failure(OperationErrorCode.InvalidStatusTransition, "Edycja anulowanej rezerwacji wymaga odblokowania.");
                }

                existingReservation.Status = ReservationStatus.Expected;
            }

            NormalizeReservation(reservation);

            var validationErrors = ValidateReservationData(reservation, config);
            if (validationErrors.Any())
            {
                return OperationResult<Reservation>.Validation(validationErrors);
            }

            var availabilityErrors = await CheckTableAvailabilityAsync(reservation, existingReservation.Id);
            if (availabilityErrors.Any())
            {
                return OperationResult<Reservation>.Validation(availabilityErrors);
            }

            existingReservation.Date = reservation.Date;
            existingReservation.Time = reservation.Time;
            existingReservation.FirstName = reservation.FirstName;
            existingReservation.LastName = reservation.LastName;
            existingReservation.PhoneNumber = reservation.PhoneNumber;
            existingReservation.Guests = reservation.Guests;
            existingReservation.Note = reservation.Note;
            existingReservation.Tables = reservation.Tables;

            _reservationRepository.Update(existingReservation);
            await _reservationRepository.SaveChangesAsync();

            var result = OperationResult<Reservation>.Success(existingReservation);

            if (await HasCapacityWarningAsync(existingReservation))
            {
                if (result.Warnings is IList<ValidationError> validationWarnings)
                {
                    validationWarnings.Add(new ValidationError("Guests", "Liczba gości przekracza łączną pojemność przypisanych stolików."));
                }
                else if (result.Warnings is IList<string> stringWarnings)
                {
                    stringWarnings.Add("Liczba gości przekracza łączną pojemność przypisanych stolików.");
                }
            }

            return result;
        }
        public async Task<OperationResult> DeleteAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation is null)
            {
                return OperationResult.Failure(OperationErrorCode.ReservationNotFound, "Nie znaleziono rezerwacji do usunięcia.");
            }

            _reservationRepository.Delete(reservation);
            await _reservationRepository.SaveChangesAsync();

            return OperationResult.Success();
        }
        public async Task<OperationResult> ChangeStatusAsync(int reservationId, ReservationStatus newStatus)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation is null)
            {
                return OperationResult.Failure(OperationErrorCode.ReservationNotFound, "Nie znaleziono rezerwacji.");
            }

            if (reservation.Status == newStatus)
            {
                return OperationResult.Success();
            }

            bool isValidTransition = reservation.Status switch
            {
                ReservationStatus.Expected => newStatus is ReservationStatus.Arrived or ReservationStatus.Late or ReservationStatus.Cancelled or ReservationStatus.NoShow,
                ReservationStatus.Late => newStatus is ReservationStatus.Arrived or ReservationStatus.Cancelled or ReservationStatus.NoShow,
                _ => false
            };

            if (!isValidTransition)
            {
                return OperationResult.Failure(OperationErrorCode.InvalidStatusTransition, "Niedozwolona zmiana statusu rezerwacji.");
            }

            reservation.Status = newStatus;
            _reservationRepository.Update(reservation);
            await _reservationRepository.SaveChangesAsync();

            return OperationResult.Success();
        }

        public async Task<int> RemovePastReservationsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var pastReservations = await _reservationRepository.GetPastReservationsAsync(today);

            if (!pastReservations.Any())
            {
                return 0;
            }

            foreach (var reservation in pastReservations)
            {
                _reservationRepository.Delete(reservation);
            }

            await _reservationRepository.SaveChangesAsync();

            return pastReservations.Count;
        }











        private void NormalizeReservation(Reservation reservation)
        {
            reservation.FirstName = string.IsNullOrWhiteSpace(reservation.FirstName) ? null : reservation.FirstName.Trim();
            reservation.LastName = string.IsNullOrWhiteSpace(reservation.LastName) ? null : reservation.LastName.Trim();
            reservation.Note = string.IsNullOrWhiteSpace(reservation.Note) ? null : reservation.Note.Trim();

            if (!string.IsNullOrWhiteSpace(reservation.PhoneNumber))
            {
                reservation.PhoneNumber = reservation.PhoneNumber.Replace(" ", "");
            }
        }

        private List<ValidationError> ValidateReservationData(Reservation reservation, InitialConfiguration config)
        {
            var errors = new List<ValidationError>();
            var now = DateTime.Now;
            var currentMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            var reservationDateTime = reservation.Date.ToDateTime(reservation.Time);
            var reservationMinute = new DateTime(reservationDateTime.Year, reservationDateTime.Month, reservationDateTime.Day, reservationDateTime.Hour, reservationDateTime.Minute, 0);

            if (reservation.Tables == null || !reservation.Tables.Any())
                errors.Add(new ValidationError(nameof(reservation.Tables), "Rezerwacja musi mieć przypisany co najmniej jeden stolik."));

            if (reservationMinute < currentMinute)
                errors.Add(new ValidationError(nameof(reservation.Date), "Rezerwacja nie może zostać dodana w czasie przeszłym."));
            if (reservation.Guests <= 0)
                errors.Add(new ValidationError(nameof(reservation.Guests), "Liczba gości musi być dodatnią liczbą całkowitą."));
            if (config.FirstNameVisible)
            {
                if (config.FirstNameRequired && string.IsNullOrEmpty(reservation.FirstName))
                    errors.Add(new ValidationError(nameof(reservation.FirstName), "Imię jest wymagane."));
                if (!string.IsNullOrEmpty(reservation.FirstName) && !Regex.IsMatch(reservation.FirstName, @"^\p{L}+$"))
                    errors.Add(new ValidationError(nameof(reservation.FirstName), "Imię może zawierać wyłącznie litery (bez spacji)."));
            }
            if (config.LastNameVisible)
            {
                if (config.LastNameRequired && string.IsNullOrEmpty(reservation.LastName))
                    errors.Add(new ValidationError(nameof(reservation.LastName), "Nazwisko jest wymagane."));
                if (!string.IsNullOrEmpty(reservation.LastName) && !Regex.IsMatch(reservation.LastName, @"^\p{L}+(-\p{L}+)*$"))
                    errors.Add(new ValidationError(nameof(reservation.LastName), "Nazwisko ma nieprawidłowy format."));
            }
            if (config.PhoneNumberVisible)
            {
                if (config.PhoneNumberRequired && string.IsNullOrEmpty(reservation.PhoneNumber))
                    errors.Add(new ValidationError(nameof(reservation.PhoneNumber), "Numer telefonu jest wymagany."));
                if (!string.IsNullOrEmpty(reservation.PhoneNumber) && !Regex.IsMatch(reservation.PhoneNumber, @"^\d{9}$"))
                    errors.Add(new ValidationError(nameof(reservation.PhoneNumber), "Numer telefonu musi składać się z dokładnie 9 cyfr."));
            }
            return errors;
        }

        private async Task<List<ValidationError>> CheckTableAvailabilityAsync(Reservation reservation, int? excludeReservationId = null)
        {
            var errors = new List<ValidationError>();
            var searchDate = reservation.Date.ToDateTime(TimeOnly.MinValue);
            var dailyReservations = await _reservationRepository.SearchAndFilterAsync(date: searchDate);
            var activeReservations = dailyReservations.Where(r =>
                r.Id != excludeReservationId &&
                (r.Status == ReservationStatus.Expected ||
                 r.Status == ReservationStatus.Late ||
                 r.Status == ReservationStatus.Arrived)).ToList();
            foreach (var requestedTable in reservation.Tables)
            {
                var hasConflict = activeReservations.Any(r =>
                    r.Time == reservation.Time &&
                    r.Tables.Any(t => t.Id == requestedTable.Id));

                if (hasConflict)
                {
                    errors.Add(new ValidationError(nameof(reservation.Tables), $"Stolik (ID: {requestedTable.Id}) jest niedostępny o wskazanej godzinie."));
                }
            }
            return errors;
        }

        private async Task<bool> HasCapacityWarningAsync(Reservation reservation)
        {
            int totalSeats = 0;
            foreach (var table in reservation.Tables)
            {
                var dbTable = await _tableService.GetByIdAsync(table.Id);
                if (dbTable != null)
                {
                    totalSeats += dbTable.Seats;
                }
            }
            return reservation.Guests > totalSeats;
        }
    }
}
