using AutoMapper;
using JNJServices.API.Controllers.v1.Web;
using JNJServices.API.Mapper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebClaimantsControllerTests
    {
        private readonly Mock<IClaimantService> _mockClaimantService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly WebClaimantsController _controller;

        public WebClaimantsControllerTests()
        {
            // Arrange: Initialize mocks for dependencies
            _mockClaimantService = new Mock<IClaimantService>();
            _mockMapper = new Mock<IMapper>();
            // Create Mapper configuration and mapper instance
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfiles()); // Add your mapping profile here
            });
            var mapper = mappingConfig.CreateMapper();

            // Act: Create instance of the controller with the mocked dependencies
            _controller = new WebClaimantsController(_mockClaimantService.Object, mapper);
        }

        [Fact]
        public async Task ClaimantSearch_ReturnsBadRequest_WhenZipCodeIsNullAndMilesIsProvided()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                ZipCode = null,
                Miles = 5
            };

            // Act
            var result = await _controller.ClaimantSearch(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Zipcode Required", response.statusMessage);
        }

        [Fact]
        public async Task ClaimantSearch_ReturnsBadRequest_WhenMilesIsNullAndZipCodeIsProvided()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                ZipCode = 12345,
                Miles = null
            };

            // Act
            var result = await _controller.ClaimantSearch(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Miles Required", response.statusMessage);
        }

        [Fact]
        public async Task ClaimantSearch_ReturnsOk_WhenDataIsFoundWithCustomerID()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                ClaimantID = null, // Can be set if needed
                CustomerID = 1,
                FirstName = "John", // Setting FirstName
                LastName = "Doe",   // Setting LastName
                Email = "johndoe@example.com", // Setting Email
                Mobile = "123-456-7890", // Setting Mobile
                LanguageCode = "EN", // Setting LanguageCode
                ZipCode = 12345,
                Miles = 5,
                Inactive = 0, // Setting Inactive flag (0 for active)
                Page = 1, // Default page
                Limit = 20 // Default limit
            };

            var claimantResults = new List<vwClaimantSearch>
            {
                new vwClaimantSearch { ClaimantID = 1, FullName = "John Doe", LastNameFirstName = "Doe, John", FirstNameLastName = "John Doe", LastName = "Doe", FirstNameLastInitial = "J",TotalCount = 1 }
            };

            _mockClaimantService.Setup(s => s.ClaimantSearch(model)).ReturnsAsync(claimantResults);
            _mockClaimantService.Setup(s => s.AssignCustomerInfoToClaimants(claimantResults, 1)).ReturnsAsync(claimantResults);

            // Act
            var result = await _controller.ClaimantSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(claimantResults, response.data);
            Assert.Equal(claimantResults.Count, response.totalData);
        }

        [Fact]
        public async Task ClaimantSearch_ReturnsOk_WhenDataIsFoundWithoutCustomerID()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                ZipCode = 12345,
                Miles = 5,
                CustomerID = null
            };

            var claimantResults = new List<vwClaimantSearch>
            {
                new vwClaimantSearch { ClaimantID = 2, FullName = "Jane Smith", LastNameFirstName = "Smith, Jane", FirstNameLastName = "Jane Smith", LastName = "Smith", FirstNameLastInitial = "J",TotalCount = 1 }
            };

            _mockClaimantService.Setup(s => s.ClaimantSearch(model)).ReturnsAsync(claimantResults);

            // Act
            var result = await _controller.ClaimantSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(claimantResults, response.data);
            Assert.Equal(claimantResults.Count, response.totalData);
        }

        [Fact]
        public async Task ClaimantSearch_ReturnsOk_WhenNoDataFound()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                ZipCode = 12345,
                Miles = 5
            };

            _mockClaimantService.Setup(s => s.ClaimantSearch(model)).ReturnsAsync(new List<vwClaimantSearch>());

            // Act
            var result = await _controller.ClaimantSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ClaimantSelDetail_ReturnsOk_WhenNoDataFound()
        {
            // Arrange
            var model = new ClaimantIDWebViewModel { ClaimantID = 1 };

            _mockClaimantService.Setup(s => s.ClaimantSearch(It.IsAny<ClaimantSearchViewModel>()))
                .ReturnsAsync(new List<vwClaimantSearch>());

            // Act
            var result = await _controller.ClaimantSelDetail(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ClaimantSelDetail_ShouldReturnSuccess_WhenClaimantExists()
        {
            // Arrange
            var model = new ClaimantIDWebViewModel
            {
                ClaimantID = 1
            };

            var mockResult = new List<vwClaimantSearch>
            {
                new vwClaimantSearch
                {
                    ClaimantID = 1,
                    FullName = "John Doe",
                    LastNameFirstName = "Doe, John",
                    FirstNameLastName = "John Doe",
                    height = "5'9\"",
                    weight = 160
                }
            };

            var expectedMappedResult = new List<ClaimantFullNameResponseModel>
            {
                new ClaimantFullNameResponseModel
                {
                    FullName = "John Doe"
                }
            };

            // Mock service to return data when ClaimantSearch is called
            _mockClaimantService
                .Setup(service => service.ClaimantSearch(It.Is<ClaimantSearchViewModel>(x => x.ClaimantID == 1)))
                .ReturnsAsync(mockResult);

            // Mock mapper to return the mapped result
            _mockMapper
                .Setup(mapper => mapper.Map<List<ClaimantFullNameResponseModel>>(mockResult))
                .Returns(expectedMappedResult);

            // Act
            var result = await _controller.ClaimantSelDetail(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            // Ensure the data matches the expected list
            //Assert.Equal(expectedMappedResult, response.data);
        }

        [Fact]
        public async Task AllClaimant_ReturnsOk_WhenDataIsFound()
        {
            // Arrange: Create a sample dynamic search model
            var dynamicSearch = new ClaimantDynamicSearchWebViewModel();

            // Sample claimant search results to return from the service
            var claimantResults = new List<ClaimantDynamicSearchWebResponseModel>
        {
            new ClaimantDynamicSearchWebResponseModel { ClaimantID = 1, FullName = "John Doe" },
            new ClaimantDynamicSearchWebResponseModel { ClaimantID = 2, FullName = "Jane Smith" }
        };

            // Setup the mocked ClaimantService to return the expected results
            _mockClaimantService.Setup(s => s.AllClaimant(dynamicSearch))
                .ReturnsAsync(claimantResults);

            // Act: Call the AllClaimant method
            var result = await _controller.AllClaimant(dynamicSearch);

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Ensure the response is not null
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            // Ensure the response data is not null and check count
            Assert.NotNull(response.data);
            var responseData = Assert.IsAssignableFrom<List<ClaimantDynamicSearchWebResponseModel>>(response.data);
            Assert.Equal(2, responseData.Count); // Check if we got two claimants
            Assert.Equal("John Doe", responseData[0].FullName); // Check the first claimant's full name
            Assert.Equal("Jane Smith", responseData[1].FullName); // Check the second claimant's full name
        }

        [Fact]
        public async Task AllClaimant_ReturnsNotFound_WhenDataIsEmpty()
        {
            // Arrange: Create a sample dynamic search model
            var dynamicSearch = new ClaimantDynamicSearchWebViewModel();

            // Setup the mocked ClaimantService to return an empty list
            _mockClaimantService.Setup(s => s.AllClaimant(dynamicSearch))
                .ReturnsAsync(new List<ClaimantDynamicSearchWebResponseModel>());

            // Act: Call the AllClaimant method
            var result = await _controller.AllClaimant(dynamicSearch);

            // Assert: Check that the response is OkObjectResult with the correct status
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Ensure the response is not null
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);

            // Ensure the response data is null
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task AllClaimant_ReturnsNotFound_WhenServiceReturnsNull()
        {
            // Arrange: Create a sample dynamic search model
            var dynamicSearch = new ClaimantDynamicSearchWebViewModel();

            // Setup the mocked ClaimantService to return null
            _mockClaimantService.Setup(s => s.AllClaimant(dynamicSearch))
                .ReturnsAsync((List<ClaimantDynamicSearchWebResponseModel>)null!);

            // Act: Call the AllClaimant method
            var result = await _controller.AllClaimant(dynamicSearch);

            // Assert: Check that the response is OkObjectResult with the correct status
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Ensure the response is not null
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);

            // Ensure the response data is null
            Assert.Equal(response.data, string.Empty);
        }

    }
}
