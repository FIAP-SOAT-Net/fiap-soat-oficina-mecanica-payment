using MongoDB.Driver;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IPaymentService
{
    // Budget operations
    Task<Budget> GenerateBudgetAsync(CreateBudgetRequest request);
    Task<Budget> SendBudgetForApprovalAsync(string budgetId);
    Task<(Budget budget, ServiceOrder order)> ApproveBudgetAsync(string budgetId);
    Task<Budget> RejectBudgetAsync(string budgetId, string reason = "");
    Task<Budget> GetBudgetAsync(string budgetId);
    Task<List<Budget>> ListBudgetsByCustomerAsync(string customerId);

    // Payment operations
    Task<Payment> RegisterPaymentAsync(CreatePaymentRequest request);
    Task<Payment> ProcessPaymentAsync(string paymentId, object transactionDetails);
    Task<Payment> CompletePaymentAsync(string paymentId);
    Task<Payment> FailPaymentAsync(string paymentId, string reason);
    Task<Payment> VerifyPaymentAsync(string paymentId);
    Task<List<Payment>> GetPaymentsByBudgetAsync(string budgetId);

    // Service Order operations
    Task<ServiceOrder> GetServiceOrderAsync(string orderId);
    Task RetryFailedSyncsAsync();
}

public class CreateBudgetRequest
{
    public string CustomerId { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public VehicleInfo? VehicleInfo { get; set; }
    public List<BudgetItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
}

public class CreatePaymentRequest
{
    public string BudgetId { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? OrderId { get; set; }
}

public class PaymentService : IPaymentService
{
    private readonly IMongoDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IMongoDbContext context,
        IEmailService emailService,
        IRabbitMqService rabbitMqService,
        IOrderServiceClient orderServiceClient,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _emailService = emailService;
        _rabbitMqService = rabbitMqService;
        _orderServiceClient = orderServiceClient;
        _logger = logger;
    }

    // ====== BUDGET SERVICES ======

    public async Task<Budget> GenerateBudgetAsync(CreateBudgetRequest request)
    {
        try
        {
            var budgetId = $"BUDGET-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-{Guid.NewGuid().ToString()[..8]}";

            var budget = new Budget
            {
                BudgetId = budgetId,
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                CustomerName = request.CustomerName,
                VehicleInfo = request.VehicleInfo,
                Items = request.Items,
                TotalAmount = request.TotalAmount,
                TaxAmount = request.TaxAmount,
                DiscountAmount = request.DiscountAmount,
                Notes = request.Notes,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Status = "pending"
            };

            await _context.Budgets.InsertOneAsync(budget);
            _logger.LogInformation($"Orçamento criado: {budgetId}");

            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar orçamento");
            throw;
        }
    }

    public async Task<Budget> SendBudgetForApprovalAsync(string budgetId)
    {
        try
        {
            var budget = await _context.Budgets.Find(b => b.BudgetId == budgetId).FirstOrDefaultAsync();

            if (budget == null)
                throw new Exception($"Orçamento não encontrado: {budgetId}");

            if (budget.Status != "pending")
                throw new Exception($"Orçamento já foi processado. Status: {budget.Status}");

            // Enviar por email
            await _emailService.SendBudgetEmailAsync(budget);

            // Atualizar status
            budget.Status = "sent";
            budget.SentAt = DateTime.UtcNow;

            var filter = Builders<Budget>.Filter.Eq(b => b.BudgetId, budgetId);
            await _context.Budgets.ReplaceOneAsync(filter, budget);

            // Publicar evento
            await _rabbitMqService.PublishBudgetCreatedAsync(budget);

            _logger.LogInformation($"Orçamento enviado: {budgetId}");
            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar orçamento");
            throw;
        }
    }

