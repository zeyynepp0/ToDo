using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.Helpers
{
    public static class HashingHelper
    {
        // Argon2 yapılandırma ayarları (Bu değerler sabittir, değiştirirseniz eski şifreler çalışmaz)
        private const int MemorySize = 65536; // 64 MB RAM kullanımı
        private const int Iterations = 4;     // İşlem tekrar sayısı
        private const int Parallelism = 2;    // Çekirdek (Thread) sayısı

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // 1. Rastgele 128-bit (16 byte) Salt oluştur
            passwordSalt = CreateSalt();

            // 2. Argon2id algoritmasını yapılandır
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = passwordSalt;
                argon2.DegreeOfParallelism = Parallelism;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;

                // 3. 256-bit (32 byte) Hash üret
                passwordHash = argon2.GetBytes(32);
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // Kayıtlı Salt ile aynı işlemi tekrarla
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = storedSalt;
                argon2.DegreeOfParallelism = Parallelism;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;

                var computedHash = argon2.GetBytes(32);

                // Hesaplanan hash ile veritabanındaki hash aynı mı
                return computedHash.SequenceEqual(storedHash);
            }
        }

        // Salt üretici
        private static byte[] CreateSalt()
        {
            var buffer = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return buffer;
        }
    }
}
