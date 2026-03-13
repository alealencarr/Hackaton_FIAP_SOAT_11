# ☁️ Guia de Deploy no Azure - FIAP X

Este guia detalha como fazer o deploy completo do sistema FIAP X no Microsoft Azure.

## 📋 Índice

1. [Pré-requisitos](#pré-requisitos)
2. [Arquitetura no Azure](#arquitetura-no-azure)
3. [Passo 1: Criar Resource Group](#passo-1-criar-resource-group)
4. [Passo 2: Azure SQL Database](#passo-2-azure-sql-database)
5. [Passo 3: Azure Service Bus (RabbitMQ)](#passo-3-azure-service-bus)
6. [Passo 4: Azure Storage Account](#passo-4-azure-storage-account)
7. [Passo 5: Azure App Service (API)](#passo-5-azure-app-service-api)
8. [Passo 6: Azure Container Instances (Worker)](#passo-6-azure-container-instances-worker)
9. [Passo 7: Configurar CI/CD](#passo-7-configurar-cicd)
10. [Passo 8: Deploy do Frontend (Vercel)](#passo-8-deploy-do-frontend-vercel)
11. [Custos Estimados](#custos-estimados)

---

## Pré-requisitos

- Conta no Azure com créditos disponíveis
- Azure CLI instalado (`az --version`)
- Docker instalado
- Conta no GitHub
- Conta no Vercel (para o frontend)

---

## Arquitetura no Azure

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              AZURE CLOUD                                     │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                        Resource Group: rg-fiapx                       │  │
│  │                                                                        │  │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────┐  │  │
│  │  │  App Service    │  │  Azure SQL      │  │  Service Bus        │  │  │
│  │  │  (API)          │  │  Database       │  │  (Mensageria)       │  │  │
│  │  │  fiapx-api      │  │  fiapx-db       │  │  fiapx-servicebus   │  │  │
│  │  └─────────────────┘  └─────────────────┘  └─────────────────────┘  │  │
│  │                                                                        │  │
│  │  ┌─────────────────┐  ┌─────────────────┐                            │  │
│  │  │  Container      │  │  Storage        │                            │  │
│  │  │  Instances      │  │  Account        │                            │  │
│  │  │  (Workers)      │  │  (Blobs)        │                            │  │
│  │  └─────────────────┘  └─────────────────┘                            │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                               VERCEL                                         │
│                        (Frontend React)                                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Passo 1: Criar Resource Group

```bash
# Login no Azure
az login

# Criar Resource Group
az group create \
  --name rg-fiapx \
  --location brazilsouth

# Verificar
az group show --name rg-fiapx
```

---

## Passo 2: Azure SQL Database

### Criar o servidor SQL
```bash
# Criar SQL Server
az sql server create \
  --name fiapx-sqlserver \
  --resource-group rg-fiapx \
  --location brazilsouth \
  --admin-user fiapxadmin \
  --admin-password "SuaSenhaForte@123"

# Criar regra de firewall para permitir serviços Azure
az sql server firewall-rule create \
  --resource-group rg-fiapx \
  --server fiapx-sqlserver \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Criar banco de dados
az sql db create \
  --resource-group rg-fiapx \
  --server fiapx-sqlserver \
  --name FiapXDb \
  --service-objective Basic \
  --backup-storage-redundancy Local
```

### Connection String
```
Server=tcp:fiapx-sqlserver.database.windows.net,1433;Initial Catalog=FiapXDb;Persist Security Info=False;User ID=fiapxadmin;Password=SuaSenhaForte@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

## Passo 3: Azure Service Bus

O Azure Service Bus substitui o RabbitMQ na nuvem.

```bash
# Criar namespace do Service Bus
az servicebus namespace create \
  --resource-group rg-fiapx \
  --name fiapx-servicebus \
  --location brazilsouth \
  --sku Basic

# Criar fila
az servicebus queue create \
  --resource-group rg-fiapx \
  --namespace-name fiapx-servicebus \
  --name video-processing

# Obter connection string
az servicebus namespace authorization-rule keys list \
  --resource-group rg-fiapx \
  --namespace-name fiapx-servicebus \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

> **Nota:** Para usar Azure Service Bus, você precisa trocar a implementação de RabbitMQ para Service Bus no código. Alternativamente, use CloudAMQP (RabbitMQ as a Service) que tem tier gratuito.

### Opção alternativa: CloudAMQP (RabbitMQ gratuito)
1. Acesse https://www.cloudamqp.com/
2. Crie uma conta gratuita (Little Lemur - Free)
3. Copie a URL de conexão AMQP

---

## Passo 4: Azure Storage Account

```bash
# Criar Storage Account
az storage account create \
  --name fiapxstorage \
  --resource-group rg-fiapx \
  --location brazilsouth \
  --sku Standard_LRS

# Criar containers para uploads e outputs
az storage container create \
  --name uploads \
  --account-name fiapxstorage

az storage container create \
  --name outputs \
  --account-name fiapxstorage

# Obter connection string
az storage account show-connection-string \
  --name fiapxstorage \
  --resource-group rg-fiapx \
  --query connectionString \
  --output tsv
```

---

## Passo 5: Azure App Service (API)

```bash
# Criar App Service Plan
az appservice plan create \
  --name fiapx-plan \
  --resource-group rg-fiapx \
  --location brazilsouth \
  --is-linux \
  --sku B1

# Criar Web App para a API
az webapp create \
  --resource-group rg-fiapx \
  --plan fiapx-plan \
  --name fiapx-api \
  --deployment-container-image-name ghcr.io/seu-usuario/fiapx-api:latest

# Configurar variáveis de ambiente
az webapp config appsettings set \
  --resource-group rg-fiapx \
  --name fiapx-api \
  --settings \
    ConnectionStrings__DefaultConnection="Server=tcp:fiapx-sqlserver.database.windows.net,1433;..." \
    Jwt__Key="SuaChaveJwtSuperSecreta123456789012" \
    Jwt__Issuer="FiapX" \
    Jwt__Audience="FiapX.Users" \
    RabbitMQ__HostName="sua-url-cloudamqp" \
    ASPNETCORE_ENVIRONMENT="Production"

# Habilitar logs
az webapp log config \
  --resource-group rg-fiapx \
  --name fiapx-api \
  --docker-container-logging filesystem
```

---

## Passo 6: Azure Container Instances (Worker)

```bash
# Criar Container Instance para o Worker
az container create \
  --resource-group rg-fiapx \
  --name fiapx-worker \
  --image ghcr.io/seu-usuario/fiapx-worker:latest \
  --cpu 1 \
  --memory 1.5 \
  --restart-policy Always \
  --environment-variables \
    ConnectionStrings__DefaultConnection="Server=tcp:fiapx-sqlserver.database.windows.net,1433;..." \
    RabbitMQ__HostName="sua-url-cloudamqp"

# Verificar logs
az container logs \
  --resource-group rg-fiapx \
  --name fiapx-worker
```

---

## Passo 7: Configurar CI/CD

### 7.1 Criar Azure Service Principal

```bash
az ad sp create-for-rbac \
  --name "fiapx-github-actions" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/rg-fiapx \
  --sdk-auth
```

Copie o JSON de saída.

### 7.2 Configurar GitHub Secrets

No seu repositório GitHub, vá em **Settings > Secrets and variables > Actions** e adicione:

| Secret | Valor |
|--------|-------|
| `AZURE_CREDENTIALS` | JSON do Service Principal |
| `AZURE_WEBAPP_NAME` | `fiapx-api` |
| `AZURE_SQL_CONNECTION_STRING` | Connection string do SQL |
| `RABBITMQ_HOSTNAME` | URL do CloudAMQP |
| `JWT_KEY` | Sua chave JWT |

### 7.3 Habilitar GitHub Container Registry

No repositório, vá em **Settings > Actions > General** e habilite:
- "Read and write permissions" para GITHUB_TOKEN

---

## Passo 8: Deploy do Frontend (Vercel)

### 8.1 Criar projeto no Vercel

1. Acesse https://vercel.com
2. Faça login com GitHub
3. Clique em "Add New Project"
4. Importe o repositório do frontend
5. Configure as variáveis de ambiente:

| Variável | Valor |
|----------|-------|
| `VITE_API_URL` | `https://fiapx-api.azurewebsites.net` |

### 8.2 Deploy automático

O Vercel faz deploy automático a cada push na branch main.

---

## Custos Estimados

| Recurso | SKU | Custo Estimado/Mês |
|---------|-----|-------------------|
| App Service | B1 | ~$13 |
| SQL Database | Basic | ~$5 |
| Container Instances | 1 vCPU, 1.5GB | ~$30 |
| Storage Account | LRS | ~$1 |
| Service Bus | Basic | ~$0.05 |
| **Total** | | **~$50/mês** |

> 💡 Com créditos Azure de estudante ($100), você consegue rodar por ~2 meses.

---

## Comandos Úteis

```bash
# Ver logs da API
az webapp log tail --resource-group rg-fiapx --name fiapx-api

# Ver logs do Worker
az container logs --resource-group rg-fiapx --name fiapx-worker --follow

# Reiniciar API
az webapp restart --resource-group rg-fiapx --name fiapx-api

# Reiniciar Worker
az container restart --resource-group rg-fiapx --name fiapx-worker

# Ver status dos recursos
az resource list --resource-group rg-fiapx --output table

# Deletar tudo (cuidado!)
az group delete --name rg-fiapx --yes --no-wait
```

---

## URLs Finais

Após o deploy, seus serviços estarão em:

| Serviço | URL |
|---------|-----|
| **API** | https://fiapx-api.azurewebsites.net |
| **Swagger** | https://fiapx-api.azurewebsites.net/swagger |
| **Frontend** | https://fiapx.vercel.app |
| **Docs** | https://seu-usuario.github.io/fiapx |

---

## Troubleshooting

### API não conecta no banco
- Verifique as regras de firewall do SQL Server
- Confirme a connection string

### Worker não processa vídeos
- Verifique se o RabbitMQ/Service Bus está acessível
- Verifique os logs: `az container logs --resource-group rg-fiapx --name fiapx-worker`

### Frontend não conecta na API
- Verifique CORS na API
- Confirme a variável `VITE_API_URL` no Vercel
