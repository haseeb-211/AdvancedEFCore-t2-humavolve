using AdvancedEFCore.Data;
using AdvancedEFCore.DTOs.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProductResponseDto>>> GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be positive.");
            }

            // Prevent excessively large requests
            pageSize = Math.Min(pageSize, 100);

            var query = _context.Products
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    RowVersion = p.RowVersion
                })
                .ToListAsync();

            var result = new PagedResultDto<ProductResponseDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)pageSize)
            };

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(
            int id,
            [FromBody] UpdateProductDto request)
        {
            if (request.RowVersion is null ||
                request.RowVersion.Length == 0)
            {
                return BadRequest("RowVersion is required.");
            }

            // 1. Load the existing product as a tracked entity.
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return NotFound();

            // 2. Validate the requested category.
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId);

            if (!categoryExists)
                return BadRequest("The specified category does not exist.");

            // 3. Use the client's version as the original concurrency value.
            _context.Entry(product)
                .Property(p => p.RowVersion)
                .OriginalValue = request.RowVersion;

            // 4. Change only the allowed fields.
            product.Name = request.Name;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;

            // 5. Save changes.
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(
                    "The product was modified by another request. Reload it and try again.");
            }

            return NoContent();
        }


    }
}
