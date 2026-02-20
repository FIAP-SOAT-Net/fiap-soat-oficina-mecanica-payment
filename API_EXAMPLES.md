# Payment Service - Exemplos de Requisições

Esta arquivo contém exemplos de requisições HTTP que você pode testar com Postman, Insomnia ou curl.

## Endpoints Disponíveis

Base URL: `http://localhost:3000`

---

## Health Check

### GET /api/health
```http
GET /api/health HTTP/1.1
Host: localhost:3000
```

**Resposta:**
```json
{
  "status": "ok",
  "service": "payment-service",
  "timestamp": "2026-02-20T10:30:00.000Z",
  "uptime": 5.123
}
```

---

## 💰 ORÇAMENTOS (BUDGETS)

### 1. Criar Orçamento
```http
POST /api/budgets HTTP/1.1
Host: localhost:3000
Content-Type: application/json

{
  "customerId": "CUST-001",
  "customerEmail": "cliente@example.com",
  "customerName": "Carlos Silva",
  "vehicleInfo": {
    "licensePlate": "XYZ-9876",
    "brand": "Toyota",
    "model": "Corolla",
    "year": 2021
  },
  "items": [
    {
      "description": "Troca de Óleo e Filtro",
      "quantity": 1,
      "unitPrice": 150.00,
      "total": 150.00
    },
    {
      "description": "Alinhamento de Rodas",
      "quantity": 1,
      "unitPrice": 120.00,
      "total": 120.00
    },
    {
      "description": "Balanceamento de Pneus",
      "quantity": 4,
      "unitPrice": 40.00,
      "total": 160.00
    }
  ],
  "totalAmount": 430.00,
  "taxAmount": 0,
  "discountAmount": 30.00,
  "notes": "Cliente VIP - desconto de 10% aplicado"
}
```

**Resposta (201):**
```json
{
  "success": true,
  "message": "Orçamento criado com sucesso",
  "data": {
    "_id": "507f1f77bcf86cd799439011",
    "budgetId": "BUDGET-1708434600000-a1b2c3d4",
    "customerId": "CUST-001",
    "customerEmail": "cliente@example.com",
    "customerName": "Carlos Silva",
    "vehicleInfo": {
      "licensePlate": "XYZ-9876",
      "brand": "Toyota",
      "model": "Corolla",
      "year": 2021
    },
    "items": [
      {
        "description": "Troca de Óleo e Filtro",
        "quantity": 1,
        "unitPrice": 150.00,
        "total": 150.00
      },
      {
        "description": "Alinhamento de Rodas",
        "quantity": 1,
        "unitPrice": 120.00,
        "total": 120.00
      },
      {
        "description": "Balanceamento de Pneus",
        "quantity": 4,
        "unitPrice": 40.00,
        "total": 160.00
      }
    ],
    "totalAmount": 430.00,
    "taxAmount": 0,
    "discountAmount": 30.00,
    "status": "pending",
    "notes": "Cliente VIP - desconto de 10% aplicado",
    "createdAt": "2026-02-20T10:30:00.000Z",
    "updatedAt": "2026-02-20T10:30:00.000Z"
  }
}
```

---

### 2. Enviar Orçamento para Aprovação
```http
POST /api/budgets/BUDGET-1708434600000-a1b2c3d4/send HTTP/1.1
Host: localhost:3000
Content-Type: application/json
```

**O cliente receberá um email com o orçamento formatado**

---

### 3. Aprovar Orçamento
```http
POST /api/budgets/BUDGET-1708434600000-a1b2c3d4/approve HTTP/1.1
Host: localhost:3000
Content-Type: application/json
```

**Resposta (200):**
```json
{
  "success": true,
  "message": "Orçamento aprovado com sucesso",
  "data": {
    "budget": {
      "budgetId": "BUDGET-1708434600000-a1b2c3d4",
      "status": "approved",
      "approvedAt": "2026-02-20T10:31:00.000Z"
    },
    "serviceOrder": {
      "orderId": "ORDER-1708434620000-x1y2z3",
      "budgetId": "BUDGET-1708434600000-a1b2c3d4",
      "customerId": "CUST-001",
      "status": "pending_payment"
    }
  }
}
```

---

### 4. Rejeitar Orçamento
```http
POST /api/budgets/BUDGET-1708434600000-a1b2c3d4/reject HTTP/1.1
Host: localhost:3000
Content-Type: application/json

{
  "reason": "Cliente solicitou revisão de valores"
}
```

