using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using JO2024.WebServers;

using MySql.Data.MySqlClient;

using static JO2024.SessionTokens;

namespace JO2024.Actions
{
    public class Login
    {
        private static Random Random = new Random();

        public static void LoginAccount(WEBCLIENT webClient, REQUEST postRequest)
        {
            Console.WriteLine("LOGIN ACTION");

            if (!IsAccountAlreadyExisting(postRequest.Parameters["email"])) return;

            string query = "SELECT password_hash FROM user WHERE email = @email";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@email", postRequest.Parameters["email"]);

            Database.SendCommand(cmd, out object result);
            if (!String.IsNullOrWhiteSpace(result.ToString()))
            {
                Console.WriteLine(postRequest.Parameters["password"] + " ?=> " + result.ToString());
                if(SecretHasher.Verify(postRequest.Parameters["password"], result.ToString()))
                {
                    Console.WriteLine("Good password");
                    CreateSessionToken(webClient, GetAccountId(postRequest.Parameters["email"]));
                    webClient.RedirectHome();
                }
                else
                {
                    Console.WriteLine("Incorrect password");
                }
            }
        }

        public static void CreateAccount(WEBCLIENT webClient, REQUEST postRequest)
        {
            Console.WriteLine("CreateAccount");

            if (IsAccountAlreadyExisting(postRequest.Parameters["email"])) { Console.WriteLine("Compte existe deja"); return; } //Le compte existe deja
            if (!IsPasswordSecure(postRequest.Parameters["password"])) { Console.WriteLine("Mot de passe pas robuste"); return; } //Le mot de passe nest pas assez robuste

            if (postRequest.Parameters["password"] != postRequest.Parameters["password2"]) { Console.WriteLine("Mot de passe non identique"); return; }

            //Peut crée le compte ici..

            string query = "INSERT INTO user (name, lastname, email, password_hash, cle) VALUES (@name, @lastname, @email, @password, @cle)";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@name", postRequest.Parameters["fname"]);
            cmd.Parameters.AddWithValue("@lastname", postRequest.Parameters["lname"]);
            cmd.Parameters.AddWithValue("@email", postRequest.Parameters["email"]);
            cmd.Parameters.AddWithValue("@password", HashPassword(postRequest.Parameters["password"]));
            cmd.Parameters.AddWithValue("@cle", CreateAccountKey());

            Console.WriteLine("Account creation : " + Database.SendCommand(cmd));

            CreateSessionToken(webClient, GetAccountId(postRequest.Parameters["email"]));

            webClient.RedirectHome();
        }

        private static string CreateAccountKey()
        {
            string cle;
            while (true)
            {
                cle = Random.RandomStringSpecial(16);
                bool r = IsAccountAlreadyExistingKey(cle);
                if (!r) break;
            }

            return cle;
        }

        public static bool IsAdministrator(WEBCLIENT webClient)
        {
            var userId = VerifGetSessionTokenUserId(webClient);
            if (userId == -1) return false;


            string query = "SELECT isAdmin FROM user WHERE id = " + userId;
            MySqlCommand cmd = new MySqlCommand(query);

            Database.SendCommand(cmd, out object isAdmin);

            return (bool)isAdmin;
        }

        public static void Logout(WEBCLIENT webClient, REQUEST postRequest)
        {
            if(VerifGetSessionTokenUserId(webClient) != -1) //si la valeur nest pas -1 cela signifie que le token present dans le cookie "session" est valide
            {
                SessionTokens.DeleteToken(webClient.GetCookie("session").Value);
                DeleteSessionCookie(webClient);
                webClient.RedirectHome();
            }
        }

        public static void GetAccountName(WEBCLIENT webClient, REQUEST postRequest)
        {
            var id = VerifGetSessionTokenUserId(webClient);
            if (id == -1) return;
            postRequest.Answer = GetAccountName(id);
        }

