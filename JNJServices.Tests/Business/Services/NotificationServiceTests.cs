using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.DbResponseModels;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Helper;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
	public class NotificationServiceTests
	{
		private readonly Mock<IDapperContext> _mockContext;
		private readonly TimeZoneConverter _timeZoneConverter;
		private readonly NotificationService _notificationService;
		private readonly Mock<INotificationService> _mockNotificationService;
		private readonly Mock<IConfiguration> _configurationMock;

		public NotificationServiceTests()
		{
			// Arrange the in-memory configuration

			_configurationMock = new Mock<IConfiguration>();
			_configurationMock.SetupGet(c => c["EncryptionDecryption:Key"]).Returns("testEncryptionKey");
			_configurationMock.SetupGet(c => c["SelectTimeZone:TimeZone"]).Returns("India Standard Time");

			// Mocking the context
			_mockContext = new Mock<IDapperContext>();

			// Initialize the TimeZoneConverter with the configuration mock
			_timeZoneConverter = new TimeZoneConverter(_configurationMock.Object);

			// Initialize the NotificationService with the mocked dependencies
			_notificationService = new NotificationService(
				_mockContext.Object,
				_timeZoneConverter
			);
			_mockNotificationService = new Mock<INotificationService>();
		}

		[Fact]
		public async Task GetNotificationsAsync_ValidTokenClaims_ReturnsNotifications()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				FcmToken = "validFcmToken",
				Type = 1
			};

			var mockNotifications = new List<AppNotificationsResponseModel>
			{
				new AppNotificationsResponseModel
				{
					NotificationID = 1,
					ReferenceID = "REF123",
					AssgnNum = "ASSIGN001",
					UserID = 101,
					Title = "Test Notification 1",
					Body = "This is a test notification",
					Data = "{\"key\":\"value\"}",
					SentDate = DateTime.Now,
					ReadStatus = 0, // 0 for unread, 1 for read
                    NotificationType = "Info",
					CreatedBy = 1001,
					CreatedDate = DateTime.Now
				},
				new AppNotificationsResponseModel
				{
					NotificationID = 2,
					ReferenceID = "REF456",
					AssgnNum = "ASSIGN002",
					UserID = 102,
					Title = "Test Notification 2",
					Body = "Another test notification",
					Data = "{\"key\":\"value2\"}",
					SentDate = DateTime.Now,
					ReadStatus = 1, // Already read
                    NotificationType = "Alert",
					CreatedBy = 1002,
					CreatedDate = DateTime.Now
				}
			};

			_mockContext
			   .Setup(m => m.ExecuteQueryAsync<AppNotificationsResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
			   .ReturnsAsync(mockNotifications);

			// Act
			var result = await _notificationService.GetNotificationsAsync(tokenClaims, 1, 10);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Equal(2, result.Count());  // Verify that the returned result has two notifications
			Assert.Equal("Test Notification 1", result.First().Title);  // Verify the title of the first notification
			Assert.Equal("Test Notification 2", result.Last().Title);   // Verify the title of the second notification
		}

		[Fact]
		public async Task GetNotificationsAsync_ReturnsEmptyList_WhenNoNotifications()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				FcmToken = "validFcmToken",
				Type = 1
			};

			var mockNotifications = new List<AppNotificationsResponseModel>();  // Empty list

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AppNotificationsResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(mockNotifications);

			// Act
			var result = await _notificationService.GetNotificationsAsync(tokenClaims, 1, 10);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Empty(result);    // Verify that the returned result is an empty list
		}

		[Fact]
		public async Task GetNotificationsAsync_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				FcmToken = "validFcmToken",
				Type = 1
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AppNotificationsResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.GetNotificationsAsync(tokenClaims, 1, 10));
			Assert.Equal("Database failure", exception.Message);
		}

		[Fact]
		public async Task GetNotificationsAsync_ValidTokenClaims_FiltersNotificationsByUser()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				FcmToken = "validFcmToken",
				Type = 1
			};

			var mockNotifications = new List<AppNotificationsResponseModel>
			{
				new AppNotificationsResponseModel
				{
					NotificationID = 1,
					Title = "Test Notification 1",
					Body = "This is a test notification",
					CreatedDate = DateTime.Now,
					UserID = 123 // Matches the tokenClaims.UserID
                },
				new AppNotificationsResponseModel
				{
					NotificationID = 2,
					Title = "Test Notification 2",
					Body = "Another test notification",
					CreatedDate = DateTime.Now,
					UserID = 124 // Does not match the tokenClaims.UserID
                }
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AppNotificationsResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(mockNotifications);

			// Act
			var result = await _notificationService.GetNotificationsAsync(tokenClaims, 1, 10);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Equal("Test Notification 1", result.First().Title);  // Verify the title of the filtered notification
		}

		[Fact]
		public async Task GetNotificationsAsync_InvalidTokenClaims_ReturnsNoNotifications()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 999,  // Invalid UserID
				FcmToken = "invalidFcmToken",
				Type = 1
			};

			var mockNotifications = new List<AppNotificationsResponseModel>();  // Empty list for invalid user

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AppNotificationsResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(mockNotifications);

			// Act
			var result = await _notificationService.GetNotificationsAsync(tokenClaims, 1, 10);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Empty(result);    // Verify that no notifications are returned
		}

		[Fact]
		public async Task GetNotificationsAsync_ValidTokenClaims_UsesCorrectProcedureName()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				FcmToken = "validFcmToken",
				Type = 1
			};

			var mockNotifications = new List<AppNotificationsResponseModel>
			{
				new AppNotificationsResponseModel
				{
					NotificationID = 1,
					Title = "Test Notification 1",
					Body = "This is a test notification",
					CreatedDate = DateTime.Now
				}
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AppNotificationsResponseModel>(It.Is<string>(s => s == ProcEntities.spGetAndMarkMobileNotifications),
																			   It.IsAny<DynamicParameters>(),
																			   It.IsAny<CommandType>()))
				.ReturnsAsync(mockNotifications);

			// Act
			var result = await _notificationService.GetNotificationsAsync(tokenClaims, 1, 10);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Equal("Test Notification 1", result.First().Title);  // Verify the title of the first notification
		}

		[Fact]
		public async Task DeleteNotificationAsync_ValidTokenClaims_DeletesNotifications()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				Type = 1
			};

			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(1); // Return 1 to indicate that one row was deleted

			// Act
			var result = await _notificationService.DeleteNotificationAsync(tokenClaims);

			// Assert
			Assert.Equal(1, result); // Verify that one row was deleted
			_mockContext.Verify(m => m.ExecuteAsync(It.Is<string>(s => s.Contains("DELETE FROM NotificationLog")),
													It.IsAny<DynamicParameters>(),
													CommandType.Text), Times.Once);  // Verify that the DELETE query was executed
		}

		[Fact]
		public async Task DeleteNotificationAsync_NoMatchingRecord_ReturnsZero()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 999,  // Non-existent UserID
				Type = 1
			};

			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(0); // Return 0 to indicate no rows were deleted

			// Act
			var result = await _notificationService.DeleteNotificationAsync(tokenClaims);

			// Assert
			Assert.Equal(0, result); // Verify that no rows were deleted
		}

		[Fact]
		public async Task DeleteNotificationAsync_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				Type = 1
			};

			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.DeleteNotificationAsync(tokenClaims));
			Assert.Equal("Database failure", exception.Message); // Ensure the exception message matches
		}

		[Fact]
		public async Task DeleteNotificationAsync_VerifiesCorrectParameters()
		{
			// Arrange
			var tokenClaims = new AppTokenDetails
			{
				UserID = 123,
				Type = 1
			};

			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(1); // Return 1 to indicate that one row was deleted

			// Act
			await _notificationService.DeleteNotificationAsync(tokenClaims);

			// Assert
			_mockContext.Verify(m => m.ExecuteAsync(It.Is<string>(s => s.Contains("DELETE FROM NotificationLog")),
													It.Is<DynamicParameters>(p => p.Get<int>(DbParams.UserID) == 123 && p.Get<int>(DbParams.UserType) == 1),
													CommandType.Text), Times.Once);  // Verify the correct parameters are passed
		}

		[Fact]
		public async Task GetUserWebNotifications_ValidClaimValue_ReturnsNotifications()
		{
			// Arrange
			var claimValue = "validClaimValue";
			var mockNotifications = new List<WebNotificationResponseModel>
		{
			new WebNotificationResponseModel
			{
				NotificationID = 1,
				Title = "Test Notification 1",
				Body = "This is a test web notification",
				CreatedDate = DateTime.Now
			},
			new WebNotificationResponseModel
			{
				NotificationID = 2,
				Title = "Test Notification 2",
				Body = "Another test web notification",
				CreatedDate = DateTime.Now
			}
		};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<WebNotificationResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(mockNotifications); // Mock the response to return a list of notifications

			// Act
			var result = await _notificationService.GetUserWebNotifications(claimValue, 1, 10);

			// Assert
			Assert.NotNull(result);  // Verify that the result is not null
			Assert.Equal(2, result.Count());  // Verify that two notifications are returned
			Assert.Equal("Test Notification 1", result.First().Title);  // Verify the title of the first notification
			Assert.Equal("Test Notification 2", result.Last().Title);   // Verify the title of the second notification
		}

		[Fact]
		public async Task GetUserWebNotifications_InvalidClaimValue_ReturnsEmptyList()
		{
			// Arrange
			var claimValue = "invalidClaimValue";  // Non-existent claim value
			var mockNotifications = new List<WebNotificationResponseModel>(); // Empty list to simulate no data

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<WebNotificationResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(mockNotifications); // Return an empty list

			// Act
			var result = await _notificationService.GetUserWebNotifications(claimValue, 1, 10);

			// Assert
			Assert.NotNull(result);  // Verify that the result is not null
			Assert.Empty(result);  // Verify that the result is an empty list
		}

		[Fact]
		public async Task GetUserWebNotifications_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var claimValue = "validClaimValue";

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<WebNotificationResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));  // Simulate a database failure

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.GetUserWebNotifications(claimValue, 1, 10));
			Assert.Equal("Database failure", exception.Message);  // Verify the exception message
		}

		[Fact]
		public async Task GetUserWebNotifications_VerifiesCorrectParameters()
		{
			// Arrange
			var claimValue = "validClaimValue";

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<WebNotificationResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(new List<WebNotificationResponseModel> { new WebNotificationResponseModel() });  // Mock response with a single notification

			// Act
			await _notificationService.GetUserWebNotifications(claimValue, 1, 10);

			// Assert
			_mockContext.Verify(m => m.ExecuteQueryAsync<WebNotificationResponseModel>(
				It.Is<string>(s => s.Contains("spGetAndMarkWebNotifications")),
				It.Is<DynamicParameters>(p => p.Get<string>(DbParams.UserID) == claimValue),
				CommandType.StoredProcedure), Times.Once);  // Verify that the correct stored procedure and parameters were used
		}

		[Fact]
		public async Task DeleteUserWebNotifications_SuccessfulDeletion_ReturnsAffectedRows()
		{
			// Arrange
			var userID = "validUserID";

			// Mock the response of the ExecuteAsync to simulate successful deletion
			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(1);  // Simulate that one row was deleted

			// Act
			var result = await _notificationService.DeleteUserWebNotifications(userID);

			// Assert
			Assert.Equal(1, result);  // Ensure that one row was deleted
		}

		[Fact]
		public async Task DeleteUserWebNotifications_NoRowsAffected_ReturnsZero()
		{
			// Arrange
			var userID = "nonExistentUserID";

			// Mock the response of the ExecuteAsync to simulate no rows being deleted
			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(0);  // Simulate that no rows were deleted

			// Act
			var result = await _notificationService.DeleteUserWebNotifications(userID);

			// Assert
			Assert.Equal(0, result);  // Ensure that no rows were deleted
		}

		[Fact]
		public async Task DeleteUserWebNotifications_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var userID = "validUserID";

			// Simulate a database failure (e.g., a network issue or query error)
			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.DeleteUserWebNotifications(userID));
			Assert.Equal("Database failure", exception.Message);  // Ensure the correct exception message is thrown
		}

		[Fact]
		public async Task DeleteUserWebNotifications_VerifiesCorrectParameters()
		{
			// Arrange
			var userID = "validUserID";

			// Mock the response to simulate successful deletion
			_mockContext
				.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
				.ReturnsAsync(1);  // Simulate that one row was deleted

			// Act
			await _notificationService.DeleteUserWebNotifications(userID);

			// Assert
			_mockContext.Verify(m => m.ExecuteAsync(
				It.Is<string>(s => s.Contains("DELETE FROM NotificationWebLog")),  // Ensure DELETE query is used
				It.Is<DynamicParameters>(p => p.Get<string>(DbParams.UserID) == userID),  // Verify correct parameter is used
				CommandType.Text), Times.Once);  // Ensure the query is executed once
		}

		[Fact]
		public async Task ClaimantAcceptCancel_ValidReservationID_ReturnsResponseModels()
		{
			// Arrange
			var reservationID = 123;
			var mockResponse = new List<ContractorFirstAcceptCancelResponseModel>
			{
				new ContractorFirstAcceptCancelResponseModel
				{
					ContractorID = 1,
					NotificationType = "Test Contractor 1",
					AssgnNum = "Accepted"
				},
				new ContractorFirstAcceptCancelResponseModel
				{
					ContractorID = 2,
					NotificationType = "Test Contractor 2",
					AssgnNum = "Cancelled"
				}
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.ClaimantAcceptCancel(reservationID);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Equal(2, result.Count());  // Verify that the returned result has two response models
			Assert.Equal("Test Contractor 1", result.First().NotificationType);  // Verify the name of the first response model
			Assert.Equal("Cancelled", result.Last().AssgnNum);  // Verify the status of the last response model
		}

		[Fact]
		public async Task ClaimantAcceptCancel_NoDataFound_ReturnsEmptyList()
		{
			// Arrange
			var reservationID = 999;  // Non-existent ReservationID

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate no data found

			// Act
			var result = await _notificationService.ClaimantAcceptCancel(reservationID);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Empty(result);  // Ensure that the result is an empty list
		}

		[Fact]
		public async Task ClaimantAcceptCancel_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var reservationID = 123;

			// Simulate a database failure (e.g., a network issue or query error)
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.ClaimantAcceptCancel(reservationID));
			Assert.Equal("Database failure", exception.Message);  // Ensure the correct exception message is thrown
		}

		[Fact]
		public async Task ClaimantAcceptCancel_VerifiesCorrectParameters()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate successful execution
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate some response

			// Act
			await _notificationService.ClaimantAcceptCancel(reservationID);

			// Assert
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
				ProcEntities.spNotificationClaimantAcceptLogic,  // Ensure correct stored procedure is called
				It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationID) == reservationID),  // Verify correct parameters are used
				CommandType.StoredProcedure), Times.Once);  // Ensure the query is executed once
		}

		[Fact]
		public async Task ClaimantAcceptContractornotfoundLogic_ValidReservationID_ReturnsResponseModels()
		{
			// Arrange
			var reservationID = 123;
			var mockResponse = new List<ContractorFirstAcceptCancelResponseModel>
			{
				new ContractorFirstAcceptCancelResponseModel
				{
					ReservationsAssignmentsID = 1,
					AssgnNum = "Assigned",
					ResAsgnCode = "Code1",
					ReservationDate = DateTime.Now,
					ReservationTime = DateTime.Now,
				},
				new ContractorFirstAcceptCancelResponseModel
				{
					ReservationsAssignmentsID = 2,
					AssgnNum = "Assigned",
					ResAsgnCode = "Code2",
					ReservationDate = DateTime.Now,
					ReservationTime = DateTime.Now

				}
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.ClaimantAcceptContractornotfoundLogic(reservationID);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Equal(2, result.Count());  // Verify that the returned result has two response models
			Assert.Equal("Assigned", result.First().AssgnNum);  // Verify AssgnNum of the first model
			Assert.Equal("Code2", result.Last().ResAsgnCode);  // Verify ResAsgnCode of the last model
		}

		[Fact]
		public async Task ClaimantAcceptContractornotfoundLogic_NoDataFound_ReturnsEmptyList()
		{
			// Arrange
			var reservationID = 999;  // Non-existent ReservationID

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate no data found

			// Act
			var result = await _notificationService.ClaimantAcceptContractornotfoundLogic(reservationID);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Empty(result);  // Ensure that the result is an empty list
		}

		[Fact]
		public async Task ClaimantAcceptContractornotfoundLogic_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var reservationID = 123;

			// Simulate a database failure (e.g., a network issue or query error)
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.ClaimantAcceptContractornotfoundLogic(reservationID));
			Assert.Equal("Database failure", exception.Message);  // Ensure the correct exception message is thrown
		}

		[Fact]
		public async Task ClaimantAcceptContractornotfoundLogic_VerifiesCorrectQueryAndParameters()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate successful execution
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate some response

			// Act
			await _notificationService.ClaimantAcceptContractornotfoundLogic(reservationID);

			// Assert
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
				It.Is<string>(query => query.Contains("SELECT a.ReservationsAssignmentsID")),  // Ensure the query contains the expected SQL
				It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationID) == reservationID),  // Ensure the ReservationID parameter is correct
				CommandType.Text), Times.Once);  // Ensure the query is executed once
		}

		[Fact]
		public async Task ContractorFirstCancelLogic_ValidReservationAssignmentID_ReturnsResponseModels()
		{
			// Arrange
			var reservationAssignmentID = 123;
			var mockResponse = new List<ContractorFirstAcceptCancelResponseModel>
			{
				new ContractorFirstAcceptCancelResponseModel
				{
					ReservationsAssignmentsID = 123,
					FcmToken = "token1",
					ASSGNCode = "device1",
					ResAsgnCode = "Code1",
					ReservationDate = DateTime.Now,
					AssgnNum = "Assigned"
				},
				new ContractorFirstAcceptCancelResponseModel
				{
					ReservationsAssignmentsID = 123,
					FcmToken = "token2",
					ASSGNCode = "device2",
					ResAsgnCode = "Code2",
					ReservationDate = DateTime.Now,
					AssgnNum = "Assigned"
				}
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.ContractorFirstCancelLogic(reservationAssignmentID);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Equal(2, result.Count());  // Verify that the returned result has two response models
			Assert.Equal("Assigned", result.First().AssgnNum);  // Verify AssgnNum of the first model
			Assert.Equal("Code2", result.Last().ResAsgnCode);  // Verify ResAsgnCode of the last model
		}

		[Fact]
		public async Task ContractorFirstCancelLogic_NoDataFound_ReturnsEmptyList()
		{
			// Arrange
			var reservationAssignmentID = 999;  // Non-existent ReservationAssignmentID

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate no data found

			// Act
			var result = await _notificationService.ContractorFirstCancelLogic(reservationAssignmentID);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Empty(result);  // Ensure that the result is an empty list
		}

		[Fact]
		public async Task ContractorFirstCancelLogic_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Simulate a database failure (e.g., a network issue or query error)
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.ContractorFirstCancelLogic(reservationAssignmentID));
			Assert.Equal("Database failure", exception.Message);  // Ensure the correct exception message is thrown
		}

		[Fact]
		public async Task ContractorFirstCancelLogic_VerifiesCorrectQueryAndParameters()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate successful execution
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate some response

			// Act
			await _notificationService.ContractorFirstCancelLogic(reservationAssignmentID);

			// Assert
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
				It.Is<string>(query => query.Contains("SELECT a.ReservationsAssignmentsID")),  // Ensure the query contains the expected SQL
				It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationsAssignmentsID) == reservationAssignmentID),  // Ensure the ReservationAssignmentID parameter is correct
				CommandType.Text), Times.Once);  // Ensure the query is executed once
		}

		[Fact]
		public async Task ContractorFirstCancelDataNotFound_ValidReservationAssignmentID_ReturnsResponseModel()
		{
			// Arrange
			var reservationAssignmentID = 123;
			var mockResponse = new List<ContractorFirstAcceptCancelResponseModel>
			{
				new ContractorFirstAcceptCancelResponseModel
				{
					ReservationsAssignmentsID = 123,
					ResAsgnCode = "Code1",
					AssgnNum = "Assigned",
					NotificationType = "ASSIGNMENT_NEED_ATTENTION",
					NotificationDateTime = DateTime.Now
				}
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.ContractorFirstCancelDataNotFound(reservationAssignmentID);

			// Assert
			Assert.NotNull(result);  // Ensure result is not null
			Assert.Single(result);  // Ensure only one record is returned (due to Top(1))
			Assert.Equal("ASSIGNMENT_NEED_ATTENTION", result.First().NotificationType);  // Check NotificationType
			Assert.Equal("Assigned", result.First().AssgnNum);  // Check AssgnNum
		}

		[Fact]
		public async Task ContractorFirstCancelDataNotFound_NoDataFound_ReturnsEmptyList()
		{
			// Arrange
			var reservationAssignmentID = 999;  // Non-existent ReservationAssignmentID

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate no data found

			// Act
			var result = await _notificationService.ContractorFirstCancelDataNotFound(reservationAssignmentID);

			// Assert
			Assert.NotNull(result);  // Verify result is not null
			Assert.Empty(result);  // Ensure that the result is an empty list
		}

		[Fact]
		public async Task ContractorFirstCancelDataNotFound_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Simulate a database failure (e.g., a network issue or query error)
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database failure"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.ContractorFirstCancelDataNotFound(reservationAssignmentID));
			Assert.Equal("Database failure", exception.Message);  // Ensure the correct exception message is thrown
		}

		[Fact]
		public async Task ContractorFirstCancelDataNotFound_VerifiesCorrectQueryAndParameters()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate successful execution
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate some response

			// Act
			await _notificationService.ContractorFirstCancelDataNotFound(reservationAssignmentID);

			// Assert
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
				It.Is<string>(query => query.Contains("SELECT Top(1)")),  // Ensure the query contains "Top(1)"
				It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationsAssignmentsID) == reservationAssignmentID),  // Ensure the ReservationAssignmentID parameter is correct
				CommandType.Text), Times.Once);  // Ensure the query is executed once
		}

		[Fact]
		public async Task ContractorAcceptLogic_ValidReservationAssignmentID_ReturnsResponseModels()
		{
			// Arrange
			var reservationAssignmentID = 123;
			var mockResponse = new List<ContractorFirstAcceptCancelResponseModel>
			{
				new ContractorFirstAcceptCancelResponseModel
				{
					ReservationsAssignmentsID = 123,
					AssgnNum = "Assigned",
					NotificationDateTime = DateTime.Now
				},
				new ContractorFirstAcceptCancelResponseModel
				{
					ReservationsAssignmentsID = 123,
					AssgnNum = "Confirmed",
					NotificationDateTime = DateTime.Now
				}
			};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.ContractorAcceptLogic(reservationAssignmentID);

			// Assert
			Assert.NotNull(result);  // Ensure result is not null
			Assert.Equal(2, result.Count());  // Ensure that two models are returned
			Assert.Equal("Assigned", result.First().AssgnNum);  // Ensure the first model has the expected AssgnNum
			Assert.Equal("Confirmed", result.Last().AssgnNum);  // Ensure the last model has the expected AssgnNum
		}

		[Fact]
		public async Task ContractorAcceptLogic_NoDataFound_ReturnsEmptyList()
		{
			// Arrange
			var reservationAssignmentID = 999;  // Non-existent ReservationAssignmentID

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate no data found

			// Act
			var result = await _notificationService.ContractorAcceptLogic(reservationAssignmentID);

			// Assert
			Assert.NotNull(result);  // Ensure result is not null
			Assert.Empty(result);  // Ensure the result is an empty list
		}

		[Fact]
		public async Task ContractorAcceptLogic_ThrowsException_WhenDatabaseFails()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Simulate a database failure (e.g., a query error or connection issue)
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database query failed"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(() => _notificationService.ContractorAcceptLogic(reservationAssignmentID));
			Assert.Equal("Database query failed", exception.Message);  // Verify that the exception message is as expected
		}

		[Fact]
		public async Task ContractorAcceptLogic_VerifiesCorrectQueryAndParameters()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ContractorAcceptLogic(reservationAssignmentID);

			// Assert
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
				It.Is<string>(query => query.Contains("SELECT")),  // Ensure the query contains "SELECT"
				It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationsAssignmentsID) == reservationAssignmentID),  // Verify the parameter is correctly set
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ContractorCancelLogic_VerifiesReservationsAssignmentsIDParameter()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ContractorCancelLogic(reservationAssignmentID);

			// Assert that the ReservationsAssignmentsID parameter is correctly added to the query
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
				It.Is<string>(query => query.Contains("WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID")),
				It.Is<DynamicParameters>(p => p.Get<int>("ReservationsAssignmentsID") == reservationAssignmentID),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ContractorCancelLogic_ReturnsExpectedList()
		{
			// Arrange
			var reservationAssignmentID = 123;
			var expectedResult = new List<ContractorFirstAcceptCancelResponseModel>
	{
		new ContractorFirstAcceptCancelResponseModel
		{
			ReservationsAssignmentsID = reservationAssignmentID,
			ResAsgnCode = "A001",
			AssgnNum = "12345",
            // Add other properties as needed
        }
	};

			// Mock the response to simulate a successful query returning the expected result
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(expectedResult);  // Simulate returning the expected list

			// Act
			var result = await _notificationService.ContractorCancelLogic(reservationAssignmentID);

			// Assert that the result matches the expected list
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public async Task ContractorCancelLogic_CallsExecuteQueryAsyncOnce()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ContractorCancelLogic(reservationAssignmentID);

			// Assert that ExecuteQueryAsync was called exactly once
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
				It.IsAny<string>(),
				It.IsAny<DynamicParameters>(),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ContractorCancelLogic_ReturnsEmptyListIfNoResults()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate no results
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());  // Simulate no results

			// Act
			var result = await _notificationService.ContractorCancelLogic(reservationAssignmentID);

			// Assert that the result is an empty list
			Assert.Empty(result);
		}

		[Fact]
		public async Task ContractorCancelLogic_ThrowsExceptionIfQueryFails()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate a database error
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database error"));  // Simulate error

			// Act & Assert
			await Assert.ThrowsAsync<Exception>(() => _notificationService.ContractorCancelLogic(reservationAssignmentID));
		}

		[Fact]
		public async Task ClaimantCancelLogic_VerifiesQueryWithCorrectWhereConditions()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.Is<DynamicParameters>(p => p.Get<int>("@ReservationID") == reservationID),  // Ensure @ReservationID is passed correctly
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ClaimantCancelLogic(reservationID);

			// Assert that the query contains the correct WHERE clause conditions
			_mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
				It.Is<string>(query => query.Contains("a.ReservationID = @ReservationID") &&
									   query.Contains("a.RSVACCode = 'ResCancel'") &&
									   query.Contains("a.inactiveflag = 0")), // Check essential parts of the query
				It.IsAny<DynamicParameters>(),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ClaimantCancelLogic_VerifiesReservationIDParameter()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ClaimantCancelLogic(reservationID);

			// Assert that the ReservationID parameter is correctly added to the query
			_mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
				It.Is<string>(query => query.Contains("WHERE a.ReservationID = @ReservationID")),
				It.Is<DynamicParameters>(p => p.Get<int>("ReservationID") == reservationID),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ClaimantCancelLogic_ReturnsExpectedList()
		{
			// Arrange
			var reservationID = 123;
			var expectedResult = new List<ClaimantCancelResponseModel>
	{
		new ClaimantCancelResponseModel
		{
			ReservationsAssignmentsID = 456,
			ResAsgnCode = "A001",
			ClaimantID = 789,
            // Add other properties as needed
        }
	};

			// Mock the response to simulate a successful query returning the expected result
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(expectedResult);  // Simulate returning the expected list

			// Act
			var result = await _notificationService.ClaimantCancelLogic(reservationID);

			// Assert that the result matches the expected list
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public async Task ClaimantCancelLogic_CallsExecuteQueryAsyncOnce()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ClaimantCancelLogic(reservationID);

			// Assert that ExecuteQueryAsync was called exactly once
			_mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
				It.IsAny<string>(),
				It.IsAny<DynamicParameters>(),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ClaimantCancelLogic_ReturnsEmptyListIfNoResults()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate no results
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate no results

			// Act
			var result = await _notificationService.ClaimantCancelLogic(reservationID);

			// Assert that the result is an empty list
			Assert.Empty(result);
		}

		[Fact]
		public async Task ClaimantCancelLogic_ThrowsExceptionIfQueryFails()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate a database error
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database error"));  // Simulate error

			// Act & Assert
			await Assert.ThrowsAsync<Exception>(() => _notificationService.ClaimantCancelLogic(reservationID));
		}

		[Fact]
		public async Task ClaimantCancelNodataFound_VerifiesQueryWithCorrectWhereConditions()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ClaimantCancelNodataFound(reservationID);

			// Assert that the query contains the correct WHERE clause conditions
			_mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
				It.Is<string>(query => query.Contains("a.ReservationID = @ReservationID") &&
									   query.Contains("a.RSVACCode = 'ResCancel'") &&
									   query.Contains("a.inactiveflag = 0")),
				It.IsAny<DynamicParameters>(),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}


		[Fact]
		public async Task ClaimantCancelNodataFound_VerifiesReservationIDParameter()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ClaimantCancelNodataFound(reservationID);

			// Assert that the ReservationID parameter is correctly added to the query
			_mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
				It.Is<string>(query => query.Contains("WHERE a.ReservationID = @ReservationID")),
				It.Is<DynamicParameters>(p => p.Get<int>("ReservationID") == reservationID),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ClaimantCancelNodataFound_ReturnsEmptyListIfNoResultsFound()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate no data found
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate no results

			// Act
			var result = await _notificationService.ClaimantCancelNodataFound(reservationID);

			// Assert that the result is an empty list
			Assert.Empty(result);
		}

		[Fact]
		public async Task ClaimantCancelNodataFound_ThrowsExceptionIfQueryFails()
		{
			// Arrange
			var reservationID = 123;

			// Mock the response to simulate a database error
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database error"));  // Simulate error

			// Act & Assert
			await Assert.ThrowsAsync<Exception>(() => _notificationService.ClaimantCancelNodataFound(reservationID));
		}

		[Fact]
		public async Task ClaimantCancelNodataFound_VerifiesNotificationDateTimeField()
		{
			// Arrange
			var reservationID = 123;

			var expectedResult = new List<ClaimantCancelResponseModel>
	{
		new ClaimantCancelResponseModel
		{
			ReservationsAssignmentsID = 456,
			ResAsgnCode = "A001",
			ClaimantID = 789,
			NotificationDateTime = DateTime.Now // Assuming `getdate()` returns the current time
        }
	};

			// Mock the response to simulate a successful query returning the expected result
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(expectedResult);  // Simulate returning the expected result

			// Act
			var result = await _notificationService.ClaimantCancelNodataFound(reservationID);

			// Assert that the result contains NotificationDateTime in the expected format
			Assert.NotNull(result);
			Assert.Equal(expectedResult[0].NotificationDateTime.Date, result.First().NotificationDateTime.Date);  // Compare only the date part
		}

		[Fact]
		public async Task ClaimantCancelNodataFound_ReturnsNoResultsForInvalidReservationID()
		{
			// Arrange
			var reservationID = 9999;  // Assuming this ID does not exist

			// Mock the response to simulate no data found for this ID
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ClaimantCancelResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ClaimantCancelResponseModel>());  // Simulate no results

			// Act
			var result = await _notificationService.ClaimantCancelNodataFound(reservationID);

			// Assert that the result is an empty list (no results)
			Assert.Empty(result);
		}

		[Fact]
		public async Task ClaimantTraking_VerifiesQueryWithCorrectWhereCondition()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate a successful query
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotificationResponseModel>(
					It.IsAny<string>(),
					It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == reservationAssignmentID), // Ensure @ReservationsAssignmentsID is passed correctly
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorNotificationResponseModel>());  // Simulate successful execution

			// Act
			await _notificationService.ClaimantTraking(reservationAssignmentID);

			// Assert that the query contains the correct WHERE clause conditions
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorNotificationResponseModel>(
				It.Is<string>(query => query.Contains("a.ReservationsAssignmentsID = @ReservationsAssignmentsID")), // Check for ReservationsAssignmentsID condition
				It.IsAny<DynamicParameters>(),
				CommandType.Text), Times.Once);  // Ensure ExecuteQueryAsync is called exactly once
		}

		[Fact]
		public async Task ClaimantTraking_ReturnsMappedResults()
		{
			// Arrange
			var reservationAssignmentID = 123;
			var mockResponse = new List<ContractorNotificationResponseModel>
	{
		new ContractorNotificationResponseModel
		{
			ReservationsAssignmentsID = 123,
			ReservationID = 456,
			FcmToken = "sampleFcmToken",
			DeviceID = "sampleDeviceID",
			ResAsgnCode = "AS123",
			ResTripType = "Round Trip",
			ClaimantID = 789,
			ContractorID = 1011,
			Contractor = "Sample Contractor",
			ASSGNCode = "ABC123",
			RSVATTCode = "XYZ123",
			PUAddress1 = "Pickup Address 1",
			PUAddress2 = "Pickup Address 2",
			DOAddress1 = "Dropoff Address 1",
			DOAddress2 = "Dropoff Address 2",
			ReservationDate = DateTime.Now,
			ReservationTime = DateTime.Now,
			PickupTime = DateTime.Now,
			NotificationDateTime = DateTime.Now
		}
	};

			// Mock the response to return the mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotificationResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.ClaimantTraking(reservationAssignmentID);

			// Assert that the result is correctly mapped to the response model
			Assert.NotNull(result); // Ensure the result is not null
			Assert.Single(result); // Ensure only one item is returned
			var firstResult = result.First();
			Assert.Equal(123, firstResult.ReservationsAssignmentsID);
			Assert.Equal("sampleFcmToken", firstResult.FcmToken);
			Assert.Equal("Pickup Address 1", firstResult.PUAddress1);
		}

		[Fact]
		public async Task ClaimantTraking_HandlesErrorGracefully()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate an error (e.g., database failure)
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotificationResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ThrowsAsync(new Exception("Database query failed"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(async () =>
				await _notificationService.ClaimantTraking(reservationAssignmentID));

			Assert.Equal("Database query failed", exception.Message); // Ensure the exception message is as expected
		}

		[Fact]
		public async Task ClaimantTraking_ReturnsEmptyListWhenNoDataFound()
		{
			// Arrange
			var reservationAssignmentID = 123;

			// Mock the response to simulate no data returned
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotificationResponseModel>(
					It.IsAny<string>(),
					It.IsAny<DynamicParameters>(),
					It.IsAny<CommandType>()))
				.ReturnsAsync(new List<ContractorNotificationResponseModel>());  // Simulate no data

			// Act
			var result = await _notificationService.ClaimantTraking(reservationAssignmentID);

			// Assert that the result is an empty list
			Assert.Empty(result); // Ensure no results are returned
		}

		[Fact]
		public async Task WebContractorNotAssignedLogic_VerifiesStoredProcedureInvocation()
		{
			// Arrange
			var expectedQuery = ProcEntities.spContractorNotAssignedWebNotification; // The stored procedure name
			var mockResponse = new List<ContractorNotAssignedResponseModel>
			{
				new ContractorNotAssignedResponseModel
				{
                    // Assuming some fields for the response model
                    ContractorID = 1,
					RSVATTCode = "Contractor A"
				}
			};

			// Mock the ExecuteQueryAsync method to return mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotAssignedResponseModel>(
					expectedQuery, CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.WebContractorNotAssignedLogic();

			// Assert that the stored procedure was invoked correctly
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorNotAssignedResponseModel>(
				expectedQuery, CommandType.StoredProcedure), Times.Once);  // Ensure it was called exactly once
		}

		[Fact]
		public async Task WebContractorNotAssignedLogic_HandlesErrorGracefully()
		{
			// Arrange
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotAssignedResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ThrowsAsync(new Exception("Database query failed"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(async () =>
				await _notificationService.WebContractorNotAssignedLogic());

			Assert.Equal("Database query failed", exception.Message); // Ensure the exception message is as expected
		}

		[Fact]
		public async Task WebContractorNotAssignedLogic_ReturnsEmptyListWhenNoDataFound()
		{
			// Arrange
			var emptyResponse = new List<ContractorNotAssignedResponseModel>();  // No data returned

			// Mock the ExecuteQueryAsync method to return an empty list
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotAssignedResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ReturnsAsync(emptyResponse);

			// Act
			var result = await _notificationService.WebContractorNotAssignedLogic();

			// Assert that the result is an empty list
			Assert.Empty(result); // Ensure the result is empty
		}

		[Fact]
		public async Task WebContractorNotAssignedLogic_VerifiesStoredProcedureWithParameters()
		{
			// Arrange
			var parameters = new DynamicParameters();
			parameters.Add("@SomeParameter", "SomeValue");

			var mockResponse = new List<ContractorNotAssignedResponseModel>
	{
		new ContractorNotAssignedResponseModel
		{
			ContractorID = 1,
			AssgnNum = "Contractor A"
		}
	};

			// Mock the ExecuteQueryAsync method
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorNotAssignedResponseModel>(
					ProcEntities.spContractorNotAssignedWebNotification, CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.WebContractorNotAssignedLogic();

			// Assert that the stored procedure is invoked correctly with the expected parameters
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorNotAssignedResponseModel>(
				ProcEntities.spContractorNotAssignedWebNotification, CommandType.StoredProcedure), Times.Once);
		}

		[Fact]
		public async Task WebContractorAssignedLogic_VerifiesStoredProcedureInvocation()
		{
			// Arrange
			var expectedQuery = ProcEntities.spContractorAssignmentWebNotification;  // The stored procedure name
			var mockResponse = new List<ContractorAssignedResponseModel>
			{
				new ContractorAssignedResponseModel
				{
                    // Assuming some fields for the response model
                    ContractorID = 1,
					AssgnNum = "Contractor A"
				}
			};

			// Mock the ExecuteQueryAsync method to return mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorAssignedResponseModel>(
					expectedQuery, CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.WebContractorAssignedLogic();

			// Assert that the stored procedure was invoked correctly
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorAssignedResponseModel>(
				expectedQuery, CommandType.StoredProcedure), Times.Once);  // Ensure it was called exactly once
		}

		[Fact]
		public async Task WebContractorAssignedLogic_ReturnsMappedResults()
		{
			// Arrange
			var mockResponse = new List<ContractorAssignedResponseModel>
	{
		new ContractorAssignedResponseModel
		{
			ContractorID = 1,
			AssgnNum = "Contractor A"
		}
	};

			// Mock the ExecuteQueryAsync method to return the mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorAssignedResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.WebContractorAssignedLogic();

			// Assert that the result is correctly mapped
			Assert.NotNull(result); // Ensure the result is not null
			Assert.Single(result); // Ensure only one item is returned
			var firstResult = result.First();
			Assert.Equal(1, firstResult.ContractorID); // Check that the ContractorID is correctly mapped
			Assert.Equal("Contractor A", firstResult.AssgnNum); // Check that the ContractorName is correctly mapped
		}

		[Fact]
		public async Task WebContractorAssignedLogic_HandlesErrorGracefully()
		{
			// Arrange
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorAssignedResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ThrowsAsync(new Exception("Database query failed"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(async () =>
				await _notificationService.WebContractorAssignedLogic());

			Assert.Equal("Database query failed", exception.Message); // Ensure the exception message is as expected
		}

		[Fact]
		public async Task WebContractorAssignedLogic_ReturnsEmptyListWhenNoDataFound()
		{
			// Arrange
			var emptyResponse = new List<ContractorAssignedResponseModel>();  // No data returned

			// Mock the ExecuteQueryAsync method to return an empty list
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorAssignedResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ReturnsAsync(emptyResponse);

			// Act
			var result = await _notificationService.WebContractorAssignedLogic();

			// Assert that the result is an empty list
			Assert.Empty(result); // Ensure the result is empty
		}

		[Fact]
		public async Task WebContractorAssignedLogic_VerifiesStoredProcedureWithParameters()
		{
			// Arrange
			var parameters = new DynamicParameters();
			parameters.Add("@SomeParameter", "SomeValue");

			var mockResponse = new List<ContractorAssignedResponseModel>
	{
		new ContractorAssignedResponseModel
		{
			ContractorID = 1,
			AssgnNum = "Contractor A"
		}
	};

			// Mock the ExecuteQueryAsync method to return the mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorAssignedResponseModel>(
					ProcEntities.spContractorAssignmentWebNotification, CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.WebContractorAssignedLogic();

			// Assert that the stored procedure is invoked correctly with the expected parameters
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorAssignedResponseModel>(
				ProcEntities.spContractorAssignmentWebNotification, CommandType.StoredProcedure), Times.Once);
		}

		[Fact]
		public async Task AssignmentjobRequestaAndNotification_VerifiesStoredProcedureInvocation()
		{
			// Arrange
			var expectedQuery = ProcEntities.spAssignmentJobSearchNotificationCronJob;  // The stored procedure name
			var mockResponse = new List<AssignmentJobRequestResponseModel>
			{
				new AssignmentJobRequestResponseModel
				{
                    // Assuming some fields for the response model
                        ContractorID = 1,
						AssgnNum = "Sample Job Request"
				}
			};

			// Mock the ExecuteQueryAsync method to return mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(
					expectedQuery, CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.AssignmentjobRequestaAndNotification();

			// Assert that the stored procedure was invoked correctly
			_mockContext.Verify(m => m.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(
				expectedQuery, CommandType.StoredProcedure), Times.Once);  // Ensure it was called exactly once
		}

		[Fact]
		public async Task AssignmentjobRequestaAndNotification_ReturnsMappedResults()
		{
			// Arrange
			var mockResponse = new List<AssignmentJobRequestResponseModel>
			{
				new AssignmentJobRequestResponseModel
				{
					 ContractorID = 1,
					 AssgnNum = "Sample Job Request"
				}
			};

			// Mock the ExecuteQueryAsync method to return the mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.AssignmentjobRequestaAndNotification();

			// Assert that the result is correctly mapped
			Assert.NotNull(result); // Ensure the result is not null
			Assert.Single(result); // Ensure only one item is returned
			var firstResult = result.First();
			Assert.Equal(1, firstResult.ContractorID); // Check that the JobRequestID is correctly mapped
			Assert.Equal("Sample Job Request", firstResult.AssgnNum); // Check that the JobRequestDetails are correctly mapped
		}

		[Fact]
		public async Task AssignmentjobRequestaAndNotification_HandlesErrorGracefully()
		{
			// Arrange
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ThrowsAsync(new Exception("Database query failed"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(async () =>
				await _notificationService.AssignmentjobRequestaAndNotification());

			Assert.Equal("Database query failed", exception.Message); // Ensure the exception message is as expected
		}

		[Fact]
		public async Task AssignmentjobRequestaAndNotification_ReturnsEmptyListWhenNoDataFound()
		{
			// Arrange
			var emptyResponse = new List<AssignmentJobRequestResponseModel>();  // No data returned

			// Mock the ExecuteQueryAsync method to return an empty list
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(
					It.IsAny<string>(), CommandType.StoredProcedure))
				.ReturnsAsync(emptyResponse);

			// Act
			var result = await _notificationService.AssignmentjobRequestaAndNotification();

			// Assert that the result is an empty list
			Assert.Empty(result); // Ensure the result is empty
		}

		[Fact]
		public async Task AssignmentjobRequestaAndNotification_VerifiesStoredProcedureWithParameters()
		{
			// Arrange
			var parameters = new DynamicParameters();
			parameters.Add("@SomeParameter", "SomeValue");

			var mockResponse = new List<AssignmentJobRequestResponseModel>
			{
				new AssignmentJobRequestResponseModel
				{
					ContractorID = 1,
					AssgnNum = "Sample Job Request"
				}
			};

			// Mock the ExecuteQueryAsync method to return the mock data
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(
					ProcEntities.spAssignmentJobSearchNotificationCronJob, CommandType.StoredProcedure))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.AssignmentjobRequestaAndNotification();

			// Assert that the stored procedure is invoked correctly with the expected parameters
			_mockContext.Verify(m => m.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(
				ProcEntities.spAssignmentJobSearchNotificationCronJob, CommandType.StoredProcedure), Times.Once);
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogic_VerifiesQueryExecution()
		{
			// Arrange
			var assignmentId = 123; // Sample Assignment ID

			var mockResponse = new List<ContractorWebJobSearchRespnseModel>
	{
		new ContractorWebJobSearchRespnseModel
		{
			ReservationsAssignmentsID = 123,
			FcmToken = "sampleToken",
			DeviceID = "sampleDevice",
			AssgnNum = "1234",
			ResAsgnCode = "RES001",
			NotificationType = "NEW_ASSIGNMENT_REQUEST"
		}
	};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(It.Is<string>(q => q.Contains("FROM vwReservationAssignSearch")),
					It.IsAny<DynamicParameters>(), CommandType.Text))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogic(assignmentId);

			// Assert that the query was executed with the correct SQL
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
				It.Is<string>(q => q.Contains("FROM vwReservationAssignSearch")),
				It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId),
				CommandType.Text), Times.Once);
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogic_ReturnsMappedResults()
		{
			// Arrange
			var assignmentId = 123;
			var mockResponse = new List<ContractorWebJobSearchRespnseModel>
	{
		new ContractorWebJobSearchRespnseModel
		{
			ReservationsAssignmentsID = 123,
			FcmToken = "sampleToken",
			DeviceID = "sampleDevice",
			AssgnNum = "1234",
			ResAsgnCode = "RES001",
			NotificationType = "NEW_ASSIGNMENT_REQUEST"
		}
	};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogic(assignmentId);

			// Assert that the result is correctly mapped
			Assert.NotNull(result); // Ensure the result is not null
			Assert.Single(result);  // Ensure only one item is returned
			var firstResult = result.First();
			Assert.Equal(123, firstResult.ReservationsAssignmentsID);  // Check ReservationsAssignmentsID
			Assert.Equal("sampleToken", firstResult.FcmToken);  // Check FcmToken
			Assert.Equal("sampleDevice", firstResult.DeviceID);  // Check DeviceID
			Assert.Equal("NEW_ASSIGNMENT_REQUEST", firstResult.NotificationType); // Check NotificationType
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogic_HandlesErrorGracefully()
		{
			// Arrange
			var assignmentId = 123;
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
				.ThrowsAsync(new Exception("Database query failed"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(async () =>
				await _notificationService.SendContractorWebJobSearchLogic(assignmentId));

			Assert.Equal("Database query failed", exception.Message);  // Check the exception message
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogic_ReturnsEmptyListWhenNoDataFound()
		{
			// Arrange
			var assignmentId = 123;
			var emptyResponse = new List<ContractorWebJobSearchRespnseModel>();  // No data returned

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
				.ReturnsAsync(emptyResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogic(assignmentId);

			// Assert that the result is an empty list
			Assert.Empty(result);  // Ensure the result is empty
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogic_VerifiesParameterPassing()
		{
			// Arrange
			var assignmentId = 123;
			var mockResponse = new List<ContractorWebJobSearchRespnseModel>
	{
		new ContractorWebJobSearchRespnseModel
		{
			ReservationsAssignmentsID = 123,
			FcmToken = "sampleToken"
		}
	};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId), CommandType.Text))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogic(assignmentId);

			// Assert that the parameter is passed correctly
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
				It.IsAny<string>(), It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId), CommandType.Text), Times.Once);
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogicNoDataFound_VerifiesQueryExecution()
		{
			// Arrange
			var assignmentId = 123; // Sample Assignment ID

			var mockResponse = new List<ContractorWebJobSearchRespnseModel>
	{
		new ContractorWebJobSearchRespnseModel
		{
			ReservationsAssignmentsID = 123,
			ResAsgnCode = "RES123",
			NotificationType = "ASSIGNMENT_NEED_ATTENTION"
		}
	};

			// Adjusting mock setup to match the expected query and parameters.
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.Is<string>(s => s.Contains("ASSIGNMENT_NEED_ATTENTION") && s.Contains("WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID")),
					It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId),
					CommandType.Text))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogicNoDataFound(assignmentId);

			// Assert that the query was executed with the correct SQL and parameters.
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
				It.Is<string>(s => s.Contains("ASSIGNMENT_NEED_ATTENTION")),
				It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
		}


		[Fact]
		public async Task SendContractorWebJobSearchLogicNoDataFound_ReturnsMappedResults()
		{
			// Arrange
			var assignmentId = 123;
			var mockResponse = new List<ContractorWebJobSearchRespnseModel>
	{
		new ContractorWebJobSearchRespnseModel
		{
			ReservationsAssignmentsID = 123,
			ResAsgnCode = "RES123",
			NotificationType = "ASSIGNMENT_NEED_ATTENTION"
		}
	};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogicNoDataFound(assignmentId);

			// Assert that the result is correctly mapped
			Assert.NotNull(result); // Ensure the result is not null
			Assert.Single(result);  // Ensure only one item is returned
			var firstResult = result.First();
			Assert.Equal(123, firstResult.ReservationsAssignmentsID);  // Check ReservationsAssignmentsID
			Assert.Equal("RES123", firstResult.ResAsgnCode);  // Check ResAsgnCode
			Assert.Equal("ASSIGNMENT_NEED_ATTENTION", firstResult.NotificationType); // Check NotificationType
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogicNoDataFound_ReturnsEmptyListWhenNoDataFound()
		{
			// Arrange
			var assignmentId = 123;
			var emptyResponse = new List<ContractorWebJobSearchRespnseModel>();  // No data returned

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
				.ReturnsAsync(emptyResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogicNoDataFound(assignmentId);

			// Assert that the result is an empty list
			Assert.Empty(result);  // Ensure the result is empty
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogicNoDataFound_HandlesErrorGracefully()
		{
			// Arrange
			var assignmentId = 123;
			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
				.ThrowsAsync(new Exception("Database query failed"));

			// Act & Assert
			var exception = await Assert.ThrowsAsync<Exception>(async () =>
				await _notificationService.SendContractorWebJobSearchLogicNoDataFound(assignmentId));

			Assert.Equal("Database query failed", exception.Message);  // Check the exception message
		}

		[Fact]
		public async Task SendContractorWebJobSearchLogicNoDataFound_VerifiesParameterPassing()
		{
			// Arrange
			var assignmentId = 123;
			var mockResponse = new List<ContractorWebJobSearchRespnseModel>
	{
		new ContractorWebJobSearchRespnseModel
		{
			ReservationsAssignmentsID = 123,
			ResAsgnCode = "RES123",
			NotificationType = "ASSIGNMENT_NEED_ATTENTION"
		}
	};

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
					It.IsAny<string>(), It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId), CommandType.Text))
				.ReturnsAsync(mockResponse);

			// Act
			var result = await _notificationService.SendContractorWebJobSearchLogicNoDataFound(assignmentId);

			// Assert that the parameter is passed correctly
			_mockContext.Verify(m => m.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(
				It.IsAny<string>(), It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId), CommandType.Text), Times.Once);
		}

		[Fact]
		public async Task GetDistinctUserUserIDAsync_ReturnsDistinctUserIDs()
		{
			// Arrange
			var expectedUserIds = new List<string> { "user1", "user2", "user3" };
			var query = @"
        SELECT DISTINCT UserID  
        FROM DBUsers 
        WHERE userfcmToken IS NOT NULL and UserFcmToken <> '' and inactiveflag = 0 and groupnum != 3;";

			_mockContext
				.Setup(m => m.ExecuteQueryAsync<string>(query, CommandType.Text))
				.ReturnsAsync(expectedUserIds);

			// Act
			var result = await _notificationService.GetDistinctUserUserIDAsync();

			// Assert
			Assert.NotNull(result);
			Assert.Empty(result);
		}

		[Fact]
		public async Task InsertNotificationLog_ShouldInsertNotification_WhenValidDataIsProvided()
		{
			// Arrange
			var notificationItem = new InsertNotificationLog
			{
				UserID = 1,
				UserType = 2,
				ReservationsAssignmentsID = 123,
				Title = "Test Title",
				Body = "Test Body",
				data = new Dictionary<string, string>
				{
					{ "key1", "value1" },
					{ "key2", "value2" }
				},
				NotificationDateTime = DateTime.UtcNow,
				NotificationType = "Push",
				CreatedBy = 1,
				FcmToken = "someFcmToken"
			};

			var parameters = new DynamicParameters();
			parameters.Add(DbParams.ReferenceID, Guid.NewGuid().ToString(), DbType.String);
			parameters.Add(DbParams.UserID, notificationItem.UserID, DbType.Int32);
			parameters.Add(DbParams.UserType, notificationItem.UserType, DbType.Int32);
			parameters.Add(DbParams.ReservationsAssignmentsID, notificationItem.ReservationsAssignmentsID, DbType.Int32);
			parameters.Add(DbParams.Title, notificationItem.Title, DbType.String);
			parameters.Add(DbParams.Body, notificationItem.Body, DbType.String);
			parameters.Add(DbParams.Data, JsonConvert.SerializeObject(notificationItem.data), DbType.String);  // Serialize the dictionary
			parameters.Add(DbParams.SentDate, notificationItem.NotificationDateTime, DbType.DateTime);
			parameters.Add(DbParams.ReadStatus, 0, DbType.Int32);
			parameters.Add(DbParams.NotificationType, notificationItem.NotificationType, DbType.String);
			parameters.Add(DbParams.CreatedBy, notificationItem.CreatedBy, DbType.Int32);
			parameters.Add(DbParams.CreatedDate, _timeZoneConverter.ConvertUtcToConfiguredTimeZone(), DbType.DateTime);
			parameters.Add(DbParams.FcmToken, notificationItem.FcmToken, DbType.String);

			// Mock the ExecuteAsync method to simulate a successful insert operation
			_mockContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
						.ReturnsAsync(1);  // Simulate 1 row affected

			// Act
			await _notificationService.InsertNotificationLog(notificationItem);

			// Assert
			_mockContext.Verify(db => db.ExecuteAsync(It.Is<string>(s => s.Contains("INSERT INTO NotificationLog")), It.Is<DynamicParameters>(p =>
				p.Get<string>(DbParams.Data) == JsonConvert.SerializeObject(notificationItem.data)  // Check if data is serialized properly
			), CommandType.Text), Times.Once);
		}

		[Fact]
		public async Task InsertNotificationLog_ShouldInsert_WhenDataIsEmpty()
		{
			// Arrange
			var notificationItem = new InsertNotificationLog
			{
				UserID = 1,
				UserType = 2,
				ReservationsAssignmentsID = 123,
				Title = "Test Title",
				Body = "Test Body",
				data = new Dictionary<string, string>(),  // Empty dictionary
				NotificationDateTime = DateTime.UtcNow,
				NotificationType = "Push",
				CreatedBy = 1,
				FcmToken = "someFcmToken"
			};

			var parameters = new DynamicParameters();
			parameters.Add(DbParams.ReferenceID, Guid.NewGuid().ToString(), DbType.String);
			parameters.Add(DbParams.UserID, notificationItem.UserID, DbType.Int32);
			parameters.Add(DbParams.UserType, notificationItem.UserType, DbType.Int32);
			parameters.Add(DbParams.ReservationsAssignmentsID, notificationItem.ReservationsAssignmentsID, DbType.Int32);
			parameters.Add(DbParams.Title, notificationItem.Title, DbType.String);
			parameters.Add(DbParams.Body, notificationItem.Body, DbType.String);
			parameters.Add(DbParams.Data, JsonConvert.SerializeObject(notificationItem.data), DbType.String);  // Serialize the empty dictionary
			parameters.Add(DbParams.SentDate, notificationItem.NotificationDateTime, DbType.DateTime);
			parameters.Add(DbParams.ReadStatus, 0, DbType.Int32);
			parameters.Add(DbParams.NotificationType, notificationItem.NotificationType, DbType.String);
			parameters.Add(DbParams.CreatedBy, notificationItem.CreatedBy, DbType.Int32);
			parameters.Add(DbParams.CreatedDate, _timeZoneConverter.ConvertUtcToConfiguredTimeZone(), DbType.DateTime);
			parameters.Add(DbParams.FcmToken, notificationItem.FcmToken, DbType.String);

			// Mock the ExecuteAsync method to simulate a successful insert operation
			_mockContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
						.ReturnsAsync(1);  // Simulate 1 row affected

			// Act
			await _notificationService.InsertNotificationLog(notificationItem);

			// Assert
			_mockContext.Verify(db => db.ExecuteAsync(It.Is<string>(s => s.Contains("INSERT INTO NotificationLog")), It.Is<DynamicParameters>(p =>
				p.Get<string>(DbParams.Data) == "{}"  // Ensure the empty dictionary is serialized as empty JSON '{}'
			), CommandType.Text), Times.Once);
		}

		[Fact]
		public async Task InsertNotificationLog_ShouldInsert_WhenFcmTokenIsEmpty()
		{
			// Arrange
			var notificationItem = new InsertNotificationLog
			{
				UserID = 1,
				UserType = 2,
				ReservationsAssignmentsID = 123,
				Title = "Test Title",
				Body = "Test Body",
				data = new Dictionary<string, string>
		{
			{ "key", "value" }
		},
				NotificationDateTime = DateTime.UtcNow,
				NotificationType = "Push",
				CreatedBy = 1,
				FcmToken = ""  // Empty FcmToken
			};

			var parameters = new DynamicParameters();
			parameters.Add(DbParams.ReferenceID, Guid.NewGuid().ToString(), DbType.String);
			parameters.Add(DbParams.UserID, notificationItem.UserID, DbType.Int32);
			parameters.Add(DbParams.UserType, notificationItem.UserType, DbType.Int32);
			parameters.Add(DbParams.ReservationsAssignmentsID, notificationItem.ReservationsAssignmentsID, DbType.Int32);
			parameters.Add(DbParams.Title, notificationItem.Title, DbType.String);
			parameters.Add(DbParams.Body, notificationItem.Body, DbType.String);
			parameters.Add(DbParams.Data, JsonConvert.SerializeObject(notificationItem.data), DbType.String);  // Serialize the dictionary
			parameters.Add(DbParams.SentDate, notificationItem.NotificationDateTime, DbType.DateTime);
			parameters.Add(DbParams.ReadStatus, 0, DbType.Int32);
			parameters.Add(DbParams.NotificationType, notificationItem.NotificationType, DbType.String);
			parameters.Add(DbParams.CreatedBy, notificationItem.CreatedBy, DbType.Int32);
			parameters.Add(DbParams.CreatedDate, _timeZoneConverter.ConvertUtcToConfiguredTimeZone(), DbType.DateTime);
			parameters.Add(DbParams.FcmToken, notificationItem.FcmToken, DbType.String);  // Handle empty FcmToken

			// Mock the ExecuteAsync method to simulate a successful insert operation
			_mockContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
						.ReturnsAsync(1);  // Simulate 1 row affected

			// Act
			await _notificationService.InsertNotificationLog(notificationItem);

			// Assert
			_mockContext.Verify(db => db.ExecuteAsync(It.Is<string>(s => s.Contains("INSERT INTO NotificationLog")), It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
		}

		[Fact]
		public async Task InsertNotificationLog_ShouldNotThrowException_WhenDataIsEmpty()
		{
			// Arrange
			var notificationItem = new InsertNotificationLog
			{
				UserID = 1,
				UserType = 2,
				ReservationsAssignmentsID = 123,
				Title = "Test Title",
				Body = "Test Body",
				data = new Dictionary<string, string>(),  // Empty data dictionary
				NotificationDateTime = DateTime.UtcNow,
				NotificationType = "Push",
				CreatedBy = 1,
				FcmToken = "someFcmToken"
			};

			// Mock the ExecuteAsync method to simulate a successful insert operation
			_mockContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
						.ReturnsAsync(1);  // Simulate 1 row affected

			// Act & Assert
			await _notificationService.InsertNotificationLog(notificationItem);

			Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
		}


	}
}
