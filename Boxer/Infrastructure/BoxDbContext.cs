using Boxer.Models;
using Microsoft.EntityFrameworkCore;

namespace Boxer.Infrastructure;

public class BoxDbContext(DbContextOptions<BoxDbContext> options) : DbContext(options)
{
    public DbSet<Box>? Boxes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(BoxDbContext).Assembly);
    }
}