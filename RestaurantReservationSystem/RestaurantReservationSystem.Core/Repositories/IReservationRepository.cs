using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;

namespace RestaurantReservationSystem.Core.Repositories {
    public interface IReservationRepository {
        Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
        void Update(Reservation reservation);
        void Delete(Reservation reservation);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Reservation>> SearchAndFilterAsync(
            DateTime? date = null,
            ReservationStatus? status = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default);
    }
}