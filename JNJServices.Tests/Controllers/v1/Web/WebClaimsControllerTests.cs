using JNJServices.API.Controllers.v1.Web;
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
    public class WebClaimsControllerTests
    {
        private readonly Mock<IClaimsService> _mockClaimsService;
        private readonly WebClaimsController _controller;

        public WebClaimsControllerTests()
        {
            // Arrange: Initialize the mocks
            _mockClaimsService = new Mock<IClaimsService>();

            // Act: Initialize the controller with the mocked service
            _controller = new WebClaimsController(_mockClaimsService.Object);
        }

        [Fact]
        public async Task ClaimsSearch_ReturnsOkWithResults_WhenClaimsExist()
        {
            // Arrange
            var searchModel = new ClaimsSearchWebViewModel
            {
                ClaimID = 1,
                ClaimNumber = "C123",
                CustomerID = 202,
                ClaimantID = 101,
                Birthdate = "01/01/1980", // Optional date for testing
                Page = 1,
                Limit = 20
            };

            var claimsList = new List<vwClaimsSearch>
            {
                new vwClaimsSearch
        {
            claimid = 1,
            claimnumber = "C123",
            ClaimantName = "John Doe",
            claimantid = 101,
            customerid = 202,
            claimintfid = "INT123",
            injurydate = new DateTime(2023, 1, 1),
            injurytype = "Type A",
            referraldate = new DateTime(2023, 2, 1),
            referraltime = new DateTime(2023, 2, 1, 10, 0, 0),
            trntycode = "TC123",
            ratecode = "RC123",
            inactiveflag = 0,
            vehszcode = "V123",
            clmrscode = "CM123",
            clmlscode = "CL123",
            ratetypcode = "RT123",
            clmdgcode1 = "DG1",
            clmdgcode2 = "DG2",
            clmdgcode3 = "DG3",
            clmdgcode4 = "DG4",
            referralcontactid = 1001,
            payerid = "Payer001",
            payername = "Payer Name",
            PayerAddress1 = "123 Payer St",
            PayerAddress2 = "Suite 456",
            PayerCity = "Payer City",
            PayerStateCode = "PC",
            PayerZipCode = "12345",
            AmerisysDBID = "DB123",
            AmerisysClaimID = "ClaimID123",
            iccflag = 1,
            notes = "Some notes about the claim",
            qaflag = 0,
            Transportation = "Yes",
            Interpretation = "No",
            ClaimStatus = "Open",
            EmployerRelated = "Yes",
            AutoAccident = "No",
            OtherAccident = "Yes",
            createdate = DateTime.UtcNow,
            createuserid = "User123",
            lastchangedate = DateTime.UtcNow,
            lastchangeuserid = "User456",
            SettledFlag = 0,
            ClaimantLastNameFirstName = "Doe, John",
            ClaimantFullName = "John Doe",
            CustomerName = "Customer Name",
            customerintfid = "CustomerINT123",
            billadjustercontactid = 2001,
            InvoiceContactFL = "ContactFL",
            InvoiceContactLF = "ContactLF",
            ICcnttycode = "CT123",
            InvoiceContact = "Invoice Contact",
            ReferralContactFL = "ReferralFL",
            ReferralContactLN = "ReferralLN",
            ReferralContactFN = "ReferralFN",
            ClaimEmployer = "Employer Name",
            ClaimEmployerAddress1 = "123 Employer St",
            ClaimEmployerAddress2 = "Suite 456",
            ClaimEmployerAddress3 = "Floor 2",
            ClaimEmployerCity = "Employer City",
            ClaimEmployerState = "ES",
            ClaimEmployerZip = "54321",
            DriverNeededFlag = 1,
            LanguCode = "EN",
            Birthdate = new DateTime(1980, 1, 1),
            height = "5'10\"",
            weight = 180,
            CustomerFL = "FL123",
            TotalCount = 2
        },
        new vwClaimsSearch
        {
            claimid = 2,
            claimnumber = "C124",
            ClaimantName = "Jane Smith",
            claimantid = 102,
            customerid = 203,
            claimintfid = "INT124",
            injurydate = new DateTime(2023, 3, 1),
            injurytype = "Type B",
            referraldate = new DateTime(2023, 4, 1),
            referraltime = new DateTime(2023, 4, 1, 11, 0, 0),
            trntycode = "TC124",
            ratecode = "RC124",
            inactiveflag = 1,
            vehszcode = "V124",
            clmrscode = "CM124",
            clmlscode = "CL124",
            ratetypcode = "RT124",
            clmdgcode1 = "DG5",
            clmdgcode2 = "DG6",
            clmdgcode3 = "DG7",
            clmdgcode4 = "DG8",
            referralcontactid = 1002,
            payerid = "Payer002",
            payername = "Payer Name 2",
            PayerAddress1 = "456 Payer Ave",
            PayerAddress2 = "",
            PayerCity = "Payer City 2",
            PayerStateCode = "PC2",
            PayerZipCode = "67890",
            AmerisysDBID = "DB124",
            AmerisysClaimID = "ClaimID124",
            iccflag = 0,
            notes = "Some notes about Jane's claim",
            qaflag = 1,
            Transportation = "No",
            Interpretation = "Yes",
            ClaimStatus = "Closed",
            EmployerRelated = "No",
            AutoAccident = "Yes",
            OtherAccident = "No",
            createdate = DateTime.UtcNow,
            createuserid = "User789",
            lastchangedate = DateTime.UtcNow,
            lastchangeuserid = "User012",
            SettledFlag = 1,
            ClaimantLastNameFirstName = "Smith, Jane",
            ClaimantFullName = "Jane Smith",
            CustomerName = "Customer Name 2",
            customerintfid = "CustomerINT124",
            billadjustercontactid = 2002,
            InvoiceContactFL = "ContactFL2",
            InvoiceContactLF = "ContactLF2",
            ICcnttycode = "CT124",
            InvoiceContact = "Invoice Contact 2",
            ReferralContactFL = "ReferralFL2",
            ReferralContactLN = "ReferralLN2",
            ReferralContactFN = "ReferralFN2",
            ClaimEmployer = "Employer Name 2",
            ClaimEmployerAddress1 = "456 Employer St",
            ClaimEmployerAddress2 = "Apt 789",
            ClaimEmployerAddress3 = "Roof",
            ClaimEmployerCity = "Employer City 2",
            ClaimEmployerState = "ES2",
            ClaimEmployerZip = "98765",
            DriverNeededFlag = 0,
            LanguCode = "FR",
            Birthdate = new DateTime(1980, 1, 1),
            height = "5'5\"",
            weight = 150,
            CustomerFL = "FL124",
            TotalCount = 2
        }
            };

            _mockClaimsService.Setup(service => service.ClaimsSearch(searchModel))
                .ReturnsAsync(claimsList);

            // Act
            var result = await _controller.ClaimsSearch(searchModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var response = result.Value as PaginatedResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(claimsList, response.data);
            Assert.Equal(claimsList.Count, response.totalData);
        }

        [Fact]
        public async Task ClaimsSearch_ReturnsNotFound_WhenNoClaimsExist()
        {
            // Arrange
            var searchModel = new ClaimsSearchWebViewModel
            {
                ClaimID = 1,
                ClaimNumber = "C123",
                CustomerID = 202,
                ClaimantID = 101,
                Birthdate = "01/01/1980", // Optional date for testing
                Page = 1,
                Limit = 20
            };

            var claimsList = new List<vwClaimsSearch>();

            _mockClaimsService.Setup(service => service.ClaimsSearch(searchModel))
                .ReturnsAsync(claimsList);

            // Act
            var result = await _controller.ClaimsSearch(searchModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var response = result.Value as PaginatedResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.FALSE);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ClaimsFacilities_ReturnsOkWithResults_WhenFacilitiesExist()
        {
            // Arrange
            var searchModel = new ClaimsIDWebViewModel
            {
                // Set the properties of the model as needed
                ClaimID = 1
            };

            var facilitiesList = new List<vwClaimsFacilities>
        {
             new vwClaimsFacilities
        {
            claimid = 1,
            FacilityID = 1001,
            origauthdate = DateTime.Now,
            clfstcode = "FAC001",
            inactiveflag = 0,
            origauthexpdate = DateTime.Now.AddDays(30),
            createuserid = "user123",
            CreateDate = DateTime.Now,
            AuthContactFL = "Contact1",
            OAuthContactFL = "Contact2",
            FacilityShortName = "ShortName1",
            factycode = "FCODE1",
            facilityname = "Facility 1",
            facilityname2 = "Facility 1A",
            contactname = "John Doe",
            complex = "Complex A",
            address1 = "123 Main St",
            address2 = "Apt 4B",
            city = "Metropolis",
            statecode = "NY",
            zipcode = "12345",
            phone = "555-1234",
            FacilityAddress = "123 Main St, Apt 4B, Metropolis, NY, 12345",
            LanguageName = "English",
            FacilityType = "Type A",
            CLFSTStatus = "Active",
            FLANGUCode1 = "EN",
            FLANGUCode2 = "ES",
            TranspExpDate = DateTime.Now.AddDays(15),
            InterpExpDate = DateTime.Now.AddDays(20)
        },
        new vwClaimsFacilities
        {
            claimid = 1,
            FacilityID = 1002,
            origauthdate = DateTime.Now,
            clfstcode = "FAC002",
            inactiveflag = 1,
            origauthexpdate = DateTime.Now.AddDays(30),
            createuserid = "user456",
            CreateDate = DateTime.Now,
            AuthContactFL = "Contact3",
            OAuthContactFL = "Contact4",
            FacilityShortName = "ShortName2",
            factycode = "FCODE2",
            facilityname = "Facility 2",
            facilityname2 = "Facility 2A",
            contactname = "Jane Smith",
            complex = "Complex B",
            address1 = "456 Elm St",
            address2 = "Suite 200",
            city = "Gotham",
            statecode = "NJ",
            zipcode = "67890",
            phone = "555-5678",
            FacilityAddress = "456 Elm St, Suite 200, Gotham, NJ, 67890",
            LanguageName = "Spanish",
            FacilityType = "Type B",
            CLFSTStatus = "Inactive",
            FLANGUCode1 = "ES",
            FLANGUCode2 = "EN",
            TranspExpDate = DateTime.Now.AddDays(10),
            InterpExpDate = DateTime.Now.AddDays(5)
        }
        };

            _mockClaimsService.Setup(service => service.ClaimsFacility(searchModel))
                .ReturnsAsync(facilitiesList);

            // Act
            var result = await _controller.ClaimsFacilities(searchModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(facilitiesList, response.data);
        }

        [Fact]
        public async Task ClaimsFacilities_ReturnsNotFound_WhenNoFacilitiesExist()
        {
            // Arrange
            var searchModel = new ClaimsIDWebViewModel
            {
                // Set the properties of the model as needed
                ClaimID = 1
            };

            var facilitiesList = new List<vwClaimsFacilities>();

            _mockClaimsService.Setup(service => service.ClaimsFacility(searchModel))
                .ReturnsAsync(facilitiesList);

            // Act
            var result = await _controller.ClaimsFacilities(searchModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.FALSE);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task ClaimsApprovedContractors_ReturnsOkWithResults_WhenContractorsExist()
        {
            // Arrange
            var searchModel = new ClaimsIDWebViewModel
            {
                ClaimID = 1
            };

            var contractorsList = new List<ClaimsApprovedContractorWebResponseModel>
            {
               new ClaimsApprovedContractorWebResponseModel
                {
                    ClaimID = 1,
                    ContractorID = 1001,
                    Miles = 150.5m,
                    Notes = "Approved for long distance.",
                    PrioritySort = 1,
                    CreateDate = DateTime.Now, // Setting CreateDate
                    CreateUserID = "userA", // Setting CreateUserID
                    LastChangeDate = DateTime.Now, // Setting LastChangeDate
                    LastChangeUserID = "userB", // Setting LastChangeUserID
                    archiveflag = 0, // Setting archiveflag
                    ContractorName = "Contractor A", // Setting ContractorName
                    Service = "Transport" // Setting Service
                },
                new ClaimsApprovedContractorWebResponseModel
                {
                    ClaimID = 1,
                    ContractorID = 1002,
                    Miles = 200.0m,
                    Notes = "Local contractor.",
                    PrioritySort = 2,
                    CreateDate = DateTime.Now.AddDays(-1), // Setting CreateDate
                    CreateUserID = "userC", // Setting CreateUserID
                    LastChangeDate = DateTime.Now.AddDays(-1), // Setting LastChangeDate
                    LastChangeUserID = "userD", // Setting LastChangeUserID
                    archiveflag = 1, // Setting archiveflag
                    ContractorName = "Contractor B", // Setting ContractorName
                    Service = "Delivery" // Setting Service
                }
            };

            _mockClaimsService.Setup(service => service.ClaimsApprovedContractors(searchModel))
                .ReturnsAsync(contractorsList);

            // Act
            var result = await _controller.ClaimsApprovedContractors(searchModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(contractorsList, response.data);
        }

        [Fact]
        public async Task ClaimsApprovedContractors_ReturnsNotFound_WhenNoContractorsExist()
        {
            // Arrange
            var searchModel = new ClaimsIDWebViewModel
            {
                ClaimID = 1
            };

            var contractorsList = new List<ClaimsApprovedContractorWebResponseModel>();

            _mockClaimsService.Setup(service => service.ClaimsApprovedContractors(searchModel))
                .ReturnsAsync(contractorsList);

            // Act
            var result = await _controller.ClaimsApprovedContractors(searchModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.FALSE);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }
    }
}
