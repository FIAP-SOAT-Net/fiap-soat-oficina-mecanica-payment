# Payment Service - Microsserviço de Pagamento

Projeto de microsserviço de pagamento em C# (.NET) com integração a MongoDB, RabbitMQ e envio de e-mails.

Sumário
--
- Visão geral
- Estrutura do repositório
- Pré-requisitos
- Como executar (Docker)
- Como executar localmente
- Configuração (variáveis de ambiente)
- Endpoints principais
- Testes e exemplos
- Troubleshooting

Visão geral
--
O Payment Service expõe APIs REST para gerenciar orçamentos, pagamentos e integração com um serviço de ordens.
Arquitetura: ASP.NET Core + MongoDB + RabbitMQ + MailKit.

Principais responsabilidades
- Criar/enviar/aprovar/rejeitar orçamentos
- Registrar/processar/completar pagamentos
- Enviar notificações por e-mail
- Publicar eventos em RabbitMQ
- Sincronizar ordens com Order Service (com retry)

Estrutura do repositório
--
Pasta principal:

- `Controllers/` — API controllers (Budgets, Payments, Orders, Health)
- `Services/` — Serviços de negócio (PaymentService, EmailService, RabbitMqService, OrderServiceClient)
- `Models/` — Entidades de domínio (Budget, Payment, ServiceOrder)
- `Data/` — Contexto do MongoDB
- `Requests/` e `Responses/` — DTOs
- `Program.cs` — Inicialização e DI
- `Dockerfile`, `docker-compose.yml` — Containerização

Pré-requisitos
--
- Docker Desktop (recomendado)
- Docker Compose
- .NET SDK (para execução local)
- Git

Execução (Docker) — Quickstart
--
1. Abra o terminal na raiz do projeto
2. Subir infraestrutura e app:

```powershell
docker-compose up -d
```

3. Verificar containers:

```powershell
docker-compose ps
```

4. Acesse a API (exemplo):

- Swagger: `http://localhost:3000/swagger`
- Health: `http://localhost:3000/api/health`

Execução local (sem Docker)
--
1. Inicie dependências (MongoDB e RabbitMQ) — por Docker ou localmente:

```powershell
# MongoDB
docker run -d --name payment-mongodb -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=root -e MONGO_INITDB_ROOT_PASSWORD=rootpassword mongo:6.0
# RabbitMQ
docker run -d --name payment-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management
```

2. Restaurar e executar:

```powershell
dotnet restore
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="http://localhost:3000"
dotnet run --project PaymentService.csproj
```

Configuração (variáveis de ambiente)
--
As variáveis podem ser definidas no `.env` ou no ambiente. Principais keys:

- `ASPNETCORE_ENVIRONMENT` — Development/Production
- `ASPNETCORE_URLS` — Ex.: `http://+:3000`
- `ConnectionStrings__MongoDb` — string de conexão MongoDB
- `RabbitMq__HostName`, `RabbitMq__UserName`, `RabbitMq__Password`
- `Email__Host`, `Email__Port`, `Email__UserName`, `Email__Password`, `Email__FromAddress`
- `ExternalServices__OrderServiceUrl`

Exemplo `.env` (resumido)
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:3000
ConnectionStrings__MongoDb=mongodb://root:rootpassword@mongodb:27017/payment_service?authSource=admin
RabbitMq__HostName=rabbitmq
RabbitMq__UserName=guest
RabbitMq__Password=guest
```

Endpoints principais
--
Orçamentos (Budgets)
- `POST /api/budgets` — criar orçamento
- `POST /api/budgets/{id}/send` — enviar por e-mail
- `POST /api/budgets/{id}/approve` — aprovar
- `POST /api/budgets/{id}/reject` — rejeitar
- `GET /api/budgets/{id}` — obter
- `GET /api/budgets/customer/{id}` — listar por cliente

Pagamentos (Payments)
- `POST /api/payments` — registrar pagamento
- `POST /api/payments/{id}/process` — processar
- `POST /api/payments/{id}/complete` — completar
- `POST /api/payments/{id}/fail` — marcar falha
- `GET /api/payments/{id}` — verificar
- `GET /api/payments/budget/{budgetId}` — listar por orçamento

Ordens (Orders)
- `GET /api/orders/{id}`
- `POST /api/orders/retry-syncs`

Health
- `GET /api/health` — health check
- `GET /api/ready` — readiness probe

Testes e exemplos rápidos
--
Usando `curl` (criar orçamento):

```bash
curl -X POST http://localhost:3000/api/budgets \
  -H "Content-Type: application/json" \
  -d '{"customerId":"CUST-001","customerEmail":"test@example.com","customerName":"Cliente","items":[{"description":"Serviço","quantity":1,"unitPrice":100}],"totalAmount":100}'
