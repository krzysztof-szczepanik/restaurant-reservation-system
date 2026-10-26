using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Core.Repositories;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository {

    // FIELDS:
    private readonly AppDbContext _context;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;


    // CONSTRUCTORS:
    public ReservationRepository(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory) {
        _context = context;
        _contextFactory = contextFactory;
    }


    // METHODS:
    public async Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default) {
        return await _context.Reservations
            .Include(r => r.Tables)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default) {
        await _context.Reservations.AddAsync(reservation, cancellationToken);
    }

    public void Update(Reservation reservation) {
        _context.Reservations.Update(reservation);
    }

    public void Delete(Reservation reservation) {
        _context.Reservations.Remove(reservation);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) {
        await _context.SaveChangesAsync(cancellationToken);
    }

    // TODO: Wrócić do tej metody po zaimplementowaniu ReservationService.
    // Dostosować wyszukiwanie i filtrowanie do ReservationFilterCriteria oraz zasad biznesowych.
    // Obecnie zaimplementowano tylko filtrowanie po statusie; parametry date i searchTerm są ignorowane.

    // Wyszukiwanie i filtrowanie – dedykowany krótkożyjący kontekst bez śledzenia
    //public async Task<IReadOnlyList<Reservation>> SearchAndFilterAsync(DateTime? date = null, ReservationStatus? status = null, string? searchTerm = null, CancellationToken cancellationToken = default) {
    //    await using var factoryContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

    //    var query = factoryContext.Reservations
    //        .AsNoTracking()
    //        .Include(r => r.Tables)
    //        .AsQueryable();

    //    if (status.HasValue) {
    //        query = query.Where(r => r.Status == status.Value);
    //    }

    //    return await query.ToListAsync(cancellationToken);
    //}
}