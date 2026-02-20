import { v4 as uuidv4 } from 'uuid';
import Budget from '../models/Budget.js';
import Payment from '../models/Payment.js';
import ServiceOrder from '../models/ServiceOrder.js';
import EmailService from './EmailService.js';
import RabbitMQService from './RabbitMQService.js';
import OrderServiceClient from './OrderServiceClient.js';

class PaymentService {
  // ====== BUDGET SERVICES ======
  
  async generateBudget(budgetData) {
    try {
      const budgetId = `BUDGET-${Date.now()}-${uuidv4().substring(0, 8)}`;
      
      const budget = new Budget({
        budgetId,
        ...budgetData,
        expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) // 30 dias
      });
      
      await budget.save();
      console.log(`[PaymentService] Orçamento criado: ${budgetId}`);
      
      return budget;
    } catch (error) {
      console.error('[PaymentService] Erro ao gerar orçamento:', error);
      throw error;
    }
  }

  async sendBudgetForApproval(budgetId) {
    try {
      const budget = await Budget.findOne({ budgetId });
      
      if (!budget) {
        throw new Error(`Orçamento não encontrado: ${budgetId}`);
      }
      
      if (budget.status !== 'pending') {
        throw new Error(`Orçamento já foi processado. Status: ${budget.status}`);
      }
      
      // Enviar por email
      await EmailService.sendBudgetEmail(budget);
      
      // Atualizar status
      budget.status = 'sent';
      budget.sentAt = new Date();
      await budget.save();
      
      // Publicar evento
      await RabbitMQService.publishBudgetCreated(budget);
      
      console.log(`[PaymentService] Orçamento enviado: ${budgetId}`);
      return budget;
    } catch (error) {
      console.error('[PaymentService] Erro ao enviar orçamento:', error);
      throw error;
    }
  }

  async approveBudget(budgetId) {
    try {
      const budget = await Budget.findOne({ budgetId });
      
      if (!budget) {
        throw new Error(`Orçamento não encontrado: ${budgetId}`);
      }
      
      if (budget.status === 'approved') {
        return budget;
      }
      
      budget.status = 'approved';
      budget.approvedAt = new Date();
      await budget.save();
      
      // Criar ordem de serviço
      const serviceOrder = await this.createServiceOrder(budget);
      
      console.log(`[PaymentService] Orçamento aprovado: ${budgetId}`);
      return { budget, serviceOrder };
    } catch (error) {
      console.error('[PaymentService] Erro ao aprovar orçamento:', error);
      throw error;
    }
  }

  async rejectBudget(budgetId, reason = '') {
    try {
      const budget = await Budget.findOne({ budgetId });
      
      if (!budget) {
        throw new Error(`Orçamento não encontrado: ${budgetId}`);
      }
      
      budget.status = 'rejected';
      budget.rejectedAt = new Date();
      budget.notes = reason;
      await budget.save();
      
      console.log(`[PaymentService] Orçamento rejeitado: ${budgetId}`);
      return budget;
    } catch (error) {
      console.error('[PaymentService] Erro ao rejeitar orçamento:', error);
      throw error;
    }
  }

  async getBudget(budgetId) {
    try {
      const budget = await Budget.findOne({ budgetId });
      
      if (!budget) {
        throw new Error(`Orçamento não encontrado: ${budgetId}`);
      }
      
      return budget;
    } catch (error) {
      console.error('[PaymentService] Erro ao buscar orçamento:', error);
      throw error;
    }
  }

  async listBudgetsByCustomer(customerId) {
    try {
      const budgets = await Budget.find({ customerId }).sort({ createdAt: -1 });
      return budgets;
    } catch (error) {
      console.error('[PaymentService] Erro ao listar orçamentos:', error);
      throw error;
    }
  }

  // ====== PAYMENT SERVICES ======
  
  async registerPayment(paymentData) {
    try {
      const paymentId = `PAY-${Date.now()}-${uuidv4().substring(0, 8)}`;
      
      const budget = await Budget.findOne({ budgetId: paymentData.budgetId });
      if (!budget) {
        throw new Error(`Orçamento não encontrado: ${paymentData.budgetId}`);
      }
      
      const payment = new Payment({
        paymentId,
        ...paymentData,
        status: 'pending'
      });
      
      await payment.save();
      console.log(`[PaymentService] Pagamento registrado: ${paymentId}`);
      
      return payment;
    } catch (error) {
      console.error('[PaymentService] Erro ao registrar pagamento:', error);
      throw error;
    }
  }

  async processPayment(paymentId, transactionDetails = {}) {
    try {
      const payment = await Payment.findOne({ paymentId });
      
      if (!payment) {
        throw new Error(`Pagamento não encontrado: ${paymentId}`);
      }
      
      // Simular processamento
      payment.status = 'processing';
      payment.processedAt = new Date();
      payment.paymentDetails = {
        ...payment.paymentDetails,
        ...transactionDetails,
        transactionId: `TXN-${Date.now()}`
      };
      
      await payment.save();
      
      // Simular confirmação (em produção, isso seria feito por webhook de gateway de pagamento)
      setTimeout(async () => {
        await this.completePayment(paymentId);
      }, 2000);
      
      return payment;
    } catch (error) {
      console.error('[PaymentService] Erro ao processar pagamento:', error);
      throw error;
    }
  }

  async completePayment(paymentId) {
    try {
      const payment = await Payment.findOne({ paymentId });
      
      if (!payment) {
        throw new Error(`Pagamento não encontrado: ${paymentId}`);
      }
      
      if (payment.status === 'completed') {
        return payment;
      }
      
      payment.status = 'completed';
      payment.completedAt = new Date();
      await payment.save();
      
      // Buscar orçamento
      const budget = await Budget.findOne({ budgetId: payment.budgetId });
      
      // Enviar email de confirmação
      await EmailService.sendPaymentConfirmationEmail(payment, budget);
      
      // Atualizar ordem de serviço
      if (payment.orderId) {
        await this.updateOrderAfterPayment(payment.orderId, paymentId);
      }
      
      // Publicar evento
      await RabbitMQService.publishPaymentCompleted(payment, budget);
      
      console.log(`[PaymentService] Pagamento completado: ${paymentId}`);
      return payment;
    } catch (error) {
      console.error('[PaymentService] Erro ao completar pagamento:', error);
      throw error;
    }
  }

  async failPayment(paymentId, reason = 'Falha no processamento') {
    try {
      const payment = await Payment.findOne({ paymentId });
      
      if (!payment) {
        throw new Error(`Pagamento não encontrado: ${paymentId}`);
      }
      
      payment.status = 'failed';
      payment.failureReason = reason;
      await payment.save();
      
      const budget = await Budget.findOne({ budgetId: payment.budgetId });
      
      // Enviar email de falha
      await EmailService.sendPaymentFailureEmail(payment, budget, reason);
      
      // Publicar evento
      await RabbitMQService.publishPaymentFailed(payment, reason);
      
      console.log(`[PaymentService] Pagamento falhou: ${paymentId}`);
      return payment;
    } catch (error) {
      console.error('[PaymentService] Erro ao marcar pagamento como falho:', error);
      throw error;
    }
  }

  async verifyPayment(paymentId) {
    try {
      const payment = await Payment.findOne({ paymentId });
      
      if (!payment) {
        throw new Error(`Pagamento não encontrado: ${paymentId}`);
      }
      
      return payment;
    } catch (error) {
      console.error('[PaymentService] Erro ao verificar pagamento:', error);
      throw error;
    }
  }

  async getPaymentsByBudget(budgetId) {
    try {
      const payments = await Payment.find({ budgetId }).sort({ createdAt: -1 });
      return payments;
    } catch (error) {
      console.error('[PaymentService] Erro ao listar pagamentos:', error);
      throw error;
    }
  }

  // ====== SERVICE ORDER SERVICES ======
  
  async createServiceOrder(budget) {
    try {
      const orderId = `ORDER-${Date.now()}-${uuidv4().substring(0, 8)}`;
      
      const serviceOrder = new ServiceOrder({
        orderId,
        budgetId: budget.budgetId,
        customerId: budget.customerId,
        status: 'pending_payment'
      });
      
      await serviceOrder.save();
      console.log(`[PaymentService] Ordem de serviço criada: ${orderId}`);
      
      return serviceOrder;
    } catch (error) {
      console.error('[PaymentService] Erro ao criar ordem de serviço:', error);
      throw error;
    }
  }

  async updateOrderAfterPayment(orderId, paymentId) {
    try {
      const serviceOrder = await ServiceOrder.findOne({ orderId });
      
      if (!serviceOrder) {
        throw new Error(`Ordem não encontrada: ${orderId}`);
      }
      
      serviceOrder.paymentId = paymentId;
      serviceOrder.status = 'in_progress';
      
      try {
        // Tentar sincronizar com microsserviço de ordem
        const result = await OrderServiceClient.updateOrderStatus(
          orderId,
          'in_progress',
          paymentId
        );
        
        serviceOrder.syncedWithOrderService = true;
        serviceOrder.lastSyncAt = new Date();
        console.log(`[PaymentService] Ordem sincronizada com sucesso: ${orderId}`);
      } catch (error) {
        // Se falhar, marcar para retry posterior
        serviceOrder.syncError = error.message;
        serviceOrder.syncAttempts = (serviceOrder.syncAttempts || 0) + 1;
        console.warn(`[PaymentService] Falha ao sincronizar ordem (tentativa ${serviceOrder.syncAttempts}): ${orderId}`);
      }
      
      await serviceOrder.save();
      return serviceOrder;
    } catch (error) {
      console.error('[PaymentService] Erro ao atualizar ordem após pagamento:', error);
      throw error;
    }
  }

  async getServiceOrder(orderId) {
    try {
      const serviceOrder = await ServiceOrder.findOne({ orderId });
      
      if (!serviceOrder) {
        throw new Error(`Ordem não encontrada: ${orderId}`);
      }
      
      return serviceOrder;
    } catch (error) {
      console.error('[PaymentService] Erro ao buscar ordem de serviço:', error);
      throw error;
    }
  }

  async retryFailedSyncs() {
    try {
      const failedOrders = await ServiceOrder.find({
        syncedWithOrderService: false,
        syncAttempts: { $lt: 5 }
      });
      
      for (const order of failedOrders) {
        try {
          await OrderServiceClient.updateOrderStatus(
            order.orderId,
            order.status,
            order.paymentId
          );
          
          order.syncedWithOrderService = true;
          order.lastSyncAt = new Date();
          order.syncError = null;
          await order.save();
          
          console.log(`[PaymentService] Retry bem-sucedido para ordem: ${order.orderId}`);
        } catch (error) {
          order.syncAttempts = (order.syncAttempts || 0) + 1;
          order.syncError = error.message;
          await order.save();
          
          console.error(`[PaymentService] Retry falhou para ordem ${order.orderId}: ${error.message}`);
        }
      }
    } catch (error) {
      console.error('[PaymentService] Erro ao reprocessar syncs: ', error);
    }
  }
}

export default new PaymentService();
