using EcommerceBackend.Models;
using EcommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll([FromQuery] string? search)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var products = await _productService.SearchAsync(search);
            return Ok(products);
        }

        var allProducts = await _productService.GetAllAsync();
        return Ok(allProducts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _productService.UpdateAsync(id, dto);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        if (!await _productService.DeleteAsync(id))
            return NotFound();

        return NoContent();
    }
}
