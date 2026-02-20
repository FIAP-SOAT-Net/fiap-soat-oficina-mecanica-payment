import express from 'express';
import PaymentService from '../services/PaymentService.js';

const router = express.Router();

// ====== PAYMENT ENDPOINTS ======

/**
 * POST /api/payments
 * Registrar novo pagamento
 */
router.post('/', async (req, res) => {
  try {
    const {
      budgetId,
      customerId,
      amount,
      paymentMethod,
      orderId
    } = req.body;

    // Validação básica
    if (!budgetId || !customerId || !amount || !paymentMethod) {
      return res.status(400).json({
        error: 'Campos obrigatórios faltando: budgetId, customerId, amount, paymentMethod'
      });
    }

    const validMethods = ['credit_card', 'debit_card', 'pix', 'boleto', 'bank_transfer'];
    if (!validMethods.includes(paymentMethod)) {
      return res.status(400).json({
        error: `paymentMethod inválido. Deve ser um de: ${validMethods.join(', ')}`
      });
    }

    const payment = await PaymentService.registerPayment({
      budgetId,
      customerId,
      amount,
      paymentMethod,
      orderId
    });

    res.status(201).json({
      success: true,
      message: 'Pagamento registrado com sucesso',
      data: payment
    });
  } catch (error) {
    console.error('[PaymentController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao registrar pagamento',
      details: error.message
    });
  }
});

/**
 * POST /api/payments/:paymentId/process
 * Processar pagamento
 */
router.post('/:paymentId/process', async (req, res) => {
  try {
    const { paymentId } = req.params;
    const transactionDetails = req.body;

    const payment = await PaymentService.processPayment(paymentId, transactionDetails);

    res.status(200).json({
      success: true,
      message: 'Pagamento enviado para processamento',
      data: payment
    });
  } catch (error) {
    console.error('[PaymentController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao processar pagamento',
      details: error.message
    });
  }
});

/**
 * POST /api/payments/:paymentId/complete
 * Completar pagamento (normalmente chamado por webhook do gateway)
 */
router.post('/:paymentId/complete', async (req, res) => {
  try {
    const { paymentId } = req.params;

    const payment = await PaymentService.completePayment(paymentId);

    res.status(200).json({
      success: true,
      message: 'Pagamento completado com sucesso',
      data: payment
    });
  } catch (error) {
    console.error('[PaymentController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao completar pagamento',
      details: error.message
    });
  }
});

/**
 * POST /api/payments/:paymentId/fail
 * Marcar pagamento como falho
 */
router.post('/:paymentId/fail', async (req, res) => {
  try {
    const { paymentId } = req.params;
    const { reason = 'Falha no processamento' } = req.body;

    const payment = await PaymentService.failPayment(paymentId, reason);

    res.status(200).json({
      success: true,
      message: 'Pagamento marcado como falho',
      data: payment
    });
  } catch (error) {
    console.error('[PaymentController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao marcar pagamento como falho',
      details: error.message
    });
  }
});

/**
 * GET /api/payments/:paymentId
 * Verificar status do pagamento
 */
router.get('/:paymentId', async (req, res) => {
  try {
    const { paymentId } = req.params;

    const payment = await PaymentService.verifyPayment(paymentId);

    res.status(200).json({
      success: true,
      data: payment
    });
  } catch (error) {
    console.error('[PaymentController] Erro:', error);
    res.status(404).json({
      error: 'Pagamento não encontrado',
      details: error.message
    });
  }
});

/**
 * GET /api/payments/budget/:budgetId
 * Listar pagamentos de um orçamento
 */
router.get('/budget/:budgetId', async (req, res) => {
  try {
    const { budgetId } = req.params;

    const payments = await PaymentService.getPaymentsByBudget(budgetId);

    res.status(200).json({
      success: true,
      data: payments
    });
  } catch (error) {
    console.error('[PaymentController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao listar pagamentos',
      details: error.message
    });
  }
});

export default router;
