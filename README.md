# TP ADO.NET : TestADO

## 1. Préparer la base (SSMS)

1. Ouvrir `sql/01_creation_base_etape1.sql` dans SSMS et l'exécuter (crée `FAMASI_DB` et la table `Etudiants`).
2. Pour l'étape 2 seulement : exécuter `sql/02_ajout_date_naissance_etape2.sql`.

## 2. Adapter la chaîne de connexion

Dans `Program.cs`, modifier `Server=localhost` selon votre instance :
`localhost`, `.\SQLEXPRESS` ou `(localdb)\MSSQLLocalDB`.
(Le TP utilise le serveur du professeur, `EHODHPMASSE` : à remplacer par le vôtre.)

## 3. Lancer

```powershell
dotnet run -- 1     # affichage de l'étape 1
dotnet run -- 2     # affichage de l'étape 2 (après le script 02)
dotnet run          # menu CRUD : C, R, U, D, L, Q
```

## Affichages attendus

Étape 1 :

```
	Doudou Fall	150000
	Modou Diop	140000
	Abdoulaye Sy	90000
	Mariama Ba	85000
```

Étape 2 :

```
	Doudou Fall	150000	04/08/1999
	Modou Diop	140000	14/02/2004
	Abdoulaye Sy	90000	01/06/2000
	Mariama Ba	85000	31/05/1996
```
