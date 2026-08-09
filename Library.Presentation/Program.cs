using Library.Business;
using Library.DataAccess;
using Library.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Add Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dang ky dich vu cua tang DataAccess & Business qua Extension Methods
builder.Services.AddDataAccessServices(builder.Configuration);
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

// Auto seed database on startup thong qua Business Layer extension
await app.Services.InitializeDatabaseAsync();

app.Run();

