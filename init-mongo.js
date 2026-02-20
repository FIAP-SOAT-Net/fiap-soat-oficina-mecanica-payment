db.createDatabase('payment_service');

db = db.getSiblingDB('payment_service');

db.createCollection('budgets');
db.createCollection('payments');
db.createCollection('serviceorders');

// Índices para budgets
db.budgets.createIndex({ budgetId: 1 }, { unique: true });
db.budgets.createIndex({ customerId: 1 });
db.budgets.createIndex({ status: 1 });
db.budgets.createIndex({ createdAt: -1 });

// Índices para payments
db.payments.createIndex({ paymentId: 1 }, { unique: true });
db.payments.createIndex({ budgetId: 1 });
db.payments.createIndex({ customerId: 1 });
db.payments.createIndex({ status: 1 });
db.payments.createIndex({ createdAt: -1 });

// Índices para service orders
db.serviceorders.createIndex({ orderId: 1 }, { unique: true });
db.serviceorders.createIndex({ budgetId: 1 });
db.serviceorders.createIndex({ customerId: 1 });
db.serviceorders.createIndex({ status: 1 });
db.serviceorders.createIndex({ syncedWithOrderService: 1 });

print('Database payment_service initialized successfully!');
