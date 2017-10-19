using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace BloodManagmentSystem.Services
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            using (var smtpClient = new SmtpClient())
            {
                var email = new MailMessage()
                {
                    From = new MailAddress("BMS administrator bms.mailingservice@gmail.com"),
                    Body = message.Body,
                    IsBodyHtml = true,
                    Subject = message.Subject
                };
                email.To.Add(message.Destination);
                await smtpClient.SendMailAsync(email);
            };
        }

        // private IRestResponse ConfigSendMailGun(IdentityMessage message)
        // {
        //     RestClient client = new RestClient();
        //     client.BaseUrl = new Uri("https://api.mailgun.net/v3");
        //     client.Authenticator =
        //         new HttpBasicAuthenticator("api",
        //             ConfigurationManager.AppSettings["apiKey"]);
        //     RestRequest request = new RestRequest();
        //     request.AddParameter("domain", ConfigurationManager.AppSettings["domainName"],
        //         ParameterType.UrlSegment);
        //     request.Resource = "{domain}/messages";
        //     request.AddParameter("from",
        //         "BMS administrator bms.mailingservice@gmail.com");
        //     request.AddParameter("to", message.Destination);
        //     request.AddParameter("subject", message.Subject);
        //     request.AddParameter("text", message.Body);
        //     request.AddParameter("html", message.Body);
        //     request.Method = Method.POST;
        //     return client.Execute(request);
        // }
    }
}