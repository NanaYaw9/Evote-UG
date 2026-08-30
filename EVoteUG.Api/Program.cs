using EVoteUG.Api.Extensions;
using EVoteUG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5043", "https://localhost:7001")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 2. Configure Database Context (EF Core + SQL Server)
builder.Services.AddDbContext<EVoteUGDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configure JWT Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// 4. Configure Application Services
builder.Services.AddApplicationServices();

// 5. Add Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 6. Configure Swagger with JWT Bearer UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwtAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Vote UG API v1");
    });

    // Automatically apply EF Core migrations and seed data in Development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EVoteUGDbContext>();
    dbContext.Database.Migrate();
    DbInitializer.SeedAsync(dbContext, app.Configuration, app.Logger).GetAwaiter().GetResult();
}

app.UseCors("AllowBlazorClient");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();