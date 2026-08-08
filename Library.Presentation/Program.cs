using Library.Business;
using Library.DataAccess.Interfaces;
using Library.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Add Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký toàn bộ dịch vụ của tầng Business & DataAccess qua Extension Method
builder.Services.AddBusinessServices(builder.Configuration);

var app = builder.Build();

// Configure Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger UI in Development & Production for easy testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library OCP API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

// Auto seed database on startup thông qua IDatabaseInitializer
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbInitializer = services.GetRequiredService<IDatabaseInitializer>();
        await dbInitializer.InitializeAsync(forceRecreate: false);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra khi khởi tạo dữ liệu Database!");
    }
}

app.Run();