    public async Task<(Budget budget, ServiceOrder order)> ApproveBudgetAsync(string budgetId)
    {
        try
        {
            var budget = await _context.Budgets.Find(b => b.BudgetId == budgetId).FirstOrDefaultAsync();

            if (budget == null)
                throw new Exception($"Orçamento não encontrado: {budgetId}");

            if (budget.Status == "approved")
                return (budget, new ServiceOrder());

            budget.Status = "approved";
            budget.ApprovedAt = DateTime.UtcNow;

            var filter = Builders<Budget>.Filter.Eq(b => b.BudgetId, budgetId);
            await _context.Budgets.ReplaceOneAsync(filter, budget);

            // Criar ordem de serviço
            var serviceOrder = await CreateServiceOrderAsync(budget);

            _logger.LogInformation($"Orçamento aprovado: {budgetId}");
            return (budget, serviceOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aprovar orçamento");
            throw;
        }
    }

    public async Task<Budget> RejectBudgetAsync(string budgetId, string reason = "")
    {
        try
        {
            var budget = await _context.Budgets.Find(b => b.BudgetId == budgetId).FirstOrDefaultAsync();

            if (budget == null)
                throw new Exception($"Orçamento não encontrado: {budgetId}");

            budget.Status = "rejected";
            budget.RejectedAt = DateTime.UtcNow;
            budget.Notes = reason;

            var filter = Builders<Budget>.Filter.Eq(b => b.BudgetId, budgetId);
            await _context.Budgets.ReplaceOneAsync(filter, budget);

            _logger.LogInformation($"Orçamento rejeitado: {budgetId}");
            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao rejeitar orçamento");
            throw;
        }
    }

    public async Task<Budget> GetBudgetAsync(string budgetId)
    {
        try
        {
            var budget = await _context.Budgets.Find(b => b.BudgetId == budgetId).FirstOrDefaultAsync();

            if (budget == null)
                throw new Exception($"Orçamento não encontrado: {budgetId}");

            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar orçamento");
            throw;
        }
    }

    public async Task<List<Budget>> ListBudgetsByCustomerAsync(string customerId)
    {
        try
        {
            var budgets = await _context.Budgets
                .Find(b => b.CustomerId == customerId)
                .SortByDescending(b => b.CreatedAt)
                .ToListAsync();
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar orçamentos");
            throw;
        }
    }

    // ====== PAYMENT SERVICES ======

    public async Task<Payment> RegisterPaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            var paymentId = $"PAY-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-{Guid.NewGuid().ToString()[..8]}";

            var budget = await _context.Budgets.Find(b => b.BudgetId == request.BudgetId).FirstOrDefaultAsync();
            if (budget == null)
                throw new Exception($"Orçamento não encontrado: {request.BudgetId}");

            var payment = new Payment
            {
                PaymentId = paymentId,
                BudgetId = request.BudgetId,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                OrderId = request.OrderId,
                Status = "pending"
            };

            await _context.Payments.InsertOneAsync(payment);
            _logger.LogInformation($"Pagamento registrado: {paymentId}");

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar pagamento");
            throw;
        }
    }

    public async Task<Payment> ProcessPaymentAsync(string paymentId, object transactionDetails)
    {
        try
        {
            var payment = await _context.Payments.Find(p => p.PaymentId == paymentId).FirstOrDefaultAsync();

            if (payment == null)
                throw new Exception($"Pagamento não encontrado: {paymentId}");

            payment.Status = "processing";
            payment.ProcessedAt = DateTime.UtcNow;

            var filter = Builders<Payment>.Filter.Eq(p => p.PaymentId, paymentId);
            await _context.Payments.ReplaceOneAsync(filter, payment);

            // Simular confirmação
            _ = Task.Delay(2000).ContinueWith(async _ => await CompletePaymentAsync(paymentId));

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento");
            throw;
        }
    }

    public async Task<Payment> CompletePaymentAsync(string paymentId)
    {
        try
        {
            var payment = await _context.Payments.Find(p => p.PaymentId == paymentId).FirstOrDefaultAsync();

            if (payment == null)
                throw new Exception($"Pagamento não encontrado: {paymentId}");

            if (payment.Status == "completed")
                return payment;

            payment.Status = "completed";
            payment.CompletedAt = DateTime.UtcNow;

            var filter = Builders<Payment>.Filter.Eq(p => p.PaymentId, paymentId);
            await _context.Payments.ReplaceOneAsync(filter, payment);

            // Buscar orçamento
            var budget = await _context.Budgets.Find(b => b.BudgetId == payment.BudgetId).FirstOrDefaultAsync();

            // Enviar email de confirmação
            if (budget != null)
                await _emailService.SendPaymentConfirmationEmailAsync(payment, budget);

            // Atualizar ordem de serviço
            if (!string.IsNullOrEmpty(payment.OrderId))
                await UpdateOrderAfterPaymentAsync(payment.OrderId, paymentId);

            // Publicar evento
            if (budget != null)
                await _rabbitMqService.PublishPaymentCompletedAsync(payment, budget);

            _logger.LogInformation($"Pagamento completado: {paymentId}");
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao completar pagamento");
            throw;
        }
    }

    public async Task<Payment> FailPaymentAsync(string paymentId, string reason)
    {
        try
        {
            var payment = await _context.Payments.Find(p => p.PaymentId == paymentId).FirstOrDefaultAsync();

            if (payment == null)
                throw new Exception($"Pagamento não encontrado: {paymentId}");

            payment.Status = "failed";
            payment.FailureReason = reason;

            var filter = Builders<Payment>.Filter.Eq(p => p.PaymentId, paymentId);
            await _context.Payments.ReplaceOneAsync(filter, payment);

            var budget = await _context.Budgets.Find(b => b.BudgetId == payment.BudgetId).FirstOrDefaultAsync();

            // Enviar email de falha
            if (budget != null)
                await _emailService.SendPaymentFailureEmailAsync(payment, budget, reason);

            // Publicar evento
            await _rabbitMqService.PublishPaymentFailedAsync(payment, reason);

            _logger.LogInformation($"Pagamento falhou: {paymentId}");
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar pagamento como falho");
            throw;
        }
    }