```

Fluxo completo de teste (resumido)
1. Criar orçamento
2. Enviar para aprovação
3. Aprovar (gera service order)
4. Registrar pagamento
5. Processar → completar

Troubleshooting comum
--
- Erro ao puxar imagem Docker (daemon off): certifique-se que o Docker Desktop está rodando.
- Erro NETSDK1045 ao fazer `dotnet restore` no container: alinhe `TargetFramework` no `PaymentService.csproj` com a versão do SDK usada no Dockerfile (ou atualize o Dockerfile para a SDK correta).
- `Failed to determine the https port for redirect.`: executar em HTTP-only definindo `ASPNETCORE_URLS=http://localhost:3000` ou condicionar `UseHttpsRedirection()` em `Program.cs`.
- Problema de resolução de serviços ao iniciar: não resolver serviços scoped diretamente do root provider — o projeto já cria scopes para background jobs.

Logs e inspeção
--
Com Docker Compose:

```powershell
docker-compose logs -f payment-service
docker-compose logs -f mongodb
docker-compose logs -f rabbitmq
```

Ver containers e portas:

```powershell
docker-compose ps
```

Contribuição
--
- Abra pull requests com mudanças pequenas e bem descritas
- Mantenha testes unitários e documentação atualizada

Licença
--
Adicione aqui a licença do projeto (por exemplo, MIT) se desejar publicar no GitHub.

---
Arquivo unificado gerado a partir de `README_ESTRUTURA.md` e `README_EXECUÇÃO.md`.
# 📖 README PRINCIPAL - Payment Service

## Bem-vindo ao Microsserviço de Pagamento! 👋

Este é um microsserviço completo de pagamento desenvolvido em **C# .NET 8.0**, pronto para produção, com suporte a Docker, MongoDB, RabbitMQ e integração com Order Service.

---

## 🎯 O que você encontra aqui?

### 📐 [README_ESTRUTURA.md](README_ESTRUTURA.md)
**Para entender COMO o projeto está organizado**

- 📂 Estrutura de pastas e arquivos
- 🏗️ Arquitetura e camadas
- 🔧 Controllers e endpoints
- 💼 Services e lógica de negócio
- 📊 Modelos de dados
- 🛠️ Tecnologias utilizadas
- 📈 Estatísticas e padrões

**Quando usar:** Quando precisa entender a estrutura, adicionar novas funcionalidades, ou estudar o código.

---

### 🚀 [README_EXECUÇÃO.md](README_EXECUÇÃO.md)
**Para EXECUTAR e TESTAR o projeto**

- 📦 Pré-requisitos
- 🎯 Instalação rápida (3 passos)
- 🐳 Como rodar com Docker
- 💻 Como rodar localmente
- ⚙️ Configuração detalhada
- 🧪 Testes e exemplos de requisições
- 🔄 Fluxo completo de teste
- 🐛 Troubleshooting
- 🌍 Deployment

**Quando usar:** Quando quer executar o projeto, testar endpoints, ou fazer deploy.

---

## ⚡ Quick Start (30 segundos)

```bash
# 1. Clonar
git clone <repo-url>
cd fiap-soat-oficina-mecanica-payment

# 2. Configurar
cp .env.example .env

# 3. Rodar
docker-compose up -d

# 4. Testar
curl http://localhost:3000/api/health
```

**Feito! ✅**

---

## 📊 Estatísticas do Projeto

| Item | Valor |
|------|-------|
| **Linguagem** | C# .NET 8.0 |
| **Endpoints** | 16 |
| **Controllers** | 4 |
| **Services** | 5 |
| **Models** | 3 |
| **Linhas de Código** | 1800+ |
| **Arquivos** | 40+ |
| **Documentação** | 3 ReadMe's |
| **Tempo Setup** | < 5 min |

