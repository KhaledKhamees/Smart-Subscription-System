using BillingService.Consumers;
using BillingService.EventContracts.Publishing_Contracts;
using BillingService.Repositories;
using BillingService.Repositories.Interfaces;
using BillingService.Services;
using BillingService.Services.BackgroundServices;
using BillingService.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

builder.Services.AddDbContext<BillingService.Data.BillingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=billing.db"));

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IBillingCycleRepository, BillingCycleRepository>();
builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

builder.Services.AddScoped<IBillingService, BillingServiceImpl>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<ISubscriptionExpiryNotificationPublisherService, SubscriptionExpiryNotificationPublisherService>();


builder.Services.AddHostedService<OverdueInvoiceChecker>();
builder.Services.AddHostedService<SubscriptionExpiryNotifier>();




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<SubscriptionCreatedConsumer>();
    cfg.AddConsumer<SubscriptionCanceledConsumer>();

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
        rabbitCfg.ReceiveEndpoint("billing-subscription-created", e =>
        {
            e.ConfigureConsumer<SubscriptionCreatedConsumer>(context);
        });

        rabbitCfg.ReceiveEndpoint("billing-subscription-canceled", e =>
        {
            e.ConfigureConsumer<SubscriptionCanceledConsumer>(context);
        });
        rabbitCfg.Message<SubscriptionExpiringSoonEvent> (options =>
        {
            options.SetEntityName("subscription-expiring-soon-event");
        });
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

app.UseAuthorization();

app.MapControllers();

app.Run();
