using ArqanumCore.Interfaces;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;

namespace Arqanum.CoreImplementations
{
    public class ShowNotyficationService : IShowNotyficationService
    {
        public void ShowNotificationAsync(string title, string message)
        {
            try
            {
                AppNotification notification = new AppNotificationBuilder()
               .AddText(title)
               .AddText(message)
               .SetAudioEvent(AppNotificationSoundEvent.Default)
               .SetTimeStamp(DateTime.Now)
               .BuildNotification();

                AppNotificationManager.Default.Show(notification);
            }
            catch { }
        }
    }
}
