using TeaAPI.Repositories.Accounts.Interfaces;
using TeaAPI.Repositories.Accounts;
using TeaAPI.Repositories.Orders.Interfaces;
using TeaAPI.Repositories.Orders;
using TeaAPI.Repositories.Products.Interfaces;
using TeaAPI.Repositories.Products;
using TeaAPI.Services.Account.Interfaces;
using TeaAPI.Services.Account;
using TeaAPI.Services.Orders.Interfaces;
using TeaAPI.Services.Orders;
using TeaAPI.Services.Products.Interfaces;
using TeaAPI.Services.Products;

namespace TeaAPI.Extensions
{
    public static class ServiceExtensions
    {


        public static IServiceCollection AddTeaAPIServices(this IServiceCollection services)
        {
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePemissionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IVariantTypeRepository, VariantTypeRepository>();
            services.AddScoped<IVariantValueRepository, VariantValueRepository>();
            services.AddScoped<IProductVariantOptionRepository, ProductVariantOptionRepository>();
            services.AddScoped<IProductSizeRepository, ProductSizeRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderItemOptionRepository, OrderItemOptionRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<IVariantService, VariantService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IOrderService, OrderService>();

            services.AddMemoryCache();
            services.AddSingleton<IRefreshTokenCacheService, RefreshTokenMemoryCacheService>();
            //services.Decorate<IProductService, CachedProductService>(); 
            return services;
        }
    }
}
