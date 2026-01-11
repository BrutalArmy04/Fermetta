using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class Category
    {
        [Key]
        public int Category_Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public bool Disponibility { get; set; } = true;

        public ICollection<Product>? Products { get; set; }

        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}