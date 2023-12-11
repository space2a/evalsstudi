using System.Text;

using Google.Protobuf.WellKnownTypes;

using JO2024.Actions;

namespace JO2024.CustomPages
{
    public static class Home
    {

        public static byte[] BuildHomePage()
        {
            string page = File.ReadAllText("webcontent" + "/index.html");

            var offres = Offres.GetAllOffres();

            string content = "";

            foreach (var offre in offres)
            {
                content += CreateOffreCard(offre);
            }

            page = page.Replace("%billets%", content);
            return Encoding.UTF8.GetBytes(page);
        }

        private static string CreateOffreCard(Dictionary<string, object> offre)
        {
            var template = File.ReadAllText("webcontent/templates/billetcard.html");
            foreach (var v in offre)
            {
                template = template.Replace("%" + v.Key + "%", v.Value.ToString());
            }
            return template;
        }
    }
}
