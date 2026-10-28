using RestaurantReservationSystem.Core.Models;

namespace RestaurantReservationSystem.Core.Repositories {
    public interface IConfigurationRepository {
        Task<InitialConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken = default);
        Task AddOrUpdateAsync(InitialConfiguration configuration, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}