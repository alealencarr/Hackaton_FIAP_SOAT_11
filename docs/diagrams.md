# Diagramas FIAP X

## 1. Arquitetura de Alto Nível

```mermaid
flowchart TB
    subgraph CLIENT["👤 Cliente"]
        WEB[Web Browser]
        MOBILE[Mobile App]
    end

    subgraph API_LAYER["🌐 API Layer"]
        LB[Load Balancer]
        API1[API Instance 1]
        API2[API Instance 2]
        API3[API Instance 3]
    end

    subgraph PROCESSING["⚙️ Processing Layer"]
        RMQ[(RabbitMQ)]
        W1[Worker 1]
        W2[Worker 2]
        W3[Worker 3]
    end

    subgraph DATA["💾 Data Layer"]
        SQL[(SQL Server)]
        REDIS[(Redis Cache)]
        STORAGE[File Storage]
    end

    subgraph EXTERNAL["📧 External Services"]
        SMTP[Email Service]
    end

    WEB --> LB
    MOBILE --> LB
    LB --> API1
    LB --> API2
    LB --> API3
    
    API1 --> SQL
    API2 --> SQL
    API3 --> SQL
    
    API1 --> RMQ
    API2 --> RMQ
    API3 --> RMQ
    
    API1 --> REDIS
    API2 --> REDIS
    API3 --> REDIS
    
    RMQ --> W1
    RMQ --> W2
    RMQ --> W3
    
    W1 --> SQL
    W2 --> SQL
    W3 --> SQL
    
    W1 --> STORAGE
    W2 --> STORAGE
    W3 --> STORAGE
    
    W1 --> SMTP
    W2 --> SMTP
    W3 --> SMTP

    style CLIENT fill:#e1f5fe
    style API_LAYER fill:#fff3e0
    style PROCESSING fill:#f3e5f5
    style DATA fill:#e8f5e9
    style EXTERNAL fill:#fce4ec
```

## 2. Clean Architecture

```mermaid
flowchart TB
    subgraph PRESENTATION["Presentation Layer"]
        direction TB
        EP[Endpoints]
        MW[Middlewares]
        SW[Swagger]
    end

    subgraph APPLICATION["Application Layer"]
        direction TB
        CTRL[Controllers]
        UC[Use Cases]
        GW[Gateways]
        PRES[Presenters]
    end

    subgraph DOMAIN["Domain Layer"]
        direction TB
        ENT[Entities]
        ENUM[Enums]
        VAL[Validations]
    end

    subgraph INFRASTRUCTURE["Infrastructure Layer"]
        direction TB
        DS[Data Sources]
        SVC[Services]
        DB[DbContext]
    end

    PRESENTATION --> APPLICATION
    APPLICATION --> DOMAIN
    APPLICATION --> INFRASTRUCTURE
    INFRASTRUCTURE --> DOMAIN

    style PRESENTATION fill:#bbdefb
    style APPLICATION fill:#c8e6c9
    style DOMAIN fill:#fff9c4
    style INFRASTRUCTURE fill:#ffccbc
```

## 3. Fluxo de Upload de Vídeo

```mermaid
sequenceDiagram
    autonumber
    participant U as 👤 Usuário
    participant A as 🌐 API
    participant S as 📁 Storage
    participant Q as 📬 RabbitMQ
    participant DB as 💾 Database

    U->>A: POST /api/videos/upload
    A->>A: Validar JWT Token
    A->>A: Validar Formato do Vídeo
    A->>S: Salvar arquivo de vídeo
    S-->>A: Caminho do arquivo
    A->>DB: Criar registro (Status: Pending)
    DB-->>A: Video ID
    A->>Q: Publicar mensagem
    Q-->>A: Confirmação
    A-->>U: 201 Created + Video ID
```

## 4. Fluxo de Processamento

```mermaid
sequenceDiagram
    autonumber
    participant Q as 📬 RabbitMQ
    participant W as ⚙️ Worker
    participant DB as 💾 Database
    participant F as 🎬 FFmpeg
    participant S as 📁 Storage
    participant E as 📧 Email

    Q->>W: Consumir mensagem (Video ID)
    W->>DB: Buscar dados do vídeo
    DB-->>W: Video + User info
    W->>DB: Atualizar status → Processing
    W->>S: Carregar arquivo de vídeo
    S-->>W: Arquivo do vídeo
    W->>F: Extrair frames (1 fps)
    F-->>W: Frames PNG
    W->>W: Criar arquivo ZIP
    W->>S: Salvar ZIP
    S-->>W: Caminho do ZIP
    W->>DB: Atualizar → Completed
    W->>S: Deletar vídeo original
    W->>E: Notificar usuário
    W->>Q: ACK mensagem
```

## 5. Diagrama de Classes - Entidades

```mermaid
classDiagram
    class User {
        +Guid Id
        +string Name
        +string Email
        +string PasswordHash
        +DateTime CreatedAt
        +ValidatePassword(password) bool
    }

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

    class VideoStatus {
        <<enumeration>>
        Pending = 0
        Processing = 1
        Completed = 2
        Failed = 3
    }

    User "1" --> "*" Video : has many
    Video --> VideoStatus : has status
```

## 6. Diagrama de Implantação (Docker)