---

### 5. Obter Detalhes do Orçamento
```http
GET /api/budgets/BUDGET-1708434600000-a1b2c3d4 HTTP/1.1
Host: localhost:3000
```

---

### 6. Listar Orçamentos do Cliente
```http
GET /api/budgets/customer/CUST-001 HTTP/1.1
Host: localhost:3000
```

---

## 💳 PAGAMENTOS (PAYMENTS)

### 1. Registrar Pagamento
```http
POST /api/payments HTTP/1.1
Host: localhost:3000
Content-Type: application/json

{
  "budgetId": "BUDGET-1708434600000-a1b2c3d4",
  "customerId": "CUST-001",
  "amount": 430.00,
  "paymentMethod": "credit_card",
  "orderId": "ORDER-1708434620000-x1y2z3"
}
```

**Métodos Disponíveis:**
- `credit_card` - Cartão de Crédito
- `debit_card` - Cartão de Débito
- `pix` - PIX
- `boleto` - Boleto Bancário
- `bank_transfer` - Transferência Bancária

---

### 2. Processar Pagamento
```http
POST /api/payments/PAY-1708434650000-p1q2r3/process HTTP/1.1
Host: localhost:3000
Content-Type: application/json

{
  "authorizationCode": "AUTH654321",
  "installments": 1,
  "cardLastDigits": "4242"
}
```

**Resposta (200):**
```json
{
  "success": true,
  "message": "Pagamento enviado para processamento",
  "data": {
    "paymentId": "PAY-1708434650000-p1q2r3",
    "budgetId": "BUDGET-1708434600000-a1b2c3d4",
    "customerId": "CUST-001",
    "amount": 430.00,
    "paymentMethod": "credit_card",
    "status": "processing",
    "paymentDetails": {
      "transactionId": "TXN-1708434652000",
      "authorizationCode": "AUTH654321",
      "installments": 1,
      "cardLastDigits": "4242"
    },
    "processedAt": "2026-02-20T10:31:00.000Z"
  }
}
```

*Note: Após 2 segundos, o pagamento será automaticamente completado*

---

### 3. Completar Pagamento (Webhook do Gateway)
```http
POST /api/payments/PAY-1708434650000-p1q2r3/complete HTTP/1.1
Host: localhost:3000
Content-Type: application/json
```

---

### 4. Marcar Pagamento como Falho
```http
POST /api/payments/PAY-1708434650000-p1q2r3/fail HTTP/1.1
Host: localhost:3000
Content-Type: application/json

{
  "reason": "Cartão expirado"
}
```

---

### 5. Verificar Status do Pagamento
```http
GET /api/payments/PAY-1708434650000-p1q2r3 HTTP/1.1
Host: localhost:3000
```

**Resposta (200):**
```json
{
  "success": true,
  "data": {
    "paymentId": "PAY-1708434650000-p1q2r3",
    "budgetId": "BUDGET-1708434600000-a1b2c3d4",
    "customerId": "CUST-001",
    "amount": 430.00,
    "paymentMethod": "credit_card",
    "status": "completed",
    "paymentDetails": {
      "transactionId": "TXN-1708434652000",
      "authorizationCode": "AUTH654321",
      "installments": 1,
      "cardLastDigits": "4242"
    },
    "processedAt": "2026-02-20T10:31:00.000Z",
    "completedAt": "2026-02-20T10:31:02.000Z"
  }
}
```

---

### 6. Listar Pagamentos de um Orçamento
```http
GET /api/payments/budget/BUDGET-1708434600000-a1b2c3d4 HTTP/1.1
Host: localhost:3000
```

---

## 📦 ORDENS DE SERVIÇO (ORDERS)

### 1. Obter Detalhes da Ordem
```http
GET /api/orders/ORDER-1708434620000-x1y2z3 HTTP/1.1
Host: localhost:3000
```

**Resposta (200):**
```json
{
  "success": true,
  "data": {
    "_id": "507f1f77bcf86cd799439012",
    "orderId": "ORDER-1708434620000-x1y2z3",
    "budgetId": "BUDGET-1708434600000-a1b2c3d4",
    "customerId": "CUST-001",
    "paymentId": "PAY-1708434650000-p1q2r3",
    "status": "in_progress",
    "syncedWithOrderService": true,
    "lastSyncAt": "2026-02-20T10:31:02.000Z",
    "syncAttempts": 1,
    "createdAt": "2026-02-20T10:31:00.000Z",
    "updatedAt": "2026-02-20T10:31:02.000Z"
  }
}
```

