using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "ok",
            service = "payment-service",
            timestamp = DateTime.UtcNow,
            uptime = Environment.TickCount / 1000.0
        });
    }

    [HttpGet("ready")]
    public IActionResult Ready()
    {
        return Ok(new
        {
            status = "ready",
            service = "payment-service",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return Ok(new
        {
            service = "Payment Service",
            version = "1.0.0",
            status = "running",
            endpoints = new
            {
                health = "/api/health",
                budgets = "/api/budgets",
                payments = "/api/payments",
                orders = "/api/orders"
            }
        });
    }
}
