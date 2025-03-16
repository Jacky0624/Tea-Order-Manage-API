using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeaAPI.Models.Requests.Products;
using TeaAPI.Models.Responses;
using TeaAPI.Services.Products.Interfaces;

namespace TeaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [Authorize(Policy = "CanManageProducts")]
        [HttpPost("DeleteProduct")]
        public async Task<IActionResult> Delete(DeleteProductRequest request)
        {
            return Ok(await _productService.DeleteAsync(request.Id));
        }

        [Authorize(Policy = "CanManageProductsOrOrder")]
        [HttpPost("GetProductById")]
        public async Task<IActionResult> GetByIdAsync(GetProductRequest request)
        {
            return Ok(await _productService.GetByIdAsync(request.Id, true));
        }

        [Authorize(Policy = "CanManageProducts")]
        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProductAsync(CreateProductRequest request)
        {
            string user = GetUserId();
            var res = await _productService.CreateAsync(request, user);
            return Ok(res);
        }

        [Authorize(Policy = "CanManageProductsOrOrder")]
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _productService.GetAllAsync());
        }

        [Authorize(Policy = "CanManageProducts")]
        [HttpPost("UpdateProduct")]
        public async Task<IActionResult> UpdateProductAsync(UpdateProductRequest request)
        {
            string user = GetUserId();
            var res = await _productService.UpdateAsync(request, user);
            return Ok(res);
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
        }
    }
}
