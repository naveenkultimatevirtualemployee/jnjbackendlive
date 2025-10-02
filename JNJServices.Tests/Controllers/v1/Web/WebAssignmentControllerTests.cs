using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using JNJServices.API.Controllers.v1.Web;
using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using static JNJServices.Utility.ApiConstants.NotificationConstants;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebAssignmentControllerTests
    {

        private readonly Mock<IReservationAssignmentsService> _mockReservationAssignmentsService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IAssignmentMetricsService> _mockAssignmentMetricsService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<NotificationHelper>> _mockLogger;
        private readonly TimeZoneConverter _timeZoneConverter;
        private readonly FirebaseSettings _settings;
        private readonly NotificationHelper _notificationHelper;
        private readonly Mock<INotificationHelper> _mockNotificationHelper;
        private readonly WebAssignmentController _controller;

        public static DateTime MinSqlDate => Convert.ToDateTime("1/1/1753");

        public WebAssignmentControllerTests()
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
            _mockReservationAssignmentsService = new Mock<IReservationAssignmentsService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockAssignmentMetricsService = new Mock<IAssignmentMetricsService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<NotificationHelper>>();

            InitializeFirebaseApp();

            // Create FirebaseSettings instance
            _settings = new FirebaseSettings
            {
                FcmCredentialsFilePath = "jnj-services-firebase-creds.json"
            };
            _mockNotificationHelper = new Mock<INotificationHelper>();
            // Create an instance of NotificationHelper with real dependencies
            _notificationHelper = new NotificationHelper(
                _timeZoneConverter,
                Options.Create(_settings),  // Use Options.Create to create IOptions<FirebaseSettings>
                _mockLogger.Object,
                _mockNotificationService.Object
            );

            // Instantiate the controller with all dependencies
            _controller = new WebAssignmentController(
                _mockReservationAssignmentsService.Object,
                configuration,
                _mockAssignmentMetricsService.Object,
                _notificationHelper  // Pass the actual NotificationHelper instance
            );
        }

        private static void InitializeFirebaseApp()
        {
            // Check if FirebaseApp has already been initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var options = new AppOptions
                {
                    Credential = GoogleCredential.FromFile("jnj-services-firebase-creds.json"),
                    // Set other necessary options here
                };
                FirebaseApp.Create(options);
            }
        }

        [Fact]
        public async Task ReservationAssignmentType_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var assignmentList = new List<AssignmentService>
        {
            new AssignmentService { code = "A1", description = "Description 1" },
            new AssignmentService { code = "A2", description = "Description 2" }
        };

            _mockReservationAssignmentsService
                .Setup(s => s.ReservationAssignmentType())
                .ReturnsAsync(assignmentList);

            // Act
            var result = await _controller.ReservationAssignmentType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.True(response.status); // Check if status is TRUE
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage); // Check status message
            var responseData = Assert.IsAssignableFrom<List<AssignmentService>>(response.data); // Ensure data is a List
            Assert.Equal(2, responseData.Count); // Check if data count matches
        }

        [Fact]
        public async Task ReservationAssignmentType_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            _mockReservationAssignmentsService
            .Setup(s => s.ReservationAssignmentType())
                .ReturnsAsync(new List<AssignmentService>()); // Return an empty list

            // Act
            var result = await _controller.ReservationAssignmentType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status); // Check if status is FALSE
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage); // Check status message
            Assert.Equal(response.data, string.Empty); // Ensure data is null when no records found
        }

        [Fact]
        public async Task ReservationAssignmentsearch_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var inputModel = new ReservationAssignmentWebViewModel(); // Add necessary properties for the test

            var searchResults = new List<vwReservationAssignmentsSearch>
            {
               new vwReservationAssignmentsSearch
                {
                    reservationid = 1,
                    contractorid = 10,
                    ReservationsAssignmentsID = 100,
                    linenum = 1,
                    BILCDECode = "BIL001",
                    claimnumber = "CL001",
                    WillCallFlag = 1,
                    quantity = 5.5m,
                    assgncode = "ASSIGN001",
                    langucode = "EN",
                    inactiveflag = 0,
                    MetricsEnteredFlag = 1,
                    totaltime = 30,
                    ReferenceNumber = "REF001",
                    notes = "Sample notes",
                    driveremailflag = 1,
                    drivercalledflag = 0,
                    driverconfirmedflag = 1,
                    driverreconfirmedflag = 1,
                    drivernaflag = 0,
                    leftmessageflag = 1,
                    PickupTimeSort = DateTime.Now.AddHours(1),
                    PickupTime = "14:30",
                    PickupTimewZone = "UTC",
                    AssgnNum = "ASSIGNNUM001",
                    StartTime = "14:00",
                    EndTime = "15:00",
                    EarlyLatePickupTime = "On Time",
                    contractor = "Sample Contractor",
                    contycode = "CTY001",
                    conctcode = "CCT001",
                    contractorcontact = "John Doe",
                    ContractorCompany = "Sample Company Inc.",
                    contractorhomephone = "123-456-7890",
                    contractorworkphone = "987-654-3210",
                    contractorcellphone = "555-123-4567",
                    ContractorFax = "555-765-4321",
                    ContractorEmail = "contractor@example.com",
                    concmcode = "CONCM001",
                    contractorintfid = "INTFID001",
                    contractorshortname = "SampleCont",
                    ResTripType = "Standard",
                    ResAssignCode = "RESASSIGN001",
                    Language = "English",
                    ReservationDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    PickupTimeZone = "UTC",
                    ClaimID = 101,
                    claimantid = 201,
                    claimantName = "Jane Smith",
                    customerid = 301,
                    customerName = "Customer A",
                    EstimatedMinutes = 60,
                    RSVACCode = "RSVAC001",
                    ActionCode = "AC001",
                    pickuplocid = 401,
                    PUZipCode = "12345",
                    CancelConfirm = "Confirmed",
                    CanceledBy = "Admin",
                    CanceledDate = DateTime.Now,
                    CanceledFlag = 0,
                    CanceledTime = DateTime.Now,
                    PUFacilityName = "PU Facility 1",
                    PUFacilityName2 = "PU Facility 2",
                    PUCity = "Pickup City",
                    DOFacilityName = "DO Facility 1",
                    DOFacilityName2 = "DO Facility 2",
                    DOAddress1 = "123 Main St",
                    DOAddress2 = "Apt 4B",
                    DOZipCode = "54321",
                    DOCity = "Delivery City",
                    PUAddress1 = "456 Elm St",
                    PUAddress2 = "Suite 300",
                    HmPhone = "123-456-7890",
                    height = "6 feet",
                    weight = 180,
                    birthdate = new DateTime(1990, 1, 1),
                    IsFeedbackComplete = 1,
                    RSVCXCode = "RSVCX001",
                    RSVCXdescription = "Description of RSVCX",
                    IsAssignmentTracking = 1,
                    JobStatus = 1,
                    IsContractorFound = 1,
                    TotalCount = 1,
                    AssignmentJobStatus = "Assigned",
                    PULatitude = 12.34567m,
                    PULongitude = 76.54321m,
                    DOLatitude = 23.45678m,
                    DOLongitude = 87.65432m,
                    ContractorMetricsFlag = 1,
                    CustomerFL = "FL001",
                    CreateDate = DateTime.Now,
                    ResAsgnCode = "RESASGN001",
                    PickupTimeHO = "14:00"
                }
            };

            _mockReservationAssignmentsService
                .Setup(s => s.ReservationAssignmentSearch(inputModel))
                .ReturnsAsync(searchResults);

            // Act
            var result = await _controller.ReservationAssignmentsearch(inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);

            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);
            Assert.Equal(1, response.totalData); // Check the total data count
        }

        [Fact]
        public async Task ReservationAssignmentsearch_ReturnsNotFound_WhenNoDataExists()
        {
            // Arrange
            var inputModel = new ReservationAssignmentWebViewModel(); // Add necessary properties for the test

            _mockReservationAssignmentsService
                .Setup(s => s.ReservationAssignmentSearch(inputModel))
                .ReturnsAsync(new List<vwReservationAssignmentsSearch>()); // Empty list

            // Act
            var result = await _controller.ReservationAssignmentsearch(inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);

            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task TrakingLive_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 1,
                isDeadMile = 0
            };

            var liveCoordinates = new List<Coordinate>
        {
            new Coordinate { Latitude = 12.9715987, Longitude = 77.594566 }
        };

            var googlePath = new List<Coordinate>
        {
            new Coordinate { Latitude = 12.9715987, Longitude = 77.594566 },
            new Coordinate { Latitude = 13.0358, Longitude = 77.5970 }
        };

            _mockReservationAssignmentsService
                .Setup(s => s.GetLiveTrackingCoordinates(model))
                .ReturnsAsync(liveCoordinates);

            _mockReservationAssignmentsService
                .Setup(s => s.GetGooglePathCoordinates(model.ReservationsAssignmentsID))
                .ReturnsAsync(googlePath);

            // Act
            var result = await _controller.TrakingLive(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            var actualData = Assert.IsType<LiveTrackingMapResponseModel>(response.data);
            Assert.Equal(model.ReservationsAssignmentsID, actualData.ReservationsAssignmentsID);
            Assert.Equal(liveCoordinates.Count, actualData.LatitudeLongitude.Count);
            Assert.Equal(googlePath.Count, actualData.GooglePath.Count);

            // Verify individual coordinates if necessary
            for (int i = 0; i < liveCoordinates.Count; i++)
            {
                Assert.Equal(liveCoordinates[i].Latitude, actualData.LatitudeLongitude[i].Latitude);
                Assert.Equal(liveCoordinates[i].Longitude, actualData.LatitudeLongitude[i].Longitude);
            }

            for (int i = 0; i < googlePath.Count; i++)
            {
                Assert.Equal(googlePath[i].Latitude, actualData.GooglePath[i].Latitude);
                Assert.Equal(googlePath[i].Longitude, actualData.GooglePath[i].Longitude);
            }
        }

        [Fact]
        public async Task ViewUserFeedback_ReturnsOkResult_WhenFeedbackIsValid()
        {
            // Arrange
            var feedback = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 };
            // Arrange
            var expectedFeedback = new List<UserFeedbackAppResponseModel>
            {
                new UserFeedbackAppResponseModel
                {
                    ReservationAssignmentID = 1,
                    FromID = 101,
                    ToID = 202,
                    Notes = "Feedback notes",
                    Answer1 = 4.5m,
                    Answer2 = 3.0m,
                    Answer3 = null,
                    Answer4 = 5.0m,
                    Answer5 = 4.0m,
                    Answer6 = null,
                    CreatedDate = DateTime.UtcNow,
                    CreatedUserID = 301
                }
            };

            // Setup the mock to return the expected feedback
            _mockReservationAssignmentsService
                .Setup(service => service.ViewUserFeedback(It.IsAny<AssignmentIDAppViewModel>()))
                .ReturnsAsync(expectedFeedback); // Ensure this is an IEnumerable<UserFeedbackAppResponseModel>


            // Act
            var result = await _controller.ViewUserFeedback(feedback) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(expectedFeedback, response.data);
        }

        [Fact]
        public async Task ViewUserFeedback_ReturnsBadRequest_WhenFeedbackIsMissing()
        {
            // Arrange
            var feedback = new AssignmentIDWebViewModel { ReservationsAssignmentsID = null };

            // Act
            var result = await _controller.ViewUserFeedback(feedback) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(response.data, string.Empty); // Expecting no data
        }

        //[Fact]
        //public async Task AssignmentMetricsDetails_ReturnsSuccess_WhenDataExists()
        //{
        //    // Arrange
        //    var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 };
        //    var expectedMetrics = new List<AssignmentMetricsResponse>
        //    {
        //        new AssignmentMetricsResponse
        //        {
        //            code = "A001",
        //            description = "Sample Metric 1",
        //            Quantity = "10",
        //            metricunitofmeasure = "kg",
        //            expectedQty = "12",
        //            Notes = "Note 1"
        //        },
        //        new AssignmentMetricsResponse
        //        {
        //            code = "A002",
        //            description = "Sample Metric 2",
        //            Quantity = "20",
        //            metricunitofmeasure = "kg",
        //            expectedQty = "18",
        //            Notes = "Note 2"
        //        }
        //    };

        //    _mockAssignmentMetricsService
        //        .Setup(service => service.FetchAssignmentMetrics(It.IsAny<int>()))
        //        .ReturnsAsync(expectedMetrics);

        //    // Act
        //    var result = await _controller.AssignmentMetricsDetails(model) as OkObjectResult;

        //    // Assert
        //    Assert.Null(result);
        //    Assert.Equal(200, result.StatusCode);

        //    var response = result.Value as PaginatedResponseModel;
        //    Assert.Null(response);
        //    Assert.Equal(ResponseStatus.TRUE, response.status);
        //    Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        //    Assert.Equal(expectedMetrics, response.data);
        //}

        [Fact]
        public async Task AssignmentMetricsDetails_ReturnsDataNotFound_WhenNoDataExists()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 };

            _mockAssignmentMetricsService
                .Setup(service => service.FetchAssignmentMetrics(It.IsAny<int>()))
                .ReturnsAsync(new List<AssignmentMetricsResponse>());

            // Act
            var result = await _controller.AssignmentMetricsDetails(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        //[Fact]
        //public async Task AssignmentMetricsDetails_ReturnsDataNotFound_WhenIDIsNull()
        //{
        //    // Arrange
        //    var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = null }; // Simulate null ID

        //    // Act
        //    var result = await _controller.AssignmentMetricsDetails(model) as OkObjectResult;

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(200, result.StatusCode);

        //    var response = result.Value as ResponseModel;
        //    Assert.NotNull(response);
        //    Assert.Equal(ResponseStatus.FALSE, response.status);
        //    Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        //}

        [Fact]
        public async Task ReservationAssignmentMetricSearch_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var model = new AssignmentMetricsSearchWebViewModel
            {
                ReservationID = 1,
                ReservationsAssignmentsID = 202,
                CustomerID = 123,
                ContractorID = 101,
                claimnumber = "Claim001",
                DateFrom = DateTime.UtcNow.AddDays(-7).ToString("yyyy/MM/dd"), // 7 days ago
                DateTo = DateTime.UtcNow.ToString("yyyy/MM/dd"), // Today
                IsMetricsEntered = 1,
                Page = 1,
                Limit = 20
            };

            var expectedResults = new List<vwReservationAssignmentsSearch>
            {
                 new vwReservationAssignmentsSearch
        {
            reservationid = 1,
            contractorid = 101,
            ReservationsAssignmentsID = 202,
            linenum = 1,
            BILCDECode = "Code1",
            claimnumber = "Claim001",
            WillCallFlag = 1,
            rsvattcode = "ATT001",
            quantity = 5,
            assgncode = "ASSIGN001",
            langucode = "ENG",
            inactiveflag = 0,
            MetricsEnteredFlag = 1,
            totaltime = 10,
            ReferenceNumber = "Ref001",
            notes = "Sample notes",
            driveremailflag = 1,
            drivercalledflag = 1,
            driverconfirmedflag = 1,
            driverreconfirmedflag = 1,
            drivernaflag = 0,
            leftmessageflag = 1,
            PickupTimeSort = DateTime.UtcNow,
            PickupTime = "10:00 AM",
            PickupTimewZone = "IST",
            AssgnNum = "AssignmentNum001",
            StartTime = "09:00 AM",
            EndTime = "11:00 AM",
            EarlyLatePickupTime = "Early",
            contractor = "ContractorName",
            contycode = "CountyCode",
            conctcode = "ContractorCode",
            contractorcontact = "Contact Name",
            ContractorCompany = "Contractor Company Name",
            contractorhomephone = "123-456-7890",
            contractorworkphone = "098-765-4321",
            contractorcellphone = "555-123-4567",
            ContractorFax = "555-765-4321",
            ContractorEmail = "contractor@example.com",
            concmcode = "CustomerCode",
            contractorintfid = "InternalID",
            contractorshortname = "ShortName",
            ResTripType = "Type1",
            ResAssignCode = "AssignCode1",
            Language = "English",
            ReservationDate = DateTime.UtcNow.ToString("dd/MM/yyyy"),
            PickupTimeZone = "IST",
            ClaimID = 1,
            claimantid = 1,
            claimantName = "Claimant Name",
            customerid = 1,
            customerName = "Customer Name",
            EstimatedMinutes = 30,
            RSVACCode = "RSVC001",
            ActionCode = "Action001",
            pickuplocid = 1,
            PUZipCode = "12345",
            CancelConfirm = "CancelConfirm001",
            CanceledBy = "User1",
            CanceledDate = DateTime.UtcNow,
            CanceledFlag = 1,
            CanceledTime = DateTime.UtcNow,
            PUFacilityName = "Pickup Facility",
            PUFacilityName2 = "Pickup Facility 2",
            PUCity = "Pickup City",
            DOFacilityName = "Drop Off Facility",
            DOFacilityName2 = "Drop Off Facility 2",
            DOAddress1 = "123 Drop Off St",
            DOAddress2 = "Suite 100",
            DOZipCode = "54321",
            DOCity = "Drop Off City",
            PUAddress1 = "456 Pickup St",
            PUAddress2 = "Apt 101",
            HmPhone = "555-678-1234",
            height = "180",
            weight = 70,
            birthdate = DateTime.UtcNow.AddYears(-25),
            IsFeedbackComplete = 1,
            RSVCXCode = "RSVCX001",
            RSVCXdescription = "Description",
            IsAssignmentTracking = 1,
            JobStatus = 1,
            IsContractorFound = 1,
            TotalCount = 1,
            AssignmentJobStatus = "Active",
            PULatitude = 12.345678m,
            PULongitude = 98.765432m,
            DOLatitude = 12.345678m,
            DOLongitude = 98.765432m,
            ContractorMetricsFlag = 1,
            CustomerFL = "FL001",
            CreateDate = DateTime.UtcNow,
            ResAsgnCode = "ResAsgn001",
            PickupTimeHO = "11:00 AM"
        }
            };

            _mockAssignmentMetricsService
                .Setup(service => service.MetricsSearch(It.IsAny<AssignmentMetricsSearchWebViewModel>()))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.ReservationAssignmentMetricSearch(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as PaginatedResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(expectedResults, response.data);
            Assert.Equal(expectedResults.Select(s => s.TotalCount).First(), response.totalData);
        }

        [Fact]
        public async Task ReservationAssignmentMetricSearch_ReturnsDataNotFound_WhenNoDataExists()
        {
            // Arrange
            var model = new AssignmentMetricsSearchWebViewModel
            {
                // Populate the model as needed for the test
            };

            _mockAssignmentMetricsService
                .Setup(service => service.MetricsSearch(It.IsAny<AssignmentMetricsSearchWebViewModel>()))
                .ReturnsAsync(new List<vwReservationAssignmentsSearch>());

            // Act
            var result = await _controller.ReservationAssignmentMetricSearch(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as PaginatedResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ReservationAssignmentMetricSearch_ReturnsDataNotFound_WhenModelIsNull()
        {
            // Arrange
            AssignmentMetricsSearchWebViewModel model = new AssignmentMetricsSearchWebViewModel();

            // Act
            var result = await _controller.ReservationAssignmentMetricSearch(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as PaginatedResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task TrackAssignmentByID_ReturnsSuccess_WhenDataIsRetrieved()
        {
            // Arrange
            var mockData = new List<TrackAssignmentByIDResponseModel>
            {
                new TrackAssignmentByIDResponseModel
                {
                    ReservationsAssignmentsID = 1,
                    ImageUrl = "image1.jpg",
                    DeadMileImageUrl = null
                }
            };

            _mockReservationAssignmentsService
                .Setup(x => x.WebTrackAssignmentByID(It.IsAny<int>()))
                .ReturnsAsync(mockData);

            _mockReservationAssignmentsService
                .Setup(x => x.RetrieveAssignmentAndContractorDetails(It.IsAny<List<TrackAssignmentByIDResponseModel>>()))
                .ReturnsAsync(mockData); // Mock the processed result

            // Act
            var result = await _controller.TrackAssignmentByID(new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Contains("https://local-api-jnjservices.betademo.net/image1.jpg", mockData[0].ImageUrl);
        }

        [Fact]
        public async Task TrackAssignmentByID_ReturnsFailure_WhenNoDataFound()
        {
            // Arrange
            var emptyData = new List<TrackAssignmentByIDResponseModel>();

            _mockReservationAssignmentsService
                .Setup(x => x.WebTrackAssignmentByID(It.IsAny<int>()))
                .ReturnsAsync(emptyData);

            // Act
            var result = await _controller.TrackAssignmentByID(new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task TrackAssignmentByID_ReturnsFailure_WhenReservationsAssignmentsIDIsNull()
        {
            // Arrange
            var emptyData = new List<TrackAssignmentByIDResponseModel>();

            _mockReservationAssignmentsService
                .Setup(x => x.WebTrackAssignmentByID(It.IsAny<int>()))
                .ReturnsAsync(emptyData);

            // Act
            var result = await _controller.TrackAssignmentByID(new AssignmentIDWebViewModel { ReservationsAssignmentsID = null });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task TrackAssignmentByID_ReturnsCorrectUrls_WhenImageUrlsArePresent()
        {
            // Arrange
            var mockData = new List<TrackAssignmentByIDResponseModel>
    {
        new TrackAssignmentByIDResponseModel
        {
            ReservationsAssignmentsID = 1,
            ImageUrl = "image1.jpg",
            DeadMileImageUrl = "deadmile1.jpg"
        }
    };

            _mockReservationAssignmentsService
                .Setup(x => x.WebTrackAssignmentByID(It.IsAny<int>()))
                .ReturnsAsync(mockData);

            _mockReservationAssignmentsService
                .Setup(x => x.RetrieveAssignmentAndContractorDetails(It.IsAny<List<TrackAssignmentByIDResponseModel>>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _controller.TrackAssignmentByID(new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Contains("https://local-api-jnjservices.betademo.net/image1.jpg", mockData[0].ImageUrl);
            Assert.Contains("https://local-api-jnjservices.betademo.net/deadmile1.jpg", mockData[0].DeadMileImageUrl);
        }

        [Fact]
        public async Task TrackAssignmentByID_HandlesEmptyImageUrls()
        {
            // Arrange
            var mockData = new List<TrackAssignmentByIDResponseModel>
            {
                new TrackAssignmentByIDResponseModel
                {
                    ReservationsAssignmentsID = 1,
                    ImageUrl = null,
                    DeadMileImageUrl = null
                }
            };

            _mockReservationAssignmentsService
                .Setup(x => x.WebTrackAssignmentByID(It.IsAny<int>()))
                .ReturnsAsync(mockData);

            _mockReservationAssignmentsService
                .Setup(x => x.RetrieveAssignmentAndContractorDetails(It.IsAny<List<TrackAssignmentByIDResponseModel>>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _controller.TrackAssignmentByID(new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Null(mockData[0].ImageUrl);
            Assert.Null(mockData[0].DeadMileImageUrl);
        }

        [Fact]
        public async Task ResAssignContractor_ReturnsBadRequest_WhenModelIsNull()
        {
            // Arrange
            AssignAssignmentContractorWebViewModel model = new AssignAssignmentContractorWebViewModel();

            // Act
            var result = await _controller.ResAssignContractor(model);

            // Assert
            var badRequestResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            //Assert.Null(response.data);
        }

        [Fact]
        public async Task ResAssignContractor_ShouldReturnSuccess_WhenAssignTypeIsForced()
        {
            // Arrange
            var model = new AssignAssignmentContractorWebViewModel
            {
                reservationassignmentid = 1,
                AssignType = ButtonStatus.FORCED,
                contractorid = 123 // Add contractor ID for testing purposes
            };

            // Simulate a non-empty result
            var mockResult = new List<ReservationAssigncontractorWebResponseModel>
            {
                new ReservationAssigncontractorWebResponseModel { result = "Success" }
            };

            _mockReservationAssignmentsService
                .Setup(service => service.AssignAssignmentContractor(model))
                .ReturnsAsync(mockResult); // Ensure the mock method returns a Task

            // Act
            var result = await _controller.ResAssignContractor(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            //// Verify the notification helper is called with the correct parameters
            //_mockNotificationHelper.Verify(
            //    ns => ns.SendNotificationWebForceRequest(It.Is<ReservationNotificationWebViewModel>(
            //        n => n.ButtonStatus == ButtonStatus.FORCED &&
            //             n.NotificationTitle == NotificationTitle.NEW_ASSIGNMENT_ASSIGNED &&
            //             n.ContractorID == 123 // Ensure contractor ID is passed correctly
            //    )),
            //    Times.Once
            //);
        }

        [Fact]
        public async Task ResAssignContractor_ShouldReturnSuccess_WhenAssignTypeIsRequest()
        {
            // Arrange
            var model = new AssignAssignmentContractorWebViewModel
            {
                reservationassignmentid = 1,
                AssignType = ButtonStatus.REQUEST,
                contractorid = 123 // Add contractor ID for testing purposes
            };

            // Simulate a non-empty result
            var mockResult = new List<ReservationAssigncontractorWebResponseModel>
            {
                new ReservationAssigncontractorWebResponseModel { result = "Success" }
            };

            _mockReservationAssignmentsService
                .Setup(service => service.AssignAssignmentContractor(model))
                .ReturnsAsync(mockResult); // Ensure the mock method returns a Task

            // Act
            var result = await _controller.ResAssignContractor(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            //// Verify the notification helper is called with the correct parameters
            //_mockNotificationHelper.Verify(
            //    ns => ns.SendNotificationWebForceRequest(It.Is<ReservationNotificationWebViewModel>(
            //        n => n.ButtonStatus == ButtonStatus.FORCED &&
            //             n.NotificationTitle == NotificationTitle.NEW_ASSIGNMENT_ASSIGNED &&
            //             n.ContractorID == 123 // Ensure contractor ID is passed correctly
            //    )),
            //    Times.Once
            //);
        }



    }
}
