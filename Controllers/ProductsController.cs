using Microsoft.AspNetCore.Mvc;
using dot_net_web_api.DTOs;
using dot_net_web_api.Services.Interfaces;  // Add this line

namespace dot_net_web_api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// Get all products with filtering, sorting, and pagination
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll(
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
                var filter = new ProductFilterDto
                {
                    Page = page,
                    PageSize = pageSize,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    InStock = inStock,
                    MinQuantity = minQuantity,
                    Search = search,
                    OrderBy = orderBy,
                    OrderDescending = orderDescending
                };

                var result = await _productService.GetAllProductsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving products.", error = ex.Message });
            }
        }

        /// Get a specific product by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid product ID." });

            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return product == null
                    ? NotFound(new { message = $"Product with ID {id} not found." })
                    : Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the product.", error = ex.Message });
            }
        }

        /// Create a new product
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productService.CreateProductAsync(createProductDto);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the product.", error = ex.Message });
            }
        }

        /// Update an existing product
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid product ID." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                return product == null
                    ? NotFound(new { message = $"Product with ID {id} not found." })
                    : Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the product.", error = ex.Message });
            }
        }

        /// Delete a product
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid product ID." });

            try
            {
                var deleted = await _productService.DeleteProductAsync(id);
                return deleted
                    ? Ok(new { message = $"Product with ID {id} has been deleted successfully." })
                    : NotFound(new { message = $"Product with ID {id} not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the product.", error = ex.Message });
            }
        }
    }
}