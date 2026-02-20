.PHONY: help install dev docker-up docker-down docker-logs docker-build test clean

help:
	@echo "Payment Service - Commands:"
	@echo ""
	@echo "Setup:"
	@echo "  make install        - Instalar dependências"
	@echo "  make env           - Criar arquivo .env"
	@echo ""
	@echo "Development:"
	@echo "  make dev           - Rodar serviço em dev mode (local)"
	@echo "  make docker-dev    - Rodar com Docker em dev mode"
	@echo ""
	@echo "Docker:"
	@echo "  make docker-up     - Iniciar containers (docker-compose up -d)"
	@echo "  make docker-down   - Parar containers (docker-compose down)"
	@echo "  make docker-logs   - Ver logs dos containers"
	@echo "  make docker-build  - Rebuild da imagem"
	@echo "  make docker-ps     - Status dos containers"
	@echo ""
	@echo "Testing:"
	@echo "  make test-health   - Testar health check"
	@echo "  make test-api      - Executar teste completo da API"
	@echo ""
	@echo "Database:"
	@echo "  make db-shell      - Conectar ao MongoDB"
	@echo "  make db-reset      - Resetar banco de dados (remove volumes)"
	@echo ""
	@echo "Clean:"
	@echo "  make clean         - Remover node_modules e .env"
	@echo "  make clean-docker  - Remover containers e volumes"

install:
	npm install

env:
	cp .env.example .env
	@echo "✓ Arquivo .env criado (configure com suas credenciais)"

dev:
	npm run dev

docker-dev:
	docker-compose up -d
	@echo "✓ Containers iniciados"
	@echo "Services:"
	@echo "  - Payment Service: http://localhost:3000"
	@echo "  - MongoDB: localhost:27017"
	@echo "  - RabbitMQ: http://localhost:15672 (guest/guest)"

docker-up:
	docker-compose up -d
	@echo "✓ Containers iniciados"

docker-down:
	docker-compose down
	@echo "✓ Containers parados"

docker-logs:
	docker-compose logs -f payment-service

docker-logs-all:
	docker-compose logs -f

docker-build:
	docker-compose build --no-cache
	@echo "✓ Imagem rebuilda"

docker-ps:
	docker-compose ps

test-health:
	@echo "Testing Health Check..."
	@curl -s http://localhost:3000/api/health | jq .
	@echo ""

test-ready:
	@echo "Testing Readiness..."
	@curl -s http://localhost:3000/api/ready | jq .
	@echo ""

test-api:
	@echo "Running full API test..."
	@bash test-api.sh

db-shell:
	mongosh "mongodb://root:rootpassword@localhost:27017/payment_service?authSource=admin"

db-reset:
	docker-compose down -v
	@echo "✓ Volumes removidos"
	docker-compose up -d
	@echo "✓ Containers recriados"

logs-payment:
	docker logs -f payment-service

logs-mongo:
	docker logs -f payment-mongodb

logs-rabbitmq:
	docker logs -f payment-rabbitmq

clean:
	rm -rf node_modules
	rm -f .env
	@echo "✓ Limpo: node_modules e .env"

clean-docker:
	docker-compose down -v
	@echo "✓ Containers e volumes removidos"

clean-all: clean clean-docker
	@echo "✓ Limpeza completa realizada"

reset-db:
	@echo "Resetting database..."
	@mongosh "mongodb://root:rootpassword@localhost:27017/payment_service?authSource=admin" <<EOF
db.budgets.deleteMany({})
db.payments.deleteMany({})
db.serviceorders.deleteMany({})
print("✓ Collections cleared")
EOF

info:
	@echo "=== Payment Service Info ==="
	@echo ""
	@echo "Environment:"
	@echo "  Node: $$(node --version)"
	@echo "  npm: $$(npm --version)"
	@echo ""
	@echo "Services:"
	@echo "  - Express API"
	@echo "  - MongoDB"
	@echo "  - RabbitMQ"
	@echo ""
	@echo "Endpoints:"
	@echo "  - Health: http://localhost:3000/api/health"
	@echo "  - Budgets: http://localhost:3000/api/budgets"
	@echo "  - Payments: http://localhost:3000/api/payments"
	@echo "  - Orders: http://localhost:3000/api/orders"
	@echo ""
	@echo "Management:"
	@echo "  - RabbitMQ: http://localhost:15672 (guest/guest)"
	@echo "  - MongoDB: mongosh or MongoDB Compass"
