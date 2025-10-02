using JNJServices.API.Helper;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace JNJServices.Tests.Helper
{
    public class CronHelperTests
    {
        private readonly Mock<NotificationHelper> _mockNotificationHelper;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly CronHelper _cronHelper;

        public CronHelperTests()
        {
            // Arrange: Set up the mock for NotificationHelper
            _mockNotificationHelper = new Mock<NotificationHelper>();

            // Arrange: Set up the mock for IServiceProvider
            _mockServiceProvider = new Mock<IServiceProvider>();

            // Arrange: Set up the mock for IServiceScopeFactory
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            // Arrange: Set up the mock for IServiceScope
            _mockServiceScope = new Mock<IServiceScope>();

            // Arrange: Mock IServiceProvider to return the mock IServiceScopeFactory
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockServiceScopeFactory.Object);

            // Arrange: Mock IServiceScopeFactory to return the mock IServiceScope
            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            // Arrange: Mock IServiceScope to return the mock NotificationHelper
            _mockServiceScope
                .Setup(x => x.ServiceProvider.GetService(typeof(NotificationHelper)))
                .Returns(_mockNotificationHelper.Object);

            // Act: Initialize CronHelper with the mocked IServiceProvider
            _cronHelper = new CronHelper(_mockServiceProvider.Object);
        }


    }
}
