using CatalogService.DTOs;
using CatalogService.Entities;

namespace CatalogService.Data.Interfaces
{
    public interface IProductRepository
    {
        Task<Guid> AddAsync(ProductDTO product);
        Task<List<Product>> GetAllActiveAsync();
        Task<List<Product>> GetAllInactiveAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task DeactivateAsync(Guid id);
        Task ActivateAsync(Guid id);
    }
}
