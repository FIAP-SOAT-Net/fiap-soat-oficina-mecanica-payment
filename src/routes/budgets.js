import express from 'express';
import PaymentService from '../services/PaymentService.js';

const router = express.Router();

// ====== BUDGET ENDPOINTS ======

/**
 * POST /api/budgets
 * Gerar novo orçamento
 */
router.post('/', async (req, res) => {
  try {
    const {
      customerId,
      customerEmail,
      customerName,
      vehicleInfo,
      items,
      totalAmount,
      taxAmount = 0,
      discountAmount = 0,
      notes
    } = req.body;

    // Validação básica
    if (!customerId || !customerEmail || !customerName || !items || items.length === 0 || !totalAmount) {
      return res.status(400).json({
        error: 'Campos obrigatórios faltando: customerId, customerEmail, customerName, items, totalAmount'
      });
    }

    const budget = await PaymentService.generateBudget({
      customerId,
      customerEmail,
      customerName,
      vehicleInfo,
      items,
      totalAmount,
      taxAmount,
      discountAmount,
      notes
    });

    res.status(201).json({
      success: true,
      message: 'Orçamento criado com sucesso',
      data: budget
    });
  } catch (error) {
    console.error('[BudgetController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao criar orçamento',
      details: error.message
    });
  }
});

/**
 * POST /api/budgets/:budgetId/send
 * Enviar orçamento para aprovação por email
 */
router.post('/:budgetId/send', async (req, res) => {
  try {
    const { budgetId } = req.params;

    const budget = await PaymentService.sendBudgetForApproval(budgetId);

    res.status(200).json({
      success: true,
      message: 'Orçamento enviado com sucesso',
      data: budget
    });
  } catch (error) {
    console.error('[BudgetController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao enviar orçamento',
      details: error.message
    });
  }
});

/**
 * POST /api/budgets/:budgetId/approve
 * Aprovar orçamento
 */
router.post('/:budgetId/approve', async (req, res) => {
  try {
    const { budgetId } = req.params;

    const result = await PaymentService.approveBudget(budgetId);

    res.status(200).json({
      success: true,
      message: 'Orçamento aprovado com sucesso',
      data: result
    });
  } catch (error) {
    console.error('[BudgetController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao aprovar orçamento',
      details: error.message
    });
  }
});

/**
 * POST /api/budgets/:budgetId/reject
 * Rejeitar orçamento
 */
router.post('/:budgetId/reject', async (req, res) => {
  try {
    const { budgetId } = req.params;
    const { reason = '' } = req.body;

    const budget = await PaymentService.rejectBudget(budgetId, reason);

    res.status(200).json({
      success: true,
      message: 'Orçamento rejeitado com sucesso',
      data: budget
    });
  } catch (error) {
    console.error('[BudgetController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao rejeitar orçamento',
      details: error.message
    });
  }
});

/**
 * GET /api/budgets/:budgetId
 * Obter detalhes do orçamento
 */
router.get('/:budgetId', async (req, res) => {
  try {
    const { budgetId } = req.params;

    const budget = await PaymentService.getBudget(budgetId);

    res.status(200).json({
      success: true,
      data: budget
    });
  } catch (error) {
    console.error('[BudgetController] Erro:', error);
    res.status(404).json({
      error: 'Orçamento não encontrado',
      details: error.message
    });
  }
});

/**
 * GET /api/budgets/customer/:customerId
 * Listar orçamentos de um cliente
 */
router.get('/customer/:customerId', async (req, res) => {
  try {
    const { customerId } = req.params;

    const budgets = await PaymentService.listBudgetsByCustomer(customerId);

    res.status(200).json({
      success: true,
      data: budgets
    });
  } catch (error) {
    console.error('[BudgetController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao listar orçamentos',
      details: error.message
    });
  }
});

export default router;
