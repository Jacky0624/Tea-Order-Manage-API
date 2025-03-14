namespace TeaAPI.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins) 
                        .AllowCredentials()        
                        .AllowAnyHeader()     
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
