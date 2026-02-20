using PaymentService.Models;
using System.Net.Http.Json;

namespace PaymentService.Services;

public interface IOrderServiceClient
{
    Task<object?> UpdateOrderStatusAsync(string orderId, string status, string paymentId);
    Task<object?> GetOrderDetailsAsync(string orderId);
}

public class OrderServiceClient : IOrderServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderServiceClient> _logger;

    public OrderServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OrderServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<object?> UpdateOrderStatusAsync(string orderId, string status, string paymentId)
    {
        try
        {
            var orderServiceUrl = _configuration["ExternalServices:OrderServiceUrl"];
            if (string.IsNullOrEmpty(orderServiceUrl))
            {
                _logger.LogWarning("OrderServiceUrl não configurada");
                return null;
            }

            var url = $"{orderServiceUrl}/orders/{orderId}/status";

            _logger.LogInformation($"Atualizando ordem {orderId} para status {status}");

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var payload = new
            {
                status = status,
                paymentId = paymentId,
                updatedBy = "payment-service",
                timestamp = DateTime.UtcNow
            };

            var response = await client.PutAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Ordem {orderId} atualizada com sucesso");
                var result = await response.Content.ReadFromJsonAsync<object>();
                return result;
            }
            else
            {
                _logger.LogError($"Erro ao atualizar ordem: {response.StatusCode}");
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Erro HTTP ao atualizar ordem: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout ao atualizar ordem");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro inesperado ao atualizar ordem: {ex.Message}");
            return null;
        }
    }

    public async Task<object?> GetOrderDetailsAsync(string orderId)
    {
        try
        {
            var orderServiceUrl = _configuration["ExternalServices:OrderServiceUrl"];
            if (string.IsNullOrEmpty(orderServiceUrl))
            {
                _logger.LogWarning("OrderServiceUrl não configurada");
                return null;
            }

            var url = $"{orderServiceUrl}/orders/{orderId}";

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<object>();
                return result;
            }
            else
            {
                _logger.LogWarning($"Ordem não encontrada: {response.StatusCode}");
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Erro HTTP ao buscar ordem: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout ao buscar ordem");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro inesperado ao buscar ordem: {ex.Message}");
            return null;
        }
    }
}
