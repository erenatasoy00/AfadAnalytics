using Microsoft.EntityFrameworkCore;
using AfadAnalytics.Data;
using AfadAnalytics.Services.AnalyticsService;
using AfadAnalytics.Services.ListingService;
using AfadAnalytics.Services.MapService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseNetTopologySuite()));

builder.Services.AddScoped<IMapService, MapManager>();
builder.Services.AddScoped<IListingsService, ListingsManager>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();