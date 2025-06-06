using ArqanumCore.Interfaces;
using System.Threading.Tasks;

namespace Arqanum.CoreImplementations
{
    internal class CaptchaProvider : ICaptchaProvider
    {
        public Task<string> GetCaptchaTokenAsync()
        {
            return Task.FromResult(string.Empty); // Placeholder implementation, replace with actual captcha logic
        }
    }
}