        public static int VerifGetSessionTokenUserId(WEBCLIENT webClient)
        {
            var sessionCookie = webClient.GetCookie("session");
            if (sessionCookie == null) { return -1; } //pas de cookie signifie que lutilisateur nest pas connecté.
            //verif le cookie

            var id = SessionTokens.GetAccountId(sessionCookie.Value);
            if (id == -1) { /*DeleteSessionCookie(webClient);*/ Console.WriteLine("Cookie invalide!"); return -1; }; //cookie invalide

            return id;
        }

        private static void CreateSessionToken(WEBCLIENT webClient, int id)
        {
            string sessionToken = SessionTokens.GenerateSessionToken(id);

            // Stockage du token si besoin...

            webClient.Context.Response.SetCookie(new System.Net.Cookie("session", sessionToken));
        }

        private static void DeleteSessionCookie(WEBCLIENT webClient)
        {
            var expiredCookie = new Cookie("session", "")
            {
                Expires = DateTime.Now.AddYears(-1) // Date d'expiration dans le passé
            };

            // Ajouter le cookie à la réponse
            webClient.Context.Response.SetCookie(expiredCookie);
        }

        public static void EmailUsed(WEBCLIENT webClient, REQUEST postRequest)
        {
            Console.WriteLine("EmailUsed " + postRequest.RawData);

            postRequest.Answer = IsAccountAlreadyExisting(postRequest.RawData).ToString();
        }

        private static bool IsPasswordSecure(string password)
        {
            int score = 0;

            if (password.Length < 8) return false; //Un mot de passe doit faire au moins 8 chars

            if (password.Length >= 8)
                score++;

            if (Regex.IsMatch(password, @"[a-z]") && Regex.IsMatch(password, @"[A-Z]"))
                score++;

            if (Regex.IsMatch(password, @"[!,@,#,$,%,^,&,*,?,_,~,-,£,(),]") || Regex.IsMatch(password, @"[-]"))
                score++;

            Console.WriteLine(score + "/" + 3 + " => " + password);
            return score >= 3; //Pour etre "secure", le mot de password doit avoir un score de 3 ou plus. 
        }

        private static string HashPassword(string password)
        {
            return SecretHasher.Hash(password);
        }

        public static bool IsAccountAlreadyExisting(string email)
        {
            string query = "SELECT COUNT(*) FROM user WHERE email = @email";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@email", email);

            Database.SendCommand(cmd, out object columns);

            return (long)columns > 0;
        }

        public static bool IsAccountAlreadyExistingKey(string key)
        {
            string query = "SELECT COUNT(*) FROM user WHERE cle = @cle";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@cle", key);

            Database.SendCommand(cmd, out object columns);

            return (long)columns > 0;
        }

        public static int GetAccountId(string email)
        {
            string query = "SELECT id FROM user WHERE email = @email";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@email", email);

            Database.SendCommand(cmd, out object result);
            return (int)result;
        }

        public static string GetAccountName(int id)
        {
            string query = "SELECT name FROM user WHERE id = @id";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@id", id);

            Database.SendCommand(cmd, out object result);
            return result.ToString();
        }

        public static string GetAccountCle(int id)
        {
            string query = "SELECT cle FROM user WHERE id = @id";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@id", id);

            Database.SendCommand(cmd, out object result);
            return result.ToString();
        }

    }

    // https://stackoverflow.com/a/73125177
    public static class SecretHasher
    {
        private const int _saltSize = 16; // 128 bits
        private const int _keySize = 32; // 256 bits
        private const int _iterations = 50000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;

        private const char segmentDelimiter = ':';

        public static string Hash(string input)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                _iterations,
                _algorithm,
                _keySize
            );
            return string.Join(
                segmentDelimiter,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                _iterations,
                _algorithm
            );
        }

        public static bool Verify(string input, string hashString)
        {
            string[] segments = hashString.Split(segmentDelimiter);
            byte[] hash = Convert.FromHexString(segments[0]);
            byte[] salt = Convert.FromHexString(segments[1]);
            int iterations = int.Parse(segments[2]);
            HashAlgorithmName algorithm = new HashAlgorithmName(segments[3]);
            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                iterations,
                algorithm,
                hash.Length
            );
            return CryptographicOperations.FixedTimeEquals(inputHash, hash);
        }
    }
}