---

### 2. Reprocessar Sincronizações Falhadas
```http
POST /api/orders/retry-syncs HTTP/1.1
Host: localhost:3000
Content-Type: application/json
```

**Resposta (200):**
```json
{
  "success": true,
  "message": "Processo de retry iniciado"
}
```

---

## 🔄 Fluxo Completo com cURL

```bash
#!/bin/bash

# 1. Criar orçamento
BUDGET=$(curl -s -X POST http://localhost:3000/api/budgets \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST-001",
    "customerEmail": "cliente@example.com",
    "customerName": "João Silva",
    "vehicleInfo": {"licensePlate": "ABC-1234", "brand": "Honda", "model": "Civic", "year": 2022},
    "items": [{"description": "Serviço", "quantity": 1, "unitPrice": 200, "total": 200}],
    "totalAmount": 200
  }')

BUDGET_ID=$(echo $BUDGET | jq -r '.data.budgetId')
echo "Budget: $BUDGET_ID"

# 2. Enviar para aprovação
curl -s -X POST http://localhost:3000/api/budgets/$BUDGET_ID/send

# 3. Aprovar
ORDER=$(curl -s -X POST http://localhost:3000/api/budgets/$BUDGET_ID/approve)
ORDER_ID=$(echo $ORDER | jq -r '.data.serviceOrder.orderId')
echo "Order: $ORDER_ID"

# 4. Registrar pagamento
PAYMENT=$(curl -s -X POST http://localhost:3000/api/payments \
  -H "Content-Type: application/json" \
  -d "{
    \"budgetId\": \"$BUDGET_ID\",
    \"customerId\": \"CUST-001\",
    \"amount\": 200,
    \"paymentMethod\": \"credit_card\",
    \"orderId\": \"$ORDER_ID\"
  }")

PAYMENT_ID=$(echo $PAYMENT | jq -r '.data.paymentId')
echo "Payment: $PAYMENT_ID"

# 5. Processar pagamento
curl -s -X POST http://localhost:3000/api/payments/$PAYMENT_ID/process \
  -H "Content-Type: application/json" \
  -d '{"authorizationCode": "AUTH123", "installments": 1, "cardLastDigits": "4242"}'

# 6. Aguardar conclusão automática
sleep 2

# 7. Verificar status
curl -s -X GET http://localhost:3000/api/payments/$PAYMENT_ID | jq '.data.status'
```

---

## Importar no Postman/Insomnia

Você pode copiar esses exemplos e importar em ferramentas como:
- [Postman](https://www.postman.com/)
- [Insomnia](https://insomnia.rest/)
- [REST Client VSCode](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

---

## Testar com REST Client (VSCode)

Instale a extensão "REST Client" e crie um arquivo `.http`:

```http
### Health Check
GET http://localhost:3000/api/health

### Create Budget
POST http://localhost:3000/api/budgets
Content-Type: application/json

{
  "customerId": "CUST-001",
  "customerEmail": "cliente@example.com",
  "customerName": "João Silva",
  "vehicleInfo": {
    "licensePlate": "ABC-1234",
    "brand": "Honda",
    "model": "Civic",
    "year": 2022
  },
  "items": [
    {
      "description": "Serviço Completo",
      "quantity": 1,
      "unitPrice": 250,
      "total": 250
    }
  ],
  "totalAmount": 250
}

### Send Budget
POST http://localhost:3000/api/budgets/BUDGET-1708434600000-a1b2c3d4/send
```

---

## Notas Importantes

- **Timestamps**: Todos os timestamps estão no formato ISO 8601
- **Moeda**: Valores monetários em Real (R$)
- **IDs**: Formato padrão com timestamp e UUID curto
- **Validação**: Campos obrigatórios são validados no backend
- **CORS**: Habilitado para qualquer origem (configure em produção)

---

## Erros Comuns

### 400 Bad Request
```json
{
  "error": "Campos obrigatórios faltando: customerId, customerEmail, customerName, items, totalAmount"
}
```

### 404 Not Found
```json
{
  "error": "Orçamento não encontrado",
  "details": "..."
}
```

### 500 Internal Server Error
```json
{
  "error": "Erro ao criar orçamento",
  "details": "..."
}
```
