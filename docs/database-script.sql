-- =============================================
-- FIAP X - Script de Criação do Banco de Dados
-- =============================================

-- Criar banco de dados
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FiapXDb')
BEGIN
    CREATE DATABASE FiapXDb;
END
GO

USE FiapXDb;
GO

-- =============================================
-- Tabela: Users
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [Name] NVARCHAR(255) NOT NULL,
        [Email] NVARCHAR(255) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    -- Índice único para e-mail
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email] ASC);
    
    PRINT 'Tabela Users criada com sucesso.';
END
GO

-- =============================================
-- Tabela: Videos
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Videos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Videos] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [OriginalFileName] NVARCHAR(500) NOT NULL,
        [StoragePath] NVARCHAR(1000) NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [FrameCount] INT NULL,
        [ZipPath] NVARCHAR(1000) NULL,
        [ErrorMessage] NVARCHAR(2000) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ProcessedAt] DATETIME2 NULL,
        
        CONSTRAINT [PK_Videos] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Videos_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );
    
    -- Índices para otimização de consultas
    CREATE NONCLUSTERED INDEX [IX_Videos_UserId] ON [dbo].[Videos] ([UserId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Videos_Status] ON [dbo].[Videos] ([Status] ASC);
    CREATE NONCLUSTERED INDEX [IX_Videos_CreatedAt] ON [dbo].[Videos] ([CreatedAt] DESC);
    
    PRINT 'Tabela Videos criada com sucesso.';
END
GO

-- =============================================
-- Dados Iniciais (Seed)
-- =============================================

-- Usuário Admin padrão (senha: admin123)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'admin@fiapx.com')
BEGIN
    INSERT INTO [dbo].[Users] ([Id], [Name], [Email], [PasswordHash], [CreatedAt])
    VALUES (
        '00000000-0000-0000-0000-000000000001',
        'Admin',
        'admin@fiapx.com',
        'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', -- admin123 hasheado
        GETUTCDATE()
    );
    
    PRINT 'Usuário admin criado com sucesso.';
END
GO

-- =============================================
-- Views úteis
-- =============================================

-- View de vídeos com informações do usuário
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VideosWithUser]'))
    DROP VIEW [dbo].[vw_VideosWithUser];
GO

CREATE VIEW [dbo].[vw_VideosWithUser]
AS
SELECT 
    v.[Id],
    v.[OriginalFileName],
    v.[Status],
    CASE v.[Status]
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Processing'
        WHEN 2 THEN 'Completed'
        WHEN 3 THEN 'Failed'
    END AS [StatusName],
    v.[FrameCount],
    v.[ZipPath],
    v.[ErrorMessage],
    v.[CreatedAt],
    v.[ProcessedAt],
    u.[Id] AS [UserId],
    u.[Name] AS [UserName],
    u.[Email] AS [UserEmail]
FROM [dbo].[Videos] v
INNER JOIN [dbo].[Users] u ON v.[UserId] = u.[Id];
GO

PRINT 'View vw_VideosWithUser criada com sucesso.';

-- =============================================
-- Stored Procedures úteis
-- =============================================

-- SP para obter estatísticas
IF EXISTS (SELECT * FROM sys.procedures WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetVideoStatistics]'))
    DROP PROCEDURE [dbo].[sp_GetVideoStatistics];
GO

CREATE PROCEDURE [dbo].[sp_GetVideoStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        COUNT(*) AS TotalVideos,
        SUM(CASE WHEN [Status] = 0 THEN 1 ELSE 0 END) AS PendingVideos,
        SUM(CASE WHEN [Status] = 1 THEN 1 ELSE 0 END) AS ProcessingVideos,
        SUM(CASE WHEN [Status] = 2 THEN 1 ELSE 0 END) AS CompletedVideos,
        SUM(CASE WHEN [Status] = 3 THEN 1 ELSE 0 END) AS FailedVideos,
        SUM(ISNULL([FrameCount], 0)) AS TotalFrames,
        (SELECT COUNT(*) FROM [dbo].[Users]) AS TotalUsers
    FROM [dbo].[Videos];
END
GO

PRINT 'Stored Procedure sp_GetVideoStatistics criada com sucesso.';

-- =============================================
-- Tabela de Health Checks (para HealthChecksUI)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HealthCheckHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HealthCheckHistory] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(500) NOT NULL,
        [Status] INT NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Duration] BIGINT NOT NULL,
        [LastCheckTime] DATETIME2 NOT NULL,
        
        CONSTRAINT [PK_HealthCheckHistory] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'Tabela HealthCheckHistory criada com sucesso.';
END
GO

PRINT '';
PRINT '=============================================';
PRINT 'Script executado com sucesso!';
PRINT '=============================================';