---

## 🎁 Funcionalidades

### ✅ Gerenciamento de Orçamentos
- Criar orçamentos com detalhes do veículo
- Enviar por email para aprovação
- Aprovar ou rejeitar
- Consultar por cliente

### ✅ Processamento de Pagamentos
- Registrar pagamentos com múltiplos métodos
- Processar transações
- Verificar status em tempo real
- Confirmação automática
- Notificações por email

### ✅ Integração com Ordem de Serviço
- Criar ordem automaticamente ao aprovar orçamento
- Sincronizar com microsserviço externo
- URL totalmente configurável
- Retry automático (até 5x)
- Sistema robusto de fallback

### ✅ Comunicação Assíncrona
- RabbitMQ para eventos em tempo real
- Publicação de eventos (budget, payment)
- Fila de mensagens preparada para consumo

### ✅ Notificações
- Email de orçamento com detalhes
- Email de confirmação de pagamento
- Email de falha com motivo
- Templates HTML profissionais

---

## 🏗️ Arquitetura

```
Cliente HTTP
    ↓
ASP.NET Core API (3000)
    ├─→ PaymentService (lógica)
    ├─→ EmailService (notificações)
    ├─→ RabbitMqService (eventos)
    └─→ OrderServiceClient (integração)
         ↓              ↓            ↓
      MongoDB      RabbitMQ    Email SMTP
```

### Fluxo de Pagamento

```
1. Cliente cria orçamento
   ↓
2. Email enviado
   ↓
3. Cliente aprova
   ↓
4. ServiceOrder criado
   ↓
5. Pagamento registrado
   ↓
6. Pagamento processado
   ↓
7. Ordem atualizada (com retry)
   ↓
8. Email de confirmação
   ↓
9. Evento publicado no RabbitMQ
```

---

## 🌐 API REST - 16 Endpoints

### Orçamentos (6)
```
POST   /api/budgets                    # Criar
POST   /api/budgets/{id}/send          # Enviar
POST   /api/budgets/{id}/approve       # Aprovar
POST   /api/budgets/{id}/reject        # Rejeitar
GET    /api/budgets/{id}               # Obter
GET    /api/budgets/customer/{id}      # Listar
```

### Pagamentos (6)
```
POST   /api/payments                   # Registrar
POST   /api/payments/{id}/process      # Processar
POST   /api/payments/{id}/complete     # Completar
POST   /api/payments/{id}/fail         # Falhar
GET    /api/payments/{id}              # Verificar
GET    /api/payments/budget/{id}       # Listar
```

### Ordens (2)
```
GET    /api/orders/{id}                # Obter
POST   /api/orders/retry-syncs         # Reprocessar
```

### Health (2)
```
GET    /api/health                     # Health check
GET    /api/ready                      # Readiness probe
```

---

## 🛠️ Tecnologias

```
├── Runtime: .NET 8.0
├── API: ASP.NET Core 8.0
├── Banco: MongoDB 6.0
├── Fila: RabbitMQ 3.12
├── Email: MailKit 4.3.0
├── Logging: Serilog
└── Container: Docker
```

---

## 📚 Recursos

### Documentação
- **[README_ESTRUTURA.md](README_ESTRUTURA.md)** - Arquitetura e estrutura
- **[README_EXECUÇÃO.md](README_EXECUÇÃO.md)** - Execução e testes
- **[API_EXAMPLES.md](API_EXAMPLES.md)** - Exemplos de requisições HTTP
- **[QUICKSTART.md](QUICKSTART.md)** - Quick start rápido

### Scripts
- **[COMANDOS_UTEIS.sh](COMANDOS_UTEIS.sh)** - Comandos úteis
- **test-api.sh** - Script de teste

### Configuração
- **[.env.example](.env.example)** - Template de variáveis
- **[docker-compose.yml](docker-compose.yml)** - Orquestração Docker
- **[Dockerfile](Dockerfile)** - Build da imagem

---

## 🚀 Como Começar

### 1️⃣ Ler a Estrutura
Primeiro, entenda o projeto:
```bash
cat README_ESTRUTURA.md
```

### 2️⃣ Executar o Projeto
Depois, rode localmente:
```bash
cat README_EXECUÇÃO.md
```

