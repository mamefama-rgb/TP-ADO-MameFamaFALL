using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;

namespace TestADO
{
    internal sealed record Etudiant(int Numero, string Nom, decimal? Bourse, DateTime? Naissance);

    // Levée quand l'utilisateur saisit Q pendant une opération : retour au menu
    internal sealed class RetourMenuException : Exception
    {
    }

    internal class Program
    {
        private const string ConnexionChaine =
            "Server=localhost\\SQLEXPRESS; TrustServerCertificate=True; Database=FAMASI_DB; Integrated Security=true";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            if (args.Length > 0 && args[0] == "1")
            {
                Etape1();
            }
            else if (args.Length > 0 && args[0] == "2")
            {
                Etape2();
            }
            else
            {
                MenuCrud();
            }
        }

        // ------------------------------------------------------------------
        // ÉTAPE 1 : nom et bourse des étudiants dont la bourse dépasse 50000
        // ------------------------------------------------------------------
        static void Etape1()
        {
            string sReq = "SELECT nomEtudiant, mtBourse from Etudiants WHERE mtBourse > @bourse ORDER BY mtBourse DESC;";
            decimal valBourse = 50000;

            using (SqlConnection connexion = new SqlConnection(ConnexionChaine))
            {
                SqlCommand req = new SqlCommand(sReq, connexion);
                req.Parameters.AddWithValue("@bourse", valBourse);
                try
                {
                    connexion.Open();
                    SqlDataReader reader = req.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine("\t{0}\t{1}", reader[0], reader[1]);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.ReadLine();
            }
        }

        // ------------------------------------------------------------------
        // ÉTAPE 2 : même requête avec la colonne date de naissance
        // ------------------------------------------------------------------
        static void Etape2()
        {
            string sReq = "SELECT nomEtudiant, mtBourse, dateNaissance from Etudiants WHERE mtBourse > @bourse ORDER BY mtBourse DESC;";
            decimal valBourse = 50000;

            using (SqlConnection connexion = new SqlConnection(ConnexionChaine))
            {
                SqlCommand req = new SqlCommand(sReq, connexion);
                req.Parameters.AddWithValue("@bourse", valBourse);
                try
                {
                    connexion.Open();
                    SqlDataReader reader = req.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine("\t{0}\t{1}\t{2:dd/MM/yyyy}", reader[0], reader[1], reader[2]);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.ReadLine();
            }
        }

        // ------------------------------------------------------------------
        // MENU CRUD
        // ------------------------------------------------------------------
        static void MenuCrud()
        {
            bool continuer = true;

            while (continuer)
            {
                Console.WriteLine();
                Console.WriteLine("===== GESTION DES ÉTUDIANTS =====");
                Console.WriteLine("C : Créer     R : Lire     U : Modifier");
                Console.WriteLine("D : Supprimer L : Lister   Q : Quitter");
                string choix = SaisirBrut("Votre choix : ").ToUpperInvariant();

                try
                {
                    switch (choix)
                    {
                        case "C": Creer(); break;
                        case "R": Lire(); break;
                        case "U": Modifier(); break;
                        case "D": Supprimer(); break;
                        case "L": Lister(); break;
                        case "Q": continuer = false; break;
                        default: Console.WriteLine("Choix invalide. Utilisez C, R, U, D, L ou Q."); break;
                    }
                }
                catch (RetourMenuException)
                {
                    Console.WriteLine("Opération annulée. Retour au menu.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                }
            }

            Console.WriteLine("Au revoir.");
        }

        // C : créer un étudiant
        static void Creer()
        {
            string nom = SaisirNom("Nom de l'étudiant : ");

            decimal? bourse = SaisirBourse("Montant de la bourse (Entrée ou espaces = vide) : ", out _);
            DateTime? naissance = SaisirDate("Date de naissance (jj/mm/aaaa, Entrée pour ignorer) : ");

            const string sql =
                "INSERT INTO Etudiants (NomEtudiant, MtBourse, DateNaissance) " +
                "OUTPUT INSERTED.Numero VALUES (@nom, @bourse, @naissance);";

            using var connexion = new SqlConnection(ConnexionChaine);
            using var req = new SqlCommand(sql, connexion);
            req.Parameters.AddWithValue("@nom", nom);
            req.Parameters.Add(ParamBourse(bourse));
            req.Parameters.Add("@naissance", SqlDbType.Date).Value = (object?)naissance ?? DBNull.Value;

            connexion.Open();
            int numero = Convert.ToInt32(req.ExecuteScalar());
            Console.WriteLine($"Étudiant créé avec le numéro {numero}.");
        }

        // R : lire (afficher) un étudiant
        static void Lire()
        {
            int numero = SaisirNumero();
            Etudiant? etudiant = Trouver(numero);

            if (etudiant is null)
            {
                Console.WriteLine($"Aucun étudiant avec le numéro {numero}.");
                return;
            }

            AfficherEntete();
            Afficher(etudiant);
        }

        // U : modifier le nom et/ou la bourse
        static void Modifier()
        {
            int numero = SaisirNumero();
            Etudiant? etudiant = Trouver(numero);

            if (etudiant is null)
            {
                Console.WriteLine($"Aucun étudiant avec le numéro {numero}.");
                return;
            }

            Console.WriteLine("Étudiant actuel :");
            AfficherEntete();
            Afficher(etudiant);

            // Nom : Entrée conserve l'ancien nom, sinon le nouveau doit commencer par une lettre
            string nom;
            while (true)
            {
                string brutNom = SaisirTelQuel($"Nouveau nom [{TexteNom(etudiant.Nom)}] : ");

                if (brutNom.Length == 0)
                {
                    nom = etudiant.Nom;
                    break;
                }

                string propre = brutNom.Trim();
                if (NomValide(propre))
                {
                    nom = propre;
                    break;
                }
                Console.WriteLine(MessageNomInvalide);
            }

            // Bourse : Entrée conserve, des espaces seuls rendent la bourse vide (NULL)
            decimal? nouvelleBourse = SaisirBourse($"Nouveau montant de la bourse [{TexteBourse(etudiant.Bourse)}] : ", out bool conserver);
            decimal? bourse = conserver ? etudiant.Bourse : nouvelleBourse;
            // Rien n'a changé : on n'envoie pas d'UPDATE
if (nom == etudiant.Nom && bourse == etudiant.Bourse)
{
    Console.WriteLine("Aucun étudiant modifié.");
    return;
}

            const string sql = "UPDATE Etudiants SET NomEtudiant = @nom, MtBourse = @bourse WHERE Numero = @numero;";

            using var connexion = new SqlConnection(ConnexionChaine);
            using var req = new SqlCommand(sql, connexion);
            req.Parameters.AddWithValue("@nom", nom);
            req.Parameters.Add(ParamBourse(bourse));
            req.Parameters.AddWithValue("@numero", numero);

            connexion.Open();
            int lignes = req.ExecuteNonQuery();
            Console.WriteLine(lignes > 0 ? "Étudiant modifié." : "Aucune modification effectuée.");
        }

        // D : supprimer un étudiant, après confirmation
        static void Supprimer()
        {
            if (CompterEtudiants() == 0)
            {
                Console.WriteLine("Il n'y a plus d'étudiant dans la base.");
                return;
            }

            int numero = SaisirNumero();
            Etudiant? etudiant = Trouver(numero);

            if (etudiant is null)
            {
                Console.WriteLine($"Aucun étudiant avec le numéro {numero}.");
                return;
            }

            AfficherEntete();
            Afficher(etudiant);

            string reponse = Saisir("Confirmer la suppression DÉFINITIVE de cet étudiant ? (O/N) : ").ToUpperInvariant();
            if (reponse != "O")
            {
                Console.WriteLine("Suppression annulée.");
                return;
            }

            const string sql = "DELETE FROM Etudiants WHERE Numero = @numero;";

            using var connexion = new SqlConnection(ConnexionChaine);
            using var req = new SqlCommand(sql, connexion);
            req.Parameters.AddWithValue("@numero", numero);

            connexion.Open();
            int lignes = req.ExecuteNonQuery();
            Console.WriteLine(lignes > 0 ? "Étudiant supprimé." : "Aucune suppression effectuée.");
        }

        // L : lister tous les étudiants
        static void Lister()
        {
            const string sql = "SELECT Numero, NomEtudiant, MtBourse, DateNaissance FROM Etudiants ORDER BY Numero;";

            using var connexion = new SqlConnection(ConnexionChaine);
            using var req = new SqlCommand(sql, connexion);
            connexion.Open();
            using SqlDataReader reader = req.ExecuteReader();

            AfficherEntete();
            int total = 0;
            while (reader.Read())
            {
                Afficher(Mapper(reader));
                total++;
            }
            Console.WriteLine($"{total} étudiant(s).");
        }

        // ------------------------------------------------------------------
        // Accès aux données
        // ------------------------------------------------------------------
        static Etudiant? Trouver(int numero)
        {
            const string sql = "SELECT Numero, NomEtudiant, MtBourse, DateNaissance FROM Etudiants WHERE Numero = @numero;";

            using var connexion = new SqlConnection(ConnexionChaine);
            using var req = new SqlCommand(sql, connexion);
            req.Parameters.AddWithValue("@numero", numero);

            connexion.Open();
            using SqlDataReader reader = req.ExecuteReader();
            return reader.Read() ? Mapper(reader) : null;
        }

        // Nombre d'étudiants présents dans la table
        static int CompterEtudiants()
        {
            const string sql = "SELECT COUNT(*) FROM Etudiants;";

            using var connexion = new SqlConnection(ConnexionChaine);
            using var req = new SqlCommand(sql, connexion);

            connexion.Open();
            return Convert.ToInt32(req.ExecuteScalar());
        }

        static Etudiant Mapper(SqlDataReader reader)
        {
            return new Etudiant(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? (decimal?)null : reader.GetDecimal(2),
                reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3));
        }

        // ------------------------------------------------------------------
        // Affichage
        // ------------------------------------------------------------------
        static void AfficherEntete()
        {
            Console.WriteLine($"{"N°",4}  {"Nom",-22}{"Bourse",10}  {"Naissance",10}");
            Console.WriteLine(new string('-', 52));
        }

        static void Afficher(Etudiant e)
        {
            Console.WriteLine($"{e.Numero,4}  {TexteNom(e.Nom),-22}{TexteBourse(e.Bourse),10}  {e.Naissance:dd/MM/yyyy}");
        }

        // ------------------------------------------------------------------
        // Saisies sécurisées
        // ------------------------------------------------------------------
        // Saisie brute : sert uniquement au menu principal, où Q signifie "quitter"
        static string SaisirBrut(string invite)
        {
            Console.Write(invite);
            return (Console.ReadLine() ?? string.Empty).Trim();
        }

        // Saisie pendant une opération : Q (ou q) annule et revient au menu
        static string Saisir(string invite)
        {
            string texte = SaisirBrut(invite);

            if (texte.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                throw new RetourMenuException();
            }

            return texte;
        }

        private const string MessageNomInvalide =
            "Nom invalide : il doit commencer par une lettre (ex. : Fatou). Un nom comme 12345 est refusé.";

        // Un nom est valide s'il n'est pas vide et si son premier caractère est une lettre (accents inclus)
        static bool NomValide(string nom) => nom.Length > 0 && char.IsLetter(nom[0]);

        // Redemande le nom tant qu'il ne commence pas par une lettre. Q annule et revient au menu.
        static string SaisirNom(string invite)
        {
            while (true)
            {
                string nom = Saisir(invite);
                if (NomValide(nom))
                {
                    return nom;
                }
                Console.WriteLine(MessageNomInvalide);
            }
        }

        static int SaisirNumero()
        {
            while (true)
            {
                string texte = Saisir("Numéro de l'étudiant : ");
                if (int.TryParse(texte, out int numero) && numero > 0)
                {
                    return numero;
                }
                Console.WriteLine("Numéro invalide : saisissez un entier positif.");
            }
        }
        static decimal? SaisirBourse(string invite, out bool conserver)
        {
            while (true)
            {
                string brut = SaisirTelQuel(invite);

                if (brut.Length == 0)
                {
                    conserver = true;
                    return null;
                }

                // Supprime tous les espaces, y compris les espaces insécables de la culture française
                string nettoye = new string(brut.Where(c => !char.IsWhiteSpace(c)).ToArray());

                if (nettoye.Length == 0)
                {
                    conserver = false;
                    return null;
                }

                if (decimal.TryParse(nettoye.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valeur)
                    && valeur >= 0)
                {
                    conserver = false;
                    return valeur;
                }
                Console.WriteLine("Montant invalide : saisissez un nombre positif, ou des espaces pour une valeur vide.");
            }
        }

        // Lecture sans nettoyage : les espaces saisis sont conservés. Q seul annule et revient au menu.
        static string SaisirTelQuel(string invite)
        {
            Console.Write(invite);
            string brut = Console.ReadLine() ?? string.Empty;

            if (brut.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                throw new RetourMenuException();
            }

            return brut;
        }

        // Paramètre SQL de la bourse : accepte la valeur vide (NULL)
        static SqlParameter ParamBourse(decimal? bourse)
        {
            return new SqlParameter("@bourse", SqlDbType.Decimal)
            {
                Precision = 12,
                Scale = 0,
                Value = (object?)bourse ?? DBNull.Value
            };
        }

        static string TexteNom(string nom) => string.IsNullOrWhiteSpace(nom) ? "(vide)" : nom;

        static string TexteBourse(decimal? bourse) => bourse.HasValue ? bourse.Value.ToString("0") : "(vide)";

        // Retourne null si la saisie est vide
        static DateTime? SaisirDate(string invite)
        {
            while (true)
            {
                string texte = Saisir(invite);

                if (texte.Length == 0)
                {
                    return null;
                }

                if (DateTime.TryParseExact(texte, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    return date;
                }
                Console.WriteLine("Date invalide : utilisez le format jj/mm/aaaa.");
            }
        }
    }
}