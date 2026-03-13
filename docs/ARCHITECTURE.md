# Documentação de Arquitetura - FIAP X

## 1. Visão Geral

O FIAP X é um sistema de processamento de vídeos que extrai frames e gera arquivos ZIP para download. Foi desenvolvido utilizando Clean Architecture em .NET 8, garantindo escalabilidade, manutenibilidade e testabilidade.

## 2. Decisões Arquiteturais

### 2.1 Clean Architecture

A escolha pela Clean Architecture se baseia em:
- **Independência de frameworks**: O domínio não depende de bibliotecas externas
- **Testabilidade**: Cada camada pode ser testada isoladamente
- **Independência de UI**: A API pode ser substituída sem afetar regras de negócio
- **Independência de banco de dados**: O SQL Server pode ser trocado facilmente

### 2.2 Mensageria com RabbitMQ

Para garantir resiliência e processamento assíncrono:
- Vídeos são enfileirados para processamento
- Workers consomem a fila de forma distribuída
- Em caso de falha, mensagens são reprocessadas
- Permite escalar workers horizontalmente

### 2.3 Autenticação JWT

- Tokens stateless para escalabilidade
- Não requer sessão no servidor
- Permite múltiplas instâncias da API

## 3. Diagramas

### 3.1 Diagrama de Componentes

```mermaid
graph TB
    subgraph "Presentation Layer"
        API[API REST]
        Swagger[Swagger/Scalar]
    end
    
    subgraph "Application Layer"
        UC[Use Cases]
        CTRL[Controllers]
        GW[Gateways]
        PRES[Presenters]
    end
    
    subgraph "Domain Layer"
        ENT[Entities]
        ENUM[Enums]
    end
    
    subgraph "Infrastructure Layer"
        DS[Data Sources]
        SVC[Services]
        DB[DbContext]
    end
    
    subgraph "External"
        SQL[(SQL Server)]
        RMQ[RabbitMQ]
        REDIS[(Redis)]
        SMTP[SMTP]
    end
    
    API --> CTRL
    CTRL --> UC
    UC --> GW
    GW --> DS
    DS --> DB
    DB --> SQL
    
    UC --> SVC
    SVC --> RMQ
    SVC --> SMTP
    SVC --> REDIS
    
    CTRL --> PRES
    GW --> ENT
```

### 3.2 Diagrama de Sequência - Upload de Vídeo

```mermaid
sequenceDiagram
    participant C as Client
    participant A as API
    participant S as Storage
    participant Q as RabbitMQ
    participant DB as Database
    
    C->>A: POST /api/videos/upload
    A->>A: Validar Token JWT
    A->>S: Salvar Vídeo
    S-->>A: Path do arquivo
    A->>DB: Criar registro (Status: Pending)
    DB-->>A: Video ID
    A->>Q: Publicar mensagem
    Q-->>A: ACK
    A-->>C: 201 Created (Video ID)
```

### 3.3 Diagrama de Sequência - Processamento

```mermaid
sequenceDiagram
    participant Q as RabbitMQ
    participant W as Worker
    participant DB as Database
    participant F as FFmpeg
    participant S as Storage
    participant E as Email Service
    
    Q->>W: Consumir mensagem
    W->>DB: Buscar vídeo
    DB-->>W: Video data
    W->>DB: Atualizar status (Processing)
    W->>F: Processar vídeo
    F-->>W: Frames extraídos
    W->>W: Criar ZIP
    W->>S: Salvar ZIP
    S-->>W: ZIP path
    W->>DB: Atualizar status (Completed)
    W->>E: Enviar notificação
    W->>Q: ACK mensagem
```

### 3.4 Diagrama de Classes - Domain

```mermaid
classDiagram
    class Video {
        +Guid Id
        +Guid UserId
        +string OriginalFileName
        +string StoragePath
        +VideoStatus Status
        +int? FrameCount
        +string? ZipPath
        +string? ErrorMessage
        +DateTime CreatedAt
        +DateTime? ProcessedAt
        +StartProcessing()
        +CompleteProcessing(frameCount, zipPath)
        +FailProcessing(errorMessage)
    }
    
    class User {
        +Guid Id
        +string Name
        +string Email
        +string PasswordHash
        +DateTime CreatedAt
        +ValidatePassword(password) bool
    }
    
    class VideoStatus {
        <<enumeration>>
        Pending
        Processing
        Completed
        Failed
    }
    
    Video --> VideoStatus
    Video --> User : belongs to
```