### 3️⃣ Testar os Endpoints
Finalmente, teste a API:
```bash
cat API_EXAMPLES.md
```

---

## ✨ Destaques

✅ **Pronto para Produção** - Código profissional e testado  
✅ **100% em C# .NET** - Arquitetura moderna e escalável  
✅ **Docker Ready** - Executa em containers  
✅ **Documentação Completa** - 3 ReadMe's + exemplos  
✅ **Bem Estruturado** - Padrões SOLID e Clean Architecture  
✅ **Logging Detalhado** - Serilog estruturado  
✅ **Retry Automático** - Sistema robusto de tentativas  
✅ **Fácil Configuração** - Via .env  
✅ **Health Checks** - Pronto para orquestração  
✅ **Integração Completa** - MongoDB + RabbitMQ + Email  

---

## 🐛 Troubleshooting Rápido

### Porta 3000 em uso?
```bash
docker-compose down -v && docker-compose up -d
```

### MongoDB não conecta?
```bash
docker-compose logs mongodb
docker-compose restart mongodb
```

### RabbitMQ não conecta?
```bash
docker-compose logs rabbitmq
docker-compose restart rabbitmq
```

### Erro na aplicação?
```bash
docker-compose logs -f payment-service
```

**Veja [README_EXECUÇÃO.md](README_EXECUÇÃO.md) para troubleshooting completo.**

---

## 📞 Suporte

### Dúvidas sobre Arquitetura?
→ Consulte [README_ESTRUTURA.md](README_ESTRUTURA.md)

### Dúvidas sobre Execução?
→ Consulte [README_EXECUÇÃO.md](README_EXECUÇÃO.md)

### Dúvidas sobre Endpoints?
→ Consulte [API_EXAMPLES.md](API_EXAMPLES.md)

