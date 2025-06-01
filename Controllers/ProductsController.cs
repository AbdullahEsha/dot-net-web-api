using Microsoft.AspNetCore.Mvc;
using dot_net_web_api.Models.Entities;
using dot_net_web_api.Repositories;
using System.ComponentModel.DataAnnotations;

namespace dot_net_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> _productRepo;

        public ProductsController(IRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] int? minQuantity = null,
            [FromQuery] string? search = null,
            [FromQuery] string orderBy = "CreatedAt",
            [FromQuery] bool orderDescending = true)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Limit max page size

                var query = await _productRepo.GetAllAsync();
                var productsQuery = query.AsQueryable();

                // Apply filters
                if (minPrice.HasValue)
                    productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);

                if (maxPrice.HasValue)
                    productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);

                if (inStock.HasValue)
                {
                    if (inStock.Value)
                        productsQuery = productsQuery.Where(p => p.StockQuantity > 0);
                    else
                        productsQuery = productsQuery.Where(p => p.StockQuantity == 0);
                }

                if (minQuantity.HasValue)
                    productsQuery = productsQuery.Where(p => p.StockQuantity >= minQuantity.Value);

                if (!string.IsNullOrWhiteSpace(search))
                    productsQuery = productsQuery.Where(p => 
                        p.Name!.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Description!.Contains(search, StringComparison.OrdinalIgnoreCase));

                // Apply ordering
                productsQuery = orderBy.ToLower() switch
                {
                    "name" => orderDescending 
                        ? productsQuery.OrderByDescending(p => p.Name)
                        : productsQuery.OrderBy(p => p.Name),
                    "price" => orderDescending 
                        ? productsQuery.OrderByDescending(p => p.Price)
                        : productsQuery.OrderBy(p => p.Price),
                    "quantity" => orderDescending 
                        ? productsQuery.OrderByDescending(p => p.StockQuantity)
                        : productsQuery.OrderBy(p => p.StockQuantity),
                    "updatedat" => orderDescending 
                        ? productsQuery.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                        : productsQuery.OrderBy(p => p.UpdatedAt ?? p.CreatedAt),
                    _ => orderDescending 
                        ? productsQuery.OrderByDescending(p => p.CreatedAt)
                        : productsQuery.OrderBy(p => p.CreatedAt)
                };

                // Get total count before pagination
                var totalCount = productsQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Apply pagination
                var products = productsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new
                {
                    Data = products,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    },
                    Filters = new
                    {
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        InStock = inStock,
                        MinQuantity = minQuantity,
                        Search = search,
                        OrderBy = orderBy,
                        OrderDescending = orderDescending
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving products.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid product ID." });

            try
            {
                var product = await _productRepo.GetByIdAsync(id);
                return product == null ? NotFound(new { message = $"Product with ID {id} not found." }) : Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the product.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = new Product
                {
                    Name = request.Name?.Trim(),
                    Description = request.Description?.Trim(),
                    Price = request.Price,
                    StockQuantity = request.StockQuantity,
                    CreatedAt = DateTime.UtcNow
                };

                await _productRepo.AddAsync(product);
                await _productRepo.SaveAsync();

                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the product.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid product ID." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productRepo.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                // Update properties
                product.Name = request.Name?.Trim() ?? product.Name;
                product.Description = request.Description?.Trim() ?? product.Description;
                product.Price = request.Price ?? product.Price;
                product.StockQuantity = request.StockQuantity ?? product.StockQuantity;
                product.UpdatedAt = DateTime.UtcNow;

                _productRepo.Update(product);
                await _productRepo.SaveAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the product.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid product ID." });

            try
            {
                var product = await _productRepo.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                _productRepo.Delete(product);
                await _productRepo.SaveAsync();

                return Ok(new { message = $"Product with ID {id} has been deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the product.", error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Product price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; } = 0;
    }

    public class UpdateProductRequest
    {
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int? StockQuantity { get; set; }
    }
}