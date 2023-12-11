using System.Xml.Linq;

using JO2024.WebServers;

namespace JO2024.Actions
{
    internal static class Cart
    {

        public static void AddToCart(WEBCLIENT webClient, REQUEST request)
        {
            Console.WriteLine("AddToCart");
            CartManager.AddCartItem(webClient.Ip, byte.Parse(request.RawData));
        }

        public static void RemoveFromCart(WEBCLIENT webClient, REQUEST request)
        {
            Console.WriteLine("RemoveFromCart");
            CartManager.GetUserCart(webClient.Ip).RemoveItem(GetItemIdFromName(request.RawData));
        }
        public static void SetItemCartCount(WEBCLIENT webClient, REQUEST request)
        {
            Console.WriteLine("SetItemCartCount ");

            string name = request.RawData.Substring(0, request.RawData.IndexOf(":"));
            byte count = byte.Parse(request.RawData.Substring(request.RawData.IndexOf(":") + 1));
            CartManager.GetUserCart(webClient.Ip).SetCount(GetItemIdFromName(name), count);
        }

        public static void GetCartCount(WEBCLIENT webClient, REQUEST request)
        {
            Console.WriteLine("GetCartCount");
            var items = CartManager.GetUserCart(webClient.Ip).Items;
            int count = 0;
            foreach (var item in items)
            {
                count += item.Value;
            }
            request.Answer = count.ToString();
            Console.WriteLine("GetCartCount => " + request.Answer);
        }

        public static void GetCartPrice(WEBCLIENT webClient, REQUEST request)
        {
            int price = 0;

            Console.WriteLine("GetCartPrice");
            var items = CartManager.GetUserCart(webClient.Ip).Items;

            foreach (var item in items)
            {
                price += GetCartItemPrice(item.Key) * item.Value;
            }

            request.Answer = price.ToString();
        }

        public static void GetCart(WEBCLIENT webClient, REQUEST request)
        {
            Console.WriteLine("GetCart");
            var items = CartManager.GetUserCart(webClient.Ip).Items;

            string content = "";

            foreach (var item in items)
            {
                content += GetCartItemName(item.Key) + ":" + item.Value + "\n";
            }

            request.Answer = content;
        }



        private static string GetCartItemName(byte id)
        {
            var offres = Offres.GetAllOffres();
            foreach (var offre in offres)
            {
                if (offre["id"].ToString() == id.ToString())
                    return offre["name"].ToString();
            }
            return "?";
        }

        private static int GetCartItemPrice(byte id)
        {
            var offres = Offres.GetAllOffres();
            foreach (var offre in offres)
            {
                if (offre["id"].ToString() == id.ToString())
                    return (int)offre["price"];
            }
            return 0;
        }

        private static byte GetItemIdFromName(string name)
        {
            var offres = Offres.GetAllOffres();
            foreach (var offre in offres)
            {
                if (offre["name"].ToString() == name)
                    return (byte)(int)offre["id"];
            }

            return 255;
        }
    }
}