using AutoMapper;
using JNJServices.API.Controllers.v1.Web;
using JNJServices.API.Mapper;
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
    public class WebCustomerControllerTests
    {
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly IMapper _mapper;
        private readonly WebCustomerController _controller;

        public WebCustomerControllerTests()
        {
            // Arrange: Initialize the mocks
            _mockCustomerService = new Mock<ICustomerService>();

            // Initialize AutoMapper with your profiles
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfiles()); // Add your mapping profiles here
            });
            _mapper = mappingConfig.CreateMapper(); // Create the mapper instance

            // Initialize the controller with the mocked services
            _controller = new WebCustomerController(_mockCustomerService.Object, _mapper);
        }


        [Fact]
        public async Task Category_ReturnsOkWithCategories_WhenCategoriesAreAvailable()
        {
            // Arrange: Setup the mock to return a list of customer categories
            var categoryList = new List<CustomerCategory>
            {
                new CustomerCategory { code = "C1", description = "Category 1" },
                new CustomerCategory { code = "C2", description = "Category 2" }
            };

            _mockCustomerService.Setup(service => service.GetCategory())
                .ReturnsAsync(categoryList);

            // Act: Call the Category method
            var result = await _controller.Category();

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);
            Assert.Equal(categoryList, response.data);
        }

        [Fact]
        public async Task Category_ReturnsOkWithDataNotFound_WhenNoCategoriesAreAvailable()
        {
            // Arrange: Setup the mock to return an empty list
            var categoryList = new List<CustomerCategory>();
            _mockCustomerService.Setup(service => service.GetCategory())
                .ReturnsAsync(categoryList);

            // Act: Call the Category method
            var result = await _controller.Category();

            // Assert: Check that the response indicates no data found
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task GetCustomerSearch_ReturnsOkWithCustomers_WhenResultsAreAvailable()
        {
            // Arrange: Create a sample model and setup the mock to return a list of customers
            var searchModel = new CustomerSearchWebViewModel
            {
                CustomerID = 1,
                CustomerName = "John Doe",
                Email = "john.doe@example.com",
                States = "NY",
                Category = "Premium",
                Inactive = 0,
                Page = 1,
                Limit = 20
            };
            var customerList = new List<vwCustomerSearch>
            {
                new vwCustomerSearch
        {
            CustomerID = 1,
            CustomerName = "John Doe",
            Address1 = "123 Main St",
            Address2 = "Apt 4B",
            Address3 = "Suite 200",
            City = "Metropolis",
            CompanyName = "Doe Inc.",
            CreateDate = DateTime.UtcNow.AddDays(-10),
            CreateUserID = "Admin",
            CUSBTCode = "BT01",
            CUSCTCode = "CT02",
            CUSLSCode = "LS01",
            CUSRSCode = "RS01",
            CustomerIntfID = "C123",
            CUSTYCode = "TY03",
            Fax = "123-456-7890",
            Firstname = "John",
            Lastname = "Doe",
            inactiveflag = 0,
            LastChangeDate = DateTime.UtcNow.AddDays(-1),
            LastChangeUserID = "Admin",
            Notes = "Important client",
            Phone = "1234567890",
            PhoneExt = "123",
            RATETYPCode = "R01",
            ShortName = "JD",
            STATECode = "NY",
            TollfreePhone = "1800123456",
            Website = "https://doeinc.com",
            ZipCode = "12345",
            CUSBFCode = "BF01",
            FaxPrefix = "123",
            TimeZone = "Eastern Standard Time",
            RATECode = "R01",
            TERMSCode = "T01",
            CUSMSGCode = "MSG01",
            CUSIFCode = "IF01",
            QBImportFlag = 1,
            EDIFlag = 1,
            EDIIdentifier = "EDI123",
            PurchaseOrderNum = "PO12345",
            ClmAssgnLogFlag = 0,
            CUSINPCode = "INP01",
            Email = "john.doe@example.com",
            EmailInvoices = 1,
            MultipleInvoiceFlag = 0,
            ClaimFacilityDateFlag = 0,
            archiveflag = 0,
            CategoryName = "Premium",
            LeadSourceName = "Referral",
            RefralSourceName = "Partner Company",
            TypeName = "Business",
            StateName = "New York",
            ZipCodeCityName = "Metropolis",
            ZipCodeCountryName = "USA",
            RateName = "Standard Rate",
            TermsName = "Net 30",
            InvoicemessageName = "Thank you for your business.",
            InvDescCodingName = "Invoice Coding",
            InvPresFormatName = "Standard Format",
            ClaimantName = "John Doe",
            TotalCount = 2
        },
        new vwCustomerSearch
        {
            CustomerID = 2,
            CustomerName = "Jane Smith",
            Address1 = "456 Elm St",
            Address2 = "",
            Address3 = "Suite 300",
            City = "Gotham",
            CompanyName = "Smith LLC",
            CreateDate = DateTime.UtcNow.AddDays(-20),
            CreateUserID = "Admin",
            CUSBTCode = "BT02",
            CUSCTCode = "CT01",
            CUSLSCode = "LS02",
            CUSRSCode = "RS02",
            CustomerIntfID = "C456",
            CUSTYCode = "TY02",
            Fax = "789-012-3456",
            Firstname = "Jane",
            Lastname = "Smith",
            inactiveflag = 0,
            LastChangeDate = DateTime.UtcNow.AddDays(-5),
            LastChangeUserID = "Admin",
            Notes = "Potential client",
            Phone = "7890123456",
            PhoneExt = "456",
            RATETYPCode = "R02",
            ShortName = "JS",
            STATECode = "CA",
            TollfreePhone = "1800654321",
            Website = "https://smithllc.com",
            ZipCode = "54321",
            CUSBFCode = "BF02",
            FaxPrefix = "789",
            TimeZone = "Pacific Standard Time",
            RATECode = "R02",
            TERMSCode = "T02",
            CUSMSGCode = "MSG02",
            CUSIFCode = "IF02",
            QBImportFlag = 0,
            EDIFlag = 0,
            EDIIdentifier = "EDI456",
            PurchaseOrderNum = "PO67890",
            ClmAssgnLogFlag = 1,
            CUSINPCode = "INP02",
            Email = "jane.smith@example.com",
            EmailInvoices = 0,
            MultipleInvoiceFlag = 1,
            ClaimFacilityDateFlag = 1,
            archiveflag = 0,
            CategoryName = "Standard",
            LeadSourceName = "Online",
            RefralSourceName = "Website",
            TypeName = "Individual",
            StateName = "California",
            ZipCodeCityName = "Gotham",
            ZipCodeCountryName = "USA",
            RateName = "Special Rate",
            TermsName = "Net 15",
            InvoicemessageName = "We appreciate your business.",
            InvDescCodingName = "Custom Coding",
            InvPresFormatName = "Advanced Format",
            ClaimantName = "Jane Smith",
            TotalCount = 2
        }
            };

            _mockCustomerService.Setup(service => service.CustomerSearch(searchModel))
                .ReturnsAsync(customerList);

            // Act: Call the GetCustomerSearch method
            var result = await _controller.GetCustomerSearch(searchModel);

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);
            Assert.Equal(customerList.First().CustomerID, ((List<vwCustomerSearch>)response.data).First().CustomerID); // Check the first customer ID
            Assert.Equal(customerList.First().CustomerName, ((List<vwCustomerSearch>)response.data).First().CustomerName); // Check the first customer name
            Assert.Equal(customerList.Select(c => c.TotalCount).First(), response.totalData); // Ensure totalData is correctly set
        }

        [Fact]
        public async Task GetCustomerSearch_ReturnsOkWithDataNotFound_WhenNoResultsAreAvailable()
        {
            // Arrange: Create a sample model and setup the mock to return an empty list
            var searchModel = new CustomerSearchWebViewModel { /* Initialize properties as needed */ };
            var customerList = new List<vwCustomerSearch>();

            _mockCustomerService.Setup(service => service.CustomerSearch(searchModel))
                .ReturnsAsync(customerList);

            // Act: Call the GetCustomerSearch method
            var result = await _controller.GetCustomerSearch(searchModel);

            // Assert: Check that the response indicates no data found
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
            Assert.Equal(0, response.totalData); // Ensure totalData is set to 0 when no data found
        }

        [Fact]
        public async Task CustomerSelDetail_ReturnsOkWithCustomerDetails_WhenCustomerExists()
        {
            // Arrange: Create a sample model and setup the mock to return a list of customers
            var model = new CustomerIDWebViewModel { CustomerID = 1 };
            var customerList = new List<vwCustomerSearch>
            {
                new vwCustomerSearch
                {
                    CustomerID = 1,
                    CustomerName = "John Doe",
                    Address1 = "123 Main St",
                    Address2 = "Apt 4B",
                    City = "Metropolis",
                    CompanyName = "John Doe"
                }
            };

            // Setup the service to return the customer list
            _mockCustomerService.Setup(service => service.CustomerSearch(It.IsAny<CustomerSearchWebViewModel>()))
                .ReturnsAsync(customerList);

            // Act: Call the CustomerSelDetail method
            var result = await _controller.CustomerSelDetail(model);

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Ensure the response.data is not null
            Assert.NotNull(response.data);

            // Check if response status and message are correct
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            // Verify the content of the response data
            var responseData = Assert.IsType<List<CustomerCompanyNameWebResponseModel>>(response.data);
            Assert.Single(responseData); // Expecting a single item
            Assert.Equal("John Doe", responseData.First().CompanyName); // Make sure to check the property based on your model
        }

        [Fact]
        public async Task CustomerSelDetail_ReturnsOkWithDataNotFound_WhenNoCustomerExists()
        {
            // Arrange: Create a sample model and setup the mock to return an empty list
            var model = new CustomerIDWebViewModel { CustomerID = 2 };
            var customerList = new List<vwCustomerSearch>();

            _mockCustomerService.Setup(service => service.CustomerSearch(It.IsAny<CustomerSearchWebViewModel>()))
                .ReturnsAsync(customerList);

            // Act: Call the CustomerSelDetail method
            var result = await _controller.CustomerSelDetail(model);

            // Assert: Check that the response indicates no data found
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task AllCustomer_ReturnsOkWithCustomers_WhenCustomersExist()
        {
            // Arrange: Create a sample model and setup the mock to return a list of customers
            var model = new CustomerDynamicSearchWebViewModel(); // Add properties if needed
            var customerList = new List<CustomerDynamicSearchWebResponseModel>
        {
            new CustomerDynamicSearchWebResponseModel
            {
                CustomerID = 1,
                FullName = "John Doe" // Sample customer
            },
            new CustomerDynamicSearchWebResponseModel
            {
                CustomerID = 2,
                FullName = "Jane Smith" // Another sample customer
            }
        };

            // Setup the service to return the customer list
            _mockCustomerService.Setup(service => service.AllCustomers(model))
                .ReturnsAsync(customerList);

            // Act: Call the AllCustomer method
            var result = await _controller.AllCustomer(model);

            // Assert: Check that the response is OkObjectResult with the correct data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Ensure the response data is not null
            Assert.NotNull(response.data);

            // Check if response status and message are correct
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            // Verify the content of the response data
            var responseData = Assert.IsType<List<CustomerDynamicSearchWebResponseModel>>(response.data);
            Assert.Equal(2, responseData.Count); // Expecting 2 items
            Assert.Equal("John Doe", responseData.First().FullName); // Check first customer's full name
        }

        [Fact]
        public async Task AllCustomer_ReturnsOkWithDataNotFound_WhenNoCustomersExist()
        {
            // Arrange: Create a sample model and setup the mock to return an empty list
            var model = new CustomerDynamicSearchWebViewModel(); // Add properties if needed
            var customerList = new List<CustomerDynamicSearchWebResponseModel>(); // Empty list

            _mockCustomerService.Setup(service => service.AllCustomers(model))
                .ReturnsAsync(customerList);

            // Act: Call the AllCustomer method
            var result = await _controller.AllCustomer(model);

            // Assert: Check that the response indicates no data found
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.False(response.status); // Check for false status
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage); // Check the message
            Assert.Equal(response.data, string.Empty); // Ensure data is null
        }
    }
}
