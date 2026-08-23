using Library.Business;
using Library.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Tầng Presentation chỉ cần gọi đăng ký tầng Business (Business sẽ tự đăng ký DataAccess theo chuỗi phân tầng)
builder.Services.AddBusinessServices(builder.Configuration);

var app = builder.Build();

// Configure Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger UI in Development & Production for easy testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library 3-Tier Layered API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

// Auto seed database on startup thông qua Business Layer extension
await app.Services.InitializeDatabaseAsync();

app.Run();
