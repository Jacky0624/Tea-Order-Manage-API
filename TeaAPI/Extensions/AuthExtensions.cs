using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TeaAPI.Models.Auths;

namespace TeaAPI.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {   
            var jwtConfig = configuration.GetSection("JWTConfig").Get<JWTConfig>();

            services.Configure<JWTConfig>(configuration.GetSection("JWTConfig"));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtConfig.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtConfig.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanManageOrders", policy => policy.RequireClaim("Permission", "ORDER_MANAGE"));
                options.AddPolicy("CanManageProducts", policy => policy.RequireClaim("Permission", "PRODUCT_MANAGE"));
                options.AddPolicy("CanManageAccounts", policy => policy.RequireClaim("Permission", "ACCOUNT_MANAGE"));
                options.AddPolicy("CanManageProductsOrOrder", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("Permission", "ORDER_MANAGE") ||
                        context.User.HasClaim("Permission", "PRODUCT_MANAGE")
                    ));
            });

            return services;
        }
    }
}
