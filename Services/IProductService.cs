using dot_net_web_api.DTOs;

namespace dot_net_web_api.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResultDto<ProductDto>> GetAllProductsAsync(ProductFilterDto filter);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
    }
}