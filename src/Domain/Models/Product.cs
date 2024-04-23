using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateOnly ProduceDate { get; set; }
        public string ManufacturePhone { get; set; }
        public string ManufactureEmail { get; set; }
        public bool IsAvailable { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
    }
}
