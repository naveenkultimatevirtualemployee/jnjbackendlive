using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.Entities;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class MiscellaneousServiceTests
    {
        private readonly Mock<IDapperContext> _mockDapperContext;
        private readonly MiscellaneousService _miscellaneousService;

        // Constructor to set up the common dependencies
        public MiscellaneousServiceTests()
        {
            // Initialize the mock for IDapperContext
            _mockDapperContext = new Mock<IDapperContext>();

            // Mocking CreateConnection to return a mocked IDbConnection
            _mockDapperContext.Setup(db => db.CreateConnection())
                .Returns(() => new Mock<IDbConnection>().Object);

            // Mocking ExecuteAsync to return a successful execution (1)
            _mockDapperContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);

            // Initialize the service with the mocked IDapperContext
            _miscellaneousService = new MiscellaneousService(_mockDapperContext.Object);
        }


        [Fact]
        public async Task VehicleList_ReturnsCorrectData()
        {
            // Arrange
            var mockVehicleLists = new List<VehicleLists>
    {
        new VehicleLists { description = "Car",  code = "abc" },
        new VehicleLists {description = "Truck", code = "abcd"}
    };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<VehicleLists>(It.IsAny<string>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockVehicleLists);

            // Act
            var result = await _miscellaneousService.VehicleList();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Equal(2, result.Count()); // Ensure the result contains 2 items
            Assert.Contains(result, v => v.description == "Car");
            Assert.Contains(result, v => v.description == "Truck");
        }

        [Fact]
        public async Task VehicleList_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var emptyList = new List<VehicleLists>();

            // Mock the ExecuteQueryAsync method to return an empty list when no data is found
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<VehicleLists>(It.Is<string>(s => s.Contains("codesVEHSZ")), It.IsAny<CommandType>()))
                              .ReturnsAsync(emptyList);

            // Act
            var result = await _miscellaneousService.VehicleList();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Empty(result); // Ensure the result contains no items
        }

        [Fact]
        public async Task VehicleList_ThrowsException_WhenQueryFails()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<VehicleLists>(It.Is<string>(s => s.Contains("codesVEHSZ")), It.IsAny<CommandType>()))
                              .ThrowsAsync(new Exception("Database query failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _miscellaneousService.VehicleList());

            Assert.Equal("Database query failed", exception.Message); // Ensure the correct exception message is thrown
        }

        [Fact]
        public async Task LanguageList_ReturnsCorrectData()
        {
            // Arrange
            var mockLanguages = new List<Languages>
            {
                new Languages { description = "English", code = "EN" },
                new Languages { description = "Spanish", code = "ES" }
            };

            // Mock the ExecuteQueryAsync method to return the mockLanguages list
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<Languages>(It.Is<string>(s => s.Contains("codesLANGU")), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockLanguages);

            // Act
            var result = await _miscellaneousService.LanguageList();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Equal(2, result.Count()); // Ensure the result contains exactly 2 items
            Assert.Contains(result, l => l.description == "English"); // Ensure "English" is in the result
            Assert.Contains(result, l => l.description == "Spanish"); // Ensure "Spanish" is in the result
            Assert.Contains(result, l => l.code == "EN"); // Ensure "EN" code is in the result
            Assert.Contains(result, l => l.code == "ES"); // Ensure "ES" code is in the result
        }

        [Fact]
        public async Task LanguageList_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var emptyList = new List<Languages>();

            // Mock the ExecuteQueryAsync method to return an empty list
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<Languages>(It.Is<string>(s => s.Contains("codesLANGU")), It.IsAny<CommandType>()))
                              .ReturnsAsync(emptyList);

            // Act
            var result = await _miscellaneousService.LanguageList();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Empty(result); // Ensure the result contains no items
        }

        [Fact]
        public async Task LanguageList_ThrowsException_WhenQueryFails()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<Languages>(It.Is<string>(s => s.Contains("codesLANGU")), It.IsAny<CommandType>()))
                              .ThrowsAsync(new Exception("Database query failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _miscellaneousService.LanguageList());

            Assert.Equal("Database query failed", exception.Message); // Ensure the correct exception message is thrown
        }

        [Fact]
        public async Task LanguageList_ReturnsNull_WhenQueryReturnsNull()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<Languages>(It.Is<string>(s => s.Contains("codesLANGU")), It.IsAny<CommandType>()))
                              .ReturnsAsync((List<Languages>)null!); // Simulating a null return value

            // Act
            var result = await _miscellaneousService.LanguageList();

            // Assert
            Assert.Null(result); // Ensure the result is null
        }

        [Fact]
        public async Task GetStates_ReturnsCorrectData()
        {
            // Arrange
            var mockStates = new List<States>
            {
                new States { description = "California", code = "CA" },
                new States { description = "Texas", code = "TX" }
            };

            // Mock the ExecuteQueryAsync method to return the mockStates list
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<States>(It.Is<string>(s => s.Contains("codesSTATE")), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockStates);

            // Act
            var result = await _miscellaneousService.GetStates();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Equal(2, result.Count()); // Ensure the result contains exactly 2 items
            Assert.Contains(result, s => s.description == "California"); // Ensure "California" is in the result
            Assert.Contains(result, s => s.description == "Texas"); // Ensure "Texas" is in the result
            Assert.Contains(result, s => s.code == "CA"); // Ensure "CA" code is in the result
            Assert.Contains(result, s => s.code == "TX"); // Ensure "TX" code is in the result
        }

        [Fact]
        public async Task GetStates_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var emptyList = new List<States>();

            // Mock the ExecuteQueryAsync method to return an empty list
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<States>(It.Is<string>(s => s.Contains("codesSTATE")), It.IsAny<CommandType>()))
                              .ReturnsAsync(emptyList);

            // Act
            var result = await _miscellaneousService.GetStates();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Empty(result); // Ensure the result contains no items
        }

        [Fact]
        public async Task GetStates_ThrowsException_WhenQueryFails()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<States>(It.Is<string>(s => s.Contains("codesSTATE")), It.IsAny<CommandType>()))
                              .ThrowsAsync(new Exception("Database query failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _miscellaneousService.GetStates());

            Assert.Equal("Database query failed", exception.Message); // Ensure the correct exception message is thrown
        }

        [Fact]
        public async Task GetStates_ReturnsNull_WhenQueryReturnsNull()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<States>(It.Is<string>(s => s.Contains("codesSTATE")), It.IsAny<CommandType>()))
                              .ReturnsAsync((List<States>)null!); // Simulating a null return value

            // Act
            var result = await _miscellaneousService.GetStates();

            // Assert
            Assert.Null(result); // Ensure the result is null
        }



    }
}
