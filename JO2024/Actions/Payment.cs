using JO2024.WebServers;

namespace JO2024.Actions
{
    public static class Payment
    {
        public static void Payement(WEBCLIENT webClient, REQUEST postRequest)
        {
            Console.WriteLine("Payement");

            //Verif que le session token soit valide...
            var userId = Login.VerifGetSessionTokenUserId(webClient);
            if (userId == -1) return; //token invalide
            //ici alors token ok

            //Verif que le panier ne soit pas vide..
            Cart.GetCartPrice(webClient, postRequest);
            if (postRequest.Answer == "0") return;
            postRequest.Answer = "";
            //ici alors panier non vide

            //Code de paiement ici...

            //Paiement ok, generation des billets...

            var cart = CartManager.GetUserCart(webClient.Ip);

            foreach (var item in cart.Items)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    BilletManager.CreateBillet(userId, item.Key); //la key ici est lID de l'offre
                }
                Offres.AddVentes(item.Key, item.Value); //augmente la valeur "ventes" pour loffre
            }

            //Tout ok, les billets sont sur la bdd

            webClient.Redirect("thanks/index.html");
        }

    }
}
