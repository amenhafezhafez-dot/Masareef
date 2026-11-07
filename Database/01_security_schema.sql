/* ============================================================================
   Masareef — Base schema (core application tables)
   ----------------------------------------------------------------------------
   Run this FIRST, before 01_security_schema.sql.

   Creates the 11 core tables and the vw_DebtSummary view. Structure only —
   no data. Safe to re-run: every object is created only if missing.

   The database itself must already exist. Recommended collation is
   Arabic_CI_AS, which is what the reference database uses:

       CREATE DATABASE MasareefDB COLLATE Arabic_CI_AS;

   Usage:
       sqlcmd -S .\SQLEXPRESS -d MasareefDB -E -i Database/00_base_schema.sql
       sqlcmd -S .\SQLEXPRESS -d MasareefDB -E -i Database/01_security_schema.sql
   ============================================================================ */

SET NOCOUNT ON;

-- Required for the filtered index on Debts.DueDate. sqlcmd defaults
-- QUOTED_IDENTIFIER to OFF, and SQL Server refuses to build a filtered
-- index in that mode — the table would be created but the index silently
-- missing, which only shows up later as a slow overdue-debts query.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------------------------------------------------------------------------
   Users — the owner of every record in the system
   (the Role column is added later by 01_security_schema.sql)
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID        INT            IDENTITY(1,1) NOT NULL,
        Name          NVARCHAR(100)  NOT NULL,
        Email         NVARCHAR(150)  NOT NULL,
        PasswordHash  NVARCHAR(255)  NOT NULL,
        PhoneNumber   NVARCHAR(20)   NULL,
        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        IsActive      BIT            NOT NULL CONSTRAINT DF_Users_IsActive  DEFAULT (1),

        CONSTRAINT PK_Users       PRIMARY KEY CLUSTERED (UserID),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
    PRINT 'Created table Users';
END
ELSE PRINT 'Users already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   Categories — shared lookup. UserID NULL = a built-in category for everyone.
   Scope: 1=business 2=home 3=both      Kind: 1=expense 2=income
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryID     INT            IDENTITY(1,1) NOT NULL,
        UserID         INT            NULL,
        CategoryName   NVARCHAR(100)  NOT NULL,
        CategoryScope  TINYINT        NOT NULL,
        CategoryKind   TINYINT        NOT NULL,
        Icon           NVARCHAR(50)   NULL,

        CONSTRAINT PK_Categories       PRIMARY KEY CLUSTERED (CategoryID),
        CONSTRAINT FK_Categories_Users FOREIGN KEY (UserID)
            REFERENCES dbo.Users (UserID) ON DELETE CASCADE,
        CONSTRAINT CK_Categories_Scope CHECK (CategoryScope IN (1, 2, 3)),
        CONSTRAINT CK_Categories_Kind  CHECK (CategoryKind  IN (1, 2))
    );

    CREATE INDEX IX_Categories_UserID ON dbo.Categories (UserID);
    PRINT 'Created table Categories';
END
ELSE PRINT 'Categories already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   Businesses — a shop belonging to a user. Deactivated, never deleted,
   so the financial history stays intact.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Businesses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Businesses
    (
        BusinessID    INT             IDENTITY(1,1) NOT NULL,
        UserID        INT             NOT NULL,
        BusinessName  NVARCHAR(100)   NOT NULL,
        Description   NVARCHAR(500)   NULL,
        Currency      NCHAR(3)        NOT NULL CONSTRAINT DF_Businesses_Currency  DEFAULT ('EGP'),
        CreatedAt     DATETIME2       NOT NULL CONSTRAINT DF_Businesses_CreatedAt DEFAULT (SYSUTCDATETIME()),
        IsActive      BIT             NOT NULL CONSTRAINT DF_Businesses_IsActive  DEFAULT (1),

        CONSTRAINT PK_Businesses       PRIMARY KEY CLUSTERED (BusinessID),
        CONSTRAINT FK_Businesses_Users FOREIGN KEY (UserID)
            REFERENCES dbo.Users (UserID) ON DELETE CASCADE
    );

    CREATE INDEX IX_Businesses_UserID ON dbo.Businesses (UserID);
    PRINT 'Created table Businesses';
