using System.ComponentModel.DataAnnotations;

namespace AdvancedEFCore.DTOs.Products
{
    public class UpdateProductDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        // The concurrency token returned by GET.
        [Required]
        public byte[] RowVersion { get; set; } = [];
    }
}
