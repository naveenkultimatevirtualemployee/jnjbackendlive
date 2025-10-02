using JNJServices.Utility.Helper;
using JNJServices.Utility.Security;
using Microsoft.Extensions.Configuration;
using Moq;

namespace JNJServices.Tests
{
    public class JwtFactoryTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<TimeZoneConverter> _timeZoneConverterMock;
        private readonly JwtFactory _jwtFactory;

        public JwtFactoryTests()
        {
            // Mock IConfiguration with required JWT settings
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["Jwt:Issuer"]).Returns("TestIssuer");
            _configurationMock.Setup(config => config["Jwt:Audience"]).Returns("TestAudience");
            _configurationMock.Setup(config => config["Jwt:Key"]).Returns("TestKey12345678901234567890");

            // Mock TimeZoneConverter
            _timeZoneConverterMock = new Mock<TimeZoneConverter>(MockBehavior.Strict);
            _timeZoneConverterMock
                .Setup(tc => tc.ConvertUtcToConfiguredTimeZone())
                .Returns(DateTime.UtcNow); // Simulate a valid UTC-to-configured time zone conversion.

            // Create instance of JwtFactory
            _jwtFactory = new JwtFactory(_configurationMock.Object, _timeZoneConverterMock.Object);
        }

        //[Fact]
        //public void GenerateToken_ShouldReturnValidToken()
        //{
        //    // Arrange
        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, "TestUser"),
        //    new Claim("Role", "Admin")
        //};

        //    // Act
        //    var token = _jwtFactory.GenerateToken(claims);

        //    // Assert
        //    Assert.NotNull(token);
        //    Assert.IsType<string>(token);

        //    // Validate token structure
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    Assert.Equal("TestIssuer", jwtToken.Issuer);
        //    Assert.Equal("TestAudience", jwtToken.Audiences.First());
        //    Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "TestUser");
        //    Assert.Contains(jwtToken.Claims, c => c.Type == "Role" && c.Value == "Admin");
        //}

        //[Fact]
        //public void GenerateToken_ShouldThrowException_WhenKeyIsMissing()
        //{
        //    // Arrange
        //    _configurationMock.Setup(config => config["Jwt:Key"]).Returns((string?)null);

        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, "TestUser")
        //};

        //    // Act & Assert
        //    var exception = Assert.Throws<ArgumentNullException>(() => _jwtFactory.GenerateToken(claims));
        //    Assert.Contains("Key", exception.Message);
        //}

        //[Fact]
        //public void GenerateToken_ShouldExpireIn365Days()
        //{
        //    // Arrange
        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, "TestUser")
        //};

        //    // Act
        //    var token = _jwtFactory.GenerateToken(claims);

        //    // Decode token and verify expiration
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    Assert.NotNull(jwtToken.ValidTo);
        //    var expectedExpiration = _timeZoneConverterMock.Object.ConvertUtcToConfiguredTimeZone().AddDays(365);
        //    Assert.Equal(expectedExpiration.ToString("yyyy-MM-dd"), jwtToken.ValidTo.ToString("yyyy-MM-dd"));
        //}
    }
}
