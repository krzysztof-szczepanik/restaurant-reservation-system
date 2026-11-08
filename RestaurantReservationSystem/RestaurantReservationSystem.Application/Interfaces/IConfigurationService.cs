using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Application.Interfaces
{
    public interface IConfigurationService
    {
        Task<InitialConfiguration?> GetAsync();
        Task<OperationResult<InitialConfiguration>> SaveInitialAsync(
        InitialConfiguration configuration);
    }
}