```mermaid
flowchart TB
    subgraph DOCKER["🐳 Docker Compose"]
        subgraph API_CONTAINER["Container: API"]
            API[".NET 8 API<br/>Port: 5000"]
        end

        subgraph WORKER_CONTAINER["Containers: Workers (x2)"]
            W1["Worker 1<br/>FFmpeg"]
            W2["Worker 2<br/>FFmpeg"]
        end

        subgraph SQL_CONTAINER["Container: SQL Server"]
            SQL["SQL Server 2022<br/>Port: 1433"]
        end

        subgraph RMQ_CONTAINER["Container: RabbitMQ"]
            RMQ["RabbitMQ 3<br/>Port: 5672<br/>Management: 15672"]
        end

        subgraph REDIS_CONTAINER["Container: Redis"]
            REDIS["Redis 7<br/>Port: 6379"]
        end
    end

    subgraph VOLUMES["📦 Volumes"]
        V1[(uploads)]
        V2[(outputs)]
        V3[(sqlserver_data)]
        V4[(rabbitmq_data)]
    end

    API --> SQL
    API --> RMQ
    API --> REDIS
    API --> V1
    API --> V2

    W1 --> SQL
    W1 --> RMQ
    W1 --> V1
    W1 --> V2

    W2 --> SQL
    W2 --> RMQ
    W2 --> V1
    W2 --> V2

    SQL --> V3
    RMQ --> V4

    style DOCKER fill:#e3f2fd
    style API_CONTAINER fill:#c8e6c9
    style WORKER_CONTAINER fill:#fff3e0
    style SQL_CONTAINER fill:#ffecb3
    style RMQ_CONTAINER fill:#f3e5f5
    style REDIS_CONTAINER fill:#ffcdd2
```

## 7. Fluxo de Autenticação

```mermaid
sequenceDiagram
    autonumber
    participant U as 👤 Usuário
    participant A as 🌐 API
    participant DB as 💾 Database
    participant JWT as 🔐 JWT Service

    Note over U,JWT: Registro
    U->>A: POST /api/auth/register
    A->>DB: Verificar email existente
    DB-->>A: Email disponível
    A->>A: Hash da senha
    A->>DB: Criar usuário
    DB-->>A: User ID
    A-->>U: 201 Created

    Note over U,JWT: Login
    U->>A: POST /api/auth/login
    A->>DB: Buscar usuário por email
    DB-->>A: User data
    A->>A: Validar senha
    A->>JWT: Gerar token
    JWT-->>A: JWT Token
    A-->>U: 200 OK + Token

    Note over U,JWT: Requisição Autenticada
    U->>A: GET /api/videos (+ Bearer Token)
    A->>A: Validar JWT
    A->>DB: Buscar dados
    DB-->>A: Videos do usuário
    A-->>U: 200 OK + Videos
```

## 8. Estados do Vídeo

```mermaid
stateDiagram-v2
    [*] --> Pending: Upload realizado
    Pending --> Processing: Worker inicia processamento
    Processing --> Completed: Processamento com sucesso
    Processing --> Failed: Erro no processamento
    Completed --> [*]
    Failed --> [*]

    note right of Pending
        Vídeo aguardando
        na fila do RabbitMQ
    end note

    note right of Processing
        FFmpeg extraindo frames
        (1 frame por segundo)
    end note

    note right of Completed
        ZIP disponível
        para download
    end note

    note right of Failed
        Usuário notificado
        por e-mail
    end note
```

## 9. Estrutura de Camadas

```mermaid
flowchart LR
    subgraph API["FiapX.API"]
        EP[Endpoints]
        DI[DependencyInjection]
        EXT[Extensions]
    end

    subgraph APP["FiapX.Application"]
        CTRL[Controllers]
        UC[UseCases]
        GW[Gateways]
        PRES[Presenters]
        INT[Interfaces]
    end

    subgraph DOM["FiapX.Domain"]
        ENT[Entities]
        ENUM[Enums]
    end

    subgraph INFRA["FiapX.Infrastructure"]
        DS[DataSources]
        SVC[Services]
        CTX[DbContexts]
        MIG[Migrations]
    end

    subgraph SHARED["FiapX.Shared"]
        DTO[DTOs]
        RES[Result]
    end

    API --> APP
    APP --> DOM
    APP --> SHARED
    INFRA --> APP
    INFRA --> DOM
    INFRA --> SHARED

    style API fill:#bbdefb
    style APP fill:#c8e6c9
    style DOM fill:#fff9c4
    style INFRA fill:#ffccbc
    style SHARED fill:#e1bee7
```

## 10. CI/CD Pipeline

```mermaid
flowchart LR
    subgraph TRIGGER["Trigger"]
        PUSH[Push to main]
        PR[Pull Request]
    end

    subgraph BUILD["Build Stage"]
        RESTORE[Restore]
        COMPILE[Build]
        TEST[Run Tests]
        COV[Code Coverage]
    end

    subgraph DOCKER["Docker Stage"]
        BUILDAPI[Build API Image]
        BUILDWORKER[Build Worker Image]
        PUSHAPI[Push API]
        PUSHWORKER[Push Worker]
    end

    subgraph DEPLOY["Deploy Stage"]
        K8S[Kubernetes Deploy]
        VERIFY[Health Check]
    end

    PUSH --> RESTORE
    PR --> RESTORE
    RESTORE --> COMPILE
    COMPILE --> TEST
    TEST --> COV
    COV --> BUILDAPI
    COV --> BUILDWORKER
    BUILDAPI --> PUSHAPI
    BUILDWORKER --> PUSHWORKER
    PUSHAPI --> K8S
    PUSHWORKER --> K8S
    K8S --> VERIFY

    style TRIGGER fill:#e1f5fe
    style BUILD fill:#c8e6c9
    style DOCKER fill:#fff3e0
    style DEPLOY fill:#f3e5f5
```
