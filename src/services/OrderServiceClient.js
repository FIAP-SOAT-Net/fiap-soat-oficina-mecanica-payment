import axios from 'axios';
import config from '../config.js';

class OrderServiceClient {
  async updateOrderStatus(orderId, status, paymentId) {
    try {
      const url = `${config.externalServices.orderServiceUrl}/orders/${orderId}/status`;
      
      console.log(`[OrderServiceClient] Atualizando ordem ${orderId} para status ${status}`);
      
      const response = await axios.put(
        url,
        {
          status: status,
          paymentId: paymentId,
          updatedBy: 'payment-service',
          timestamp: new Date().toISOString()
        },
        {
          timeout: 5000,
          headers: {
            'Content-Type': 'application/json',
            'X-API-Key': config.security.apiKey
          }
        }
      );
      
      console.log(`[OrderServiceClient] Ordem ${orderId} atualizada com sucesso`);
      return response.data;
    } catch (error) {
      console.error(`[OrderServiceClient] Erro ao atualizar ordem: ${error.message}`);
      throw new Error(`Falha ao sincronizar com serviço de ordem: ${error.message}`);
    }
  }

  async getOrderDetails(orderId) {
    try {
      const url = `${config.externalServices.orderServiceUrl}/orders/${orderId}`;
      
      const response = await axios.get(url, {
        timeout: 5000,
        headers: {
          'X-API-Key': config.security.apiKey
        }
      });
      
      return response.data;
    } catch (error) {
      console.error(`[OrderServiceClient] Erro ao buscar ordem: ${error.message}`);
      throw error;
    }
  }
}

export default new OrderServiceClient();
