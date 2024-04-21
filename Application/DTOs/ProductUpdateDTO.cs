using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ProductUpdateDTO
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ManufacturePhone { get; set; }

        [Required]
        public string ManufactureEmail { get; set; }

        public bool IsAvailable { get; set; }

    }
}
