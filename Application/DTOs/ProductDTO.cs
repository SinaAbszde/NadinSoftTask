namespace Application.DTOs
{
    public class ProductDTO
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateOnly ProduceDate { get; set; }

        public string ManufacturePhone { get; set; }

        public string ManufactureEmail { get; set; }

        public bool IsAvailable { get; set; }
    }
}
