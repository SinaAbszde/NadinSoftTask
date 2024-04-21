using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    internal class ProductCreateDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ManufacturePhone { get; set; }

        [Required]
        public string ManufactureEmail { get; set; }

        public bool IsAvailable { get; set; }
    }
}
