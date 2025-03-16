using Microsoft.Extensions.Caching.Memory;
using TeaAPI.Dtos.Products;
using TeaAPI.Models.Requests.Products;
using TeaAPI.Models.Responses;
using TeaAPI.Services.Products.Interfaces;

namespace TeaAPI.Services.Products
{
    public class CachedProductService : IProductService
    {
        private readonly IProductService _productService;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public CachedProductService(IProductService productService, IMemoryCache cache)
        {
            _productService = productService;
            _cache = cache;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var cacheKey = GetCacheKey(nameof(GetAllAsync));
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _productService.GetAllAsync();
            });
        }

        public async Task<ProductDTO> GetByIdAsync(int id, bool includeDeleted = false)
        {
            var cacheKey = GetCacheKey(nameof(GetByIdAsync), id, includeDeleted);
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _productService.GetByIdAsync(id, includeDeleted);
            });
        }

        public async Task<IEnumerable<ProductDTO>> GetActiveProductsAsync()
        {
            var cacheKey = GetCacheKey(nameof(GetActiveProductsAsync));
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _productService.GetActiveProductsAsync();
            });
        }

        public async Task<IEnumerable<ProductDTO>> GetActiveProductsByCategoryIdAsync(int categoryId)
        {
            var cacheKey = GetCacheKey(nameof(GetActiveProductsByCategoryIdAsync), categoryId);
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _productService.GetActiveProductsByCategoryIdAsync(categoryId);
            });
        }

        public async Task<ResponseBase> CreateAsync(CreateProductRequest request, string user)
        {
            var response = await _productService.CreateAsync(request, user);
            if (response.ResultCode == 0)
            {
                _cache.Remove(GetCacheKey(nameof(GetAllAsync)));
                _cache.Remove(GetCacheKey(nameof(GetActiveProductsAsync)));
                _cache.Remove(GetCacheKey(nameof(GetActiveProductsByCategoryIdAsync), request.CategoryId));
            }
            return response;
        }

        public async Task<ResponseBase> UpdateAsync(UpdateProductRequest request, string user)
        {
            var response = await _productService.UpdateAsync(request, user);
            if (response.ResultCode == 0)
            {
                _cache.Remove(GetCacheKey(nameof(GetByIdAsync), request.Id));
                _cache.Remove(GetCacheKey(nameof(GetAllAsync)));
                _cache.Remove(GetCacheKey(nameof(GetActiveProductsAsync)));
                _cache.Remove(GetCacheKey(nameof(GetActiveProductsByCategoryIdAsync), request.CategoryId));
            }
            return response;
        }

        public async Task<ResponseBase> DeleteAsync(int id)
        {
            var response = await _productService.DeleteAsync(id);
            if (response.ResultCode == 0)
            {
                _cache.Remove(GetCacheKey(nameof(GetByIdAsync), id));
                _cache.Remove(GetCacheKey(nameof(GetAllAsync)));
                _cache.Remove(GetCacheKey(nameof(GetActiveProductsAsync)));
            }
            return response;
        }

        private static string GetCacheKey(string method, params object[] parameters)
        {
            return $"product_{method}:{string.Join("_", parameters)}";
        }
    }
}
