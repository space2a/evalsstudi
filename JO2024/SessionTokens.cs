using MySql.Data.MySqlClient;

namespace JO2024
{
    public static class SessionTokens
    {
        public static Random Random = new Random();

        public static string GenerateSessionToken(int id)
        {
            string token;

            while (true)
            {
                token = Random.RandomStringSpecial(32);
                if (GetAccountId(token) == -1) break; //si nest pas sur deja sur la bdd on va ajouter ce token
            }

            string query = "INSERT INTO sessiontoken (userId, creation, token) VALUES (@userId, @creation, @token)";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@userId", id);
            cmd.Parameters.AddWithValue("@creation", DateTime.Now);
            cmd.Parameters.AddWithValue("@token", token);

            Console.WriteLine("Account creation : " + Database.SendCommand(cmd));

            return token;
        }

        public static int GetAccountId(string token)
        {
            string query = "SELECT userId FROM sessiontoken WHERE token = @token";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@token", token);

            Database.SendCommand(cmd, out object result);
            if (result == null) return -1;
            if (int.TryParse(result.ToString(), out int r)) return r;
            return -1;
        }

        public static void DeleteExpiredTokens()
        {
            DateTime date = DateTime.Now.AddDays(-7); 

            string deleteQuery = "DELETE FROM sessiontoken WHERE creation < @date";
            MySqlCommand deleteCmd = new MySqlCommand(deleteQuery);
            deleteCmd.Parameters.AddWithValue("@date", date);
            Database.SendCommand(deleteCmd);
        }

        public static void DeleteToken(string token)
        {
            string deleteQuery = "DELETE FROM sessiontoken WHERE token = @token";
            MySqlCommand deleteCmd = new MySqlCommand(deleteQuery);
            deleteCmd.Parameters.AddWithValue("@token", token);

            Database.SendCommand(deleteCmd);
        }

        public static void StartExpiredTokensThread()
        {
            new Thread(() =>
            {

                while (true)
                {
                    Console.WriteLine("Suppression des tokens expirés..");
                    DeleteExpiredTokens();
                    Thread.Sleep(7200000); //attente de 120 minutes
                }

            }).Start();
        }


        public struct SessionToken
        {
            public DateTime Creation;
            public int AccountId = -1;

            public SessionToken() { }
        }

    }

}
