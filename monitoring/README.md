# 📊 Monitoramento FIAP X - Prometheus + Grafana

## Estrutura de Arquivos

```
FiapX-Monitoring/
├── docker-compose.monitoring.yml
├── prometheus/
│   └── prometheus.yml
├── grafana/
│   ├── provisioning/
│   │   ├── datasources/
│   │   │   └── datasources.yml
│   │   └── dashboards/
│   │       └── dashboards.yml
│   └── dashboards/
│       └── fiapx-dashboard.json
└── loki/
    └── loki-config.yml
```

## 1️⃣ Configurar a API para expor métricas

### Passo 1: Adicionar pacote NuGet

No projeto `FiapX.API`, adicione o pacote:

```bash
cd src/FiapX.API
dotnet add package prometheus-net.AspNetCore
```

### Passo 2: Configurar no Program.cs

Adicione no início do arquivo:
```csharp
using Prometheus;
```

Adicione após `var app = builder.Build();`:
```csharp
// Métricas do Prometheus
app.UseHttpMetrics();
app.MapMetrics(); // Expõe endpoint /metrics
```

### Passo 3 (Opcional): Adicionar métricas customizadas

Crie um arquivo `src/FiapX.API/Metrics/VideoMetrics.cs`:

```csharp
using Prometheus;

namespace FiapX.API.Metrics;

public static class VideoMetrics
{
    public static readonly Counter VideosUploaded = Metrics
        .CreateCounter("videos_uploaded_total", "Total de vídeos enviados para processamento");

    public static readonly Counter VideosProcessed = Metrics
        .CreateCounter("videos_processed_total", "Total de vídeos processados com sucesso");

    public static readonly Counter VideosFailed = Metrics
        .CreateCounter("videos_failed_total", "Total de vídeos que falharam no processamento");

    public static readonly Histogram VideoProcessingDuration = Metrics
        .CreateHistogram("video_processing_duration_seconds", "Tempo de processamento de vídeos",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(1, 2, 10) // 1s, 2s, 4s, 8s, ...
            });
}
```

Use nos endpoints/worker:
```csharp
// No upload
VideoMetrics.VideosUploaded.Inc();

// No worker quando completar
VideoMetrics.VideosProcessed.Inc();

// No worker quando falhar
VideoMetrics.VideosFailed.Inc();

// Para medir duração
using (VideoMetrics.VideoProcessingDuration.NewTimer())
{
    // código de processamento
}
```

## 2️⃣ Subir o Monitoramento

```bash
# Na pasta FiapX-Monitoring
docker-compose -f docker-compose.monitoring.yml up -d
```

## 3️⃣ Acessar os Dashboards

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **Grafana** | http://localhost:3000 | admin / admin123 |
| **Prometheus** | http://localhost:9090 | - |
| **Loki** | http://localhost:3100 | - |

## 4️⃣ Verificar se está funcionando

1. Acesse http://localhost:5000/metrics - deve mostrar as métricas da API
2. Acesse http://localhost:9090/targets - deve mostrar a API como "UP"
3. Acesse http://localhost:3000 - Dashboard do Grafana

## 📈 O que o Dashboard mostra

- **Total de Requisições (24h)** - Quantidade de requests na API
- **Latência P95** - Tempo de resposta (95º percentil)
- **Vídeos Processados (24h)** - Quantos vídeos foram processados
- **Vídeos com Falha (24h)** - Quantos falharam
- **Gráfico de Requisições/segundo** - Por endpoint
- **Gráfico de Latência** - P50, P95, P99
- **Requisições por Status Code** - 200, 400, 500, etc.
- **CPU e Memória** - Uso de recursos

## 🛑 Parar o Monitoramento

```bash
docker-compose -f docker-compose.monitoring.yml down
```

## 🗑️ Limpar dados (reset completo)

```bash
docker-compose -f docker-compose.monitoring.yml down -v
```
