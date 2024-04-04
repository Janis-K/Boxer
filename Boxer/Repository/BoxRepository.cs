using Boxer.Infrastructure;
using Boxer.Interfaces;
using Boxer.Models;

namespace Boxer.Repository;

public class BoxRepository(BoxDbContext context) : IBoxRepository
{
    /// <inheritdoc />
    public async Task AddBoxAsync(Box box)
    {
        if (context.Boxes != null) await context.Boxes.AddAsync(box);
        await context.SaveChangesAsync();
    }
}