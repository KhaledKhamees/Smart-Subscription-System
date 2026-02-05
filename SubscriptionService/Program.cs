using Microsoft.EntityFrameworkCore;
using Polly.Extensions.Http;
using Serilog;
using SubscriptionService.Data.Interfaces;
using SubscriptionService.Repository;
using SubscriptionService.Sync_communication.Interfaces;
using Polly;
var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SubscriptionService.Data.SubscriptionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'SubscriptionContext' not found.")));
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

var catalogServiceUrl = builder.Configuration["ServiceUrls:Catalog"]
    ?? throw new InvalidOperationException("Catalog service URL not configured.");
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri(catalogServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TaskCanceledException>()
        .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(2))
)
.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TaskCanceledException>()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