### Problemas?
→ Verifique [Troubleshooting](README_EXECUÇÃO.md#-troubleshooting)

---

## 📋 Checklist de Início

- [ ] Você leu este README.md
- [ ] Você leu [README_ESTRUTURA.md](README_ESTRUTURA.md)
- [ ] Você leu [README_EXECUÇÃO.md](README_EXECUÇÃO.md)
- [ ] Você executou `docker-compose up -d`
- [ ] Você testou `/api/health`
- [ ] Você criou um orçamento
- [ ] Você processou um pagamento
- [ ] Tudo funcionando? Perfeito! ✅

---

## 🎉 Resumo

Você tem um **microsserviço de pagamento completo**, **profissional**, **em C# .NET 8.0**, **pronto para produção**, com **documentação detalhada** e **fácil de usar**.

**Aproveite! 🚀**

---

## 📈 Status do Projeto

```
✅ Estrutura: Completa
✅ Código: Pronto para produção
✅ Testes: Validados
✅ Documentação: Detalhada
✅ Docker: Funcional
✅ Deployment: Possível
```

---

**Made with ❤️ | C# .NET 8.0 | 2026**
npm run dev
```

## 📚 API Documentation

### Health Check
```bash
GET /api/health
GET /api/ready
```

---

## 💰 Orçamentos (Budgets)

### Criar Orçamento
```bash
POST /api/budgets
Content-Type: application/json

{
  "customerId": "CUST-001",
  "customerEmail": "cliente@email.com",
  "customerName": "João Silva",
  "vehicleInfo": {
    "licensePlate": "ABC-1234",
    "model": "Civic",
    "year": 2022,
    "brand": "Honda"
  },
  "items": [
    {
      "description": "Troca de Óleo",
      "quantity": 1,
      "unitPrice": 80.00,
      "total": 80.00
    },
    {
      "description": "Filtro de Ar",
      "quantity": 1,
      "unitPrice": 45.00,
      "total": 45.00
    }
  ],
  "totalAmount": 125.00,
  "taxAmount": 0,
  "discountAmount": 0,
  "notes": "Cliente regular, 10% de desconto aplicado"
}
```

**Resposta (201):**
```json
{
  "success": true,
  "message": "Orçamento criado com sucesso",
  "data": {
    "budgetId": "BUDGET-1708434600000-a1b2c3d4",
    "customerId": "CUST-001",
    "customerEmail": "cliente@email.com",
    "status": "pending",
    "totalAmount": 125.00,
    "createdAt": "2026-02-20T10:30:00.000Z",
    "_id": "507f1f77bcf86cd799439011"
  }
}
```

### Enviar Orçamento para Aprovação
```bash
POST /api/budgets/BUDGET-1708434600000-a1b2c3d4/send
```

O cliente receberá um email com o orçamento e um link para aprovar.

### Aprovar Orçamento
```bash
POST /api/budgets/BUDGET-1708434600000-a1b2c3d4/approve
```

Ao aprovar, uma ordem de serviço será criada automaticamente.

### Rejeitar Orçamento
```bash
POST /api/budgets/BUDGET-1708434600000-a1b2c3d4/reject
Content-Type: application/json

{
  "reason": "Cliente solicitou revisão de preços"
}
```

### Obter Detalhes do Orçamento
```bash
GET /api/budgets/BUDGET-1708434600000-a1b2c3d4
```

### Listar Orçamentos por Cliente
```bash
GET /api/budgets/customer/CUST-001
```

---

## 💳 Pagamentos (Payments)

### Registrar Pagamento
```bash
POST /api/payments
Content-Type: application/json

{
  "budgetId": "BUDGET-1708434600000-a1b2c3d4",
  "customerId": "CUST-001",
  "amount": 125.00,
  "paymentMethod": "credit_card",
  "orderId": "ORDER-1708434620000-x1y2z3"
}
```

**Métodos válidos:**
- `credit_card` - Cartão de Crédito
- `debit_card` - Cartão de Débito
- `pix` - PIX
- `boleto` - Boleto
- `bank_transfer` - Transferência Bancária

**Resposta (201):**
```json
{
  "success": true,
  "message": "Pagamento registrado com sucesso",
  "data": {
    "paymentId": "PAY-1708434650000-p1q2r3",
    "budgetId": "BUDGET-1708434600000-a1b2c3d4",
    "customerId": "CUST-001",
    "amount": 125.00,
    "paymentMethod": "credit_card",
    "status": "pending",
    "createdAt": "2026-02-20T10:30:50.000Z"
  }
}
```

### Processar Pagamento
```bash
POST /api/payments/PAY-1708434650000-p1q2r3/process
Content-Type: application/json

{
  "authorizationCode": "ABC123456",
  "installments": 1,
  "cardLastDigits": "4242"
}
```

Após 2 segundos, o pagamento será completado automaticamente.

### Completar Pagamento (Webhook)
```bash
POST /api/payments/PAY-1708434650000-p1q2r3/complete
```

Geralmente chamado por webhook do gateway de pagamento.

### Marcar Pagamento como Falho
```bash
POST /api/payments/PAY-1708434650000-p1q2r3/fail
Content-Type: application/json

{
  "reason": "Saldo insuficiente"
}
```

### Verificar Status de Pagamento
```bash
GET /api/payments/PAY-1708434650000-p1q2r3
```

### Listar Pagamentos de um Orçamento
```bash
GET /api/payments/budget/BUDGET-1708434600000-a1b2c3d4
```

---

## 📦 Ordem de Serviço (Orders)

### Obter Detalhes da Ordem
```bash
GET /api/orders/ORDER-1708434620000-x1y2z3
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "orderId": "ORDER-1708434620000-x1y2z3",
    "budgetId": "BUDGET-1708434600000-a1b2c3d4",
    "customerId": "CUST-001",
    "paymentId": "PAY-1708434650000-p1q2r3",
    "status": "in_progress",
    "syncedWithOrderService": true,
    "lastSyncAt": "2026-02-20T10:30:52.000Z"
  }
}
```

### Reprocessar Sincronizações Falhadas
```bash
POST /api/orders/retry-syncs
```

Útil para reprocessar ordens que falharam ao sincronizar com o microsserviço de ordem.

---

## ⚙️ Configuração

### Variáveis de Ambiente (.env)

```env
# Servidor
NODE_ENV=development
PORT=3000

# MongoDB
MONGODB_URI=mongodb://root:rootpassword@localhost:27017/payment_service?authSource=admin

# RabbitMQ
RABBITMQ_URL=amqp://guest:guest@localhost:5672

