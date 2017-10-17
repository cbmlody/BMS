﻿using System;
using System.Configuration;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using BloodManagmentSystem.Core.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using RestSharp;
using RestSharp.Authenticators;

namespace BloodManagmentSystem
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

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var manager =
                new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager,
            IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager) UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options,
            IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(),
                context.Authentication);
        }
    }
}