using JO2024.Actions;
using JO2024.WebServers;

namespace JO2024
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Database.Ini();
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Erreur, impossible d'ouvrir la connexion à la base de donnée !\nFermeture de l'application.");
                Console.ReadLine();
                Environment.Exit(1);
            }

            WebServer webServer = new WebServer(2048, 80);
            webServer.REQUESTANSWERs.Add("LoginAccount", new REQUESTANSWER() { PostActionName = "LoginAccount", RequestFunction = Login.LoginAccount, REQUESTTYPE = REQUESTANSWER.RequesType.Post });
            webServer.REQUESTANSWERs.Add("CreateAccount", new REQUESTANSWER() { PostActionName = "CreateAccount", RequestFunction = Login.CreateAccount, REQUESTTYPE = REQUESTANSWER.RequesType.Post });
            webServer.REQUESTANSWERs.Add("EmailUsed", new REQUESTANSWER() { PostActionName = "EmailUsed", RequestFunction = Login.EmailUsed, REQUESTTYPE = REQUESTANSWER.RequesType.Get });
            webServer.REQUESTANSWERs.Add("GetAccountName", new REQUESTANSWER() { PostActionName = "GetAccountName", RequestFunction = Login.GetAccountName, REQUESTTYPE = REQUESTANSWER.RequesType.Get });
            webServer.REQUESTANSWERs.Add("Logout", new REQUESTANSWER() { PostActionName = "Logout", RequestFunction = Login.Logout, REQUESTTYPE = REQUESTANSWER.RequesType.Post });

            webServer.REQUESTANSWERs.Add("AddToCart", new REQUESTANSWER() { PostActionName = "AddToCart", RequestFunction = Cart.AddToCart, REQUESTTYPE = REQUESTANSWER.RequesType.Post });
            webServer.REQUESTANSWERs.Add("SetItemCartCount", new REQUESTANSWER() { PostActionName = "SetItemCartCount", RequestFunction = Cart.SetItemCartCount, REQUESTTYPE = REQUESTANSWER.RequesType.Post });
            webServer.REQUESTANSWERs.Add("RemoveFromCart", new REQUESTANSWER() { PostActionName = "RemoveFromCart", RequestFunction = Cart.RemoveFromCart, REQUESTTYPE = REQUESTANSWER.RequesType.Post });
            webServer.REQUESTANSWERs.Add("GetCartCount", new REQUESTANSWER() { PostActionName = "GetCartCount", RequestFunction = Cart.GetCartCount, REQUESTTYPE = REQUESTANSWER.RequesType.Get });
            webServer.REQUESTANSWERs.Add("GetCart", new REQUESTANSWER() { PostActionName = "GetCart", RequestFunction = Cart.GetCart, REQUESTTYPE = REQUESTANSWER.RequesType.Get });
            webServer.REQUESTANSWERs.Add("GetCartPrice", new REQUESTANSWER() { PostActionName = "GetCartPrice", RequestFunction = Cart.GetCartPrice, REQUESTTYPE = REQUESTANSWER.RequesType.Get });

            webServer.REQUESTANSWERs.Add("Payment", new REQUESTANSWER() { PostActionName = "Payment", RequestFunction = Payment.Payement, REQUESTTYPE = REQUESTANSWER.RequesType.Post });

            webServer.REQUESTANSWERs.Add("CreateNewOffre", new REQUESTANSWER() { PostActionName = "CreateNewOffre", RequestFunction = Offres.CreateNewOffre, REQUESTTYPE = REQUESTANSWER.RequesType.Get });
            webServer.REQUESTANSWERs.Add("EditOffre", new REQUESTANSWER() { PostActionName = "EditOffre", RequestFunction = Offres.EditOffre, REQUESTTYPE = REQUESTANSWER.RequesType.Post });
            webServer.REQUESTANSWERs.Add("RemoveOffre", new REQUESTANSWER() { PostActionName = "RemoveOffre", RequestFunction = Offres.RemoveOffre, REQUESTTYPE = REQUESTANSWER.RequesType.Get });

            SessionTokens.StartExpiredTokensThread();

            webServer.Start(); //Lancer le serveur bloque le thread principal
        }
    }
}
