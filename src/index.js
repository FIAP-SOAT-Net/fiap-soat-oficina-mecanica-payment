import express from 'express';
import mongoose from 'mongoose';
import 'dotenv/config';
import config from './config.js';
import RabbitMQService from './services/RabbitMQService.js';
import PaymentService from './services/PaymentService.js';
import budgetRoutes from './routes/budgets.js';
import paymentRoutes from './routes/payments.js';
import orderRoutes from './routes/orders.js';
import healthRoutes from './routes/health.js';

const app = express();

// ====== MIDDLEWARE ======
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// CORS
app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Authorization');
  res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  next();
});

// Request logging
app.use((req, res, next) => {
  console.log(`[${new Date().toISOString()}] ${req.method} ${req.path}`);
  next();
});

// ====== ROUTES ======
app.use('/api/health', healthRoutes);
app.use('/api/budgets', budgetRoutes);
app.use('/api/payments', paymentRoutes);
app.use('/api/orders', orderRoutes);

// Default route
app.get('/', (req, res) => {
  res.json({
    service: 'Payment Service',
    version: '1.0.0',
    status: 'running',
    endpoints: {
      health: '/api/health',
      budgets: '/api/budgets',
      payments: '/api/payments',
      orders: '/api/orders'
    }
  });
});

// ====== ERROR HANDLING ======
app.use((err, req, res, next) => {
  console.error('[ERROR]', err);
  res.status(500).json({
    error: 'Internal Server Error',
    details: process.env.NODE_ENV === 'development' ? err.message : undefined
  });
});

app.use((req, res) => {
  res.status(404).json({ error: 'Route not found' });
});

// ====== DATABASE CONNECTION ======
async function connectDatabase() {
  try {
    await mongoose.connect(config.mongodb.uri);
    console.log('[MongoDB] Conectado com sucesso');
  } catch (error) {
    console.error('[MongoDB] Erro ao conectar:', error);
    setTimeout(connectDatabase, 5000);
  }
}

// ====== RABBITMQ CONNECTION ======
async function connectRabbitMQ() {
  await RabbitMQService.connect();
}

// ====== RETRY SCHEDULED TASK ======
function startRetryScheduler() {
  // Executar retry a cada 30 segundos
  setInterval(async () => {
    try {
      await PaymentService.retryFailedSyncs();
    } catch (error) {
      console.error('[Scheduler] Erro no retry:', error);
    }
  }, 30000);
  
  console.log('[Scheduler] Iniciado - Retry a cada 30s');
}

// ====== SERVER STARTUP ======
async function startServer() {
  try {
    // Conectar ao banco de dados
    await connectDatabase();
    
    // Conectar ao RabbitMQ
    await connectRabbitMQ();
    
    // Iniciar scheduler de retry
    startRetryScheduler();
    
    // Iniciar servidor
    const PORT = config.port;
    app.listen(PORT, () => {
      console.log(`\n[✓] Payment Service rodando na porta ${PORT}`);
      console.log(`[✓] Ambiente: ${config.nodeEnv}`);
      console.log(`[✓] MongoDB: ${config.mongodb.uri}`);
      console.log(`[✓] RabbitMQ: ${config.rabbitmq.url}`);
      console.log(`[✓] Order Service URL: ${config.externalServices.orderServiceUrl}`);
      console.log('\nEndpoints disponíveis:');
      console.log('  GET  /api/health');
      console.log('  POST /api/budgets');
      console.log('  POST /api/budgets/:budgetId/send');
      console.log('  POST /api/budgets/:budgetId/approve');
      console.log('  POST /api/payments');
      console.log('  POST /api/payments/:paymentId/process');
      console.log('  POST /api/payments/:paymentId/complete');
      console.log('  GET  /api/payments/:paymentId');
    });
  } catch (error) {
    console.error('Erro ao iniciar servidor:', error);
    process.exit(1);
  }
}

// Graceful shutdown
process.on('SIGTERM', async () => {
  console.log('[SIGTERM] Encerrando gracefully...');
  await RabbitMQService.close();
  await mongoose.disconnect();
  process.exit(0);
});

process.on('SIGINT', async () => {
  console.log('[SIGINT] Encerrando gracefully...');
  await RabbitMQService.close();
  await mongoose.disconnect();
  process.exit(0);
});

startServer();
