using JNJServices.API.Controllers.v1.Web;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebMiscControllerTests
    {
        private readonly Mock<IMiscellaneousService> _mockMiscellaneousService;
        private readonly Mock<IClaimantService> _mockClaimantService;  // Add this line
        private readonly Mock<ICustomerService> _mockCustomerService;  // Add this line
        private readonly WebMiscController _controller;

        public WebMiscControllerTests()
        {
            // Arrange: Initialize the mocks
            this._mockMiscellaneousService = new Mock<IMiscellaneousService>();
            this._mockClaimantService = new Mock<IClaimantService>();    // Initialize the mock for IClaimantService
            this._mockCustomerService = new Mock<ICustomerService>();    // Initialize the mock for ICustomerService

            // Initialize the controller with the mocked services
            this._controller = new WebMiscController(
                this._mockMiscellaneousService.Object,  // Mocked IMiscellaneousService
                this._mockCustomerService.Object,        // Use the mocked ICustomerService
                this._mockClaimantService.Object         // Use the mocked IClaimantService
            );
        }

        [Fact]
        public async Task VehicleList_ReturnsOkWithData_WhenVehiclesAreAvailable()
        {
            // Arrange: Setup the mock to return a list of vehicles
            var vehicleList = new List<VehicleLists> { new VehicleLists { code = "abc", description = "Car" } };
            _mockMiscellaneousService.Setup(service => service.VehicleList())
                .ReturnsAsync(vehicleList);

            // Act: Call the VehicleList method
            var result = await _controller.VehicleList();

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);
            Assert.Equal(vehicleList, response.data);
        }

        [Fact]
        public async Task VehicleList_ReturnsOkWithNoData_WhenNoVehiclesAreAvailable()
        {
            // Arrange: Setup the mock to return an empty list
            _mockMiscellaneousService.Setup(service => service.VehicleList())
                .ReturnsAsync(new List<VehicleLists>());

            // Act: Call the VehicleList method
            var result = await _controller.VehicleList();

            // Assert: Check that the response is OkObjectResult with data not found message
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task LanguageList_ReturnsOkWithData_WhenLanguagesAreAvailable()
        {
            // Arrange: Setup the mock to return a list of languages
            var languageList = new List<Languages>
        {
            new Languages {code = "abc", description = "English"},
            new Languages {code = "abc", description = "Spanish"}
        };
            _mockMiscellaneousService.Setup(service => service.LanguageList())
                .ReturnsAsync(languageList);

            // Act: Call the LanguageList method
            var result = await _controller.LanguageList();

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            // Validate that the returned data matches the expected list
            var returnedData = Assert.IsType<List<Languages>>(response.data);
            Assert.Equal(languageList.Count, returnedData.Count);
        }

        [Fact]
        public async Task LanguageList_ReturnsOkWithNoData_WhenNoLanguagesAreAvailable()
        {
            // Arrange: Setup the mock to return an empty list
            _mockMiscellaneousService.Setup(service => service.LanguageList())
                .ReturnsAsync(new List<Languages>());

            // Act: Call the LanguageList method
            var result = await _controller.LanguageList();

            // Assert: Check that the response is OkObjectResult with data not found message
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task Dashboard_ReturnsOkWithDashboardData_WhenDataIsAvailable()
        {
            // Arrange: Setup the mock to return a DashboardWebResponseModel
            var dashboardData = new DashboardWebResponseModel
            {
                counts = new List<Counts>
            {
                new Counts { name = "Total Reservations", value = 150, icon = "reservation-icon" },
                new Counts { name = "Active Users", value = 75, icon = "user-icon" }
            },
                reservationbyMonths = new DatabyMonths
                {
                    name = "Reservations by Month",
                    data = new List<GraphByMonth>
                {
                    new GraphByMonth { month = "January", value = 20 },
                    new GraphByMonth { month = "February", value = 30 }
                }
                },
                reservationStatus = new DataByStatus
                {
                    name = "Reservation Status",
                    data = new List<GraphByStatus>
                {
                    new GraphByStatus { group = "Confirmed", value = 100 },
                    new GraphByStatus { group = "Pending", value = 50 }
                }
                }
                // Populate other properties as needed
            };

            _mockMiscellaneousService.Setup(service => service.GetDashboardData())
                .ReturnsAsync(dashboardData);

            // Act: Call the Dashboard method
            var result = await _controller.Dashboard();

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            // Validate the data
            var dashboardResponse = Assert.IsType<DashboardWebResponseModel>(response.data);
            Assert.Equal(2, dashboardResponse.counts.Count);
            Assert.Equal("Total Reservations", dashboardResponse.counts[0].name);
            Assert.Equal(150, dashboardResponse.counts[0].value);
            Assert.Equal("Active Users", dashboardResponse.counts[1].name);
            Assert.Equal(75, dashboardResponse.counts[1].value);
            Assert.Equal("Reservations by Month", dashboardResponse.reservationbyMonths.name);
            Assert.Equal(2, dashboardResponse.reservationbyMonths.data.Count);
            Assert.Equal("January", dashboardResponse.reservationbyMonths.data[0].month);
            Assert.Equal(20, dashboardResponse.reservationbyMonths.data[0].value);
        }

        [Fact]
        public async Task Dashboard_ReturnsOkWithNoData_WhenDashboardDataIsNull()
        {
            // Arrange: Setup the mock to return null
            _mockMiscellaneousService.Setup(service => service.GetDashboardData())
                .ReturnsAsync((DashboardWebResponseModel?)null!);


            // Act: Call the Dashboard method
            var result = await _controller.Dashboard();

            // Assert: Check that the response is OkObjectResult with data not found message
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task GetStates_ReturnsOkWithData_WhenStatesAreAvailable()
        {
            // Arrange: Setup the mock to return a list of states
            var statesList = new List<States>
        {
            new States { code = "NY", description = "New York" },
            new States { code = "CA", description = "California" }
        };

            _mockMiscellaneousService.Setup(service => service.GetStates())
                .ReturnsAsync(statesList);

            // Act: Call the GetStates method
            var result = await _controller.GetStates();

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            // Validate the data
            var statesResponse = Assert.IsAssignableFrom<List<States>>(response.data);
            Assert.Equal(2, statesResponse.Count);
            Assert.Equal("NY", statesResponse[0].code);
            Assert.Equal("New York", statesResponse[0].description);
            Assert.Equal("CA", statesResponse[1].code);
            Assert.Equal("California", statesResponse[1].description);
        }

        [Fact]
        public async Task GetStates_ReturnsOkWithNoData_WhenStatesAreEmpty()
        {
            // Arrange: Setup the mock to return an empty list
            _mockMiscellaneousService.Setup(service => service.GetStates())
                .ReturnsAsync(new List<States>());

            // Act: Call the GetStates method
            var result = await _controller.GetStates();

            // Assert: Check that the response is OkObjectResult with data not found message
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ClaimantCustomerSearch_ReturnsOkWithClaimants_WhenCustomerIDIsValid()
        {
            // Arrange: Setup the mock to return a list of claimants
            var model = new ClaimantCustomerSearchWebViewModel { CustomerID = 1 };
            var claimantsList = new List<vwClaimantSearch>
        {
            new vwClaimantSearch {
            ClaimantID = 1,
            FullName = "John Doe",
            LastNameFirstName = "Doe, John",
            FirstNameLastName = "John Doe",
            LastName = "Doe",
            FirstNameLastInitial = "John D.",
            FirstInitialLastName = "J. Doe",
            height = "5'11\"",
            weight = 180,
            HomePhone = "1234567890",
            CellPhone = "0987654321",
            hmcomplex = "Complex A",
            hmaddress1 = "123 Street Name",
            HmAddress2 = "Apt 1",
            hmcity = "CityX",
            hmSTATECode = "ST",
            HmZipCode = "12345",
            hmPickupNotes = "Leave at door.",
            AltComplex = "Alt Complex",
            AltAddress1 = "Alt Address 1",
            AltAddress2 = "Alt Address 2",
            AltCity = "Alt City",
            altSTATECode = "AC",
            AltZipCode = "54321",
            altPickupNotes = "Ring bell.",
            WkComplex = "Work Complex",
            WkAddress1 = "Work Address 1",
            WkAddress2 = "Work Address 2",
            wkaddress3 = "Work Address 3",
            WkCity = "Work City",
            wkSTATECode = "WC",
            WkZipCode = "67890",
            wkPickupNotes = "Leave at reception.",
            birthdate = new DateTime(1985, 1, 1),
            Gender = "Male",
            shortname = "JD",
            hmStairs = 3,
            LANGUCode1 = "EN",
            ssn = "123-45-6789",
            wkshortname = "Work JD",
            cnttycode = "US",
            cusifcode = "C123",
            HeightWeight = "5'11\"/180",
            HomePhonewH = "123-456-7890",
            ClaimantHomeAddresswNotes = "123 Street Name, CityX, ST 12345 (Leave at door)",
            ClaimantAltAddresswNotes = "Alt Address 1, Alt City, AC 54321 (Ring bell)",
            ClaimantWorkAddresswNotes = "Work Address 1, Work City, WC 67890 (Leave at reception)",
            claimantnotes = "Special instructions for claimant.",
            CreateDate = DateTime.Now,
            CreateUserID = "admin",
            LastChangeDate = DateTime.Now,
            LastChangeUserID = "admin",
            ClaimantStatus = "Active",
            ClaimantCompany = "Company A",
            ClaimantCompanycontact = "Jane Smith",
            WkPhone = "5551234567",
            wkPhoneExt = "101",
            Email = "john.doe@example.com",
            LANGUCode2 = "FR",
            CLMMSCode = "CLM001",
            Spouse = "Jane Doe",
            altPerson = "Alternate Contact",
             altRelationship = "Friend",
            AltPhone = "1234567890",
            wkStairs = 2,
            altStairs = 1,
            FirstName = "John",
            HmPhone = "0987654321",
            inactiveflag = 1,
            MiddleName = "Michael",
            SUFFXCode = "Jr.",
            TITLECode = "Mr.",
            wkContactID = 1001,
            altTimeZone = "Eastern Standard Time",
            hmTimeZone = "Pacific Standard Time",
            Email2 = "john2@example.com",
            Email3 = "john3@example.com",
            AmerisysClaimantID = "A123456",
            AmerisysClaimantEntryDate = new DateTime(2023, 10, 15),
            AmerisysClaimantUpdateDate = new DateTime(2024, 1, 1),
            WeightChair = 250,
            qaflag = 1,
            PhoneTypCode = "Mobile",
            archiveflag = 1,
            CustomerName = "ABC Corp.",
            companyName = "XYZ Ltd.",
            TotalCount = 50
            }
        };

            _mockClaimantService.Setup(service => service.ClaimantByCustomerSearch(model))
                .ReturnsAsync(claimantsList);

            // Act: Call the ClaimantCustomerSearch method
            var result = await _controller.ClaimantCustomerSearch(model);

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            var claimantsResponse = Assert.IsAssignableFrom<List<vwClaimantSearch>>(response.data);
            Assert.Single(claimantsResponse);
            Assert.Equal("John Doe", claimantsResponse[0].FullName);
        }

        [Fact]
        public async Task ClaimantCustomerSearch_ReturnsOkWithNoData_WhenCustomerIDIsInvalid()
        {
            // Arrange: Setup the model with an invalid CustomerID
            var model = new ClaimantCustomerSearchWebViewModel { CustomerID = 0 };

            // Act: Call the ClaimantCustomerSearch method
            var result = await _controller.ClaimantCustomerSearch(model);

            // Assert: Check that the response is OkObjectResult with data not found message
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ClaimantCustomerSearch_ReturnsOkWithNoData_WhenClaimantIDIsInvalid()
        {
            // Arrange: Setup the model with an invalid ClaimantID
            var model = new ClaimantCustomerSearchWebViewModel { ClaimantID = 0 };

            // Act: Call the ClaimantCustomerSearch method
            var result = await _controller.ClaimantCustomerSearch(model);

            // Assert: Check that the response is OkObjectResult with data not found message
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ClaimantCustomerSearch_ReturnsOkWithNoData_WhenModelIsInvalid()
        {
            // Arrange: Setup the model with no valid IDs
            var model = new ClaimantCustomerSearchWebViewModel();

            // Act: Call the ClaimantCustomerSearch method
            var result = await _controller.ClaimantCustomerSearch(model);

            // Assert: Check that the response is OkObjectResult with data not found message
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

    }
}
