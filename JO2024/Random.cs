namespace JO2024
{
    //a moi
    public class Random : System.Random
    {
        /// <summary>
        /// Generate a simple randomized string with a length of "length" with only numbers and letters
        /// </summary>
        /// <param name="length"></param>
        /// <returns>Returns the randomized string</returns>
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[base.Next(s.Length)]).ToArray());
        }

        public string RandomStringSpecial(int length)
        {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789!@#$%^&*?_-()";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[base.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generate a simple randomized file name (using RandomString(...))
        /// </summary>
        /// <param name="length"></param>
        /// <param name="directoryInfo">Targeted directory</param>
        /// <param name="ext">Example : .txt</param>
        /// <returns>Returns the randomized file name string</returns>
        public string RandomFileName(int length, DirectoryInfo directoryInfo, string ext = "")
        {
            while (true)
            {
                string rs = RandomString(length);
                if (ext == "") { if (!File.Exists(directoryInfo.FullName + "/" + rs)) return directoryInfo.FullName + "/" + rs; }
                else { if (!File.Exists(directoryInfo.FullName + "/" + rs + "ext")) return directoryInfo.FullName + "/" + rs + ext; }
            }
        }

    }
}
