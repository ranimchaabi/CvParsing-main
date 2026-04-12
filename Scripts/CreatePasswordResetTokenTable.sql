-- Password reset tokens (hashed, single-use, time-limited). Run once on your SQL Server database.

IF OBJECT_ID(N'dbo.PasswordResetToken', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetToken (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        TokenHashHex NVARCHAR(64) NOT NULL,
        ExpiresAtUtc DATETIME2 NOT NULL,
        Used BIT NOT NULL CONSTRAINT DF_PasswordResetToken_Used DEFAULT (0),
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PasswordResetToken_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_PasswordResetToken_Utilisateur FOREIGN KEY (UserId) REFERENCES dbo.Utilisateur(id) ON DELETE CASCADE
    );

    CREATE INDEX IX_PasswordResetToken_TokenHashHex ON dbo.PasswordResetToken(TokenHashHex);
    CREATE INDEX IX_PasswordResetToken_UserId ON dbo.PasswordResetToken(UserId);
END
