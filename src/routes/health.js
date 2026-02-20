import express from 'express';

const router = express.Router();

/**
 * GET /api/health
 * Health check endpoint
 */
router.get('/health', (req, res) => {
  res.status(200).json({
    status: 'ok',
    service: 'payment-service',
    timestamp: new Date().toISOString(),
    uptime: process.uptime()
  });
});

/**
 * GET /api/ready
 * Readiness check endpoint
 */
router.get('/ready', (req, res) => {
  // Aqui você poderia adicionar verificações de dependências
  // Por enquanto, apenas retornamos sucesso
  res.status(200).json({
    status: 'ready',
    service: 'payment-service',
    timestamp: new Date().toISOString()
  });
});

export default router;
