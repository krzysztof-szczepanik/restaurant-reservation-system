using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Application.Interfaces
{
    public interface ITableService
    {
        Task<IReadOnlyList<Table>> GetAllAsync();
        Task<Table?> GetByIdAsync(int tableId);
        Task<OperationResult<Table>> AddAsync(Table table);
        Task<OperationResult<Table>> UpdateAsync(Table table);
        Task<OperationResult> DeleteAsync(int tableId);
    }
}