END
ELSE PRINT 'Businesses already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   Debts — money owed in either direction.
   Direction: 1 = I owe   2 = owed to me
   Status:    1 = open  2 = partially paid  3 = settled  4 = written off
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Debts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Debts
    (
        DebtID          INT             IDENTITY(1,1) NOT NULL,
        UserID          INT             NOT NULL,
        BusinessID      INT             NULL,              -- NULL = a personal debt
        PersonName      NVARCHAR(100)   NOT NULL,
        PersonPhone     NVARCHAR(20)    NULL,
        Direction       TINYINT         NOT NULL,
        Amount          DECIMAL(18,2)   NOT NULL,
        DebtDate        DATE            NOT NULL,
        DueDate         DATE            NULL,
        Status          TINYINT         NOT NULL CONSTRAINT DF_Debts_Status    DEFAULT (1),
        Description     NVARCHAR(500)   NULL,
        CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_Debts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        WriteOffReason  NVARCHAR(500)   NULL,
        WriteOffDate    DATETIME2       NULL,
        RowVersion      ROWVERSION      NOT NULL,          -- optimistic concurrency

        CONSTRAINT PK_Debts            PRIMARY KEY CLUSTERED (DebtID),
        CONSTRAINT FK_Debts_Users      FOREIGN KEY (UserID)
            REFERENCES dbo.Users (UserID) ON DELETE CASCADE,
        CONSTRAINT FK_Debts_Businesses FOREIGN KEY (BusinessID)
            REFERENCES dbo.Businesses (BusinessID),
        CONSTRAINT CK_Debts_Amount     CHECK (Amount > 0),
        CONSTRAINT CK_Debts_Direction  CHECK (Direction IN (1, 2)),
        CONSTRAINT CK_Debts_Status     CHECK (Status IN (1, 2, 3, 4)),
        CONSTRAINT CK_Debts_DueDate    CHECK (DueDate IS NULL OR DueDate >= DebtDate)
    );

    CREATE INDEX IX_Debts_UserID     ON dbo.Debts (UserID);
    CREATE INDEX IX_Debts_BusinessID ON dbo.Debts (BusinessID);
    CREATE INDEX IX_Debts_Status     ON dbo.Debts (UserID, Status);

    -- only unsettled debts can be overdue, so the index covers just those
    CREATE INDEX IX_Debts_DueDate ON dbo.Debts (DueDate) WHERE Status IN (1, 2);

    PRINT 'Created table Debts';
END
ELSE PRINT 'Debts already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   DebtPayments — instalments against a debt
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.DebtPayments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DebtPayments
    (
        PaymentID      INT             IDENTITY(1,1) NOT NULL,
        DebtID         INT             NOT NULL,
        PaidAmount     DECIMAL(18,2)   NOT NULL,
        PaymentDate    DATE            NOT NULL,
        PaymentMethod  TINYINT         NOT NULL CONSTRAINT DF_DebtPayments_PaymentMethod DEFAULT (1),
        Note           NVARCHAR(300)   NULL,
        CreatedAt      DATETIME2       NOT NULL CONSTRAINT DF_DebtPayments_CreatedAt     DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_DebtPayments        PRIMARY KEY CLUSTERED (PaymentID),
        CONSTRAINT FK_DebtPayments_Debts  FOREIGN KEY (DebtID)
            REFERENCES dbo.Debts (DebtID) ON DELETE CASCADE,
        CONSTRAINT CK_DebtPayments_Amount CHECK (PaidAmount > 0)
    );

    CREATE INDEX IX_DebtPayments_DebtID ON dbo.DebtPayments (DebtID);
    CREATE INDEX IX_DebtPayments_Date   ON dbo.DebtPayments (PaymentDate);
    PRINT 'Created table DebtPayments';
END
ELSE PRINT 'DebtPayments already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   HomeExpenses / HomeIncomes — household money, tied straight to the user
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.HomeExpenses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HomeExpenses
    (
        HomeExpenseID  INT             IDENTITY(1,1) NOT NULL,
        UserID         INT             NOT NULL,
        CategoryID     INT             NOT NULL,
        Amount         DECIMAL(18,2)   NOT NULL,
        ExpenseDate    DATE            NOT NULL,
        PaymentMethod  TINYINT         NOT NULL CONSTRAINT DF_HomeExpenses_PaymentMethod DEFAULT (1),
        Description    NVARCHAR(500)   NULL,
        CreatedAt      DATETIME2       NOT NULL CONSTRAINT DF_HomeExpenses_CreatedAt     DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_HomeExpenses            PRIMARY KEY CLUSTERED (HomeExpenseID),
        CONSTRAINT FK_HomeExpenses_Users      FOREIGN KEY (UserID)
            REFERENCES dbo.Users (UserID) ON DELETE CASCADE,
        CONSTRAINT FK_HomeExpenses_Categories FOREIGN KEY (CategoryID)
            REFERENCES dbo.Categories (CategoryID),
        CONSTRAINT CK_HomeExpenses_Amount     CHECK (Amount > 0)
    );

    CREATE INDEX IX_HomeExpenses_UserID ON dbo.HomeExpenses (UserID);
    CREATE INDEX IX_HomeExpenses_Date   ON dbo.HomeExpenses (UserID, ExpenseDate);
    PRINT 'Created table HomeExpenses';
