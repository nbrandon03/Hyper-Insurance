using HyperInsurance.Data;
using HyperInsurance.Middleware;
using HyperInsurance.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<VehicleClassificationService>();
builder.Services.AddScoped<UnderwritingService>();
builder.Services.AddScoped<IUnderwritingRule, DriverHistoryRule>();
builder.Services.AddScoped<IUnderwritingRule, DrivingExperienceRule>();
builder.Services.AddScoped<IUnderwritingRule, VehiclePerformanceRule>();
builder.Services.AddScoped<IUnderwritingRule, VehicleValueRule>();
builder.Services.AddScoped<IUnderwritingRule, AnnualMileageRule>();
builder.Services.AddScoped<IUnderwritingRule, OutdoorStorageRule>();
builder.Services.AddScoped<IUnderwritingRule, TrackUsageRule>();
builder.Services.AddScoped<RatingService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();