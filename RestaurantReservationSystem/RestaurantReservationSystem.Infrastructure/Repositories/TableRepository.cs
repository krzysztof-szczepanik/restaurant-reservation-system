using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Core.Repositories;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.Infrastructure.Repositories;

public class TableRepository : ITableRepository {

    // FIELDS:
    private readonly AppDbContext _context;


    // CONSTRUCTORS:
    public TableRepository(AppDbContext context) {
        _context = context;
    }


    // METHODS:
    public async Task<Table?> GetByIdAsync(int id, CancellationToken cancellationToken = default) {
        return await _context.Tables
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Table>> GetAllAsync(CancellationToken cancellationToken = default) {
        return await _context.Tables
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Table table, CancellationToken cancellationToken = default) {
        await _context.Tables.AddAsync(table, cancellationToken);
    }

    public void Update(Table table) {
        _context.Tables.Update(table);
    }

    public void Delete(Table table) {
        _context.Tables.Remove(table);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) {
        await _context.SaveChangesAsync(cancellationToken);
    }
}