END
ELSE PRINT 'HomeExpenses already exists — skipped';
GO

IF OBJECT_ID(N'dbo.HomeIncomes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HomeIncomes
    (
        HomeIncomeID  INT             IDENTITY(1,1) NOT NULL,
        UserID        INT             NOT NULL,
        CategoryID    INT             NOT NULL,
        Amount        DECIMAL(18,2)   NOT NULL,
        IncomeDate    DATE            NOT NULL,
        Source        NVARCHAR(100)   NULL,
        Description   NVARCHAR(500)   NULL,
        CreatedAt     DATETIME2       NOT NULL CONSTRAINT DF_HomeIncomes_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_HomeIncomes            PRIMARY KEY CLUSTERED (HomeIncomeID),
        CONSTRAINT FK_HomeIncomes_Users      FOREIGN KEY (UserID)
            REFERENCES dbo.Users (UserID) ON DELETE CASCADE,
        CONSTRAINT FK_HomeIncomes_Categories FOREIGN KEY (CategoryID)
            REFERENCES dbo.Categories (CategoryID),
        CONSTRAINT CK_HomeIncomes_Amount     CHECK (Amount > 0)
    );

    CREATE INDEX IX_HomeIncomes_UserID ON dbo.HomeIncomes (UserID);
    PRINT 'Created table HomeIncomes';
END
ELSE PRINT 'HomeIncomes already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   BusinessExpenses / BusinessIncomes — shop money, tied to a business
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.BusinessExpenses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusinessExpenses
    (
        ExpenseID      INT             IDENTITY(1,1) NOT NULL,
        BusinessID     INT             NOT NULL,
        CategoryID     INT             NOT NULL,
        Amount         DECIMAL(18,2)   NOT NULL,
        ExpenseDate    DATE            NOT NULL,
        PaymentMethod  TINYINT         NOT NULL CONSTRAINT DF_BusinessExpenses_PaymentMethod DEFAULT (1),
        Description    NVARCHAR(500)   NULL,
        CreatedAt      DATETIME2       NOT NULL CONSTRAINT DF_BusinessExpenses_CreatedAt     DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_BusinessExpenses            PRIMARY KEY CLUSTERED (ExpenseID),
        CONSTRAINT FK_BusinessExpenses_Businesses FOREIGN KEY (BusinessID)
            REFERENCES dbo.Businesses (BusinessID) ON DELETE CASCADE,
        CONSTRAINT FK_BusinessExpenses_Categories FOREIGN KEY (CategoryID)
            REFERENCES dbo.Categories (CategoryID),
        CONSTRAINT CK_BusinessExpenses_Amount     CHECK (Amount > 0)
    );

    CREATE INDEX IX_BusinessExpenses_BusinessID ON dbo.BusinessExpenses (BusinessID);
    CREATE INDEX IX_BusinessExpenses_CategoryID ON dbo.BusinessExpenses (CategoryID);
    CREATE INDEX IX_BusinessExpenses_Date       ON dbo.BusinessExpenses (BusinessID, ExpenseDate);
    PRINT 'Created table BusinessExpenses';
END
ELSE PRINT 'BusinessExpenses already exists — skipped';
GO

IF OBJECT_ID(N'dbo.BusinessIncomes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusinessIncomes
    (
        IncomeID     INT             IDENTITY(1,1) NOT NULL,
        BusinessID   INT             NOT NULL,
        CategoryID   INT             NOT NULL,
        Amount       DECIMAL(18,2)   NOT NULL,
        IncomeDate   DATE            NOT NULL,
        Source       NVARCHAR(100)   NULL,
        Description  NVARCHAR(500)   NULL,
        CreatedAt    DATETIME2       NOT NULL CONSTRAINT DF_BusinessIncomes_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_BusinessIncomes            PRIMARY KEY CLUSTERED (IncomeID),
        CONSTRAINT FK_BusinessIncomes_Businesses FOREIGN KEY (BusinessID)
            REFERENCES dbo.Businesses (BusinessID) ON DELETE CASCADE,
        CONSTRAINT FK_BusinessIncomes_Categories FOREIGN KEY (CategoryID)
            REFERENCES dbo.Categories (CategoryID),
        CONSTRAINT CK_BusinessIncomes_Amount     CHECK (Amount > 0)
    );

    CREATE INDEX IX_BusinessIncomes_BusinessID ON dbo.BusinessIncomes (BusinessID);
    CREATE INDEX IX_BusinessIncomes_Date       ON dbo.BusinessIncomes (BusinessID, IncomeDate);
    PRINT 'Created table BusinessIncomes';
END
ELSE PRINT 'BusinessIncomes already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   Products / ProductSales — shop inventory. Recording a sale decrements
   StockQuantity; deleting one puts it back.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductID      INT             IDENTITY(1,1) NOT NULL,
        BusinessID     INT             NOT NULL,
        ProductName    NVARCHAR(150)   NOT NULL,
        ProductCode    NVARCHAR(50)    NULL,
        OriginalPrice  DECIMAL(18,2)   NOT NULL,          -- cost
        SellingPrice   DECIMAL(18,2)   NULL,
        IsActive       BIT             NOT NULL CONSTRAINT DF_Products_IsActive      DEFAULT (1),
        CreatedAt      DATETIME2       NOT NULL CONSTRAINT DF_Products_CreatedAt     DEFAULT (SYSUTCDATETIME()),
        Description    NVARCHAR(500)   NULL,
        StockQuantity  INT             NOT NULL CONSTRAINT DF_Products_StockQuantity DEFAULT (0),

        CONSTRAINT PK_Products            PRIMARY KEY CLUSTERED (ProductID),
        CONSTRAINT FK_Products_Businesses FOREIGN KEY (BusinessID)
            REFERENCES dbo.Businesses (BusinessID)
    );

    CREATE INDEX IX_Products_BusinessID ON dbo.Products (BusinessID);
    PRINT 'Created table Products';
END
ELSE PRINT 'Products already exists — skipped';
GO

IF OBJECT_ID(N'dbo.ProductSales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductSales
    (
        SaleID        INT             IDENTITY(1,1) NOT NULL,
        ProductID     INT             NOT NULL,
        QuantitySold  INT             NOT NULL,
        SalePrice     DECIMAL(18,2)   NOT NULL,
        SaleDate      DATETIME2       NOT NULL,
        CustomerName  NVARCHAR(100)   NULL,
        Note          NVARCHAR(300)   NULL,
        CreatedAt     DATETIME2       NOT NULL CONSTRAINT DF_ProductSales_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ProductSales          PRIMARY KEY CLUSTERED (SaleID),
        CONSTRAINT FK_ProductSales_Products FOREIGN KEY (ProductID)
            REFERENCES dbo.Products (ProductID)
    );

    CREATE INDEX IX_ProductSales_ProductID ON dbo.ProductSales (ProductID);
    CREATE INDEX IX_ProductSales_SaleDate  ON dbo.ProductSales (SaleDate);
    PRINT 'Created table ProductSales';
END
ELSE PRINT 'ProductSales already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   vw_DebtSummary — each debt with its payments rolled up, so the API does
   not recompute outstanding balances on every read.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.vw_DebtSummary', N'V') IS NOT NULL
    DROP VIEW dbo.vw_DebtSummary;
GO

CREATE VIEW dbo.vw_DebtSummary
AS
SELECT
    d.DebtID,
    d.UserID,
    d.BusinessID,
    d.PersonName,
    d.PersonPhone,
    d.Direction,
    d.Amount                                   AS OriginalAmount,
    ISNULL(SUM(p.PaidAmount), 0)               AS TotalPaid,
    d.Amount - ISNULL(SUM(p.PaidAmount), 0)    AS Outstanding,
    d.DebtDate,
    d.DueDate,
    d.Status,
    CASE
        WHEN d.DueDate IS NOT NULL
         AND d.DueDate < CAST(GETDATE() AS DATE)
         AND d.Status IN (1, 2)
        THEN 1 ELSE 0
    END                                        AS IsOverdue,
    COUNT(p.PaymentID)                         AS PaymentsCount
FROM dbo.Debts d
LEFT JOIN dbo.DebtPayments p ON p.DebtID = d.DebtID
GROUP BY
    d.DebtID, d.UserID, d.BusinessID, d.PersonName, d.PersonPhone,
    d.Direction, d.Amount, d.DebtDate, d.DueDate, d.Status;
GO


/* ---------------------------------------------------------------------------
   Verification — every row must read OK before running 01_security_schema.sql
   --------------------------------------------------------------------------- */
PRINT '';
PRINT '================ verification ================';

SELECT o.ObjectName,
       CASE WHEN OBJECT_ID(N'dbo.' + o.ObjectName, o.ObjectType) IS NOT NULL
            THEN 'OK' ELSE 'MISSING' END AS Status
FROM (VALUES
    ('Users','U'), ('Categories','U'), ('Businesses','U'), ('Debts','U'),
    ('DebtPayments','U'), ('HomeExpenses','U'), ('HomeIncomes','U'),
    ('BusinessExpenses','U'), ('BusinessIncomes','U'), ('Products','U'),
    ('ProductSales','U'), ('vw_DebtSummary','V')
) AS o(ObjectName, ObjectType);
GO
