using JO2024.Actions;

using MySql.Data.MySqlClient;

namespace JO2024
{
    public static class BilletManager
    {
        public static Random Random = new Random();
        
        public static void CreateBillet(int userId, byte billetId)
        {
            string userKey = Login.GetAccountCle(userId);
            string billet;
            while (true)
            {
                billet = userKey + billetId.ToString() + Random.RandomStringSpecial(15);
                //cle du compte + id de loffre + cle du billet random
                //check si elle existe deja dans la table billet

                var result = GetBillet(billet);
                if (result == null) break;
            }

            //Envoie de la cle sur la bdd...

            string query = "INSERT INTO billet (cle) VALUES (@cle)";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@cle", billet);
            Database.SendCommand(cmd);
        }

        public static object GetBillet(string billetCle)
        {
            string query = "SELECT * FROM billet WHERE cle = @key";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@key", billetCle);

            Database.SendCommand(cmd, out object key);

            return key;
        }

        public struct Billet
        {
            public string Cle;
            public int UserId = -1;

            public Billet()
            {

            }
        }
    }
}