    public async Task<Payment> VerifyPaymentAsync(string paymentId)
    {
        try
        {
            var payment = await _context.Payments.Find(p => p.PaymentId == paymentId).FirstOrDefaultAsync();

            if (payment == null)
                throw new Exception($"Pagamento não encontrado: {paymentId}");

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar pagamento");
            throw;
        }
    }

    public async Task<List<Payment>> GetPaymentsByBudgetAsync(string budgetId)
    {
        try
        {
            var payments = await _context.Payments
                .Find(p => p.BudgetId == budgetId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
            return payments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar pagamentos");
            throw;
        }
    }

    // ====== SERVICE ORDER SERVICES ======

    private async Task<ServiceOrder> CreateServiceOrderAsync(Budget budget)
    {
        try
        {
            var orderId = $"ORDER-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-{Guid.NewGuid().ToString()[..8]}";

            var serviceOrder = new ServiceOrder
            {
                OrderId = orderId,
                BudgetId = budget.BudgetId,
                CustomerId = budget.CustomerId,
                Status = "pending_payment"
            };

            await _context.ServiceOrders.InsertOneAsync(serviceOrder);
            _logger.LogInformation($"Ordem de serviço criada: {orderId}");

            return serviceOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar ordem de serviço");
            throw;
        }
    }

    private async Task<ServiceOrder> UpdateOrderAfterPaymentAsync(string orderId, string paymentId)
    {
        try
        {
            var serviceOrder = await _context.ServiceOrders.Find(o => o.OrderId == orderId).FirstOrDefaultAsync();

            if (serviceOrder == null)
                throw new Exception($"Ordem não encontrada: {orderId}");

            serviceOrder.PaymentId = paymentId;
            serviceOrder.Status = "in_progress";

            try
            {
                // Tentar sincronizar com microsserviço de ordem
                await _orderServiceClient.UpdateOrderStatusAsync(orderId, "in_progress", paymentId);

                serviceOrder.SyncedWithOrderService = true;
                serviceOrder.LastSyncAt = DateTime.UtcNow;
                _logger.LogInformation($"Ordem sincronizada com sucesso: {orderId}");
            }
            catch (Exception ex)
            {
                // Se falhar, marcar para retry posterior
                serviceOrder.SyncError = ex.Message;
                serviceOrder.SyncAttempts = (serviceOrder.SyncAttempts > 0 ? serviceOrder.SyncAttempts : 0) + 1;
                _logger.LogWarning($"Falha ao sincronizar ordem (tentativa {serviceOrder.SyncAttempts}): {orderId}");
            }

            var filter = Builders<ServiceOrder>.Filter.Eq(o => o.OrderId, orderId);
            await _context.ServiceOrders.ReplaceOneAsync(filter, serviceOrder);

            return serviceOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar ordem após pagamento");
            throw;
        }
    }

    public async Task<ServiceOrder> GetServiceOrderAsync(string orderId)
    {
        try
        {
            var serviceOrder = await _context.ServiceOrders.Find(o => o.OrderId == orderId).FirstOrDefaultAsync();

            if (serviceOrder == null)
                throw new Exception($"Ordem não encontrada: {orderId}");

            return serviceOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ordem de serviço");
            throw;
        }
    }

    public async Task RetryFailedSyncsAsync()
    {
        try
        {
            var failedOrders = await _context.ServiceOrders
                .Find(o => !o.SyncedWithOrderService && o.SyncAttempts < 5)
                .ToListAsync();

            foreach (var order in failedOrders)
            {
                try
                {
                    await _orderServiceClient.UpdateOrderStatusAsync(order.OrderId, order.Status, order.PaymentId ?? "");

                    order.SyncedWithOrderService = true;
                    order.LastSyncAt = DateTime.UtcNow;
                    order.SyncError = null;

                    var filter = Builders<ServiceOrder>.Filter.Eq(o => o.OrderId, order.OrderId);
                    await _context.ServiceOrders.ReplaceOneAsync(filter, order);

                    _logger.LogInformation($"Retry bem-sucedido para ordem: {order.OrderId}");
                }
                catch (Exception ex)
                {
                    order.SyncAttempts++;
                    order.SyncError = ex.Message;

                    var filter = Builders<ServiceOrder>.Filter.Eq(o => o.OrderId, order.OrderId);
                    await _context.ServiceOrders.ReplaceOneAsync(filter, order);

                    _logger.LogError($"Retry falhou para ordem {order.OrderId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reprocessar syncs");
        }
    }
}
