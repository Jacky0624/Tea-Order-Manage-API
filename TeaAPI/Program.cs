using TeaAPI.AutoMappers;
using TeaAPI.Extensions;
using TeaAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimitingPolicy();

builder.Services.AddCorsPolicy(builder.Configuration);

//AutoMappers
builder.Services.AddAutoMapper(typeof(TeaMappingProfile));

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorizationPolicies();

// Add services to the container.
builder.Services.AddTeaAPIServices();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();


var app = builder.Build();
app.UseRateLimiter();
app.UseMiddleware<RequestResponseLoggingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUIWithConfig();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
