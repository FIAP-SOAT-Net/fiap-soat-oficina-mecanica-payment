import amqp from 'amqplib';
import config from '../config.js';

class RabbitMQService {
  constructor() {
    this.connection = null;
    this.channel = null;
  }

  async connect() {
    try {
      this.connection = await amqp.connect(config.rabbitmq.url);
      this.channel = await this.connection.createChannel();
      
      // Declarar exchanges
      await this.channel.assertExchange('payment-events', 'topic', { durable: true });
      
      // Declarar filas
      await this.channel.assertQueue('budget-generated', { durable: true });
      await this.channel.assertQueue('payment-completed', { durable: true });
      await this.channel.assertQueue('payment-failed', { durable: true });
      
      // Bindings
      await this.channel.bindQueue('budget-generated', 'payment-events', 'budget.created');
      await this.channel.bindQueue('payment-completed', 'payment-events', 'payment.completed');
      await this.channel.bindQueue('payment-failed', 'payment-events', 'payment.failed');
      
      console.log('[RabbitMQ] Conectado com sucesso');
      
      // Reconectar automaticamente
      this.connection.on('error', (err) => {
        console.error('[RabbitMQ] Erro de conexão:', err);
        setTimeout(() => this.connect(), 5000);
      });
    } catch (error) {
      console.error('[RabbitMQ] Falha ao conectar:', error);
      setTimeout(() => this.connect(), 5000);
    }
  }

  async publishBudgetCreated(budget) {
    if (!this.channel) return;
    
    try {
      this.channel.publish(
        'payment-events',
        'budget.created',
        Buffer.from(JSON.stringify({
          budgetId: budget.budgetId,
          customerId: budget.customerId,
          totalAmount: budget.totalAmount,
          timestamp: new Date().toISOString()
        }))
      );
    } catch (error) {
      console.error('[RabbitMQ] Erro ao publicar budget.created:', error);
    }
  }

  async publishPaymentCompleted(payment, budget) {
    if (!this.channel) return;
    
    try {
      this.channel.publish(
        'payment-events',
        'payment.completed',
        Buffer.from(JSON.stringify({
          paymentId: payment.paymentId,
          budgetId: payment.budgetId,
          customerId: payment.customerId,
          amount: payment.amount,
          orderId: payment.orderId,
          timestamp: new Date().toISOString()
        }))
      );
    } catch (error) {
      console.error('[RabbitMQ] Erro ao publicar payment.completed:', error);
    }
  }

  async publishPaymentFailed(payment, reason) {
    if (!this.channel) return;
    
    try {
      this.channel.publish(
        'payment-events',
        'payment.failed',
        Buffer.from(JSON.stringify({
          paymentId: payment.paymentId,
          budgetId: payment.budgetId,
          customerId: payment.customerId,
          amount: payment.amount,
          reason: reason,
          timestamp: new Date().toISOString()
        }))
      );
    } catch (error) {
      console.error('[RabbitMQ] Erro ao publicar payment.failed:', error);
    }
  }

  async consume(queueName, callback) {
    if (!this.channel) return;
    
    try {
      await this.channel.consume(queueName, async (msg) => {
        if (msg) {
          try {
            const content = JSON.parse(msg.content.toString());
            await callback(content);
            this.channel.ack(msg);
          } catch (error) {
            console.error(`[RabbitMQ] Erro ao processar mensagem da fila ${queueName}:`, error);
            this.channel.nack(msg, false, true);
          }
        }
      });
    } catch (error) {
      console.error(`[RabbitMQ] Erro ao consumir da fila ${queueName}:`, error);
    }
  }

  async close() {
    try {
      if (this.channel) await this.channel.close();
      if (this.connection) await this.connection.close();
      console.log('[RabbitMQ] Conexão fechada');
    } catch (error) {
      console.error('[RabbitMQ] Erro ao fechar conexão:', error);
    }
  }
}

export default new RabbitMQService();
