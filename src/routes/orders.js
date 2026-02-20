import express from 'express';
import PaymentService from '../services/PaymentService.js';

const router = express.Router();

// ====== SERVICE ORDER ENDPOINTS ======

/**
 * GET /api/orders/:orderId
 * Obter detalhes da ordem de serviço
 */
router.get('/:orderId', async (req, res) => {
  try {
    const { orderId } = req.params;

    const serviceOrder = await PaymentService.getServiceOrder(orderId);

    res.status(200).json({
      success: true,
      data: serviceOrder
    });
  } catch (error) {
    console.error('[ServiceOrderController] Erro:', error);
    res.status(404).json({
      error: 'Ordem não encontrada',
      details: error.message
    });
  }
});

/**
 * POST /api/orders/retry-syncs
 * Reprocessar sincronizações falhadas
 */
router.post('/retry-syncs', async (req, res) => {
  try {
    await PaymentService.retryFailedSyncs();

    res.status(200).json({
      success: true,
      message: 'Processo de retry iniciado'
    });
  } catch (error) {
    console.error('[ServiceOrderController] Erro:', error);
    res.status(500).json({
      error: 'Erro ao reprocessar syncs',
      details: error.message
    });
  }
});

export default router;
