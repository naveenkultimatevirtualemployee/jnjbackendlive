using JNJServices.API.Controllers.v1.Web;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebReservationControllerTests
    {
        private readonly Mock<IReservationService> _reservationServiceMock;
        private readonly WebReservationController _controller;

        public WebReservationControllerTests()
        {
            // Arrange: Initialize the mock service
            _reservationServiceMock = new Mock<IReservationService>();

            // Act: Inject the mock service into the controller constructor
            _controller = new WebReservationController(_reservationServiceMock.Object);
        }

        [Fact]
        public async Task ReservationActionCode_ReturnsSuccessResponse_WhenDataExists()
        {
            var mockResult = new List<ReservationActionCode>
        {
            new ReservationActionCode { code = "ACTION1", description = "First Action" },
            new ReservationActionCode { code = "ACTION2", description = "Second Action" }
        };


            // Correctly setup the mock
            _reservationServiceMock
                .Setup(service => service.ReservationActionCode())
                .ReturnsAsync(mockResult); // Ensure the return type is Task<List<string>>

            // Act
            var result = await _controller.ReservationActionCode() as OkObjectResult;
            var response = result?.Value as ResponseModel;
            var returnedData = response?.data as List<ReservationActionCode>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, returnedData);
        }

        [Fact]
        public async Task ReservationActionCode_ReturnsFailureResponse_WhenNoDataExists()
        {
            // Arrange
            // Setup the mock to return an empty list
            _reservationServiceMock
                .Setup(service => service.ReservationActionCode())
                .ReturnsAsync(new List<ReservationActionCode>()); // No data returned

            // Act
            var result = await _controller.ReservationActionCode() as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ReservationService_ReturnsSuccessResponse_WhenDataExists()
        {
            // Arrange
            var mockResult = new List<ReservationsServices>
        {
            new ReservationsServices { code = "SERVICE1", description = "First Service" },
            new ReservationsServices { code = "SERVICE2", description = "Second Service" }
        };

            // Setup the mock to return the mock data
            _reservationServiceMock
                .Setup(service => service.ReservationServices())
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ReservationService() as OkObjectResult;
            var response = result?.Value as ResponseModel;
            var returnedData = response?.data as List<ReservationsServices>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, returnedData);
        }

        [Fact]
        public async Task ReservationService_ReturnsFailureResponse_WhenNoDataExists()
        {
            // Arrange
            // Setup the mock to return an empty list
            _reservationServiceMock
                .Setup(service => service.ReservationServices())
                .ReturnsAsync(new List<ReservationsServices>()); // No data returned

            // Act
            var result = await _controller.ReservationService() as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ReservationTripType_ReturnsSuccessResponse_WhenDataExists()
        {
            // Arrange
            var mockResult = new List<ReservationTripType>
        {
            new ReservationTripType { code = "TRIP1", description = "First Trip Type" },
            new ReservationTripType { code = "TRIP2", description = "Second Trip Type" }
        };

            // Setup the mock to return the mock data
            _reservationServiceMock
                .Setup(service => service.ReservationTripType())
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ReservationTripType() as OkObjectResult;
            var response = result?.Value as ResponseModel;
            var returnedData = response?.data as List<ReservationTripType>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, returnedData);
        }

        [Fact]
        public async Task ReservationTripType_ReturnsFailureResponse_WhenNoDataExists()
        {
            // Arrange
            // Setup the mock to return an empty list
            _reservationServiceMock
                .Setup(service => service.ReservationTripType())
                .ReturnsAsync(new List<ReservationTripType>()); // No data returned

            // Act
            var result = await _controller.ReservationTripType() as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);

        }

        [Fact]
        public async Task ReservationTransportType_ReturnsSuccessResponse_WhenDataExists()
        {
            // Arrange
            var mockResult = new List<ReservationTransportType>
            {
                new ReservationTransportType { code = "TRANSPORT1", description = "First Transport Type" },
                new ReservationTransportType { code = "TRANSPORT2", description = "Second Transport Type" }
            };

            // Setup the mock to return the mock data
            _reservationServiceMock
                .Setup(service => service.ReservationTransportType())
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ReservationTransportType() as OkObjectResult;
            var response = result?.Value as ResponseModel;
            var returnedData = response?.data as List<ReservationTransportType>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, returnedData);
        }

        [Fact]
        public async Task ReservationTransportType_ReturnsFailureResponse_WhenNoDataExists()
        {
            // Arrange
            // Setup the mock to return an empty list
            _reservationServiceMock
                .Setup(service => service.ReservationTransportType())
                .ReturnsAsync(new List<ReservationTransportType>()); // No data returned

            // Act
            var result = await _controller.ReservationTransportType() as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);

        }

        [Fact]
        public async Task ReservationSearch_ReturnsSuccessResponse_WhenDataExists()
        {
            // Arrange
            var searchModel = new ReservationSearchWebViewModel
            {
                Page = 1,
                Limit = 10
            };

            var mockResult = new List<ReservationSearchResponseModel>
            {
                new ReservationSearchResponseModel { TotalCount = 1  },
                new ReservationSearchResponseModel { TotalCount = 1  }
            };

            // Setup the mock to return the mock data
            _reservationServiceMock
                .Setup(service => service.ReservationSearch(searchModel))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ReservationSearch(searchModel) as OkObjectResult;
            var response = result?.Value as PaginatedResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, response.data); // Assuming data is the same type
            Assert.Equal(mockResult.First().TotalCount, response.totalData); // Assuming totalData is set correctly
        }

        [Fact]
        public async Task ReservationSearch_ReturnsFailureResponse_WhenNoDataExists()
        {
            // Arrange
            var searchModel = new ReservationSearchWebViewModel
            {
                reservationid = 123,                 // Example reservation ID
                claimnumber = "CLM123456",           // Example claim number
                ClaimID = 456,                        // Example Claim ID
                CustomerID = 789,                     // Example Customer ID
                ClaimantID = 1011,                    // Example Claimant ID
                Contractorid = 1213,                  // Example Contractor ID
                DateFrom = "2024/01/01",              // Example start date
                DateTo = "2024/12/31",                // Example end date
                Service = "Transportation",            // Example service type
                inactiveflag = 0,                     // Example inactive flag (0 for active)
                TransportType = "Bus",                // Example transport type
                TripType = "One-way",                 // Example trip type
                ClaimantConfirmation = "Yes",         // Example claimant confirmation
                ActionCode = "AC123",                 // Example action code
                ConAssignStatus = 1,                  // Example contractor assignment status
                Page = 1,                             // Current page for pagination
                Limit = 10                             // Number of records per page
            };

            // Setup the mock to return an empty list
            _reservationServiceMock
                .Setup(service => service.ReservationSearch(searchModel))
                .ReturnsAsync(new List<ReservationSearchResponseModel>()); // No data returned

            // Act
            var result = await _controller.ReservationSearch(searchModel) as OkObjectResult;
            var response = result?.Value as PaginatedResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }
    }
}