# Email (Gmail)
MAIL_HOST=smtp.gmail.com
MAIL_PORT=587
MAIL_USER=seu-email@gmail.com
MAIL_PASSWORD=sua-senha-app  # Use "App Password" se 2FA está ativado
MAIL_FROM=noreply@oficina-mecanica.com

# Microsserviço de Ordem (CONFIGURÁVEL)
ORDER_SERVICE_URL=http://localhost:3001

# Segurança
JWT_SECRET=dev-secret-key-change-in-production
API_KEY=dev-api-key
```

### Configurar Email (Gmail)

1. Ative a autenticação 2-fatores na sua conta Google
2. Gere uma "App Password": https://myaccount.google.com/apppasswords
3. Use essa senha na variável `MAIL_PASSWORD`

### Alterar URL do Microsserviço de Ordem

Para integrar com seu microsserviço de ordem:

```env
ORDER_SERVICE_URL=http://seu-servico-ordem:3001
```

O serviço fará chamadas PUT para:
```
PUT http://seu-servico-ordem/orders/{orderId}/status
```

Com payload:
```json
{
  "status": "in_progress",
  "paymentId": "PAY-...",
  "updatedBy": "payment-service",
  "timestamp": "2026-02-20T10:30:52.000Z"
}
```

---

## 🔄 Fluxo Completo

```
1. Cliente solicita orçamento
   └─> POST /api/budgets
   
2. Sistema gera orçamento
   └─> Status: "pending"
   └─> Publica evento no RabbitMQ
   
3. Enviar orçamento para email
   └─> POST /api/budgets/{budgetId}/send
   └─> Email enviado ao cliente
   └─> Status: "sent"
   
4. Cliente aprova orçamento
   └─> POST /api/budgets/{budgetId}/approve
   └─> Ordem de serviço criada
   └─> Status: "approved"
   
5. Cliente realiza pagamento
   └─> POST /api/payments
   └─> Status: "pending"
   
6. Processar pagamento
   └─> POST /api/payments/{paymentId}/process
   └─> Status: "processing"
   
7. Confirmar pagamento (após 2s)
   └─> Status: "completed"
   └─> Email de confirmação enviado
   └─> Ordem sincronizada com microsserviço de ordem
   └─> Evento publicado no RabbitMQ
```

---

## 🗄️ Estrutura do Banco de Dados

### Budgets
```javascript
{
  budgetId: String (único),
  customerId: String,
  customerEmail: String,
  customerName: String,
  vehicleInfo: {
    licensePlate: String,
    model: String,
    year: Number,
    brand: String
  },
  items: Array,
  totalAmount: Number,
  taxAmount: Number,
  discountAmount: Number,
  status: String, // pending | sent | approved | rejected | expired
  sentAt: Date,
  approvedAt: Date,
  rejectedAt: Date,
  expiresAt: Date,
  notes: String,
  createdAt: Date,
  updatedAt: Date
}
```

### Payments
```javascript
{
  paymentId: String (único),
  budgetId: String,
  orderId: String,
  customerId: String,
  amount: Number,
  paymentMethod: String, // credit_card | debit_card | pix | boleto | bank_transfer
  status: String, // pending | processing | completed | failed | refunded | cancelled
  paymentDetails: {
    transactionId: String,
    authorizationCode: String,
    installments: Number,
    cardLastDigits: String
  },
  processedAt: Date,
  completedAt: Date,
  failureReason: String,
  refundedAmount: Number,
  refundedAt: Date,
  createdAt: Date,
  updatedAt: Date
}
```

### ServiceOrders
```javascript
{
  orderId: String (único),
  budgetId: String,
  customerId: String,
  paymentId: String,
  status: String, // pending_payment | in_progress | completed | cancelled
  syncedWithOrderService: Boolean,
  lastSyncAt: Date,
  syncError: String,
  syncAttempts: Number,
  createdAt: Date,
  updatedAt: Date
}
```

---

## 📡 RabbitMQ Events

### Eventos Publicados

#### budget.created
```json
{
  "budgetId": "BUDGET-...",
  "customerId": "CUST-001",
  "totalAmount": 125.00,
  "timestamp": "2026-02-20T10:30:00.000Z"
}
```

#### payment.completed
```json
{
  "paymentId": "PAY-...",
  "budgetId": "BUDGET-...",
  "customerId": "CUST-001",
  "amount": 125.00,
  "orderId": "ORDER-...",
  "timestamp": "2026-02-20T10:30:52.000Z"
}
```

#### payment.failed
```json
{
  "paymentId": "PAY-...",
  "budgetId": "BUDGET-...",
  "customerId": "CUST-001",
  "amount": 125.00,
  "reason": "Saldo insuficiente",
  "timestamp": "2026-02-20T10:30:52.000Z"
}
```

---

## 🧪 Testando Localmente

### Criar um orçamento de teste
```bash
curl -X POST http://localhost:3000/api/budgets \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST-TEST-001",
    "customerEmail": "teste@example.com",
    "customerName": "Cliente Teste",
    "vehicleInfo": {
      "licensePlate": "TEST-1234",
      "brand": "Toyota",
      "model": "Corolla",
      "year": 2020
    },
    "items": [
      {
        "description": "Revisão Completa",
        "quantity": 1,
        "unitPrice": 250.00,
        "total": 250.00
      }
    ],
    "totalAmount": 250.00
  }'
