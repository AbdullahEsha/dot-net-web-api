using Microsoft.AspNetCore.Mvc;
using dot_net_web_api.Models.Entities;
using dot_net_web_api.Repositories;

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
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepo.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            await _productRepo.AddAsync(product);
            await _productRepo.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
    }
}