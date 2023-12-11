using System.Net;
using System.Text;

using JO2024.Actions;
using JO2024.WebServers;

using MySql.Data.MySqlClient;

namespace JO2024.CustomPages
{
    public static class MonEspace
    {
        public static byte[] BuildMonEspacePage(HttpListenerContext context)
        {
            var wbclient = new WEBCLIENT() { Context = context };
            int UserId = Login.VerifGetSessionTokenUserId(wbclient);
            if (UserId == -1) { return File.ReadAllBytes("webcontent/logincreate/index.html"); }
            string page = File.ReadAllText("webcontent" + "/monespace/index.html");

            string content = "";

            //foreach (var offre in offres)
            //{
            //    content += CreateOffreCard(offre);
            //}

            string selectQuery = "SELECT * FROM billet WHERE LEFT(cle, 16) = @userKey";
            MySqlCommand selectCmd = new MySqlCommand(selectQuery, Database.mySqlConnection);
            selectCmd.Parameters.AddWithValue("@userKey", Login.GetAccountCle(UserId));

            var billets = Database.ReadMultiples(selectCmd);

            foreach (var billet in billets)
            {
                foreach (var b in billet)
                {
                    content += GetBilletInfo(b.Value.ToString());
                }
            }

            if (content == "")
                content = "Vous ne disposez d'aucun billet !";
            else
                content = "<p>Vous disposez des billets suivant :</p> <br>" + content;

            page = page.Replace("%billets%", content);
            return Encoding.UTF8.GetBytes(page);

        }

        public static string GetBilletInfo(string entireKey)
        {
            //string user = entireKey.Substring(0, 16);
            int offreID = int.Parse(entireKey.Substring(16, 1));
            //string billetKey = entireKey.Substring(17, entireKey.Length - 18);

            

            return "<p>- " + Offres.GetOffresNameFromId(offreID) + "</p>";
        }

    }
}