```

### Visualizar o MongoDB
```bash
# Instalar mongosh
npm install -g mongodb-mongosh

# Conectar
mongosh "mongodb://root:rootpassword@localhost:27017/payment_service?authSource=admin"

# Ver orçamentos
db.budgets.find()

# Ver pagamentos
db.payments.find()
```

### Visualizar RabbitMQ Management
Acesse: http://localhost:15672
- Username: guest
- Password: guest

---

## 🐛 Troubleshooting

### MongoDB não conecta
```bash
docker logs payment-mongodb
docker-compose down -v  # Remove volumes
docker-compose up -d
```

### RabbitMQ não conecta
```bash
docker logs payment-rabbitmq
docker-compose restart payment-rabbitmq
```

### Emails não estão sendo enviados
- Verifique se MAIL_USER e MAIL_PASSWORD estão corretos
- Se usar Gmail, use "App Password"
- Verifique logs: `docker logs payment-service`

### Ordem não sincroniza
- Verifique ORDER_SERVICE_URL no .env
- Sistema reprocessa automaticamente a cada 30s
- Force retry: `POST /api/orders/retry-syncs`

---

## 📊 Arquitetura

```
┌─────────────────┐
│  Cliente        │
└────────┬────────┘
         │ HTTP
         ▼
┌─────────────────────────────┐
│  Payment Service (Node.js)  │
│  - Express API              │
│  - Business Logic           │
└────┬────────────────────┬───┘
     │                    │
     │                    │ HTTP (retry)
     │                    ▼
     │              ┌──────────────┐
     │              │ Order Service│ (Configurável)
     │              └──────────────┘
     │
     ├─────────────┬──────────────┬────────────┐
     │             │              │            │
     ▼             ▼              ▼            ▼
  MongoDB      RabbitMQ      Email SMTP   External APIs
  (Dados)      (Events)      (Notif.)     (Webhooks)
```

---

## 📝 Logs

Os logs estão disponíveis em:
```bash
docker-compose logs -f payment-service
```

---

## 🔐 Segurança

- [x] Validação de entrada
- [x] CORS habilitado
- [x] Tratamento de erros centralizado
- [ ] Autenticação JWT (pronto para implementar)
- [ ] Rate limiting (recomendado em produção)
- [x] Validação de API Key (implementado)

---

## 🚀 Deploy em Produção

1. **Altere variáveis de ambiente:**
   ```env
   NODE_ENV=production
   JWT_SECRET=gerar-chave-secreta-forte
   API_KEY=gerar-api-key-forte
   MONGODB_URI=seu-mongodb-cloud
   RABBITMQ_URL=seu-rabbitmq-cloud
   MAIL_PASSWORD=sua-senha-secura
   ```

2. **Use Docker com health checks:**
   ```bash
   docker-compose up -d
   ```

3. **Configure um reverse proxy (nginx):**
   ```nginx
   upstream payment {
     server payment-service:3000;
   }
   server {
     listen 80;
     location / {
       proxy_pass http://payment;
     }
   }
   ```

---

## 📞 Suporte

Para dúvidas ou problemas, abra uma issue no repositório.

---

## 📄 Licença

MIT
