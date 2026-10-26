using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;

namespace RestaurantReservationSystem.Core.Repositories {
    public interface IReservationRepository {
        Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
        void Update(Reservation reservation);
        void Delete(Reservation reservation);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        // TODO: Wrócić do tej metody po zaimplementowaniu ReservationService.
        // Dostosować wyszukiwanie i filtrowanie do ReservationFilterCriteria oraz zasad biznesowych.
        // Obecnie zaimplementowano tylko filtrowanie po statusie; parametry date i searchTerm są ignorowane.

        // Task<IReadOnlyList<Reservation>> SearchAndFilterAsync(
        //     DateTime? date = null,
        //     ReservationStatus? status = null,
        //     string? searchTerm = null,
        //     CancellationToken cancellationToken = default);
    }
}