namespace CatalogService.DTOs
{
    public class ProductDTO
    {
        public string Name { get;  set; }
        public string Description { get;  set; }
        public decimal BasePrice { get;  set; }
        public string Currency { get;  set; }
        public bool IsActive { get;  set; }
        public DateTime CreatedAt { get;  set; }
    }
}
