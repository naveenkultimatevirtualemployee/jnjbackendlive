using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class SettingsServiceTests
    {
        private readonly Mock<IDapperContext> _mockDapperContext;
        private readonly SettingsService _settingsService;
        public SettingsServiceTests()
        {
            _mockDapperContext = new Mock<IDapperContext>();

            // Arrange - Set up any necessary method setups for the mock (if needed)
            _mockDapperContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(1); // Mocking ExecuteAsync to return a successful execution (1)

            // Initialize UserService with the mocked IDapperContext
            _settingsService = new SettingsService(_mockDapperContext.Object);
        }

        [Fact]
        public async Task SettingsAsync_Should_Return_Settings_List()
        {
            // Arrange
            var expectedSettings = new List<Settings>
        {
            new Settings { SettingID = 1, SettingLabel = "Setting 1", SettingKey = "Key1" },
            new Settings { SettingID = 2, SettingLabel = "Setting 2", SettingKey = "Key2" }
        };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<Settings>(It.IsAny<string>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedSettings);

            // Act
            var result = await _settingsService.SettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Setting 1", result.First().SettingLabel);
        }

        [Fact]
        public async Task SettingsAsync_Should_Return_Empty_List_When_No_Active_Settings()
        {
            // Arrange
            var expectedSettings = new List<Settings>(); // Empty list when no active settings

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<Settings>(It.IsAny<string>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedSettings);

            // Act
            var result = await _settingsService.SettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SettingsAsync_Should_Throw_Exception_When_Query_Fails()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<Settings>(It.IsAny<string>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _settingsService.SettingsAsync());
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task UpdateSettingsAsync_Should_Return_Correct_Response_On_Success()
        {
            // Arrange
            var settings = new List<SettingWebViewModel>
        {
            new SettingWebViewModel { Value = "Setting 1", Key = "Key1" }
        };

            // Setup the mock to return expected output parameters
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ResponseCode, 200, DbType.Int32, ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "Settings updated successfully", DbType.String, ParameterDirection.Output);

            _mockDapperContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .Callback<string, DynamicParameters, CommandType>((sp, param, cmd) =>
                {
                    param.Add(DbParams.ResponseCode, 200, DbType.Int32, ParameterDirection.Output);
                    param.Add(DbParams.Msg, "Settings updated successfully", DbType.String, ParameterDirection.Output);
                })
                .ReturnsAsync(1); // Assume successful execution

            // Act
            var (responseCode, message) = await _settingsService.UpdateSettingsAsync(settings);

            // Assert
            Assert.Equal(200, responseCode);
            Assert.Equal("Settings updated successfully", message);
        }

        [Fact]
        public async Task UpdateSettingsAsync_Should_Throw_Exception_When_ExecuteAsync_Fails()
        {
            // Arrange
            var settings = new List<SettingWebViewModel>
            {
                new SettingWebViewModel { Value = "Setting 1", Key = "Key1" }
            };

            // Setup the mock to throw an exception during execution
            _mockDapperContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _settingsService.UpdateSettingsAsync(settings));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task GetSettingByKeyAsync_ReturnsSetting_WhenFound()
        {
            // Arrange
            var inputModel = new SettingKeyViewModel { SettingKey = "EnableFeatureX" };
            var expectedSetting = new SettingValueResponseModel { SettingValue = "true" };

            _mockDapperContext.Setup(db =>
                    db.ExecuteQueryFirstOrDefaultAsync<SettingValueResponseModel?>(
                        It.IsAny<string>(),
                        It.IsAny<DynamicParameters>(),
                        CommandType.Text
                    ))
                .ReturnsAsync(expectedSetting);

            // Act
            var result = await _settingsService.GetSettingByKeyAsync(inputModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSetting.SettingValue, result.SettingValue);
        }

        [Fact]
        public async Task GetSettingByKeyAsync_ReturnsDefault_WhenNotFound()
        {
            // Arrange
            var inputModel = new SettingKeyViewModel { SettingKey = "NonExistentKey" };

            _mockDapperContext.Setup(db =>
                    db.ExecuteQueryFirstOrDefaultAsync<SettingValueResponseModel?>(
                        It.IsAny<string>(),
                        It.IsAny<DynamicParameters>(),
                        CommandType.Text
                    ))
                .ReturnsAsync((SettingValueResponseModel?)null); // Simulate no data

            // Act
            var result = await _settingsService.GetSettingByKeyAsync(inputModel);

            // Assert
            Assert.NotNull(result); // Should return a new SettingValueResponseModel()
            Assert.Equal(string.Empty, result.SettingValue); // Assuming default has null SettingValue
        }
    }
}
