using JNJServices.API.Controllers.v1.Web;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebSettingsControllerTests
    {
        private readonly Mock<ISettingsService> _settingsServiceMock;
        private readonly WebSettingsController _controller;

        public WebSettingsControllerTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"BaseUrl:Images", "https://local-api-jnjservices.betademo.net"}
            };

            var configurationMock = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _settingsServiceMock = new Mock<ISettingsService>();
            _controller = new WebSettingsController(_settingsServiceMock.Object, configurationMock);
        }

        [Fact]
        public async Task Settings_ReturnsSuccessResponse_WhenSettingsExist()
        {
            // Arrange
            var mockSettings = new List<Settings>
            {
                new Settings
                {
                    InputType = "file",
                    SettingValue = "image.jpg", // Relative path
                    InputOptions = "Option1;Option2"
                }
            };

            _settingsServiceMock
                .Setup(service => service.SettingsAsync())
                .ReturnsAsync(mockSettings);

            // Act
            var result = await _controller.Settings() as OkObjectResult;
            var response = result?.Value as ResponseModel;
            var returnedSettings = response?.data as List<Settings>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(returnedSettings);
            Assert.Single(returnedSettings); // Ensure one setting is returned

            // Check if the image URL is prefixed correctly
            string expectedUrl = "https://local-api-jnjservices.betademo.net/image.jpg";
            Assert.Equal(expectedUrl, returnedSettings[0].SettingValue);

            // Verify input options parsing
            Assert.Equal(new List<string> { "Option1", "Option2" }, returnedSettings[0].InputOptionsList);
        }

        [Fact]
        public async Task Settings_ReturnsFailureResponse_WhenNoSettingsExist()
        {
            // Arrange: Mock the service to return an empty list
            _settingsServiceMock
                .Setup(service => service.SettingsAsync())
                .ReturnsAsync(new List<Settings>()); // No settings returned

            // Act: Call the controller's Settings method
            var result = await _controller.Settings() as OkObjectResult;
            var response = result?.Value as ResponseModel;
            var returnedSettings = response?.data as List<Settings>;

            // Assert: Verify that the response indicates failure
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);

            // Check if the data is either null or an empty list
            Assert.True(returnedSettings == null || returnedSettings.Count == 0,
                        "Expected data to be null or an empty list.");
        }

        [Fact]
        public async Task Update_ReturnsSuccessResponse_WhenSettingsUpdated()
        {
            // Arrange
            var mockSettings = new List<SettingWebViewModel>
            {
                new SettingWebViewModel { Key = "abc", Value = "Key1"},
                new SettingWebViewModel { Key = "abc", Value = "Key2" }
            };

            _settingsServiceMock
                .Setup(service => service.UpdateSettingsAsync(It.IsAny<List<SettingWebViewModel>>()))
                .ReturnsAsync((1, "Settings updated successfully"));

            // Act
            var result = await _controller.Update(mockSettings) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal("Settings updated successfully", response.data);
        }

        [Fact]
        public async Task Update_ReturnsSuccessResponse_WhenSettingsNOtUpdated()
        {
            // Arrange
            var mockSettings = new List<SettingWebViewModel>
            {
                new SettingWebViewModel { Key = "abc", Value = "Key1" },
                new SettingWebViewModel { Key = "abc", Value = "Key2" }
            };

            _settingsServiceMock
                .Setup(service => service.UpdateSettingsAsync(It.IsAny<List<SettingWebViewModel>>()))
                .ReturnsAsync((0, "Settings not updated successfully"));

            // Act
            var result = await _controller.Update(mockSettings) as BadRequestObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);  // Check result is not null
            Assert.Equal(400, result.StatusCode);  // Ensure status code is 200 OK
            Assert.NotNull(result);
            Assert.Equal(ResponseStatus.FALSE, response?.status);  // Validate response status
            Assert.Equal(ResponseMessage.NOTUPDATED, response?.statusMessage);  // Validate message
            Assert.Equal("Settings not updated successfully", response?.data);  // Validate response data
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenNoSettingsProvided()
        {
            // Arrange
            var emptySettings = new List<SettingWebViewModel>();

            // Act
            var result = await _controller.Update(emptySettings) as BadRequestObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ShowSettingValue_ReturnsOk_WhenSettingValueExists()
        {
            // Arrange
            var model = new SettingKeyViewModel { SettingKey = "TestKey" };
            var mockResponse = new SettingValueResponseModel { SettingValue = "TestValue" };

            _settingsServiceMock
                .Setup(service => service.GetSettingByKeyAsync(model))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.ShowSettingValue(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal("TestValue", ((SettingValueResponseModel)response.data).SettingValue);
        }

        [Fact]
        public async Task ShowSettingValue_ReturnsBadRequest_WhenSettingValueIsEmpty()
        {
            // Arrange
            var model = new SettingKeyViewModel { SettingKey = "EmptyKey" };
            var mockResponse = new SettingValueResponseModel { SettingValue = "" };

            _settingsServiceMock
                .Setup(service => service.GetSettingByKeyAsync(model))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.ShowSettingValue(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal("", ((SettingValueResponseModel)response.data).SettingValue);
        }
    }
}
