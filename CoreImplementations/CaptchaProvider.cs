using Arqanum.Windows;
using ArqanumCore.Interfaces;
using System.Threading.Tasks;

namespace Arqanum.CoreImplementations
{
    public class CaptchaProvider : ICaptchaProvider
    {
        public async Task<string> GetCaptchaTokenAsync()
        {
            var captchaWindow = new CaptchaWindow();
            captchaWindow.Activate();

            return await captchaWindow.GetCaptchaTokenAsync();
        }
    }
}
