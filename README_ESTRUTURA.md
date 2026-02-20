# 📐 README ESTRUTURA - Payment Service

Documentação completa da arquitetura, estrutura de pastas e componentes do microsserviço de pagamento em **C# .NET 8.0**.

---

## 📋 Índice

1. [Visão Geral da Arquitetura](#visão-geral-da-arquitetura)
2. [Estrutura de Pastas](#estrutura-de-pastas)
3. [Componentes e Camadas](#componentes-e-camadas)
4. [Modelos de Dados](#modelos-de-dados)
5. [Serviços Implementados](#serviços-implementados)
6. [Controllers e Endpoints](#controllers-e-endpoints)
7. [Infraestrutura](#infraestrutura)
8. [Tecnologias Utilizadas](#tecnologias-utilizadas)

---

## 🏗️ Visão Geral da Arquitetura

### Diagrama Geral

```
┌──────────────────────────────────────────┐
│      CLIENT (Frontend / API Consumer)    │
└─────────────────────┬────────────────────┘
                      │ HTTP/REST
                      ▼
┌──────────────────────────────────────────┐
│    ASP.NET Core 8.0 - Payment Service    │
│                                          │
│  ┌────────────────────────────────────┐  │
│  │     4 Controllers (16 Endpoints)   │  │
│  │  - Budgets, Payments, Orders,      │  │
│  │    Health                          │  │
│  └────────────────────────────────────┘  │
│              ↓                            │
│  ┌────────────────────────────────────┐  │
│  │    5 Services (Business Logic)     │  │
│  │  - PaymentService, EmailService,   │  │
│  │    RabbitMqService, OrderClient    │  │
│  └────────────────────────────────────┘  │
│              ↓                            │
│  ┌────────────────────────────────────┐  │
│  │    3 Models (Domain Entities)      │  │
│  │  - Budget, Payment, ServiceOrder   │  │
│  └────────────────────────────────────┘  │
└────────┬──────────────┬────────────────┬──┘
         │              │                │
         ▼              ▼                ▼
      MongoDB      RabbitMQ         Email SMTP
```

### Padrão Arquitetural

- **Padrão**: Clean Architecture / Layered Architecture
- **DI Container**: ASP.NET Core Built-in
- **Async/Await**: Totalmente assíncrono
- **Configuração**: appsettings.json com ambiente específico

---

## 📂 Estrutura de Pastas

```
PaymentService/
│
├── Controllers/                    # API REST Controllers (4 arquivos)
│   ├── BudgetsController.cs       # 6 endpoints de orçamento
│   ├── PaymentsController.cs      # 6 endpoints de pagamento
│   ├── OrdersController.cs        # 2 endpoints de ordem
│   └── HealthController.cs        # 3 endpoints de health check
│
├── Models/                         # Domain Models (3 arquivos)
│   ├── Budget.cs                 # Modelo de orçamento (~90 linhas)
│   ├── Payment.cs                # Modelo de pagamento (~80 linhas)
│   └── ServiceOrder.cs           # Modelo de ordem de serviço (~50 linhas)
│
├── Services/                       # Business Logic (5 arquivos)
│   ├── IPaymentService.cs        # Interface de pagamento
│   ├── PaymentService.cs         # Implementação (~400 linhas)
│   ├── IEmailService.cs          # Interface de email
│   ├── EmailService.cs           # Implementação com MailKit (~140 linhas)
│   ├── IRabbitMqService.cs       # Interface de fila
│   ├── RabbitMqService.cs        # Implementação com RabbitMQ.Client (~140 linhas)
│   ├── IOrderServiceClient.cs    # Interface de integração
│   └── OrderServiceClient.cs     # Implementação com HttpClient (~70 linhas)
│
├── Data/                           # Database Context (1 arquivo)
│   ├── IMongoDbContext.cs        # Interface do contexto
│   └── MongoDbContext.cs         # Implementação MongoDB (~30 linhas)
│
├── Requests/                       # Request DTOs (4 arquivos)
│   ├── CreateBudgetRequest.cs
│   ├── CreatePaymentRequest.cs
│   ├── RejectBudgetRequest.cs
│   └── FailPaymentRequest.cs
│
├── Responses/                      # Response DTOs (2 arquivos)
│   ├── ApiResponse.cs
│   └── ApiErrorResponse.cs
│
├── Program.cs                      # Startup & DI Configuration (~80 linhas)
├── appsettings.json               # Configuração local
├── appsettings.Development.json    # Configuração Docker
├── PaymentService.csproj           # Projeto .NET com dependências
│
├── Dockerfile                      # Multi-stage build
├── docker-compose.yml              # Orquestração de containers
├── .env                            # Variáveis de ambiente
├── .env.example                    # Template de ambiente
├── .gitignore                      # Git ignore patterns
│
└── Documentation/
    ├── README.md                   # Principal
    ├── README_ESTRUTURA.md         # Este arquivo
    ├── README_EXECUÇÃO.md          # Execução e testes
    ├── QUICKSTART.md               # Quick start rápido
    ├── API_EXAMPLES.md             # Exemplos de requisições
    └── COMANDOS_UTEIS.sh           # Scripts úteis
```

---

## 🔧 Componentes e Camadas

### 1. Camada de Apresentação (Controllers)

#### BudgetsController.cs
```
📋 6 Endpoints:
├── POST   /api/budgets                - Criar orçamento
├── POST   /api/budgets/{id}/send      - Enviar por email
├── POST   /api/budgets/{id}/approve   - Aprovar
├── POST   /api/budgets/{id}/reject    - Rejeitar
├── GET    /api/budgets/{id}           - Obter detalhes
└── GET    /api/budgets/customer/{id}  - Listar por cliente
```

#### PaymentsController.cs
```
💰 6 Endpoints:
├── POST   /api/payments                    - Registrar
├── POST   /api/payments/{id}/process       - Processar
├── POST   /api/payments/{id}/complete      - Completar
├── POST   /api/payments/{id}/fail          - Marcar falha
├── GET    /api/payments/{id}               - Verificar
└── GET    /api/payments/budget/{budgetId}  - Listar por orçamento
```

#### OrdersController.cs
```
📦 2 Endpoints:
├── GET    /api/orders/{id}            - Obter ordem
└── POST   /api/orders/retry-syncs     - Reprocessar
```

#### HealthController.cs
```
❤️ 3 Endpoints:
├── GET    /api/health                 - Health check
├── GET    /api/ready                  - Readiness probe
└── GET    /api                        - Index/info
```

**Total**: 16 endpoints

### 2. Camada de Negócio (Services)

#### PaymentService.cs (400+ linhas)
Interface: `IPaymentService`

**Métodos de Orçamento:**
- `GenerateBudgetAsync(request)` - Cria novo orçamento
- `SendBudgetForApprovalAsync(budgetId)` - Envia email de aprovação
- `ApproveBudgetAsync(budgetId)` - Aprova orçamento (cria ordem)
- `RejectBudgetAsync(budgetId, reason)` - Rejeita orçamento
- `GetBudgetAsync(budgetId)` - Obtém um orçamento
- `ListBudgetsByCustomerAsync(customerId)` - Lista por cliente

**Métodos de Pagamento:**
- `RegisterPaymentAsync(request)` - Registra novo pagamento
- `ProcessPaymentAsync(paymentId)` - Inicia processamento
- `CompletePaymentAsync(paymentId)` - Completa pagamento e sincroniza
- `FailPaymentAsync(paymentId, reason)` - Marca como falha
- `VerifyPaymentAsync(paymentId)` - Verifica status
- `GetPaymentsByBudgetAsync(budgetId)` - Lista por orçamento

**Métodos Internos:**
- `CreateServiceOrderAsync()` - Cria ordem internamente
- `UpdateOrderAfterPaymentAsync()` - Sincroniza com Order Service
- `RetryFailedSyncsAsync()` - Job agendado a cada 30s

#### EmailService.cs (140+ linhas)
Interface: `IEmailService`

**Métodos:**
- `SendBudgetEmailAsync(budget, recipient)` - Template HTML orçamento
- `SendPaymentConfirmationEmailAsync(payment, recipient)` - Template confirmação
- `SendPaymentFailureEmailAsync(payment, recipient, reason)` - Template erro

**Templates HTML:**
- Budget com detalhes do veículo e itens
- Payment Confirmation com ID e valor
- Payment Failure com motivo da falha

#### RabbitMqService.cs (140+ linhas)
Interface: `IRabbitMqService`

**Métodos:**
- `ConnectAsync()` - Conecta ao RabbitMQ
- `PublishBudgetCreatedAsync(budget)` - Publica evento de orçamento
- `PublishPaymentCompletedAsync(payment)` - Publica pagamento completo
- `PublishPaymentFailedAsync(payment)` - Publica pagamento falho
- `CloseAsync()` - Fecha conexão gracefully

**Configuração:**
- Exchange: `payment-events` (tipo: topic)
- Filas: `budget-generated`, `payment-completed`, `payment-failed`
- Routing keys: `budget.created`, `payment.completed`, `payment.failed`

#### OrderServiceClient.cs (70+ linhas)
Interface: `IOrderServiceClient`

**Métodos:**
- `UpdateOrderStatusAsync(orderId, status)` - PUT com retry
- `GetOrderDetailsAsync(orderId)` - GET do Order Service

**Features:**
- HttpClientFactory para gerenciamento de conexões
- Retry automático (5 tentativas)
- Timeout configurável (5s)
- Backoff exponencial

#### MongoDbContext.cs (30+ linhas)
Interface: `IMongoDbContext`

**Propriedades:**
- `IMongoCollection<Budget> Budgets` - Acesso a orçamentos
- `IMongoCollection<Payment> Payments` - Acesso a pagamentos
- `IMongoCollection<ServiceOrder> ServiceOrders` - Acesso a ordens

### 3. Camada de Dados (Models)

#### Budget.cs (~90 linhas)
```csharp
Properties:
├── BudgetId: string (unique)
├── CustomerId: string
├── CustomerEmail: string
├── CustomerName: string
├── VehicleInfo: object (brand, model, licensePlate, year)
├── Items: List<BudgetItem> (description, quantity, unitPrice, total)
├── TotalAmount: decimal
├── TaxAmount: decimal
├── DiscountAmount: decimal
├── Status: string (pending|sent|approved|rejected|expired)
├── ExpiresAt: DateTime
└── Timestamps: (CreatedAt, UpdatedAt)

Índices:
├── budgetId (unique)
├── customerId
├── status
└── createdAt (descending)
```

#### Payment.cs (~80 linhas)
```csharp
Properties:
├── PaymentId: string (unique)
├── BudgetId: string
├── CustomerId: string
├── OrderId: string
├── Amount: decimal
├── PaymentMethod: string (credit_card|debit_card|pix|boleto|bank_transfer)
├── Status: string (pending|processing|completed|failed|refunded)
├── PaymentDetails: object
│   ├── TransactionId: string
│   ├── AuthorizationCode: string
│   ├── Installments: int
│   └── CardLastDigits: string
├── FailureReason: string
└── Timestamps: (ProcessedAt, CompletedAt)

Índices:
├── paymentId (unique)
├── budgetId
├── customerId
├── status
└── createdAt (descending)
```

#### ServiceOrder.cs (~50 linhas)
```csharp
Properties:
├── OrderId: string (unique)
├── BudgetId: string
├── CustomerId: string
├── PaymentId: string
├── Status: string (pending|synced|failed)
├── SyncedWithOrderService: bool
├── LastSyncAt: DateTime
├── SyncError: string
└── SyncAttempts: int (max 5)

Índices:
├── orderId (unique)
├── budgetId
├── customerId
├── syncedWithOrderService
└── status
```

---

## 📊 Serviços Implementados

### Resumo de Funcionalidades

| Serviço | Funcionalidade | Status |
|---------|----------------|--------|
| **PaymentService** | Gerenciamento completo de orçamentos e pagamentos | ✅ |
| **EmailService** | Envio de notificações por email com templates | ✅ |
| **RabbitMqService** | Publicação de eventos assíncronos | ✅ |
| **OrderServiceClient** | Integração com microsserviço de ordens | ✅ |
| **MongoDbContext** | Acesso ao banco de dados | ✅ |

### Fluxos Implementados

#### Fluxo de Orçamento
```
1. Cliente cria orçamento
   ↓
2. Orçamento salvo em MongoDB
   ↓
3. Evento publicado em RabbitMQ
   ↓
4. Email enviado automaticamente
   ↓
5. Cliente aprova/rejeita
   ↓
6. ServiceOrder criado (se aprovado)
```

#### Fluxo de Pagamento
```
1. Cliente registra pagamento
   ↓
2. Pagamento salvo como "pending"
   ↓
3. Sistema processa (muda para "processing")
   ↓
4. Auto-completa após 2s
   ↓
5. Email de confirmação enviado
   ↓
6. Order Service sincronizado (com retry)
   ↓
7. Evento publicado em RabbitMQ
   ↓
8. ServiceOrder atualizado
```

---

## 🌐 Controllers e Endpoints

### Resumo de Endpoints

```
Total: 16 Endpoints

ORÇAMENTOS (6):
├── POST   /api/budgets
├── POST   /api/budgets/{id}/send
├── POST   /api/budgets/{id}/approve
├── POST   /api/budgets/{id}/reject
├── GET    /api/budgets/{id}
└── GET    /api/budgets/customer/{id}

PAGAMENTOS (6):
├── POST   /api/payments
├── POST   /api/payments/{id}/process
├── POST   /api/payments/{id}/complete
├── POST   /api/payments/{id}/fail
├── GET    /api/payments/{id}
└── GET    /api/payments/budget/{budgetId}

ORDENS (2):
├── GET    /api/orders/{id}
└── POST   /api/orders/retry-syncs

HEALTH (2):
├── GET    /api/health
└── GET    /api/ready
```

---

## 🐳 Infraestrutura

### Docker Compose Services

```yaml
Services:
├── MongoDB 6.0
│   ├── Porta: 27017
│   ├── Username: root
│   ├── Password: rootpassword
│   ├── Database: payment_service
│   └── Volume: mongodb_data
│
├── RabbitMQ 3.12
│   ├── Porta AMQP: 5672
│   ├── Porta Management: 15672
│   ├── Username: guest
│   ├── Password: guest
│   └── Volume: rabbitmq_data
│
└── Payment Service
    ├── Imagem: .NET 8.0 ASP.NET Core
    ├── Porta: 3000
    ├── Depends On: [mongodb, rabbitmq]
    └── Environment: appsettings.Development.json
```

### Build Multi-stage Dockerfile

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 3000
ENTRYPOINT ["dotnet", "PaymentService.dll"]
```

### Network

```
Rede: payment-network
├── payment-service (localhost:3000)
├── mongodb (mongodb:27017 dentro da rede)
└── rabbitmq (rabbitmq:5672 dentro da rede)
```

---

## 💾 Configuração e Variáveis

### appsettings.json (Local)
```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://root:rootpassword@localhost:27017/payment_service?authSource=admin"
  },
  "RabbitMq": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "seu-email@gmail.com",
    "Password": "seu-app-password",
    "FromAddress": "noreply@oficina-mecanica.com"
  },
  "ExternalServices": {
    "OrderServiceUrl": "http://localhost:3001"
  }
}
```

### appsettings.Development.json (Docker)
```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://root:rootpassword@mongodb:27017/payment_service?authSource=admin"
  },
  "RabbitMq": {
    "HostName": "rabbitmq",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

---

## 🛠️ Tecnologias Utilizadas

### Framework & Runtime
- **ASP.NET Core 8.0**
- **.NET 8.0 SDK**

### Banco de Dados
- **MongoDB 6.0**
- **MongoDB.Driver 2.21.0**

### Message Queue
- **RabbitMQ 3.12**
- **RabbitMQ.Client 6.6.0**

### Email
- **MailKit 4.3.0**

### Logging
- **Serilog 3.1.1**
- **Serilog.AspNetCore 8.0.1**

### Validação
- **FluentValidation 11.7.1**

### API Documentation
- **Swashbuckle 6.4.6** (Swagger/OpenAPI)

### Containerização
- **Docker**
- **Docker Compose**

---

## 📈 Estatísticas do Projeto

| Métrica | Valor |
|---------|-------|
| **Linguagem** | C# .NET 8.0 |
| **Arquivos de Código** | 12 |
| **Linhas de Código** | 1800+ |
| **Controllers** | 4 |
| **Services** | 5 |
| **Models** | 3 |
| **Endpoints** | 16 |
| **DTOs** | 6 |
| **Arquivos de Config** | 6 |
| **Documentação** | 10+ arquivos |
| **Total de Arquivos** | 40+ |

---

## 🎯 Padrões e Boas Práticas

✅ **Clean Architecture** - Separação clara de responsabilidades  
✅ **SOLID Principles** - Código modular e testável  
✅ **Async/Await** - Operações não-bloqueantes  
✅ **Dependency Injection** - Container ASP.NET Core  
✅ **Error Handling** - Tratamento centralizado  
✅ **Logging** - Serilog estruturado  
✅ **Configuration** - appsettings por ambiente  
✅ **Docker** - Containerização  
✅ **Health Checks** - Pronto para orquestração  

---

**Estrutura completa e profissional! 🎉**
