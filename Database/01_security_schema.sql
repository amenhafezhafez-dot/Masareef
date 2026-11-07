/* ============================================================================
   Masareef — Security schema (Roles, Refresh Tokens, Audit Log)
   ----------------------------------------------------------------------------
   شغّل الملف ده مرة واحدة على داتابيز MasareefDB قبل تشغيل الـ API.
   الملف آمن لو اتشغّل أكتر من مرة — كل خطوة بتتأكد الأول إن الحاجة مش موجودة.

   Run once against the MasareefDB database before starting the API.
   This script is idempotent: re-running it makes no further changes.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------------------
   1) Users.Role  —  صلاحية المستخدم: 'Admin' أو 'User'
      الكود بيقرا العمود ده في كل عملية دخول، فلو ناقص كل تسجيل دخول هيفشل.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'Role')
BEGIN
    ALTER TABLE dbo.Users
        ADD [Role] NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Users_Role DEFAULT ('User');

    PRINT 'Added column Users.Role';
END
ELSE
    PRINT 'Users.Role already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   2) RefreshTokens  —  تخزين الـ refresh tokens
      بنخزّن SHA-256 hash (64 حرف hex) مش التوكن الخام، فلو الداتابيز اتسربت
      التوكنات تفضل غير قابلة للاستخدام.
      ReplacedByTokenHash بيسجّل سلسلة التدوير (rotation chain).
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id                   INT            IDENTITY(1,1) NOT NULL,
        UserId               INT            NOT NULL,
        TokenHash            NVARCHAR(64)   NOT NULL,
        CreatedAt            DATETIME2      NOT NULL
            CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ExpiresAt            DATETIME2      NOT NULL,
        RevokedAt            DATETIME2      NULL,
        ReplacedByTokenHash  NVARCHAR(64)   NULL,

        CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (UserID)
    );

    -- الـ lookup بيحصل بالـ hash في كل عملية refresh / logout
    CREATE INDEX IX_RefreshTokens_TokenHash ON dbo.RefreshTokens (TokenHash);
    CREATE INDEX IX_RefreshTokens_UserID    ON dbo.RefreshTokens (UserId);

    PRINT 'Created table RefreshTokens';
END
ELSE
    PRINT 'RefreshTokens already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   3) AuditLogs  —  سجل التدقيق الدائم (مين عمل ايه وامتى)
      مفيش FK على UserId عن قصد: لازم نقدر نسجّل محاولة دخول فاشلة بإيميل
      مش موجود (UserId = NULL)، والسجل لازم يفضل محفوظ لو المستخدم اتحذف.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        UserId      INT             NULL,
        [Action]    NVARCHAR(100)   NOT NULL,
        EntityType  NVARCHAR(50)    NULL,
        EntityId    INT             NULL,
        Details     NVARCHAR(MAX)   NULL,
        IpAddress   NVARCHAR(45)    NULL,   -- 45 = أطول عنوان IPv6
        Success     BIT             NOT NULL
            CONSTRAINT DF_AuditLogs_Success DEFAULT (1),
        CreatedAt   DATETIME2       NOT NULL
            CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id)
    );

    CREATE INDEX IX_AuditLogs_UserID ON dbo.AuditLogs (UserId);
    CREATE INDEX IX_AuditLogs_Action ON dbo.AuditLogs ([Action]);

    PRINT 'Created table AuditLogs';
END
ELSE
    PRINT 'AuditLogs already exists — skipped';
GO


/* ---------------------------------------------------------------------------
   4) التحقق النهائي — لازم التلاتة يرجعوا OK قبل تشغيل الـ API
   --------------------------------------------------------------------------- */
PRINT '';
PRINT '================ verification ================';

SELECT 'Users.Role'    AS Item,
       CASE WHEN EXISTS (SELECT 1 FROM sys.columns
                         WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'Role')
            THEN 'OK' ELSE 'MISSING' END AS Status
UNION ALL
SELECT 'RefreshTokens',
       CASE WHEN OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NOT NULL
            THEN 'OK' ELSE 'MISSING' END
UNION ALL
SELECT 'AuditLogs',
       CASE WHEN OBJECT_ID(N'dbo.AuditLogs', N'U') IS NOT NULL
            THEN 'OK' ELSE 'MISSING' END;
GO


/* ---------------------------------------------------------------------------
   5) ترقية أول مستخدم لـ Admin  —  اختياري، شيل التعليق وغيّر الإيميل
      مفيش طريقة تعمل Admin من الـ API عن قصد: التسجيل دايمًا بينتج Role='User'.
   --------------------------------------------------------------------------- */
-- UPDATE dbo.Users SET [Role] = 'Admin' WHERE Email = 'admin@example.com';
-- GO
