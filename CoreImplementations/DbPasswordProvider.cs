using ArqanumCore.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Arqanum.CoreImplementations
{
    internal class DbPasswordProvider : IDbPasswordProvider
    {
        private const string FileName = "dbkey_protected.bin";

        public async Task<string> GetDatabasePassword()
        {
            var path = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
           FileName);

            if (File.Exists(path))
            {
                var protectedData = File.ReadAllBytes(path);
                var unprotected = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(unprotected);
            }
            else
            {
                var rawPassword = RandomNumberGenerator.GetBytes(32);
                var protectedData = ProtectedData.Protect(rawPassword, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(path, protectedData);
                return Convert.ToBase64String(rawPassword);
            }
        }
    }
}
