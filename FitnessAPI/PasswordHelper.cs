using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;


namespace FitnessAPI
{
    public static class PasswordHelper
    {
        public static byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }

        public static byte[] HashPassword(string password , byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));


            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8; // number of threads
            argon2.Iterations = 4;
            argon2.MemorySize = 1024 * 128; // 64 MB
            
            return argon2.GetBytes(16); 
        }

        public static bool VerifyPassword(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return newHash.SequenceEqual(hash);
        }
    }
}
