using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;


namespace JNJServices.Tests.Business.Services
{
    public class ClaimsServiceTests
    {
        private readonly Mock<IDapperContext> _mockContext;
        private readonly ClaimsService _claimsService;

        public ClaimsServiceTests()
        {
            _mockContext = new Mock<IDapperContext>();
            _claimsService = new ClaimsService(_mockContext.Object);
        }

        [Fact]
        public async Task ClaimsSearch_ShouldCallExecuteQueryAsync_WithValidParameters()
        {
            // Arrange
            var model = new ClaimsSearchWebViewModel
            {
                ClaimID = 123,
                ClaimNumber = "CLM123",
                CustomerID = 456,
                ClaimantID = 789,
                Birthdate = "1990-01-01",
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<vwClaimsSearch> { new vwClaimsSearch() }; // Mock expected result

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimsSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimsSearch>(
                ProcEntities.spClaimsSearch,
                It.Is<DynamicParameters>(p => p.ParameterNames.Contains(DbParams.ClaimID) &&
                                              p.ParameterNames.Contains(DbParams.ClaimNumber) &&
                                              p.ParameterNames.Contains(DbParams.CustomerID) &&
                                              p.ParameterNames.Contains(DbParams.ClaimantID) &&
                                              p.ParameterNames.Contains(DbParams.Birthdate) &&
                                              p.ParameterNames.Contains(DbParams.Page) &&
                                              p.ParameterNames.Contains(DbParams.Limit)),
                CommandType.StoredProcedure), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimsSearch_ShouldReturnEmpty_WhenModelHasEmptyParameters()
        {
            // Arrange
            var model = new ClaimsSearchWebViewModel
            {
                ClaimID = 0, // Invalid claim ID
                ClaimNumber = null, // Empty claim number
                CustomerID = 0, // Invalid customer ID
                ClaimantID = 0, // Invalid claimant ID
                Birthdate = null, // Empty birthdate
                Page = null, // Default to 0
                Limit = null // Default to 0
            };

            var expectedResult = new List<vwClaimsSearch>(); // Empty result

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimsSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimsSearch>(
                ProcEntities.spClaimsSearch,
                It.Is<DynamicParameters>(p => !p.ParameterNames.Contains(DbParams.ClaimID) &&
                                              !p.ParameterNames.Contains(DbParams.ClaimNumber) &&
                                              !p.ParameterNames.Contains(DbParams.CustomerID) &&
                                              !p.ParameterNames.Contains(DbParams.ClaimantID) &&
                                              !p.ParameterNames.Contains(DbParams.Birthdate) &&
                                              p.ParameterNames.Contains(DbParams.Page) &&
                                              p.ParameterNames.Contains(DbParams.Limit)),
                CommandType.StoredProcedure), Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result is empty
        }

        [Fact]
        public async Task ClaimsSearch_ShouldUseDefaultValues_WhenPageOrLimitIsNull()
        {
            // Arrange
            var model = new ClaimsSearchWebViewModel
            {
                ClaimID = 123,
                ClaimNumber = "CLM123",
                CustomerID = 456,
                ClaimantID = 789,
                Birthdate = "1990-01-01",
                Page = null, // Default should be used
                Limit = null // Default should be used
            };

            var expectedResult = new List<vwClaimsSearch> { new vwClaimsSearch() }; // Mock expected result

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimsSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimsSearch>(
                ProcEntities.spClaimsSearch,
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.Page) == DefaultAppSettings.PageSize &&
                                              p.Get<int>(DbParams.Limit) == DefaultAppSettings.PageLimit),
                CommandType.StoredProcedure), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimsFacility_ShouldCallExecuteQueryAsync_WithValidClaimID()
        {
            // Arrange
            var model = new ClaimsIDWebViewModel { ClaimID = 123 };
            var expectedResult = new List<vwClaimsFacilities> { new vwClaimsFacilities() };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimsFacilities>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsFacility(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimsFacilities>(
                It.Is<string>(q => q.Contains("ClaimID=@claimid")),
                It.Is<DynamicParameters>(p => p.ParameterNames.Contains(DbParams.ClaimID) && p.Get<int>(DbParams.ClaimID) == 123),
                CommandType.Text), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimsFacility_ShouldCallExecuteQueryAsync_WithoutClaimID_WhenClaimIDIsZeroOrNull()
        {
            // Arrange
            var model = new ClaimsIDWebViewModel { ClaimID = 0 }; // or `ClaimID = null;` for testing null case
            var expectedResult = new List<vwClaimsFacilities> { new vwClaimsFacilities() };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimsFacilities>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsFacility(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimsFacilities>(
                It.Is<string>(q => q.Contains("(@claimId IS NULL OR ClaimID=@claimid)")),
                It.Is<DynamicParameters>(p => !p.ParameterNames.Contains(DbParams.ClaimID)), // Verify that ClaimID was not added to parameters
                CommandType.Text), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimsFacility_ShouldReturnEmptyList_WhenNoRecordsFound()
        {
            // Arrange
            var model = new ClaimsIDWebViewModel { ClaimID = 999 }; // Assume 999 is an ID that does not exist
            var expectedResult = new List<vwClaimsFacilities>(); // Expected empty result

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimsFacilities>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsFacility(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimsFacilities>(
                It.Is<string>(q => q.Contains("ClaimID=@claimid")),
                It.Is<DynamicParameters>(p => p.ParameterNames.Contains(DbParams.ClaimID) && p.Get<int>(DbParams.ClaimID) == 999),
                CommandType.Text), Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ClaimsApprovedContractors_ShouldCallExecuteQueryAsync_WithValidClaimID()
        {
            // Arrange
            var model = new ClaimsIDWebViewModel { ClaimID = 101 };
            var expectedResult = new List<ClaimsApprovedContractorWebResponseModel>
        {
            new ClaimsApprovedContractorWebResponseModel()
        };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<ClaimsApprovedContractorWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsApprovedContractors(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<ClaimsApprovedContractorWebResponseModel>(
                It.Is<string>(q => q.Contains("cac.ClaimID=@claimid")),
                It.Is<DynamicParameters>(p => p.ParameterNames.Contains(DbParams.ClaimID) && p.Get<int>(DbParams.ClaimID) == 101),
                CommandType.Text), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimsApprovedContractors_ShouldCallExecuteQueryAsync_WithoutClaimID_WhenClaimIDIsZeroOrNull()
        {
            // Arrange
            var model = new ClaimsIDWebViewModel { ClaimID = 0 }; // Testing with ClaimID as 0
            var expectedResult = new List<ClaimsApprovedContractorWebResponseModel>
        {
            new ClaimsApprovedContractorWebResponseModel()
        };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<ClaimsApprovedContractorWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsApprovedContractors(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<ClaimsApprovedContractorWebResponseModel>(
                It.Is<string>(q => q.Contains("(@claimId IS NULL OR cac.ClaimID=@claimid)")),
                It.Is<DynamicParameters>(p => !p.ParameterNames.Contains(DbParams.ClaimID)), // Ensure ClaimID is not added to parameters
                CommandType.Text), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimsApprovedContractors_ShouldReturnEmptyList_WhenNoRecordsFound()
        {
            // Arrange
            var model = new ClaimsIDWebViewModel { ClaimID = 999 }; // Use a ClaimID that does not exist
            var expectedResult = new List<ClaimsApprovedContractorWebResponseModel>(); // Expected empty result

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<ClaimsApprovedContractorWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimsService.ClaimsApprovedContractors(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<ClaimsApprovedContractorWebResponseModel>(
                It.Is<string>(q => q.Contains("cac.ClaimID=@claimid")),
                It.Is<DynamicParameters>(p => p.ParameterNames.Contains(DbParams.ClaimID) && p.Get<int>(DbParams.ClaimID) == 999),
                CommandType.Text), Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
