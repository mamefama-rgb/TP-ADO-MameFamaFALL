-- ÉTAPE 2 : ajout de la colonne DateNaissance (à exécuter dans SSMS)
USE FAMASI_DB;
GO

IF COL_LENGTH(N'dbo.Etudiants', N'DateNaissance') IS NULL
    ALTER TABLE dbo.Etudiants ADD DateNaissance DATE NULL;
GO


UPDATE dbo.Etudiants SET DateNaissance = '1999-08-04' WHERE NomEtudiant = N'Doudou Fall';
UPDATE dbo.Etudiants SET DateNaissance = '2004-02-14' WHERE NomEtudiant = N'Modou Diop';
UPDATE dbo.Etudiants SET DateNaissance = '2000-06-01' WHERE NomEtudiant = N'Abdoulaye Sy';
UPDATE dbo.Etudiants SET DateNaissance = '1996-05-31' WHERE NomEtudiant = N'Mariama Ba';
UPDATE dbo.Etudiants SET DateNaissance = '2001-12-25' WHERE NomEtudiant = N'Ismaila Sarr';
UPDATE dbo.Etudiants SET DateNaissance = '2002-07-15' WHERE NomEtudiant = N'Talla Sylla';

GO

SELECT NomEtudiant, MtBourse, DateNaissance, Numero FROM dbo.Etudiants;
GO
