using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;

namespace VKSaver.Core.Services
{
    public sealed class EmailService : IEmailService
    {
        public EmailService(ILocService locService)
        {
            _locService = locService;
        }

        public async Task SendSupportEmail()
        {
            var recipient = new EmailRecipient
            {
                Address = SUPPORT_EMAIL,
                Name = _locService["AppNameText"]
            };
            
            EmailMessage mail = new EmailMessage();
            mail.Subject = _locService["Email_SupportTitle_Text"];

#if WINDOWS_UWP
            mail.Body = $"{_locService["AppNameText"]} UWP {GetApplicationVersion()}";
#else
            mail.Body = $"{_locService["AppNameText"]} {GetApplicationVersion()}"; 
#endif

            mail.To.Add(recipient);
            
            await EmailManager.ShowComposeNewEmailAsync(mail);
        }

        private static string GetApplicationVersion()
        {
            var version = Package.Current.Id.Version;
            return String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private readonly ILocService _locService;

        private const string SUPPORT_EMAIL = "vksaverapp@outlook.com";
    }
}
