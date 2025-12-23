/*
    Single setup script for BasicPaymentsDb
    - Creates database if not exists
    - Creates tables (Customers, Payments)
    - Creates stored procedures
    - Seeds 3 customers (no payments)
*/

-- Create database if it doesn't exist
IF DB_ID(N'BasicPaymentsDb') IS NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE [BasicPaymentsDb]';
    EXEC (@sql);
END
GO

USE [BasicPaymentsDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* Tables */

-- Customers table (simple registry of known customers)
IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt  DATETIME2(7)     NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Customers PRIMARY KEY (CustomerId)
    );
END
GO

-- Payments table
IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        PaymentId       UNIQUEIDENTIFIER NOT NULL,
        CustomerId      UNIQUEIDENTIFIER NOT NULL,
        ServiceProvider NVARCHAR(100)   NOT NULL,
        Amount          DECIMAL(18,2)   NOT NULL CHECK (Amount >= 0),
        Currency        NVARCHAR(10)    NOT NULL CONSTRAINT DF_Payments_Currency DEFAULT N'Bs',
        Status          NVARCHAR(20)    NOT NULL,
        CreatedAt       DATETIME2(7)    NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Payments PRIMARY KEY (PaymentId),
        CONSTRAINT CK_Payments_Status CHECK (Status IN (N'pendiente', N'completado', N'rechazado')),
        CONSTRAINT CK_Payments_Currency CHECK (Currency IN (N'Bs')),
        CONSTRAINT FK_Payments_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_Payments_CustomerId_CreatedAt'
      AND object_id = OBJECT_ID(N'dbo.Payments')
)
BEGIN
    CREATE INDEX IX_Payments_CustomerId_CreatedAt
        ON dbo.Payments (CustomerId, CreatedAt DESC);
END
GO

/* Stored Procedures */

/* Register a new payment */
CREATE OR ALTER PROCEDURE dbo.usp_RegisterPayment
    @PaymentId       UNIQUEIDENTIFIER,
    @CustomerId      UNIQUEIDENTIFIER,
    @ServiceProvider NVARCHAR(100),
    @Amount          DECIMAL(18,2),
    @Currency        NVARCHAR(10),
    @Status          NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure customer exists
    IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @CustomerId)
    BEGIN
        RAISERROR('El cliente no existe.', 16, 1);
        RETURN;
    END

    IF @Amount < 0
    BEGIN
        RAISERROR('El monto no puede ser negativo.', 16, 1);
        RETURN;
    END

    DECLARE @CreatedAt DATETIME2(7) = SYSUTCDATETIME();

    INSERT INTO dbo.Payments (PaymentId, CustomerId, ServiceProvider, Amount, Currency, Status, CreatedAt)
    VALUES (@PaymentId, @CustomerId, @ServiceProvider, @Amount, @Currency, @Status, @CreatedAt);

    SELECT PaymentId, CustomerId, ServiceProvider, Amount, Currency, Status, CreatedAt
    FROM dbo.Payments
    WHERE PaymentId = @PaymentId;
END;
GO

/* Get payments by customer */
CREATE OR ALTER PROCEDURE dbo.usp_GetPaymentsByCustomer
    @CustomerId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PaymentId, CustomerId, ServiceProvider, Amount, Currency, Status, CreatedAt
    FROM dbo.Payments
    WHERE CustomerId = @CustomerId
    ORDER BY CreatedAt DESC;
END;
GO

/* Update payment status */
CREATE OR ALTER PROCEDURE dbo.usp_UpdatePaymentStatus
    @PaymentId UNIQUEIDENTIFIER,
    @Status    NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Status NOT IN (N'pendiente', N'completado', N'rechazado')
    BEGIN
        RAISERROR('Estado inválido. Use pendiente/completado/rechazado.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Payments
    SET Status = @Status
    WHERE PaymentId = @PaymentId;

    SELECT PaymentId, CustomerId, ServiceProvider, Amount, Currency, Status, CreatedAt
    FROM dbo.Payments
    WHERE PaymentId = @PaymentId;
END;
GO

/* Get single payment by id */
CREATE OR ALTER PROCEDURE dbo.usp_GetPaymentById
    @PaymentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PaymentId, CustomerId, ServiceProvider, Amount, Currency, Status, CreatedAt
    FROM dbo.Payments
    WHERE PaymentId = @PaymentId;
END;
GO

/* Seed: 3 customers only (no payments) */
SET NOCOUNT ON;

DECLARE @CustomerId1 UNIQUEIDENTIFIER = 'cfe8b150-2f84-4a1a-bdf4-923b20e34973';
DECLARE @CustomerId2 UNIQUEIDENTIFIER = '8d9f1c20-01b2-4f55-9a5c-1c1c3a2b7a10';
DECLARE @CustomerId3 UNIQUEIDENTIFIER = 'a4a6c3f5-7d42-4e2e-9b77-2f8a6cbf3a21';

IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @CustomerId1)
    INSERT INTO dbo.Customers (CustomerId) VALUES (@CustomerId1);
IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @CustomerId2)
    INSERT INTO dbo.Customers (CustomerId) VALUES (@CustomerId2);
IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @CustomerId3)
    INSERT INTO dbo.Customers (CustomerId) VALUES (@CustomerId3);
