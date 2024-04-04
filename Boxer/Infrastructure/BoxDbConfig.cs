using Boxer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boxer.Infrastructure;

public abstract class BoxDbConfig
{
    /// <summary>
    ///     Represents the configuration for the Box entity in the database.
    /// </summary>
    public class BoxConfig : IEntityTypeConfiguration<Box>
    {
        public void Configure(EntityTypeBuilder<Box> builder)
        {
            builder.HasKey(x => new { x.Identifier });  // working on assumption that Identifier is unique, if not we can try to combine it with SupplierIdentifier or create entirely new PK
    
            builder.Property(x => x.SupplierIdentifier).HasMaxLength(20); 
            builder.Property(x => x.Identifier).HasMaxLength(20); 
    
            builder.OwnsMany(x => x.Contents, p => 
            {
                p.Property(x => x.PoNumber).HasMaxLength(20); 
                p.Property(x => x.Isbn).HasMaxLength(20);
            });
        }
    }
}