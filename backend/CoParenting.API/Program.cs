using CoParenting.API.Endpoints;
using CoParenting.API.Services;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Co-Parenting Calendar API", Version = "v1" });
});

// Add DbContext
builder.Services.AddDbContext<CoParentingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddScoped<DayAssignmentService>();
builder.Services.AddScoped<ConfigurationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Map endpoints
app.MapDayAssignmentEndpoints();
app.MapConfigurationEndpoints();
app.MapStatisticsEndpoints();

app.MapGet("/", () => "Co-Parenting Calendar API is running")
    .ExcludeFromDescription();

app.Run();
