using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Models;

namespace RestaurantReservationSystem.Application.Interfaces {
    public interface IConfigurationService {
        Task<InitialConfiguration?> GetAsync();
        Task<OperationResult<InitialConfiguration>> SaveInitialAsync(InitialConfiguration configuration);
    }
}