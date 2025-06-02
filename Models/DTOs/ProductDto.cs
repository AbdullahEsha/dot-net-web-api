using System.ComponentModel.DataAnnotations;

namespace dot_net_web_api.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool InStock => StockQuantity > 0;
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; } = 0;
    }

    public class UpdateProductDto
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

    public class ProductFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public int? MinQuantity { get; set; }
        public string? Search { get; set; }
        public string OrderBy { get; set; } = "CreatedAt";
        public bool OrderDescending { get; set; } = true;
    }

    public class PagedResultDto<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public PaginationInfoDto Pagination { get; set; } = new();
        public ProductFilterDto Filters { get; set; } = new();
    }

    public class PaginationInfoDto
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}