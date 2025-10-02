using Asp.Versioning.ApiExplorer;
using JNJServices.API.Helper;
using Moq;

namespace JNJServices.Tests.Helper
{
    public class ConfigureSwaggerOptionsTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProvider_WhenValidProviderIsPassed()
        {
            // Arrange
            var mockProvider = new Mock<IApiVersionDescriptionProvider>();

            // Act
            var configureSwaggerOptions = new ConfigureSwaggerOptions(mockProvider.Object);

            // Assert
            Assert.NotNull(configureSwaggerOptions); // Ensure the instance is created
        }


    }
}
