-- Run once against the same database as DefaultConnection (SQL Server).
-- Adds candidate profile fields and per-application status on Cv.

IF COL_LENGTH('dbo.Candidat', 'telephone') IS NULL
    ALTER TABLE dbo.Candidat ADD telephone NVARCHAR(50) NULL;

IF COL_LENGTH('dbo.Candidat', 'departement') IS NULL
    ALTER TABLE dbo.Candidat ADD departement NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.Candidat', 'designation') IS NULL
    ALTER TABLE dbo.Candidat ADD designation NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.Candidat', 'langues') IS NULL
    ALTER TABLE dbo.Candidat ADD langues NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.Candidat', 'bio') IS NULL
    ALTER TABLE dbo.Candidat ADD bio NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.Candidat', 'photo_chemin') IS NULL
    ALTER TABLE dbo.Candidat ADD photo_chemin NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.Cv', 'statut_candidature') IS NULL
    ALTER TABLE dbo.Cv ADD statut_candidature NVARCHAR(50) NULL;

-- Optional: default existing rows to pending
UPDATE dbo.Cv SET statut_candidature = N'Pending' WHERE statut_candidature IS NULL;
