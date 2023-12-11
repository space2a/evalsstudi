using System.Net;
using System.Text;

using JO2024.Actions;
using JO2024.WebServers;

namespace JO2024.CustomPages
{
    public static class Admin
    {

        public static byte[] BuildAdminPage(HttpListenerContext context)
        {
            if (Login.IsAdministrator(new WEBCLIENT() { Context = context }))
            {
                string page = File.ReadAllText("webcontent" + "/admin/index.html");

                page = page.Replace("%offres%", CreateOffresAdmin());
                return Encoding.UTF8.GetBytes(page);
            }
            else
                return File.ReadAllBytes("webcontent/errors/404.html");
        }

        public static string CreateOffresAdmin()
        {
            var offres = Offres.GetAllOffres();
            if (offres.Length == 0) return "Aucune offres créées.";

            string content = "";

            foreach (var offre in offres)
            {
                string offreHtml = File.ReadAllText("webcontent/templates/offreadmin.html");
                foreach (var value in offre)
                {
                    offreHtml = offreHtml.Replace("%" + value.Key + "%", value.Value.ToString());
                }
                content += offreHtml;
            }

            return content;
        }

    }
}
