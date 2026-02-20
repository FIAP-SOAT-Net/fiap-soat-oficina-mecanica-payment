using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterPayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.BudgetId) || 
                string.IsNullOrEmpty(request.CustomerId) || request.Amount <= 0 ||
                string.IsNullOrEmpty(request.PaymentMethod))
            {
                return BadRequest(new { error = "Campos obrigatórios faltando" });
            }

            var validMethods = new[] { "credit_card", "debit_card", "pix", "boleto", "bank_transfer" };
            if (!validMethods.Contains(request.PaymentMethod))
            {
                return BadRequest(new { error = "paymentMethod inválido" });
            }

            var payment = await _paymentService.RegisterPaymentAsync(request);

            return CreatedAtAction(nameof(VerifyPayment), new { paymentId = payment.PaymentId },
                new { success = true, message = "Pagamento registrado com sucesso", data = payment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar pagamento");
            return StatusCode(500, new { error = "Erro ao registrar pagamento", details = ex.Message });
        }
    }

    [HttpPost("{paymentId}/process")]
    public async Task<IActionResult> ProcessPayment(string paymentId, [FromBody] object? transactionDetails)
    {
        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(paymentId, transactionDetails ?? new { });

            return Ok(new { success = true, message = "Pagamento enviado para processamento", data = payment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento");
            return StatusCode(500, new { error = "Erro ao processar pagamento", details = ex.Message });
        }
    }

    [HttpPost("{paymentId}/complete")]
    public async Task<IActionResult> CompletePayment(string paymentId)
    {
        try
        {
            var payment = await _paymentService.CompletePaymentAsync(paymentId);

            return Ok(new { success = true, message = "Pagamento completado com sucesso", data = payment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao completar pagamento");
            return StatusCode(500, new { error = "Erro ao completar pagamento", details = ex.Message });
        }
    }

    [HttpPost("{paymentId}/fail")]
    public async Task<IActionResult> FailPayment(string paymentId, [FromBody] FailPaymentRequest request)
    {
        try
        {
            var payment = await _paymentService.FailPaymentAsync(paymentId, request?.Reason ?? "Falha no processamento");

            return Ok(new { success = true, message = "Pagamento marcado como falho", data = payment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar pagamento como falho");
            return StatusCode(500, new { error = "Erro ao marcar pagamento como falho", details = ex.Message });
        }
    }

    [HttpGet("{paymentId}")]
    public async Task<IActionResult> VerifyPayment(string paymentId)
    {
        try
        {
            var payment = await _paymentService.VerifyPaymentAsync(paymentId);

            return Ok(new { success = true, data = payment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar pagamento");
            return NotFound(new { error = "Pagamento não encontrado", details = ex.Message });
        }
    }

    [HttpGet("budget/{budgetId}")]
    public async Task<IActionResult> GetPaymentsByBudget(string budgetId)
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByBudgetAsync(budgetId);

            return Ok(new { success = true, data = payments });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar pagamentos");
            return StatusCode(500, new { error = "Erro ao listar pagamentos", details = ex.Message });
        }
    }
}

public class FailPaymentRequest
{
    public string? Reason { get; set; }
}
