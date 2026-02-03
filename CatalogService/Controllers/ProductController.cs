using CatalogService.Data;
using CatalogService.Data.Interfaces;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductRepository _productRepository;
        public ProductController(ILogger<ProductController> logger, IProductRepository productService,CatalogDbContext catalogDb)
        {
            _logger = logger;
            _productRepository = productService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO product)
        {
            if (product == null)
            {
                _logger.LogWarning("CreateProduct called with null product.");
                return BadRequest("Product data is required.");
            }

            try
            {
                var productId = await _productRepository.AddAsync(product); 
                _logger.LogInformation("Product created with ID: {ProductId}", productId);

                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = productId },
                    new { id = productId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product.");
                return StatusCode(500, "An error occurred while creating the product.");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with id {ProductId} not found.", id);
                return NotFound();
            }
            return Ok(product);
        }
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveProducts()
        {
            var products = await _productRepository.GetAllActiveAsync();
            return Ok(products);
        }
        [HttpPost("deactivate/{id}")]
        public async Task<IActionResult> DeactivateProduct(Guid id)
        {
            try
            {
                await _productRepository.DeactivateAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound();
            }
        }
        [HttpPost("activate/{id}")]
        public async Task<IActionResult> ActivateProduct(Guid id)
        {
            try
            {
                await _productRepository.ActivateAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound();
            }
        }

        [HttpGet("inactive")]
        public async Task<IActionResult> GetInActiveProducts()
        {
            var products = await _productRepository.GetAllInactiveAsync();
            return Ok(products);
        }
    }
}