### 3.5 Diagrama de Infraestrutura

```mermaid
graph TB
    subgraph "Kubernetes Cluster"
        subgraph "API Pods"
            API1[API Pod 1]
            API2[API Pod 2]
            API3[API Pod 3]
        end
        
        subgraph "Worker Pods"
            W1[Worker 1]
            W2[Worker 2]
        end
        
        LB[Load Balancer]
    end
    
    subgraph "Data Layer"
        SQL[(SQL Server)]
        RMQ[RabbitMQ]
        REDIS[(Redis)]
        STORAGE[Storage Volume]
    end
    
    CLIENT[Client] --> LB
    LB --> API1
    LB --> API2
    LB --> API3
    
    API1 --> SQL
    API2 --> SQL
    API3 --> SQL
    
    API1 --> RMQ
    API2 --> RMQ
    API3 --> RMQ
    
    RMQ --> W1
    RMQ --> W2
    
    W1 --> SQL
    W2 --> SQL
    
    W1 --> STORAGE
    W2 --> STORAGE
```

## 4. Estrutura de Pastas

```
FiapX/
├── src/
│   ├── FiapX.API/                 # Camada de apresentação
│   │   ├── Endpoints/             # Minimal APIs
│   │   ├── Extensions/            # Configurações e middlewares
│   │   └── Program.cs
│   │
│   ├── FiapX.Application/         # Camada de aplicação
│   │   ├── Controllers/           # Orquestradores de use cases
│   │   ├── Gateways/              # Abstrações de dados
│   │   ├── Interfaces/            # Contratos
│   │   ├── Presenter/             # Transformação de dados
│   │   └── UseCases/              # Casos de uso
│   │
│   ├── FiapX.Domain/              # Camada de domínio
│   │   ├── Entities/              # Entidades de negócio
│   │   └── Enums/                 # Enumerações
│   │
│   ├── FiapX.Infrastructure/      # Camada de infraestrutura
│   │   ├── DataSources/           # Implementações de acesso a dados
│   │   ├── DbContexts/            # Entity Framework
│   │   ├── DbModels/              # Modelos de banco
│   │   ├── Services/              # Serviços externos
│   │   └── Migrations/            # Migrations EF
│   │
│   ├── FiapX.Shared/              # Código compartilhado
│   │   ├── DTO/                   # Data Transfer Objects
│   │   └── Result/                # Padrão Result
│   │
│   └── FiapX.Worker/              # Worker de processamento
│
├── tests/
│   └── FiapX.UnitTests/           # Testes unitários
│
├── docs/                          # Documentação
├── .github/workflows/             # CI/CD
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## 5. Fluxo de Dados

### 5.1 Camadas e Responsabilidades

| Camada | Responsabilidade |
|--------|------------------|
| **API** | Receber requisições HTTP, validar, rotear para controllers |
| **Application** | Orquestrar casos de uso, transformar dados |
| **Domain** | Regras de negócio, validações de entidade |
| **Infrastructure** | Acesso a dados, serviços externos |
| **Shared** | DTOs, interfaces comuns |

### 5.2 Padrões Utilizados

- **Gateway Pattern**: Abstração de acesso a dados
- **Use Case Pattern**: Encapsulamento de regras de negócio
- **Presenter Pattern**: Transformação de entidades para DTOs
- **Result Pattern**: Retorno padronizado de operações
- **Factory Method**: Criação de instâncias nos use cases

## 6. Escalabilidade

### 6.1 Horizontal

- **API**: Múltiplas instâncias atrás de load balancer
- **Workers**: Escalar conforme demanda de processamento
- **Banco**: Réplicas de leitura

### 6.2 Vertical

- Ajustar recursos de containers conforme necessidade
- Otimizar queries do Entity Framework

## 7. Monitoramento

- **Health Checks**: `/health` para verificação de saúde
- **Dashboard**: `/dashboard` para visualização
- **Logs**: Serilog com output para console e arquivo
- **Métricas**: Preparado para Prometheus/Grafana

## 8. Segurança

- Autenticação JWT com expiração
- Senhas hasheadas com SHA256
- CORS configurado
- Validação de tipos de arquivo
- Isolamento de dados por usuário
