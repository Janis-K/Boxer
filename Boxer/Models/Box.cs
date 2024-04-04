namespace Boxer.Models;

public class Box
{
    public Box(IReadOnlyCollection<Content> contents, string supplierIdentifier, string identifier)
    {
        Contents = contents;
        SupplierIdentifier = supplierIdentifier;
        Identifier = identifier;
    }

    public Box(){}

    public string SupplierIdentifier { get; set; }
    public string Identifier { get; set; }

    public IReadOnlyCollection<Content> Contents { get; set; } 

    public class Content
    {
        public string PoNumber { get; set; }
        public string Isbn { get; set; }
        public int Quantity { get; set; }
    }
}