using System;

namespace Fermetta.Models.ChangeRequests
{
    public class CategoryProposal
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Disponibility { get; set; } = true;
    }

    public class ProductProposal
    {
        public string Name { get; set; }
        public int Weight { get; set; }
        public DateTime Valability { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool Personalised { get; set; }
        public int Category_Id { get; set; }
        public string? ImagePath { get; set; }
        public string? Description { get; set; }
    }
}
