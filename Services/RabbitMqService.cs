using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IRabbitMqService
{
    Task ConnectAsync();
    Task PublishBudgetCreatedAsync(Budget budget);
    Task PublishPaymentCompletedAsync(Payment payment, Budget budget);
    Task PublishPaymentFailedAsync(Payment payment, string reason);
    Task CloseAsync();
}

public class RabbitMqService : IRabbitMqService
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ConnectAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMq:HostName"] ?? "localhost",
                    UserName = _configuration["RabbitMq:UserName"] ?? "guest",
                    Password = _configuration["RabbitMq:Password"] ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchange
                _channel.ExchangeDeclare("payment-events", ExchangeType.Topic, durable: true, autoDelete: false);

                // Declarar filas
                _channel.QueueDeclare("budget-generated", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare("payment-completed", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare("payment-failed", durable: true, exclusive: false, autoDelete: false);

                // Bindings
                _channel.QueueBind("budget-generated", "payment-events", "budget.created");
                _channel.QueueBind("payment-completed", "payment-events", "payment.completed");
                _channel.QueueBind("payment-failed", "payment-events", "payment.failed");

                _logger.LogInformation("RabbitMQ conectado com sucesso");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar ao RabbitMQ");
            throw;
        }
    }

    public async Task PublishBudgetCreatedAsync(Budget budget)
    {
        if (_channel == null) return;

        try
        {
            await Task.Run(() =>
            {
                var message = new
                {
                    budgetId = budget.BudgetId,
                    customerId = budget.CustomerId,
                    totalAmount = budget.TotalAmount,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish("payment-events", "budget.created", null, body);

                _logger.LogInformation($"Evento budget.created publicado: {budget.BudgetId}");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar budget.created");
        }
    }

    public async Task PublishPaymentCompletedAsync(Payment payment, Budget budget)
    {
        if (_channel == null) return;

        try
        {
            await Task.Run(() =>
            {
                var message = new
                {
                    paymentId = payment.PaymentId,
                    budgetId = payment.BudgetId,
                    customerId = payment.CustomerId,
                    amount = payment.Amount,
                    orderId = payment.OrderId,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish("payment-events", "payment.completed", null, body);

                _logger.LogInformation($"Evento payment.completed publicado: {payment.PaymentId}");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar payment.completed");
        }
    }

    public async Task PublishPaymentFailedAsync(Payment payment, string reason)
    {
        if (_channel == null) return;

        try
        {
            await Task.Run(() =>
            {
                var message = new
                {
                    paymentId = payment.PaymentId,
                    budgetId = payment.BudgetId,
                    customerId = payment.CustomerId,
                    amount = payment.Amount,
                    reason = reason,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish("payment-events", "payment.failed", null, body);

                _logger.LogInformation($"Evento payment.failed publicado: {payment.PaymentId}");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar payment.failed");
        }
    }

    public async Task CloseAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            });

            _logger.LogInformation("RabbitMQ desconectado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desconectar RabbitMQ");
        }
    }
}
