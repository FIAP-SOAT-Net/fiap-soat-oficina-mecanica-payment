using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IPaymentService paymentService, ILogger<OrdersController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetServiceOrder(string orderId)
    {
        try
        {
            var order = await _paymentService.GetServiceOrderAsync(orderId);

            return Ok(new { success = true, data = order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ordem");
            return NotFound(new { error = "Ordem não encontrada", details = ex.Message });
        }
    }

    [HttpPost("retry-syncs")]
    public async Task<IActionResult> RetrySyncs()
    {
        try
        {
            await _paymentService.RetryFailedSyncsAsync();

            return Ok(new { success = true, message = "Processo de retry iniciado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reprocessar syncs");
            return StatusCode(500, new { error = "Erro ao reprocessar syncs", details = ex.Message });
        }
    }
}
