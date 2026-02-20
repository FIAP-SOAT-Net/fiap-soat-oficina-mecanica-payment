import nodemailer from 'nodemailer';
import config from '../config.js';

class EmailService {
  constructor() {
    this.transporter = nodemailer.createTransport({
      host: config.mail.host,
      port: config.mail.port,
      secure: config.mail.port === 465,
      auth: {
        user: config.mail.user,
        pass: config.mail.password
      }
    });
  }

  async sendBudgetEmail(budget) {
    try {
      const mailOptions = {
        from: config.mail.from,
        to: budget.customerEmail,
        subject: `Orçamento para ${budget.vehicleInfo.licensePlate} - ${budget.budgetId}`,
        html: this.getBudgetEmailTemplate(budget)
      };

      const result = await this.transporter.sendMail(mailOptions);
      console.log(`Email enviado com sucesso para ${budget.customerEmail}`);
      return result;
    } catch (error) {
      console.error('Erro ao enviar email:', error);
      throw new Error(`Falha ao enviar email: ${error.message}`);
    }
  }

  async sendPaymentConfirmationEmail(payment, budget) {
    try {
      const mailOptions = {
        from: config.mail.from,
        to: budget.customerEmail,
        subject: `Confirmação de Pagamento - ${payment.paymentId}`,
        html: this.getPaymentConfirmationTemplate(payment, budget)
      };

      await this.transporter.sendMail(mailOptions);
      console.log(`Email de confirmação enviado para ${budget.customerEmail}`);
    } catch (error) {
      console.error('Erro ao enviar email de confirmação:', error);
      throw error;
    }
  }

  async sendPaymentFailureEmail(payment, budget, reason) {
    try {
      const mailOptions = {
        from: config.mail.from,
        to: budget.customerEmail,
        subject: `Falha no Pagamento - ${payment.paymentId}`,
        html: this.getPaymentFailureTemplate(payment, budget, reason)
      };

      await this.transporter.sendMail(mailOptions);
    } catch (error) {
      console.error('Erro ao enviar email de falha:', error);
      throw error;
    }
  }

  getBudgetEmailTemplate(budget) {
    return `
      <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <h2>Orçamento para Revisão</h2>
        <p>Olá ${budget.customerName},</p>
        
        <p>Segue o orçamento para o seu veículo:</p>
        
        <div style="background-color: #f5f5f5; padding: 15px; border-radius: 5px;">
          <p><strong>Número do Orçamento:</strong> ${budget.budgetId}</p>
          <p><strong>Placa do Veículo:</strong> ${budget.vehicleInfo.licensePlate}</p>
          <p><strong>Marca/Modelo:</strong> ${budget.vehicleInfo.brand} ${budget.vehicleInfo.model}</p>
          
          <h3>Itens:</h3>
          <table style="width: 100%; border-collapse: collapse;">
            <tr style="background-color: #ddd;">
              <th style="padding: 8px; text-align: left;">Descrição</th>
              <th style="padding: 8px; text-align: right;">Qtd</th>
              <th style="padding: 8px; text-align: right;">Valor Unit.</th>
              <th style="padding: 8px; text-align: right;">Total</th>
            </tr>
            ${budget.items.map(item => `
              <tr>
                <td style="padding: 8px; border-bottom: 1px solid #ddd;">${item.description}</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd; text-align: right;">${item.quantity}</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd; text-align: right;">R$ ${item.unitPrice.toFixed(2)}</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd; text-align: right;">R$ ${item.total.toFixed(2)}</td>
              </tr>
            `).join('')}
          </table>
          
          <div style="margin-top: 15px; text-align: right;">
            <p><strong>Subtotal:</strong> R$ ${(budget.totalAmount - (budget.discountAmount || 0) - (budget.taxAmount || 0)).toFixed(2)}</p>
            ${budget.discountAmount ? `<p><strong>Desconto:</strong> -R$ ${budget.discountAmount.toFixed(2)}</p>` : ''}
            ${budget.taxAmount ? `<p><strong>Impostos:</strong> R$ ${budget.taxAmount.toFixed(2)}</p>` : ''}
            <p style="font-size: 18px;"><strong>TOTAL:</strong> R$ ${budget.totalAmount.toFixed(2)}</p>
          </div>
          
          ${budget.notes ? `<p><strong>Observações:</strong> ${budget.notes}</p>` : ''}
          
          <p style="color: #666; font-size: 12px;">Este orçamento é válido até ${new Date(budget.expiresAt).toLocaleDateString('pt-BR')}</p>
        </div>
        
        <p>Para aprovar este orçamento, clique no link abaixo:</p>
        <p><a href="http://localhost:3000/budgets/${budget.budgetId}/approve" style="background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;">Aprovar Orçamento</a></p>
        
        <p>Qualquer dúvida, entre em contato conosco.</p>
        <p>Atenciosamente,<br/>Equipe de Oficina</p>
      </div>
    `;
  }

  getPaymentConfirmationTemplate(payment, budget) {
    return `
      <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <h2>Pagamento Confirmado</h2>
        <p>Olá ${budget.customerName},</p>
        
        <p>Seu pagamento foi processado com sucesso! Confira os detalhes:</p>
        
        <div style="background-color: #f5f5f5; padding: 15px; border-radius: 5px;">
          <p><strong>ID do Pagamento:</strong> ${payment.paymentId}</p>
          <p><strong>Orçamento:</strong> ${payment.budgetId}</p>
          <p><strong>Valor:</strong> R$ ${payment.amount.toFixed(2)}</p>
          <p><strong>Método:</strong> ${payment.paymentMethod}</p>
          <p><strong>Status:</strong> <span style="color: green; font-weight: bold;">CONFIRMADO</span></p>
          <p><strong>Data:</strong> ${new Date(payment.completedAt).toLocaleDateString('pt-BR')} às ${new Date(payment.completedAt).toLocaleTimeString('pt-BR')}</p>
        </div>
        
        <p>Seu veículo será atendido em breve. Acompanhe o status pelo número do orçamento.</p>
        
        <p>Obrigado!<br/>Equipe de Oficina</p>
      </div>
    `;
  }

  getPaymentFailureTemplate(payment, budget, reason) {
    return `
      <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <h2>Falha no Pagamento</h2>
        <p>Olá ${budget.customerName},</p>
        
        <p>Infelizmente, o seu pagamento não foi processado. Confira os detalhes:</p>
        
        <div style="background-color: #fff3cd; padding: 15px; border-radius: 5px;">
          <p><strong>ID do Pagamento:</strong> ${payment.paymentId}</p>
          <p><strong>Valor:</strong> R$ ${payment.amount.toFixed(2)}</p>
          <p><strong>Motivo:</strong> ${reason}</p>
        </div>
        
        <p>Por favor, tente novamente ou entre em contato conosco para mais informações.</p>
        
        <p>Atenciosamente,<br/>Equipe de Oficina</p>
      </div>
    `;
  }
}

export default new EmailService();
