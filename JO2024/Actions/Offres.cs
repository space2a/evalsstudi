using JO2024.CustomPages;
using JO2024.WebServers;

using MySql.Data.MySqlClient;

namespace JO2024.Actions
{
    public static class Offres
    {

        public static void CreateNewOffre(WEBCLIENT webClient, REQUEST postRequest)
        {
            if (!Login.IsAdministrator(webClient)) return;
            string insertQuery = "INSERT INTO offre DEFAULT VALUES";
            MySqlCommand insertCmd = new MySqlCommand(insertQuery);
            Database.SendCommand(insertCmd);
        }

        public static void EditOffre(WEBCLIENT webClient, REQUEST postRequest)
        {
            if (!Login.IsAdministrator(webClient)) return;

            string updateQuery = "UPDATE offre SET imagepath = @imagepath, access = @access, price = @price, displayTitle = @displayTitle, displayDescription = @displayDescription," +
                "name = @name WHERE id = @id";
            MySqlCommand updateCmd = new MySqlCommand(updateQuery);

            foreach (var para in postRequest.Parameters)
            {
                updateCmd.Parameters.AddWithValue("@" + para.Key, para.Value);
            }

            Database.SendCommand(updateCmd);

            webClient.Redirect(Admin.BuildAdminPage(webClient.Context));
        }

        public static void RemoveOffre(WEBCLIENT webClient, REQUEST postRequest)
        {
            if (!Login.IsAdministrator(webClient)) return;

            string updateQuery = "DELETE FROM offre WHERE id = @id";
            MySqlCommand updateCmd = new MySqlCommand(updateQuery);
            updateCmd.Parameters.AddWithValue("@id", postRequest.RawData);

            Database.SendCommand(updateCmd);
        }

        public static Dictionary<string, object>[] GetAllOffres()
        {
            return Database.ReadMultiples("SELECT id, imagepath, access, price, displayTitle, displayDescription, name, ventes FROM offre");
        }

        public static string GetOffresNameFromId(int id)
        {
            string updateQuery = "SELECT name FROM offre WHERE id = @id";
            MySqlCommand updateCmd = new MySqlCommand(updateQuery);
            updateCmd.Parameters.AddWithValue("@id", id);

            Database.SendCommand(updateCmd, out object name);
            if (name == null) return "";
            return name.ToString();
        }

        public static void AddVentes(int id, int amount = 1)
        {
            string updateQuery = "UPDATE offre SET ventes = ventes + @amount WHERE id = @id";
            MySqlCommand updateCmd = new MySqlCommand(updateQuery);
            updateCmd.Parameters.AddWithValue("@amount", amount);
            updateCmd.Parameters.AddWithValue("@id", id);

            Database.SendCommand(updateCmd);
        }

    }
}
