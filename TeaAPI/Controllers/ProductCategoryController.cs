using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaAPI.Models.Requests.Products;
using TeaAPI.Models.Responses;
using TeaAPI.Services.Products.Interfaces;

namespace TeaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "CanManageProducts")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoryController(
            IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        [HttpGet("GetAllProductCategories")]
        public async Task<IActionResult> GetAllProductCategoriesAsync()
        {
            return Ok(await _productCategoryService.GetAllAsync());
        }

        [HttpPost("GetProductCategoryById")]
        public async Task<IActionResult> GetProductCategoryByIdAsync(GetProductCategoryRequest request)
        {
            return Ok(await _productCategoryService.GetByIdAsync(request.Id));  
        }

    }
}
