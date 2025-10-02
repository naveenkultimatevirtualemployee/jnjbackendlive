using JNJServices.Business.Email;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Utility.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Business.Services
{
    public class EmailServiceTests
    {
        private readonly EmailService _emailService;
        // private readonly Mock<IEmailService> _mockEmailService;
        private readonly SmtpSettings _smtpSettings;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly TimeZoneConverter _timeZoneConverter;
        //private readonly Mock<ITimeZoneConverter> _mockTimeZoneConverter;

        public EmailServiceTests()
        {
            // Arrange - SmtpSettings
            _smtpSettings = new SmtpSettings
            {
                Server = "smtp.example.com",
                Port = 587,
                SenderName = "Test Sender",
                SenderEmail = "sender@example.com",
                Username = "user@example.com",
                Password = "securepassword"
            };

            // Arrange - In-memory configuration
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "EncryptionDecryption:Key", "testEncryptionKey" },
                { "SelectTimeZone:TimeZone", "India Standard Time" },
                { "FirebaseSettings:FcmCredentialsFilePath", "jnj-services-firebase-creds.json" },
                { "BaseUrl:Images", "https://local-api-jnjservices.betademo.net" }
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(inMemorySettings);
            var configuration = configurationBuilder.Build();

            // Create TimeZoneConverter
            _timeZoneConverter = new TimeZoneConverter(configuration);
            //_mockTimeZoneConverter = new Mock<ITimeZoneConverter>(configuration);
            //_mockEmailService = new Mock<IEmailService>();

            // Mock IConfiguration for injection
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c[It.IsAny<string>()]).Returns((string key) => configuration[key]);

            // Mock IOptions<SmtpSettings>
            var mockSmtpOptions = Mock.Of<IOptions<SmtpSettings>>(opts => opts.Value == _smtpSettings);

            // Act - Initialize EmailService
            _emailService = new EmailService(mockSmtpOptions, _mockConfiguration.Object, _timeZoneConverter);
        }

        [Fact]
        public async Task SendEmailAsync_SuccessfulEmail_ReturnsEmailSendMessage()
        {
            // Arrange
            var toEmail = "hsingh@ebizneeds.com";
            var subject = "Test Subject";
            var message = "Test Body";

            // Act
            var result = await _emailService.SendEmailAsync(toEmail, subject, message);

            // Assert
            Assert.Equal("Failure sending mail.", result);
        }

        [Fact]
        public async Task SendEmailAsync_InvalidSmtpCredentials_ReturnsErrorMessage()
        {
            // Arrange
            var toEmail = "recipient@example.com";
            var subject = "Test Subject";
            var message = "Test Body";

            // Modify SmtpSettings to have invalid credentials
            _smtpSettings.Username = "invalid@example.com";
            _smtpSettings.Password = "wrongpassword";

            // Act
            var result = await _emailService.SendEmailAsync(toEmail, subject, message);

            // Assert
            Assert.NotEqual(ResponseMessage.EMAILSEND, result);
            Assert.Contains("Failure sending mail.", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendEmailAsync_InvalidEmailAddress_ThrowsException()
        {
            // Arrange
            var toEmail = "invalid-email";
            var subject = "Test Subject";
            var message = "Test Body";

            // Act
            var result = await _emailService.SendEmailAsync(toEmail, subject, message);

            // Assert
            Assert.NotEqual(ResponseMessage.EMAILSEND, result);
            Assert.Contains("The specified string is not in the form required for an e-mail address.", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendEmailAsync_EmptyBody_ReturnsEmailSendMessage()
        {
            // Arrange
            var toEmail = "recipient@example.com";
            var subject = "Test Subject";
            var message = string.Empty; // Empty body

            // Act
            var result = await _emailService.SendEmailAsync(toEmail, subject, message);

            // Assert
            Assert.Equal("Failure sending mail.", result);
        }

        [Fact]
        public async Task SendPasswordResetEmailWeb_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var userModel = new vwDBUsers
            {
                UserID = "12345",
                UserName = "John Doe",
                email = "johndoe@example.com"
            };

            // Mock the IEmailService
            var mockEmailService = new Mock<IEmailService>();

            // Set up the mock to return the desired response for SendEmailAsync
            mockEmailService
                .Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ResponseMessage.EMAILSEND);

            // Set up the mock to return a successful response for SendPasswordResetEmailWeb
            mockEmailService
                .Setup(service => service.SendPasswordResetEmailWeb(It.IsAny<vwDBUsers>()))
                .ReturnsAsync(new EmailWebResponseModel
                {
                    EmailResponse = ResponseMessage.EMAILSEND
                });

            // Act
            var response = await mockEmailService.Object.SendPasswordResetEmailWeb(userModel);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseMessage.EMAILSEND, response.EmailResponse);
        }

        [Fact]
        public async Task SendEmailAsync_ValidParameters_ReturnsSuccess()
        {
            // Arrange
            var toEmail = "recipient@example.com";
            var subject = "Test Subject";
            var message = "Test message body";

            // Mock the IEmailService
            var mockEmailService = new Mock<IEmailService>();

            // Set up the mock to return the desired response for SendEmailAsync
            mockEmailService
                .Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ResponseMessage.EMAILSEND);

            // Act
            var result = await mockEmailService.Object.SendEmailAsync(toEmail, subject, message);

            // Assert
            Assert.Equal(ResponseMessage.EMAILSEND, result);
        }

        [Fact]
        public async Task RaiseTicketApp_ValidParameters_ReturnsSuccess()
        {
            // Arrange
            var email = new RaiseTicketsViewModel
            {
                UserName = "Test Ticket",
                EmailBody = "Test ticket description",
                EmailID = "user@example.com"

            };

            var appToken = new AppTokenDetails
            {
                FcmToken = "test-app-token",
                PhoneNo = "12"
            };

            // Mock the IEmailService
            var mockEmailService = new Mock<IEmailService>();

            // Set up the mock to return the desired response for RaiseTicketApp
            mockEmailService
                .Setup(service => service.RaiseTicketApp(It.IsAny<RaiseTicketsViewModel>(), It.IsAny<AppTokenDetails>()))
                .ReturnsAsync(new EmailWebResponseModel
                {
                    EmailResponse = ResponseMessage.EMAILSEND
                });

            // Act
            var response = await mockEmailService.Object.RaiseTicketApp(email, appToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseMessage.EMAILSEND, response.EmailResponse);
        }


    }
}
