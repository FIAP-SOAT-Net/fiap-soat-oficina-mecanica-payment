#!/bin/bash

# Script para testar o Payment Service

BASE_URL="http://localhost:3000"

echo "=== Payment Service - Test Script ==="
echo ""

# 1. Health Check
echo "1. Testing Health Check..."
curl -X GET "$BASE_URL/api/health" -H "Content-Type: application/json"
echo -e "\n"

# 2. Create Budget
echo "2. Creating Budget..."
BUDGET_RESPONSE=$(curl -X POST "$BASE_URL/api/budgets" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST-TEST-001",
    "customerEmail": "teste@example.com",
    "customerName": "João da Silva",
    "vehicleInfo": {
      "licensePlate": "ABC-1234",
      "brand": "Honda",
      "model": "Civic",
      "year": 2022
    },
    "items": [
      {
        "description": "Troca de Óleo Completa",
        "quantity": 1,
        "unitPrice": 120.00,
        "total": 120.00
      },
      {
        "description": "Filtro de Ar",
        "quantity": 1,
        "unitPrice": 45.00,
        "total": 45.00
      },
      {
        "description": "Filtro de Combustível",
        "quantity": 1,
        "unitPrice": 65.00,
        "total": 65.00
      }
    ],
    "totalAmount": 230.00,
    "taxAmount": 0,
    "discountAmount": 0,
    "notes": "Cliente prioritário - revisão preventiva"
  }')

echo "$BUDGET_RESPONSE"
BUDGET_ID=$(echo "$BUDGET_RESPONSE" | grep -o '"budgetId":"[^"]*' | cut -d'"' -f4)
echo "Budget ID: $BUDGET_ID"
echo -e "\n"

# 3. Get Budget Details
echo "3. Getting Budget Details..."
curl -X GET "$BASE_URL/api/budgets/$BUDGET_ID" \
  -H "Content-Type: application/json"
echo -e "\n"

# 4. Send Budget for Approval
echo "4. Sending Budget for Approval..."
curl -X POST "$BASE_URL/api/budgets/$BUDGET_ID/send" \
  -H "Content-Type: application/json"
echo -e "\n"

# 5. Approve Budget
echo "5. Approving Budget..."
APPROVE_RESPONSE=$(curl -X POST "$BASE_URL/api/budgets/$BUDGET_ID/approve" \
  -H "Content-Type: application/json")
echo "$APPROVE_RESPONSE"
ORDER_ID=$(echo "$APPROVE_RESPONSE" | grep -o '"orderId":"[^"]*' | cut -d'"' -f4)
echo "Order ID: $ORDER_ID"
echo -e "\n"

# 6. Register Payment
echo "6. Registering Payment..."
PAYMENT_RESPONSE=$(curl -X POST "$BASE_URL/api/payments" \
  -H "Content-Type: application/json" \
  -d "{
    \"budgetId\": \"$BUDGET_ID\",
    \"customerId\": \"CUST-TEST-001\",
    \"amount\": 230.00,
    \"paymentMethod\": \"credit_card\",
    \"orderId\": \"$ORDER_ID\"
  }")

echo "$PAYMENT_RESPONSE"
PAYMENT_ID=$(echo "$PAYMENT_RESPONSE" | grep -o '"paymentId":"[^"]*' | cut -d'"' -f4)
echo "Payment ID: $PAYMENT_ID"
echo -e "\n"

# 7. Process Payment
echo "7. Processing Payment..."
curl -X POST "$BASE_URL/api/payments/$PAYMENT_ID/process" \
  -H "Content-Type: application/json" \
  -d '{
    "authorizationCode": "AUTH123456",
    "installments": 1,
    "cardLastDigits": "4242"
  }'
echo -e "\n"

# 8. Wait for automatic completion
echo "8. Waiting 3 seconds for automatic payment completion..."
sleep 3

# 9. Check Payment Status
echo "9. Checking Payment Status..."
curl -X GET "$BASE_URL/api/payments/$PAYMENT_ID" \
  -H "Content-Type: application/json"
echo -e "\n"

# 10. Get Service Order
echo "10. Getting Service Order Details..."
curl -X GET "$BASE_URL/api/orders/$ORDER_ID" \
  -H "Content-Type: application/json"
echo -e "\n"

# 11. List Customer Budgets
echo "11. Listing Customer Budgets..."
curl -X GET "$BASE_URL/api/budgets/customer/CUST-TEST-001" \
  -H "Content-Type: application/json"
echo -e "\n"

echo "=== Test Completed ==="
