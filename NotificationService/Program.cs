using MassTransit;
using NotificationService.Consumers;
using NotificationService.EventContracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
