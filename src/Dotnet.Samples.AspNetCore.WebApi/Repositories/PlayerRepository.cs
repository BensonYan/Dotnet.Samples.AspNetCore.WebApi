using Dotnet.Samples.AspNetCore.WebApi.Data;
using Dotnet.Samples.AspNetCore.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Samples.AspNetCore.WebApi.Repositories;

public sealed class PlayerRepository(PlayerDbContext dbContext)
    : Repository<Player>(dbContext),
        IPlayerRepository
{
    public async Task<Player?> FindBySquadNumberAsync(int squadNumber) =>
        await _dbSet.FirstOrDefaultAsync(p => p.SquadNumber == squadNumber);

    public async Task<(
    List<Player> Items,
    int TotalCount
)> SearchAsync(
    string? team,
    string? position,
    int page,
    int pageSize,
    CancellationToken cancellationToken)
    {
        IQueryable<Player> query =
            _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(team))
        {
            query = query.Where(
                player => player.Team == team);
        }

        if (!string.IsNullOrWhiteSpace(position))
        {
            query = query.Where(
                player =>
                    player.AbbrPosition == position);
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var offset =
            ((long)page - 1) * pageSize;

        if (offset > int.MaxValue)
        {
            return (
                new List<Player>(),
                totalCount);
        }

        var items =
            await query
                .OrderBy(player =>
                    player.SquadNumber)
                .Skip((int)offset)
                .Take(pageSize)
                .ToListAsync(
                    cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> SquadNumberExistsAsync(int squadNumber)
    {
        return await dbContext.Players.AnyAsync(p => p.SquadNumber == squadNumber);
    }
}
