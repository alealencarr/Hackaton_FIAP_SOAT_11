# 🎬 FIAP X - API Principal

Sistema de processamento de vídeos com extração de frames.

## 🏗️ Arquitetura

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  fiapx-api  │────▶│  RabbitMQ   │────▶│fiapx-worker │
└─────────────┘     └─────────────┘     └─────────────┘
       │                   │
       │                   ▼
       │            ┌─────────────┐
       │            │ fiapx-email │
       │            └─────────────┘
       ▼
┌─────────────┐
│  SQL Server │
└─────────────┘
```

## 🚀 Executar Local

```bash
# Docker Compose (recomendado)
docker-compose up -d

# Ou manualmente
dotnet run --project src/FiapX.API
```

## 📡 Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | /api/auth/register | Registrar usuário |
| POST | /api/auth/login | Login |
| POST | /api/videos/upload | Upload de vídeo |
| GET | /api/videos | Listar vídeos |
| GET | /api/videos/{id} | Detalhes do vídeo |
| GET | /api/videos/{id}/download | Download ZIP |

## 🧪 Testes

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## ☁️ Deploy

Ver pasta `infra/` para Terraform e Kubernetes.
