
using MySql.Data.MySqlClient;

namespace JO2024
{
    public static class Database
    {
        public static string Host = "localhost";
        public static string DBName = "JO2024";
        public static string Username = "root";
        public static string Password = "";

        public static MySqlConnection mySqlConnection;

        public static void Ini()
        {
            mySqlConnection = new MySqlConnection($"SERVER={Host};DATABASE={DBName};UID={Username};PASSWORD={Password}");
            mySqlConnection.Open();

            mySqlConnection.InfoMessage += MySqlConnection_InfoMessage;            
        }

        public static bool SendCommand(MySqlCommand cmd) { return SendCommand(cmd, out object columns); }

        public static bool SendCommand(MySqlCommand cmd, out object columns)
        {
            columns = 0;

            cmd.Connection = mySqlConnection;
            cmd.Prepare();

            if (!cmd.IsPrepared) { Console.WriteLine("La commande n'était pas prete."); return false; }

            columns = (object)cmd.ExecuteScalar();
            return true;
        }

        public static Dictionary<string, object>[] ReadMultiples(string selectQuery)
        {
            MySqlCommand selectCmd = new MySqlCommand(selectQuery, mySqlConnection);

            return ReadMultiples(selectCmd);
        }

        public static Dictionary<string, object>[] ReadMultiples(MySqlCommand selectCmd)
        {
            selectCmd.Prepare();

            Dictionary<string, object>[] resultDictionaryArray = new Dictionary<string, object>[0];

            using (MySqlDataReader reader = selectCmd.ExecuteReader())
            {

                if (reader.HasRows)
                {
                    List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

                    while (reader.Read())
                    {
                        Dictionary<string, object> rowValues = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                            rowValues.Add(reader.GetName(i), reader.GetValue(i));

                        resultList.Add(rowValues);
                    }

                    resultDictionaryArray = resultList.ToArray();
                }
            }

            return resultDictionaryArray;
        }

        private static void MySqlConnection_InfoMessage(object sender, MySqlInfoMessageEventArgs args)
        {
            Console.WriteLine("MYSQL Info : ");
            for (int i = 0; i < args.errors.Length; i++)
            {
                Console.WriteLine(args.errors[i].Level + " => " + args.errors[i].Code + " : " + args.errors[i].Message);
            }
        }
    }
}
