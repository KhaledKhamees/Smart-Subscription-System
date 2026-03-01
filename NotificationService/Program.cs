using MassTransit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consumers;
using NotificationService.Data;
using NotificationService.EventContracts;
using NotificationService.Services;
using NotificationService.Services.Interfaces;
using NotificationService.Sync_communication.HttpClients;
using NotificationService.Sync_communication.Interfaces;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=notification.db"));

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<SubscriptionExpiringSoonEventConsumer>();

    cfg.UsingRabbitMq((context, rabbitCfg) =>
    {
        rabbitCfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        rabbitCfg.ConfigureJsonSerializerOptions(options =>
        {
            options.PropertyNamingPolicy = null;
            return options;
        });

        rabbitCfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        // VERY IMPORTANT PART
        rabbitCfg.ReceiveEndpoint("notification-billing-expiredSubscription", e =>
        {
            e.ConfigureConsumer<SubscriptionExpiringSoonEventConsumer>(context);
        });
    });
});

builder.Services.AddHttpClient<IEmailService, BrevoEmailService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Brevo:BaseUrl"] ?? "https://api.brevo.com/v3/");
    client.DefaultRequestHeaders.Add("api-key", config["Brevo:ApiKey"] ?? "");
});

var UserServiceBaseUrl = builder.Configuration["UserService:BaseUrl"] ?? "https://localhost:7169";
builder.Services.AddHttpClient<IUserClient, UserClient>(client =>
{
    client.BaseAddress = new Uri(UserServiceBaseUrl);
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
