using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<BudgetsController> _logger;

    public BudgetsController(IPaymentService paymentService, ILogger<BudgetsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.CustomerId) || 
                string.IsNullOrEmpty(request.CustomerEmail) || request.Items.Count == 0)
            {
                return BadRequest(new { error = "Campos obrigatórios faltando" });
            }

            var budget = await _paymentService.GenerateBudgetAsync(request);

            return CreatedAtAction(nameof(GetBudget), new { budgetId = budget.BudgetId }, 
                new { success = true, message = "Orçamento criado com sucesso", data = budget });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar orçamento");
            return StatusCode(500, new { error = "Erro ao criar orçamento", details = ex.Message });
        }
    }

    [HttpPost("{budgetId}/send")]
    public async Task<IActionResult> SendBudgetForApproval(string budgetId)
    {
        try
        {
            var budget = await _paymentService.SendBudgetForApprovalAsync(budgetId);
            return Ok(new { success = true, message = "Orçamento enviado com sucesso", data = budget });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar orçamento");
            return StatusCode(500, new { error = "Erro ao enviar orçamento", details = ex.Message });
        }
    }

    [HttpPost("{budgetId}/approve")]
    public async Task<IActionResult> ApproveBudget(string budgetId)
    {
        try
        {
            var (budget, order) = await _paymentService.ApproveBudgetAsync(budgetId);
            return Ok(new { success = true, message = "Orçamento aprovado com sucesso", data = new { budget, order } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aprovar orçamento");
            return StatusCode(500, new { error = "Erro ao aprovar orçamento", details = ex.Message });
        }
    }

    [HttpPost("{budgetId}/reject")]
    public async Task<IActionResult> RejectBudget(string budgetId, [FromBody] RejectBudgetRequest request)
    {
        try
        {
            var budget = await _paymentService.RejectBudgetAsync(budgetId, request?.Reason ?? "");
            return Ok(new { success = true, message = "Orçamento rejeitado com sucesso", data = budget });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao rejeitar orçamento");
            return StatusCode(500, new { error = "Erro ao rejeitar orçamento", details = ex.Message });
        }
    }

    [HttpGet("{budgetId}")]
    public async Task<IActionResult> GetBudget(string budgetId)
    {
        try
        {
            var budget = await _paymentService.GetBudgetAsync(budgetId);
            return Ok(new { success = true, data = budget });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar orçamento");
            return NotFound(new { error = "Orçamento não encontrado", details = ex.Message });
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> ListBudgetsByCustomer(string customerId)
    {
        try
        {
            var budgets = await _paymentService.ListBudgetsByCustomerAsync(customerId);
            return Ok(new { success = true, data = budgets });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar orçamentos");
            return StatusCode(500, new { error = "Erro ao listar orçamentos", details = ex.Message });
        }
    }
}

public class RejectBudgetRequest
{
    public string? Reason { get; set; }
}
