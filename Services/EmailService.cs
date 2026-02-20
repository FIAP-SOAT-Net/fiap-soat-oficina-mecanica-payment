using MailKit.Net.Smtp;
using MimeKit;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IEmailService
{
    Task SendBudgetEmailAsync(Budget budget);
    Task SendPaymentConfirmationEmailAsync(Payment payment, Budget budget);
    Task SendPaymentFailureEmailAsync(Payment payment, Budget budget, string reason);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendBudgetEmailAsync(Budget budget)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Oficina", _configuration["Email:FromAddress"]));
            message.To.Add(new MailboxAddress(budget.CustomerName, budget.CustomerEmail));
            message.Subject = $"Orçamento para {budget.VehicleInfo?.LicensePlate} - {budget.BudgetId}";
            message.Body = new TextPart("html") { Text = BuildBudgetEmailBody(budget) };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:UserName"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation($"Email enviado com sucesso para {budget.CustomerEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao enviar email: {ex.Message}");
            throw;
        }
    }

    public async Task SendPaymentConfirmationEmailAsync(Payment payment, Budget budget)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Oficina", _configuration["Email:FromAddress"]));
            message.To.Add(new MailboxAddress(budget.CustomerName, budget.CustomerEmail));
            message.Subject = $"Confirmação de Pagamento - {payment.PaymentId}";
            message.Body = new TextPart("html") { Text = BuildPaymentConfirmationBody(payment, budget) };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:UserName"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation($"Email de confirmação enviado para {budget.CustomerEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao enviar email: {ex.Message}");
            throw;
        }
    }

    public async Task SendPaymentFailureEmailAsync(Payment payment, Budget budget, string reason)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Oficina", _configuration["Email:FromAddress"]));
            message.To.Add(new MailboxAddress(budget.CustomerName, budget.CustomerEmail));
            message.Subject = $"Falha no Pagamento - {payment.PaymentId}";
            message.Body = new TextPart("html") { Text = BuildPaymentFailureBody(payment, budget, reason) };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:UserName"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao enviar email: {ex.Message}");
            throw;
        }
    }

    private string BuildBudgetEmailBody(Budget budget)
    {
        var itemsHtml = string.Join("", budget.Items.Select(item => $@"
            <tr>
                <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{item.Description}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>{item.Quantity}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>R$ {item.UnitPrice:F2}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>R$ {item.Total:F2}</td>
            </tr>
        "));

        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Orçamento para Revisão</h2>
                <p>Olá {budget.CustomerName},</p>
                
                <p>Segue o orçamento para o seu veículo:</p>
                
                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px;'>
                    <p><strong>Número do Orçamento:</strong> {budget.BudgetId}</p>
                    <p><strong>Placa do Veículo:</strong> {budget.VehicleInfo?.LicensePlate}</p>
                    <p><strong>Marca/Modelo:</strong> {budget.VehicleInfo?.Brand} {budget.VehicleInfo?.Model}</p>
                    
                    <h3>Itens:</h3>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr style='background-color: #ddd;'>
                            <th style='padding: 8px; text-align: left;'>Descrição</th>
                            <th style='padding: 8px; text-align: right;'>Qtd</th>
                            <th style='padding: 8px; text-align: right;'>Valor Unit.</th>
                            <th style='padding: 8px; text-align: right;'>Total</th>
                        </tr>
                        {itemsHtml}
                    </table>
                    
                    <div style='margin-top: 15px; text-align: right;'>
                        <p><strong>Subtotal:</strong> R$ {(budget.TotalAmount - budget.DiscountAmount - budget.TaxAmount):F2}</p>
                        {(budget.DiscountAmount > 0 ? $"<p><strong>Desconto:</strong> -R$ {budget.DiscountAmount:F2}</p>" : "")}
                        {(budget.TaxAmount > 0 ? $"<p><strong>Impostos:</strong> R$ {budget.TaxAmount:F2}</p>" : "")}
                        <p style='font-size: 18px;'><strong>TOTAL:</strong> R$ {budget.TotalAmount:F2}</p>
                    </div>
                    
                    {(string.IsNullOrEmpty(budget.Notes) ? "" : $"<p><strong>Observações:</strong> {budget.Notes}</p>")}
                    
                    <p style='color: #666; font-size: 12px;'>Este orçamento é válido até {budget.ExpiresAt:dd/MM/yyyy}</p>
                </div>
                
                <p>Para aprovar este orçamento, clique no link abaixo:</p>
                <p><a href='http://localhost:3000/api/budgets/{budget.BudgetId}/approve' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Aprovar Orçamento</a></p>
                
                <p>Qualquer dúvida, entre em contato conosco.</p>
                <p>Atenciosamente,<br/>Equipe de Oficina</p>
            </div>
        ";
    }

    private string BuildPaymentConfirmationBody(Payment payment, Budget budget)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Pagamento Confirmado</h2>
                <p>Olá {budget.CustomerName},</p>
                
                <p>Seu pagamento foi processado com sucesso! Confira os detalhes:</p>
                
                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px;'>
                    <p><strong>ID do Pagamento:</strong> {payment.PaymentId}</p>
                    <p><strong>Orçamento:</strong> {payment.BudgetId}</p>
                    <p><strong>Valor:</strong> R$ {payment.Amount:F2}</p>
                    <p><strong>Método:</strong> {payment.PaymentMethod}</p>
                    <p><strong>Status:</strong> <span style='color: green; font-weight: bold;'>CONFIRMADO</span></p>
                    <p><strong>Data:</strong> {payment.CompletedAt:dd/MM/yyyy HH:mm:ss}</p>
                </div>
                
                <p>Seu veículo será atendido em breve. Acompanhe o status pelo número do orçamento.</p>
                
                <p>Obrigado!<br/>Equipe de Oficina</p>
            </div>
        ";
    }

    private string BuildPaymentFailureBody(Payment payment, Budget budget, string reason)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Falha no Pagamento</h2>
                <p>Olá {budget.CustomerName},</p>
                
                <p>Infelizmente, o seu pagamento não foi processado. Confira os detalhes:</p>
                
                <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px;'>
                    <p><strong>ID do Pagamento:</strong> {payment.PaymentId}</p>
                    <p><strong>Valor:</strong> R$ {payment.Amount:F2}</p>
                    <p><strong>Motivo:</strong> {reason}</p>
                </div>
                
                <p>Por favor, tente novamente ou entre em contato conosco para mais informações.</p>
                
                <p>Atenciosamente,<br/>Equipe de Oficina</p>
            </div>
        ";
    }
}
