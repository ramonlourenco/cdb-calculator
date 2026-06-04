# 🚀 Quick Start Guide

Bem-vindo ao **CDB Calculator**! Este é um guia rápido para começar.

## ⚡ 5 Minutos para Rodar

### Pré-requisitos
- Docker & Docker Compose instalados
- Porta 80, 8080, 3000 disponíveis

### Start
```bash
# 1. Acesse a pasta
cd cdb-calculator

# 2. Inicie tudo
docker-compose up --build

# 3. Aguarde (2-3 minutos para build)
```

### URLs Padrão

| O Quê | URL | Detalhes |
|-------|-----|---------|
| 🌐 Frontend | http://localhost | SPA Angular 17+ |
| 🔧 API | http://localhost:8080/swagger | Swagger OpenAPI |
| 📊 Logs | http://localhost:3000 | Grafana (admin/admin) |
| ❤️ Health | http://localhost:8080/health | Health check |

---

## 📝 Testando a API

### Via Browser (Swagger)
1. Abra http://localhost:8080/swagger
2. Click em "POST /api/cdbcalculator/calculate"
3. Clique "Try it out"
4. Insira JSON:
```json
{
  "initialValue": 1000,
  "months": 12
}
```
5. Clique "Execute"
6. Veja resultado e Correlation ID header

### Via cURL (Linux/Mac)
```bash
curl -X POST http://localhost:8080/api/cdbcalculator/calculate \
  -H "Content-Type: application/json" \
  -d '{"initialValue": 1000, "months": 12}'
```

### Via Script
```bash
# Linux/Mac
./test-api.sh

# Windows
test-api.bat
```

---

## 🌐 Usando o Frontend

1. Abra http://localhost
2. Insira **Valor Inicial:** 1000
3. Insira **Prazo (meses):** 12
4. Clique **Calculate**
5. Veja resultados (Bruto, IR, Líquido)

### Exemplos de Teste
| VI | Meses | Resultado Esperado | IR % |
|----|-------|-------------------|------|
| 1000 | 6 | ~1054 Bruto | 22.5% |
| 1000 | 12 | ~1109 Bruto | 20% |
| 1000 | 24 | ~1221 Bruto | 17.5% |
| 1000 | 36 | ~1346 Bruto | 15% |

---

## 📊 Verificando Logs (Grafana)

1. Abra http://localhost:3000
2. Login: **admin** / **admin**
3. Menu → **Explore**
4. Datasource: **Loki**
5. Query: `{job="cdb-backend"}`
6. Clique no botão **Logs** (não gráfico)

### Por Correlation ID
1. No Swagger, copie o `X-Correlation-ID` da response
2. No Grafana, mude query para:
```
{job="cdb-backend"} | json CorrelationId="<YOUR_ID>"
```

---

## 🧪 Rodando Testes

### Backend (.NET 8)
```bash
cd backend
dotnet restore
dotnet test

# Esperado: Todos os testes passam
```

### Frontend (Angular 17+)
```bash
cd frontend
npm install
npm test -- --watch=false

# Esperado: Todos os testes passam
```

---

## 🛑 Parando

```bash
# Parar containers
docker-compose down

# Limpar completamente
docker-compose down -v
./cleanup.sh  # ou cleanup.bat no Windows
```

## 🔗 Estrutura Rápida

```
cdb-calculator/
├── backend/          (.NET 8, Hexagonal)
├── frontend/         (Angular 17+, Signals)
├── observability/    (Grafana config)
└── docker-compose.yml
```

---

## ✅ Checklist

- [ ] Docker instalado? (`docker --version`)
- [ ] Portas livres? (80, 8080, 3000)
- [ ] `docker-compose up --build` rodando?
- [ ] http://localhost abrindo?
- [ ] http://localhost:8080/swagger disponível?
- [ ] Conseguiu calcular algo?

---

## 💡 Dicas

**Performance Lenta?**
- Primeira build leva 2-3 minutos
- Verifique `docker images` (muitas imagens?)
- Limpe com `docker system prune -a`

**Port Conflict?**
- Mude ports em `docker-compose.yml`
- Ou libere a porta: `sudo lsof -i :80`

**API não responde?**
- Aguarde health check passar
- Verifique `docker-compose logs cdb-backend`

---

