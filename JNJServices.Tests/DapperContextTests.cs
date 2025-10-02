using JNJServices.Data;
using Microsoft.Extensions.Configuration;
using Moq;

namespace JNJServices.Tests
{
    public class DapperContextTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly DapperContext _dapperContext;

        public DapperContextTests()
        {
            // Mock the IConfiguration
            _mockConfiguration = new Mock<IConfiguration>();

            // Mock the GetSection method to return a configuration value
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns("Server=myServer;Database=myDb;");

            // Set up the mock configuration to return the mock section when GetSection is called
            _mockConfiguration.Setup(config => config.GetSection("ConnectionStrings:MyConnection"))
                              .Returns(mockSection.Object);

            // Create the instance of DapperContext with the mocked IConfiguration
            _dapperContext = new DapperContext(_mockConfiguration.Object);
        }


    }
}
