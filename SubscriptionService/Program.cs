using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using SubscriptionService.Data.Interfaces;
using SubscriptionService.EventContracts;
using SubscriptionService.Repository;
using SubscriptionService.Services;
using SubscriptionService.Services.Interfaces;
using SubscriptionService.Sync_communication.Interfaces;
using System.Text;
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

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "49SAWtHlgDXWF6uMVjyyvQv00DxYXHF4IjnEWAnB9j259aPeP2kahfjahiuajalfkja";

var key = Encoding.UTF8.GetBytes(secretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "SmartSubscriptionIdentityService",
        ValidAudience = jwtSettings["Audience"] ?? "SmartSubscriptionServices",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

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

// Register the subscription publisher service
builder.Services.AddScoped<ISubscriptionPublisherService, SubscriptionPublisherService>();
// MassTransit and RabbitMQ configuration
builder.Services.AddMassTransit( cfg =>
{
    cfg.UsingRabbitMq((context, rabbitCfg) =>
    {
        // Configure RabbitMQ host and credentials from configuration
        rabbitCfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        // Configure JSON serializer options to use PascalCase for property names
        rabbitCfg.ConfigureJsonSerializerOptions(options =>
        {
            options.PropertyNamingPolicy = null; 
            return options;
        });
        // Configure message retry policy for transient failures
        rabbitCfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        // Configure message conventions for the events
        rabbitCfg.Message<SubscriptionCreatedEvent>(e => e.SetEntityName("subscription-created"));
        rabbitCfg.Message<SubscriptionCanceledEvent>(e => e.SetEntityName("subscription-canceled"));
        rabbitCfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
