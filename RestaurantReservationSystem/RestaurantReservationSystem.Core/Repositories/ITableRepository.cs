using RestaurantReservationSystem.Core.Models;

namespace RestaurantReservationSystem.Core.Repositories {
    public interface ITableRepository {
        Task<Table?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Table>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Table table, CancellationToken cancellationToken = default);
        void Update(Table table);
        void Delete(Table table);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}