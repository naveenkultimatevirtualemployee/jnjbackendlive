using JNJServices.API.Controllers.v1.App;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ViewModels.App;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.App
{
    public class AppReservationControllerTests
    {
        private readonly AppReservationController _controller;
        private readonly Mock<IReservationService> _reservationServiceMock;

        public AppReservationControllerTests()
        {
            // Arrange
            _reservationServiceMock = new Mock<IReservationService>();
            _controller = new AppReservationController(_reservationServiceMock.Object);
        }

        [Fact]
        public async Task GetReservations_Returns_Ok_When_Reservations_Exist()
        {
            // Arrange
            var model = new ReservationSearchViewModel
            {
                Type = 1,
                reservationid = 1,
                ClaimantID = 123,
                DateFrom = DateTime.Now.AddDays(-7).ToString("yyyy/MM/dd"), // 7 days ago
                DateTo = DateTime.Now.ToString("yyyy/MM/dd"), // today
                ReservationTimeFrom = DateTime.Now.AddHours(-1), // 1 hour ago
                ReservationTimeTo = DateTime.Now.AddHours(2), // 2 hours later
                ActionCode = "Action001",
                ReservationTextSearch = "test",
                IsTodayJobNotStarted = 1,
                IsJobComplete = 0,
                ClaimantConfirmation = "Confirmed",
                inactiveflag = 0,
                Page = 1,
                Limit = 10
            };

            var reservations = new List<ReservationAppResponseModel>
            {
                new ReservationAppResponseModel
                {
                    reservationid = 1,
                    ReservationDate = DateTime.Now,
                    ReservationTime = DateTime.Now.AddHours(2), // Assuming a reservation time 2 hours later
                    RSVSVCode = "SV001",
                    ServiceName = "Airport Pickup",
                    RSVTTCode = "TT001",
                    TripType = "One Way",
                    RSVACCode = "AC001",
                    ActionCode = "Action001",
                    PUAddress1 = "123 Main St",
                    PUAddress2 = "Apt 4B",
                    DOFacilityName = "Central Hospital",
                    DOFacilityName2 = "City Clinic",
                    DOPhone = "555-1234",
                    DOAddress1 = "456 Side St",
                    DOAddress2 = "Suite 200",
                    inactiveflag = 0, // Assuming 0 means active
                    RSVCXdescription = "Description of the service",
                    CanceledDate = null, // No cancellation
                    TotalCount = 1
                }
            };

            _reservationServiceMock
                .Setup(s => s.MobileReservationSearch(model))
                .ReturnsAsync(reservations);

            // Mock claims to simulate the user identity
            var claims = new List<Claim>
            {
                new Claim("Type", "1"),
                new Claim("UserID", "123")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            // Act
            var result = await _controller.GetReservations(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);
            var reservationData = Assert.Single((IEnumerable<ReservationAppResponseModel>)response.data);
            Assert.Equal(reservations.First().reservationid, reservationData.reservationid);
            Assert.Equal(reservations.First().ReservationDate, reservationData.ReservationDate);
            Assert.Equal(reservations.First().ReservationTime, reservationData.ReservationTime);
            Assert.Equal(reservations.First().RSVSVCode, reservationData.RSVSVCode);
            Assert.Equal(reservations.First().ServiceName, reservationData.ServiceName);
            Assert.Equal(reservations.First().RSVTTCode, reservationData.RSVTTCode);
            Assert.Equal(reservations.First().TripType, reservationData.TripType);
            Assert.Equal(reservations.First().RSVACCode, reservationData.RSVACCode);
            Assert.Equal(reservations.First().ActionCode, reservationData.ActionCode);
            Assert.Equal(reservations.First().PUAddress1, reservationData.PUAddress1);
            Assert.Equal(reservations.First().PUAddress2, reservationData.PUAddress2);
            Assert.Equal(reservations.First().DOFacilityName, reservationData.DOFacilityName);
            Assert.Equal(reservations.First().DOFacilityName2, reservationData.DOFacilityName2);
            Assert.Equal(reservations.First().DOPhone, reservationData.DOPhone);
            Assert.Equal(reservations.First().DOAddress1, reservationData.DOAddress1);
            Assert.Equal(reservations.First().DOAddress2, reservationData.DOAddress2);
            Assert.Equal(reservations.First().inactiveflag, reservationData.inactiveflag);
            Assert.Equal(reservations.First().RSVCXdescription, reservationData.RSVCXdescription);
            Assert.Equal(reservations.First().CanceledDate, reservationData.CanceledDate);
            Assert.Equal(reservations.First().TotalCount, reservationData.TotalCount);
        }

        [Fact]
        public async Task GetReservations_Returns_Ok_When_No_Reservations_Found()
        {
            // Arrange
            var model = new ReservationSearchViewModel
            {
                Type = 1,
                Page = 1,
                Limit = 10
            };

            _reservationServiceMock
                .Setup(s => s.MobileReservationSearch(model))
                .ReturnsAsync(new List<ReservationAppResponseModel>());

            // Mock claims to simulate the user identity
            var claims = new List<Claim>
        {
            new Claim("Type", "1"),
            new Claim("UserID", "123")
        };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            // Act
            var result = await _controller.GetReservations(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }
    }
}
