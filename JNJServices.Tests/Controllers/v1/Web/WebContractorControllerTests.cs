using JNJServices.API.Controllers.v1.Web;
using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebContractorControllerTests
    {
        private readonly Mock<IContractorService> _mockContractorService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<NotificationHelper>> _mockLogger;
        private readonly TimeZoneConverter _timeZoneConverter;
        private readonly FirebaseSettings _settings;
        private readonly NotificationHelper _notificationHelper;
        private readonly WebContractorController _controller;

        public static DateTime MinSqlDate => Convert.ToDateTime("1/1/1753");

        public WebContractorControllerTests()
        {
            // Arrange: In-memory configuration settings
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "EncryptionDecryption:Key", "testEncryptionKey" },
                { "SelectTimeZone:TimeZone", "India Standard Time" },
                { "FirebaseSettings:FcmCredentialsFilePath", "jnj-services-firebase-creds.json" },
                { "BaseUrl:Images", "https://local-api-jnjservices.betademo.net" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Initialize the TimeZoneConverter with the in-memory configuration
            _timeZoneConverter = new TimeZoneConverter(configuration);

            // Initialize other mocks
            _mockContractorService = new Mock<IContractorService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<NotificationHelper>>();

            // Create FirebaseSettings instance
            _settings = new FirebaseSettings
            {
                FcmCredentialsFilePath = "jnj-services-firebase-creds.json"
            };

            // Create an instance of NotificationHelper with real dependencies
            _notificationHelper = new NotificationHelper(
                _timeZoneConverter,
                Options.Create(_settings),  // Use Options.Create to create IOptions<FirebaseSettings>
                _mockLogger.Object,
                _mockNotificationService.Object
            );

            // Instantiate the controller with all dependencies
            _controller = new WebContractorController(
                _mockContractorService.Object,
                _notificationHelper  // Pass the actual NotificationHelper instance
            );
        }

        [Fact]
        public async Task ContractorService_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var contractorServices = new List<ContractorServiceType>
            {
                new ContractorServiceType { code = "001", description = "Service A" },
                new ContractorServiceType { code = "002", description = "Service B" }
            };
            _mockContractorService.Setup(service => service.GetContractorService()).ReturnsAsync(contractorServices);

            // Act
            var result = await _controller.contractorService();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(contractorServices, response.data);
        }

        [Fact]
        public async Task ContractorService_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            _mockContractorService.Setup(service => service.GetContractorService()).ReturnsAsync(new List<ContractorServiceType>());

            // Act
            var result = await _controller.contractorService();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ContractorStatus_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var contractorStatuses = new List<ContractorStatus>
            {
                new ContractorStatus { code = "Active", description = "Contractor is active" },
                new ContractorStatus { code = "Inactive", description = "Contractor is inactive" }
            };
            _mockContractorService.Setup(service => service.GetContractorStatus()).ReturnsAsync(contractorStatuses);

            // Act
            var result = await _controller.contractorstatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(contractorStatuses, response.data);
        }

        [Fact]
        public async Task ContractorStatus_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            _mockContractorService.Setup(service => service.GetContractorStatus()).ReturnsAsync(new List<ContractorStatus>());

            // Act
            var result = await _controller.contractorstatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ContractorServiceLocation_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var contractorID = 1;
            var model = new ContractorIDWebViewModel { ContractorID = contractorID };
            var locations = new List<ContractorServiceLocation>
            {
                new ContractorServiceLocation
                {
                    ContractorID = contractorID,
                    ZipCode = "12345",
                    LastChangeDate = DateTime.Now,
                    City = "Test City",
                    State = "Test State",
                    CountyName = "Test County",
                    InactiveFlag = 0
                }
            };

            _mockContractorService.Setup(service => service.ContractorServiceLoc(model)).ReturnsAsync(locations);

            // Act
            var result = await _controller.ContractorServiceLocation(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(locations, response.data);
        }

        [Fact]
        public async Task ContractorServiceLocation_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorIDWebViewModel { ContractorID = 1 };
            _mockContractorService.Setup(service => service.ContractorServiceLoc(model)).ReturnsAsync(new List<ContractorServiceLocation>());

            // Act
            var result = await _controller.ContractorServiceLocation(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ResConAvlSearch_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel { ContractorID = 1 };
            _mockContractorService.Setup(service => service.ContractorAvlSearch(model)).ReturnsAsync(new List<ContractorAvailableSearchWebResponseModel>());

            // Act
            var result = await _controller.ResConAvlSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.CONTRACTOR_NOT_AVAILABLE, response.statusMessage); // Test for CONTRACTOR_NOT_AVAILABLE message
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ResConAvlSearch_ReturnsNotFound_WhenContractorIDIsNull()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel { ContractorID = null };
            _mockContractorService.Setup(service => service.ContractorAvlSearch(model)).ReturnsAsync(new List<ContractorAvailableSearchWebResponseModel>());

            // Act
            var result = await _controller.ResConAvlSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.CONTRACTOR_NOT_FOUND, response.statusMessage); // Test for CONTRACTOR_NOT_FOUND message
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ResConAvlSearch_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel
            {
                reservationid = 1,
                reservationassignmentsid = 100,
                zipCode = "12345",
                Miles = 10.5f,
                vehsize = "Large",
                language = "English",
                vehtype = "Truck",
                certified = "Yes",
                ContractorID = 1,
                status = "Active"
            };

            var contractors = new List<ContractorAvailableSearchWebResponseModel>
        {
            new ContractorAvailableSearchWebResponseModel
            {
                contractorid = 1,
                contractorname = "Test Contractor",
                company = "Test Company",
                CellPhone = "1234567890",
                contycode = "US",
                conctcode = "CT",
                conpccode = "06103",
                gender = "Male",
                city = "Hartford",
                statecode = "CT",
                zipcode = "06103",
                miles = 15.5f,
                cstatus = "Active",
                constcode = "C123",
                RatePerMiles = "2.50",
                Cost = "37.50"
            }
        };

            _mockContractorService.Setup(service => service.ContractorAvlSearch(model)).ReturnsAsync(contractors);

            // Act
            var result = await _controller.ResConAvlSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(contractors, response.data);
        }

        [Fact]
        public async Task ResApprovedContractorSearch_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel { ContractorID = 1 };
            _mockContractorService.Setup(service => service.ApprovedContractorAvailableSearch(model)).ReturnsAsync(new List<ContractorAvailableSearchWebResponseModel>());

            // Act
            var result = await _controller.ResApprovedContractorSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ResApprovedContractorSearch_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel { ContractorID = 1 };
            var contractors = new List<ContractorAvailableSearchWebResponseModel>
            {
                new ContractorAvailableSearchWebResponseModel
                {
                    contractorid = 1,
                    contractorname = "Test Contractor",
                    company = "Test Company",
                    CellPhone = "1234567890",
                    // ...initialize other properties as needed
                }
            };

            _mockContractorService.Setup(service => service.ApprovedContractorAvailableSearch(model)).ReturnsAsync(contractors);

            // Act
            var result = await _controller.ResApprovedContractorSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(contractors, response.data);
        }

        [Fact]
        public async Task ContractorRates_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorRatesSearchViewModel
            {
                RateCode = "1",           // Required property
                StateCode = "CA",         // Optional property
                EffectiveDate = "2024/10/29", // Optional property formatted as a date string
                Inactiveflag = 0
            };
            _mockContractorService.Setup(service => service.ContractorRates(model)).ReturnsAsync(new List<ContractorRates>());

            // Act
            var result = await _controller.ContractorRates(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ContractorRates_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var model = new ContractorRatesSearchViewModel { RateCode = "1" };
            var rates = new List<ContractorRates>
            {
                new ContractorRates
                {
                    ContractorRatesID = 1,
                    RATECTCode = "RCT01",
                    EffectiveDateFrom = DateTime.Now.AddDays(-1),
                    EffectiveDateTo = DateTime.Now.AddDays(30),
                    STATECode = "CA",
                    RateDescription = "Standard Rate",
                    ContractorID = 1
                }
            };

            _mockContractorService.Setup(service => service.ContractorRates(model)).ReturnsAsync(rates);

            // Act
            var result = await _controller.ContractorRates(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(rates, response.data);
        }

        [Fact]
        public async Task ContractorRateDetails_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorRatesDetailSearchViewModel { ContractorRatesID = 1 };
            _mockContractorService.Setup(service => service.ContractorRateDetails(model)).ReturnsAsync(new List<ContractorRatesDetails>());

            // Act
            var result = await _controller.ContractorRateDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task ContractorRateDetails_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var model = new ContractorRatesDetailSearchViewModel
            {
                ContractorRatesID = 1,     // Required property
                ACCTGCode = "ACCT01",      // Optional property
                TransType = "TypeA",       // Optional property
                Language = "EN",           // Optional property
                LOB = "LOB1"
            };
            var details = new List<ContractorRatesDetails>
                {
                    new ContractorRatesDetails
                    {
                        ContractorRatesDetID = 1,
                        ContractorRatesID = 1,
                        ACCTGCode = "ACCT01",
                        TRNTYCode = "TRNT01",
                        LANGUCode = "EN",
                        rate = 100.00m,
                        flatrateflag = 1,
                        TMQty = 10,
                        MPQty = 5,
                        RoundInt = 2
                    }
                };

            _mockContractorService.Setup(service => service.ContractorRateDetails(model)).ReturnsAsync(details);

            // Act
            var result = await _controller.ContractorRateDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(details, response.data);
        }

        [Fact]
        public async Task ContractorAvailableHours_ReturnsOk_WhenDataFound()
        {
            // Arrange
            var contractorId = new ContractorIDWebViewModel { ContractorID = 1 };
            var availableHours = new List<ContractorsAvailableHours>
            {
                new ContractorsAvailableHours { AvailableDayNum = 1, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) },
                new ContractorsAvailableHours { AvailableDayNum = 2, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0) }
            };
            _mockContractorService.Setup(s => s.ContractorAvailablehours(contractorId))
                                  .ReturnsAsync(availableHours);

            // Act
            var result = await _controller.ContractorAvailableHours(contractorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(availableHours, response.data);
        }

        [Fact]
        public async Task ContractorAvailableHours_ReturnsNotFound_WhenNoDataFound()
        {
            // Arrange
            var contractorId = new ContractorIDWebViewModel { ContractorID = 1 };
            var emptyList = new List<ContractorsAvailableHours>();
            _mockContractorService.Setup(s => s.ContractorAvailablehours(contractorId))
                                  .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.ContractorAvailableHours(contractorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorAvailableHours_ReturnsBadRequest_WhenContractorIDIsNull()
        {
            // Arrange
            var contractorId = new ContractorIDWebViewModel { ContractorID = null };

            // Mock the HttpContext
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };

            _controller.ControllerContext = controllerContext;
            // Act
            var result = await _controller.ContractorAvailableHours(contractorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorAvailableHours_ReturnsBadRequest_WhenContractorIDIsInvalid()
        {
            // Arrange
            var contractorId = new ContractorIDWebViewModel { ContractorID = 0 }; // Assuming 0 is an invalid ID

            // Act
            var result = await _controller.ContractorAvailableHours(contractorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage); // Adjust according to your validation message
        }

        [Fact]
        public async Task ContractorLanguage_ReturnsBadRequest_WhenContractorIDIsNull()
        {
            // Arrange
            var contractorIdModel = new ContractorIDWebViewModel { ContractorID = null };

            // Act
            var result = await _controller.ContractorLanguage(contractorIdModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorLanguage_ReturnsOk_WhenDataFound()
        {
            // Arrange
            var contractorIdModel = new ContractorIDWebViewModel { ContractorID = 1 };
            var mockResult = new List<vwContractorLanguageSearch>
            {
                new vwContractorLanguageSearch
                {
                    ContractorID = 1,
                    LANGUCode = "EN",
                    Interpretflag = 1,
                    Translateflag = 1,
                    CertifiedFlag = 1,
                    // Populate other fields as necessary
                }
            };

            _mockContractorService.Setup(s => s.ContractorLang(contractorIdModel)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ContractorLanguage(contractorIdModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, response.data);
        }

        [Fact]
        public async Task ContractorLanguage_ReturnsNotFound_WhenNoDataIsFound()
        {
            // Arrange
            var contractorIdModel = new ContractorIDWebViewModel { ContractorID = 1 };
            var emptyResult = new List<vwContractorLanguageSearch>();

            _mockContractorService.Setup(s => s.ContractorLang(contractorIdModel)).ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.ContractorLanguage(contractorIdModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorVehicle_ReturnsBadRequest_WhenContractorIDIsNull()
        {
            // Arrange
            var model = new ContractorVehicleSearchWebViewModel { ContractorID = null };

            // Act
            var result = await _controller.ContractorVehicle(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorVehicle_ReturnsOk_WhenDataFound()
        {
            // Arrange
            var model = new ContractorVehicleSearchWebViewModel { ContractorID = 1, IsPrimary = 1 };
            var mockResult = new List<vwContractorVehicleSearch>
            {
                new vwContractorVehicleSearch
                {
                    ContractorID = 1,
                    VIN = "1HGCM82633A123456",
                    CarMake = "Honda",
                    CarModel = "Civic",
                    CarYear = 2020,
                    InsuranceCo = "AllState",
                    Insured = "John Doe",
                    // Populate other fields as necessary
                }
            };

            _mockContractorService.Setup(s => s.ContractorVehicle(model)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ContractorVehicle(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, response.data);
        }

        [Fact]
        public async Task ContractorVehicle_ReturnsNotFound_WhenNoDataIsFound()
        {
            // Arrange
            var model = new ContractorVehicleSearchWebViewModel { ContractorID = 1, IsPrimary = 1 };
            var emptyResult = new List<vwContractorVehicleSearch>();

            _mockContractorService.Setup(s => s.ContractorVehicle(model)).ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.ContractorVehicle(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorDriver_ReturnsBadRequest_WhenContractorIDIsNull()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel { ContractorID = null };

            // Act
            var result = await _controller.ContractorDriver(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorDriver_ReturnsOk_WhenDataFound()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel { ContractorID = 1, IsPrimary = 1 };
            var mockResult = new List<vwContractorDriversSearch>
            {
                new vwContractorDriversSearch
                {
                    ContractorID = 1,
                    DriverName = "John Doe",
                    Birthdate = new DateTime(1985, 1, 1),
                    DLSTATECode = "CA",
                    DrivLicExp = new DateTime(2025, 12, 31),
                    DrivLicNumber = "D1234567",
                    Gender = "M",
                    SSN = "123-45-6789",
                    // Populate other fields as necessary
                }
            };

            _mockContractorService.Setup(s => s.ContractorDriver(model)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ContractorDriver(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, response.data);
        }

        [Fact]
        public async Task ContractorDriver_ReturnsNotFound_WhenNoDataIsFound()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel { ContractorID = 1, IsPrimary = 1 };
            var emptyResult = new List<vwContractorDriversSearch>();

            _mockContractorService.Setup(s => s.ContractorDriver(model)).ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.ContractorDriver(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorSelDetail_ReturnsBadRequest_WhenContractorIDIsNull()
        {
            // Arrange
            var model = new ContractorIDWebViewModel { ContractorID = null };

            // Act
            var result = await _controller.ContractorSelDetail(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorSelDetail_ReturnsOk_WhenDataFound()
        {
            // Arrange
            var model = new ContractorIDWebViewModel { ContractorID = 1 };
            var mockResult = new List<ContractorShowSelectiveWebResponseModel>
                {
                    new ContractorShowSelectiveWebResponseModel
                    {
                        Company = "Company A" // Example data for testing
                    }
                };

            _mockContractorService.Setup(s => s.ContractorSelectiveDetails(model)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ContractorSelDetail(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, response.data);
        }

        [Fact]
        public async Task ContractorSelDetail_ReturnsNotFound_WhenNoDataIsFound()
        {
            // Arrange
            var model = new ContractorIDWebViewModel { ContractorID = 1 };
            var emptyResult = new List<ContractorShowSelectiveWebResponseModel>();
            _mockContractorService.Setup(s => s.ContractorSelectiveDetails(model)).ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.ContractorSelDetail(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ReturnsBadRequest_WhenReservationsAssignmentsIDIsNull()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = null };

            // Act
            var result = await _controller.ContractorAssignmentJobStatus(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ReturnsOk_WhenDataFound()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 };
            var mockResult = new List<ContractorJobSearch>
                {
                   new ContractorJobSearch
                    {
                        JobSearchID = 1,
                        ReservationsAssignmentsID = 1,
                        ContractorId = 101,
                        ContractorName = "John Doe",
                        Company = "Doe Enterprises",
                        CellPhone = "123-456-7890",
                        Contycode = "US",
                        Conctcode = "CA",
                        Conpccode = "12345",
                        Gender = "Male",
                        City = "Los Angeles",
                        StateCode = "CA",
                        ZipCode = "90001",
                        Miles = 10.5f,
                        Cstatus = "Active",
                        ConstCode = "C123",
                        Cost = "100.00",
                        RatePerMiles = 5.00m,
                        NotificationDateTime = DateTime.Now,
                        JobStatus = 1 // Example job status
                    },
                    new ContractorJobSearch
                    {
                        JobSearchID = 2,
                        ReservationsAssignmentsID = 1,
                        ContractorId = 102,
                        ContractorName = "Jane Smith",
                        Company = "Smith Inc.",
                        CellPhone = "987-654-3210",
                        Contycode = "US",
                        Conctcode = "NY",
                        Conpccode = "54321",
                        Gender = "Female",
                        City = "New York",
                        StateCode = "NY",
                        ZipCode = "10001",
                        Miles = 20.0f,
                        Cstatus = "Active",
                        ConstCode = "C456",
                        Cost = "200.00",
                        RatePerMiles = 7.50m,
                        NotificationDateTime = DateTime.Now,
                        JobStatus = 1 // Example job status
                    }
                };

            _mockContractorService.Setup(s => s.ContractorAssignmentJobStatus(model)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ContractorAssignmentJobStatus(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, response.data);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ReturnsNotFound_WhenNoDataIsFound()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 };
            var emptyResult = new List<ContractorJobSearch>();

            _mockContractorService.Setup(s => s.ContractorAssignmentJobStatus(model)).ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.ContractorAssignmentJobStatus(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task AllContractor_ReturnsBadRequest_WhenContractorNameIsNullOrEmpty()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel { }; // Or use string.Empty

            // Act
            var result = await _controller.AllContractor(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task AllContractor_ReturnsOk_WhenDataFound()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel { ContractorName = "John" };
            var mockResult = new List<ContractorDynamicSearchWebResponseModel>
            {
                new ContractorDynamicSearchWebResponseModel
                {
                    ContractorID = 1,
                    FullName = "John Doe"
                },
                new ContractorDynamicSearchWebResponseModel
                {
                    ContractorID = 2,
                    FullName = "John Smith"
                }
            };

            _mockContractorService.Setup(s => s.AllContractor(model)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.AllContractor(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockResult, response.data);
        }

        [Fact]
        public async Task AllContractor_ReturnsNotFound_WhenNoDataIsFound()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel { ContractorName = "NonExistentName" };
            var emptyResult = new List<ContractorDynamicSearchWebResponseModel>();

            _mockContractorService.Setup(s => s.AllContractor(model)).ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.AllContractor(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ReturnsBadRequest_WhenReservationIDOrAssignmentIDIsNull()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = null,
                ReservationAssignmentsID = null
            };

            // Act
            var result = await _controller.ContractorWebJobSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ReturnsOk_WhenValidDataIsProvided()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = 1,
                ReservationAssignmentsID = 1
            };
            var mockResult = (1, "Success");

            _mockContractorService.Setup(s => s.ContractorWebJobSearch(model)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.ContractorWebJobSearch(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task ContractorSearch_WhenMilesIsMissing_ReturnsBadRequest()
        {
            // Arrange
            var model = new ContractorSearchViewModel
            {
                ZipCode = 12345,
                Miles = null
            };

            var controller = new WebContractorController(_mockContractorService.Object, _notificationHelper);

            // Act
            var result = await controller.ContractorSearch(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Miles Required", response.statusMessage);
        }

        [Fact]
        public async Task ContractorSearch_WhenZipCodeIsMissing_ReturnsBadRequest()
        {
            // Arrange
            var model = new ContractorSearchViewModel
            {
                ZipCode = null,
                Miles = 10
            };

            var controller = new WebContractorController(_mockContractorService.Object, _notificationHelper);

            // Act
            var result = await controller.ContractorSearch(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Zipcode Required", response.statusMessage);
        }

        [Fact]
        public async Task ContractorMediaUpload_ValidJpegFile_ReturnsOk()
        {
            // Arrange
            var contractorId = 123;
            var media = new ContractorMediaViewModel
            {
                ContractorID = contractorId
            };

            var content = new byte[500 * 1024]; // 500 KB
            var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _mockContractorService.Setup(s => s.UpsertContractorMedia(It.IsAny<ContractorMediaViewModel>()))
                .ReturnsAsync((1, "Upload successful"));

            // Act
            var result = await _controller.ContractorMediaUpload(media, new List<IFormFile> { formFile });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal("Upload successful", response.statusMessage);
            Assert.NotNull(response.data);
        }

        [Fact]
        public async Task ContractorMediaUpload_NoFileProvided_ReturnsBadRequest()
        {
            // Arrange
            var media = new ContractorMediaViewModel { ContractorID = 123 };

            // Act
            var result = await _controller.ContractorMediaUpload(media, null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequest.Value);
            Assert.False(response.status);
            Assert.Equal("File image must be provided.", response.statusMessage);
        }

        [Fact]
        public async Task ContractorMediaUpload_EmptyFileInList_ReturnsBadRequest()
        {
            // Arrange
            var media = new ContractorMediaViewModel { ContractorID = 123 };
            var file = new FormFile(Stream.Null, 0, 0, "file", "empty.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _controller.ContractorMediaUpload(media, new List<IFormFile> { file });

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequest.Value);
            Assert.False(response.status);
            Assert.Equal("Invalid file.", response.statusMessage);
        }

        [Fact]
        public async Task ContractorMediaUpload_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var media = new ContractorMediaViewModel { ContractorID = 123 };
            var content = new byte[200 * 1024]; // 200 KB
            var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", "file.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            // Act
            var result = await _controller.ContractorMediaUpload(media, new List<IFormFile> { formFile });

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequest.Value);
            Assert.False(response.status);
            Assert.Equal("Invalid file type. Only JPEG, PNG, and GIF images are allowed.", response.statusMessage);
        }

        [Fact]
        public async Task ContractorMediaUpload_FileSizeExceedsLimit_ReturnsBadRequest()
        {
            // Arrange
            var media = new ContractorMediaViewModel { ContractorID = 123 };
            var content = new byte[2 * 1024 * 1024]; // 2MB
            var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", "large.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _controller.ContractorMediaUpload(media, new List<IFormFile> { formFile });

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequest.Value);
            Assert.False(response.status);
            Assert.Equal("File size exceeds 1MB limit.", response.statusMessage);
        }

    }
}
