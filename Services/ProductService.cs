using dot_net_web_api.Data;
using dot_net_web_api.DTOs;
using dot_net_web_api.Models.Entities;
using dot_net_web_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dot_net_web_api.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ProductDto>> GetAllProductsAsync(ProductFilterDto filter)
        {
            // Validate and sanitize filter parameters
            ValidateFilterParameters(filter);

            var query = _context.Products.AsQueryable();

            // Apply filters
            query = ApplyFilters(query, filter);

            // Apply ordering
            query = ApplyOrdering(query, filter);

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            // Apply pagination
            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var productDtos = products.Select(MapToProductDto).ToList();

            return new PagedResultDto<ProductDto>
            {
                Data = productDtos,
                Pagination = new PaginationInfoDto
                {
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = filter.Page < totalPages,
                    HasPreviousPage = filter.Page > 1
                },
                Filters = filter
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            return product != null ? MapToProductDto(product) : null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = new Product
            {
                Name = createProductDto.Name.Trim(),
                Description = createProductDto.Description.Trim(),
                Price = createProductDto.Price,
                StockQuantity = createProductDto.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return MapToProductDto(product);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            // Update properties only if provided
            if (!string.IsNullOrWhiteSpace(updateProductDto.Name))
                product.Name = updateProductDto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(updateProductDto.Description))
                product.Description = updateProductDto.Description.Trim();

            if (updateProductDto.Price.HasValue)
                product.Price = updateProductDto.Price.Value;

            if (updateProductDto.StockQuantity.HasValue)
                product.StockQuantity = updateProductDto.StockQuantity.Value;

            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return MapToProductDto(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        // Private helper methods
        private static void ValidateFilterParameters(ProductFilterDto filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.PageSize > 100) filter.PageSize = 100; // Limit max page size
        }

        private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterDto filter)
        {
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.InStock.HasValue)
            {
                if (filter.InStock.Value)
                    query = query.Where(p => p.StockQuantity > 0);
                else
                    query = query.Where(p => p.StockQuantity == 0);
            }

            if (filter.MinQuantity.HasValue)
                query = query.Where(p => p.StockQuantity >= filter.MinQuantity.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p =>
                    p.Name!.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description!.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));

            return query;
        }

        private static IQueryable<Product> ApplyOrdering(IQueryable<Product> query, ProductFilterDto filter)
        {
            return filter.OrderBy.ToLower() switch
            {
                "name" => filter.OrderDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" => filter.OrderDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "quantity" => filter.OrderDescending
                    ? query.OrderByDescending(p => p.StockQuantity)
                    : query.OrderBy(p => p.StockQuantity),
                "updatedat" => filter.OrderDescending
                    ? query.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                    : query.OrderBy(p => p.UpdatedAt ?? p.CreatedAt),
                _ => filter.OrderDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };
        }

        private static ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name ?? string.Empty,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}