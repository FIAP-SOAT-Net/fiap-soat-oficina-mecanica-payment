using Serilog;
using PaymentService;
using PaymentService.Services;
using PaymentServiceImpl = PaymentService.Services.PaymentService;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Adicionar services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar serviços
builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddScoped<IOrderServiceClient, OrderServiceClient>();
builder.Services.AddScoped<IPaymentService, PaymentServiceImpl>();

// Adicionar HttpClientFactory
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// Inicializar RabbitMQ e serviços de background
try
{
    using (var scope = app.Services.CreateScope())
    {
        var rabbitMqService = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();
        await rabbitMqService.ConnectAsync();
        Log.Information("RabbitMQ conectado com sucesso");
    }
}
catch (Exception ex)
{
    Log.Warning(ex, "Falha ao conectar RabbitMQ. Continuando sem fila...");
}

// Inicia scheduler de retry — cria um scope a cada execução para resolver serviços scoped
var retryTask = Task.Run(async () =>
{
    while (true)
    {
        try
        {
            await Task.Delay(30000); // A cada 30 segundos
            using var scope = app.Services.CreateScope();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
            await paymentService.RetryFailedSyncsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro no scheduler de retry");
        }
    }
});

// Graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(async () =>
{
    Log.Information("Aplicação encerrando...");
    try
    {
        var rabbitMq = app.Services.GetRequiredService<IRabbitMqService>();
        await rabbitMq.CloseAsync();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Erro ao fechar RabbitMQ");
    }
});

Log.Information("Payment Service iniciado");
Log.Information($"Ambiente: {app.Environment.EnvironmentName}");

app.Run();
