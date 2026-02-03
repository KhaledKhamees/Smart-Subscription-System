using CatalogService.Data;
using CatalogService.Data.Interfaces;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;
        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }


        public async Task<Guid> AddAsync(ProductDTO product)
        {
            var productEntity = new Product(
                product.Name,
                product.Description,
                product.BasePrice,
                product.Currency
            );
            await _context.Products.AddAsync(productEntity);
            await _context.SaveChangesAsync();

            return productEntity.Id;
        }

        public async Task DeactivateAsync(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found.");
            }
            product.Deactivate();
            await _context.SaveChangesAsync();
        }
        public async Task<List<Product>> GetAllActiveAsync()
        {
            return await _context.Products.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found.");
            }
            return product;
        }
        public async Task ActivateAsync(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found.");
            }
            product.Activate();
            await _context.SaveChangesAsync();
        }
        public async Task<List<Product>> GetAllInactiveAsync()
        {
            return await _context.Products.Where(p => !p.IsActive).ToListAsync();
        }
    }
}
