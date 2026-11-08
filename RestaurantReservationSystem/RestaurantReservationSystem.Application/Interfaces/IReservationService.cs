using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.OperationResults;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Application.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation?> GetByIdAsync(int reservationId);
        Task<IReadOnlyList<Reservation>> GetTodayAsync();
        Task<OperationResult> ValidateFiltersAsync(
        ReservationFilterCriteria criteria);
        Task<OperationResult<IReadOnlyList<Reservation>>> GetFilteredAsync(
        ReservationFilterCriteria criteria);
        Task<OperationResult<Reservation>> AddAsync(
        Reservation reservation);
        Task<OperationResult<Reservation>> UpdateAsync(
        Reservation reservation,
        bool unlockCancelledOrNoShow = false);
        Task<OperationResult> DeleteAsync(int reservationId);
        Task<OperationResult> ChangeStatusAsync(
        int reservationId,
        ReservationStatus status);
        Task<int> RemovePastReservationsAsync();
    }
}
