using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Application.Services
{
    public class ApplicationResetService : IApplicationResetService
    {
        private readonly AppDbContext _context;

        public ApplicationResetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ResetAsync()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Reservations.ExecuteDeleteAsync();
                await _context.Tables.ExecuteDeleteAsync();
                await _context.InitialConfigurations.ExecuteDeleteAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}