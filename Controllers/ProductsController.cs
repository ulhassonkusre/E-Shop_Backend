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
    public ActionResult<List<Product>> GetAll([FromQuery] string? search)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var products = _productService.Search(search);
            return Ok(products);
        }
        
        var allProducts = _productService.GetAll();
        return Ok(allProducts);
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _productService.GetById(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [Authorize]
    [HttpPost]
    public ActionResult<Product> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _productService.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [Authorize]
    [HttpPut("{id}")]
    public ActionResult<Product> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _productService.Update(id, dto);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        if (!_productService.Delete(id))
            return NotFound();

        return NoContent();
    }
}
