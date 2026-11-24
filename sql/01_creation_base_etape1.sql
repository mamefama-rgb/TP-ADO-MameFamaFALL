-- ÉTAPE 1 : création de la base et de la table Etudiants (à exécuter dans SSMS)
IF DB_ID(N'FAMASI_DB') IS NULL
    CREATE DATABASE FAMASI_DB;
GO

USE FAMASI_DB;
GO

IF OBJECT_ID(N'dbo.Etudiants', N'U') IS NOT NULL
    DROP TABLE dbo.Etudiants;
GO

CREATE TABLE dbo.Etudiants
(
    Numero      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Etudiants PRIMARY KEY,
    NomEtudiant NVARCHAR(100)     NOT NULL,
    MtBourse    DECIMAL(12,0)     NOT NULL
);
GO

INSERT INTO dbo.Etudiants (NomEtudiant, MtBourse) VALUES
    (N'Modou Diop',     140000),
    (N'Doudou Fall',    150000),
    (N'Ismaila Sarr',    50000),
    (N'Abdoulaye Sy',    90000),
    (N'Mariama Ba',      85000),
    (N'Talla Sylla',     48000);
GO

SELECT NomEtudiant, MtBourse, Numero FROM dbo.Etudiants;
GO
