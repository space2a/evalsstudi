using JO2024.Actions;

namespace JO2024
{
    public static class CartManager
    {
        public static Dictionary<string, UserCart> Carts = new Dictionary<string, UserCart>();


        public static void AddCartItem(string id, byte item)
        {
            if (Carts.TryGetValue(id, out UserCart cart))
            {
                cart.AddItem(item);
            }
            else
            {
                var c = new UserCart();
                Carts.Add(id, c);
                c.AddItem(item);
            }
        }

        public static UserCart GetUserCart(string id)
        {
            if (Carts.TryGetValue(id, out UserCart cart))
            {
                return cart;
            }
            return new UserCart();
        }

    }

    public struct UserCart
    {
        public Dictionary<byte, byte> Items; //id,count //byte ? => il ny pas beaucoup d'objets donc pour minimiser la memoire nous utilisons des bytes

        public UserCart()
        {
            Items = new Dictionary<byte, byte>();
        }

        public void AddItem(byte item)
        {
            if (Items.ContainsKey(item))
            {
                Items[item]++;
            }
            else
                Items.Add(item, 1);
        }

        public void SetCount(byte item, byte count)
        {
            if (Items.ContainsKey(item))
            {
                Items[item] = count;
            }
        }

        public void RemoveItemQuantity(byte item)
        {
            if (Items.ContainsKey(item))
            {
                Items[item]--;
                if (Items.Count == 0)
                    Items.Remove(item);
            }
        }

        public void RemoveItem(byte item)
        {
            if (Items.ContainsKey(item))
            {
                Items.Remove(item);
            }
        }
    }

}
