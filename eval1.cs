//Dos Santos Aaron

namespace eval1
{
    internal class eval1
    {
        static void Main(string[] args)
        {
            //1.1 Cercle
            Cercle cercle = new Cercle(10);
            Console.WriteLine("Area : " + cercle.GetArea() + ", perimeter : " + cercle.GetPerimeter() + "\n");

            //1.2 Somme
            Console.WriteLine("\nSomme : " + Somme(1, 2));
            Console.WriteLine("Somme : " + Somme(100, 200));
            Console.WriteLine("Somme : " + Somme(-3, -2));

            //1.3 Dernier element
            string[] villes = new string[3] { "Paris", "Lyon", "Marseille" };
            Console.WriteLine("\nDerniere ville : " + RetourneDernierElementTableau(villes));

            //1.4 Ajout 1 nombre
            Console.WriteLine("\n29 + 1 : " + AjoutUn(29));

            //1.5 Soigneur animalier 
            Console.WriteLine("Nombres de pattes pour 5 poules 3 chats et 8 araignees : " + NombresPattes(5, 3, 8));
        }


        public static int Somme(int a, int b) { return a + b; }

        public static object RetourneDernierElementTableau(object[] tab)
        {
            if (tab.Length != 0)
                return tab[tab.Length - 1];
            else
            {
                throw new Exception("Le tableau est vide");
                return null;
            }
        }

        public static int AjoutUn(int nombre) { return nombre + 1; }

        public static uint NombresPattes(uint poules, uint chats, uint araignees) //uint pour eviter des nombres negatifs
        {
            //peut etre fait sur une seule ligne :
            //return (poules * 2) + (chats * 4) + (araignees * 8);
            //mais plus lisible sur 5
            uint total = 0;
            total += poules * 2;
            total += chats * 4;
            total += araignees * 8;

            return total;
        }

        public static int NombresPattesInt(uint poules, uint chats, uint araignees) //celle ci retourne un int si préférée
        {
            return (int)NombresPattes(poules, chats, araignees);
        }
    }

    public class Cercle
    {
        public readonly double Rayon = 0;

        public Cercle(double rayon)
        {
            Rayon = rayon;
        }

        /// <summary>
        /// Retourne l'aire du cercle
        /// </summary>
        /// <returns></returns>
        public double GetArea()
        {
            return Math.PI * Math.Pow(Rayon, 2);
        }

        /// <summary>
        /// Retourne le périmètre du cercle
        /// </summary>
        /// <returns></returns>
        public double GetPerimeter()
        {
            return (2 * Math.PI) * Rayon;
        }
    }

}