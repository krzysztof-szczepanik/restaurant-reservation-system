using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.Application.Services {
    public class ApplicationResetService : IApplicationResetService {

        // FIELDS:
        private readonly IDbContextFactory<AppDbContext> _contextFactory;


        // CONSTRUCTORS:
        public ApplicationResetService(IDbContextFactory<AppDbContext> contextFactory) {
            _contextFactory = contextFactory;
        }


        // METHODS:
        public async Task ResetAsync() {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try {
                await context.Reservations.ExecuteDeleteAsync();
                await context.Tables.ExecuteDeleteAsync();
                await context.InitialConfigurations.ExecuteDeleteAsync();

                await transaction.CommitAsync();
            } catch {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}