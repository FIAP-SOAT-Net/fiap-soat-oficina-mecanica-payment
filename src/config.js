export default {
  port: process.env.PORT || 3000,
  nodeEnv: process.env.NODE_ENV || 'development',
  
  mongodb: {
    uri: process.env.MONGODB_URI || 'mongodb://root:rootpassword@localhost:27017/payment_service?authSource=admin'
  },
  
  rabbitmq: {
    url: process.env.RABBITMQ_URL || 'amqp://guest:guest@localhost:5672'
  },
  
  mail: {
    host: process.env.MAIL_HOST || 'smtp.gmail.com',
    port: parseInt(process.env.MAIL_PORT) || 587,
    user: process.env.MAIL_USER || '',
    password: process.env.MAIL_PASSWORD || '',
    from: process.env.MAIL_FROM || 'noreply@oficina-mecanica.com'
  },
  
  externalServices: {
    orderServiceUrl: process.env.ORDER_SERVICE_URL || 'http://localhost:3001'
  },
  
  security: {
    jwtSecret: process.env.JWT_SECRET || 'dev-secret-key',
    apiKey: process.env.API_KEY || 'dev-api-key'
  }
};
