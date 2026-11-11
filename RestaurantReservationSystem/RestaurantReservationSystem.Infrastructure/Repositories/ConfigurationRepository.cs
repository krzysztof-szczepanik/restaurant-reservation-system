using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Core.Repositories;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly AppDbContext _context;

    public ConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InitialConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InitialConfigurations.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddOrUpdateAsync(InitialConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var existing = await _context.InitialConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            await _context.InitialConfigurations.AddAsync(configuration, cancellationToken);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(configuration);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}