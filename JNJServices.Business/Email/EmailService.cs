using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using JNJServices.Utility.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Business.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly IConfiguration _configuration;
        private readonly TimeZoneConverter _timeZoneConverter;
        private readonly ISettingsService _settingsService;

        public EmailService(IOptions<SmtpSettings> smtpSettings, IConfiguration configuration, TimeZoneConverter timeZoneConverter, ISettingsService settingsService)
        {
            _smtpSettings = smtpSettings.Value;
            _configuration = configuration;
            _timeZoneConverter = timeZoneConverter;
            _settingsService = settingsService;
        }

        public async Task<string> SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    return "Recipient email address is required.";
                }

                var smtpClient = new SmtpClient(_smtpSettings.Server)
                {
                    Port = _smtpSettings.Port,
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                var emailAddresses = toEmail.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emailAddresses)
                {
                    var trimmedEmail = email.Trim();
                    if (!string.IsNullOrEmpty(trimmedEmail))
                    {
                        mailMessage.To.Add(trimmedEmail);
                    }
                }

                if (mailMessage.To.Count == 0)
                {
                    return "No valid recipient email addresses provided.";
                }

                await smtpClient.SendMailAsync(mailMessage);
                return ResponseMessage.EMAILSEND;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public async Task<EmailWebResponseModel> SendPasswordResetEmailWeb(vwDBUsers model)
        {
            ForgotPasswordMailTokenWebViewModel mailToken = new ForgotPasswordMailTokenWebViewModel();
            EmailWebResponseModel emailWebResponse = new EmailWebResponseModel();
            MailArguments mailArguments = new MailArguments();

            string WebHost = _configuration.GetSection("BaseUrl:WebHost").Value ?? string.Empty;
            string EncryptKey = _configuration.GetSection("EncryptionDecryption:Key").Value ?? string.Empty;
            mailToken.UserID = model.UserID ?? string.Empty;
            mailToken.DateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone().AddMinutes(10);

            dynamic res = Cryptography.Encrypt(JsonSerializer.Serialize(mailToken), EncryptKey);

            mailArguments.MailTo = !string.IsNullOrEmpty(model.email) ? model.email : string.Empty;
            string changePasswordLink = $"{WebHost}/change-password?id={res}";
            mailArguments.MySubject = "JNJ application Password Change";
            mailArguments.MyMessage = $@"<html><head><meta charset=""UTF-8""><title>Password Change Request</title></head><body style=""font-family: Arial, sans-serif; line-height: 1.6;""><p>Dear {model.UserName},</p><p>We have received a request to change the password for your account. Please click the link below to proceed with updating your password:</p><p><a href=""{changePasswordLink}"" style=""color: #1a73e8;"">Change Password</a></p><p>If you did not request a password change, please ignore this email or contact our support team immediately.</p><p>For security reasons, the link will expire in 10 minutes. If you encounter any issues or need further assistance, do not hesitate to reach out to us.</p><p>Thank you for your attention to this matter.</p></body></html>";


            emailWebResponse.EmailResponse = await SendEmailAsync(mailArguments.MailTo, mailArguments.MySubject, mailArguments.MyMessage);

            return emailWebResponse;
        }

        public async Task<EmailWebResponseModel> RaiseTicketApp(RaiseTicketsViewModel email, AppTokenDetails appToken)
        {
            EmailWebResponseModel response = new EmailWebResponseModel();
            MailArguments mail = new MailArguments();
            string toEmail = _configuration.GetSection("RaiseTicketSetting:ToEmail").Value ?? string.Empty;

            string user = appToken.Type == (int)Utility.Enums.Type.Contractor
             ? $"Contractor"
             : $"Claimant";

            mail.MailFrom = email.EmailID;
            mail.MailTo = toEmail;
            mail.MySubject = $"Ticket Raised by {user}, {email.UserName} - {appToken.UserID}";
            mail.MyMessage = $"<p>Dear Admin,</p><p>The {user}, {email.UserName} has raised a ticket.</p><p><strong>Issue Description:</strong><br>{email.EmailBody}</p><p><strong>Contact Info:</strong><br>Name: {email.UserName}<br>{user} ID: {appToken.UserID}<br>Email ID: {email.EmailID}</p><p>Please address this issue at your earliest convenience.</p><p>Thank you.</p>";


            response.EmailResponse = await SendEmailAsync(mail.MailTo, mail.MySubject, mail.MyMessage);
            return response;
        }

        public async Task<EmailWebResponseModel> PreferredContractorNotMatchedMail(int ReservationID, string AssgnNum)
        {
            EmailWebResponseModel response = new EmailWebResponseModel();
            MailArguments mail = new MailArguments();

            var emailSetting = await _settingsService.GetSettingByKeyAsync(new SettingKeyViewModel
            {
                SettingKey = "PREFERRED_CONTRACTOR_EMAILTO"
            });


            mail.MailTo = emailSetting?.SettingValue?.Trim();

            if (string.IsNullOrWhiteSpace(mail.MailTo))
            {
                response.EmailResponse = "No recipient email address configured for PREFERRED_CONTRACTOR_EMAILTO.";
                return response;
            }

            mail.MySubject = "Preferred Contractor Not Matched with Requirements";
            mail.MyMessage = $@"<p>Dear Admin,</p> <p>This is to inform you that a preferred contractor was found but did not match the specified requirements for the following assignment:</p><p><strong>Reservation Assignment ID:</strong> {AssgnNum}<br /></p><p>Please review this case and take the necessary action.</p><p>Thank you.</p>";

            response.EmailResponse = await SendEmailAsync(mail.MailTo, mail.MySubject, mail.MyMessage);
            return response;
        }

        public async Task<EmailWebResponseModel> PreferredContractorMatchedNotAssigned(string AssgnNum)
        {
            EmailWebResponseModel response = new EmailWebResponseModel();
            MailArguments mail = new MailArguments();

            var emailSetting = await _settingsService.GetSettingByKeyAsync(new SettingKeyViewModel
            {
                SettingKey = "PREFERRED_CONTRACTOR_EMAILTO"
            });

            mail.MailTo = emailSetting?.SettingValue?.Trim();

            if (string.IsNullOrWhiteSpace(mail.MailTo))
            {
                response.EmailResponse = "No recipient email address configured for PREFERRED_CONTRACTOR_EMAILTO.";
                return response;
            }

            mail.MySubject = "Preferred Contractor Found but Not Assigned";
            mail.MyMessage = $@"<p>Dear Admin,</p><p>This is to inform you that a preferred contractor was found for the following assignment, but no contractor has been assigned yet.</p><p><strong>Reservation Assignment ID:</strong> {AssgnNum}</p><p>Please review this case and take the necessary action.</p><p>Thank you.</p>";


            response.EmailResponse = await SendEmailAsync(mail.MailTo, mail.MySubject, mail.MyMessage);
            return response;
        }

    }
}
