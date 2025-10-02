using JNJServices.API.Controllers.v1;
using JNJServices.Models.ApiResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1
{
    public class MediaControllerTests
    {
        private readonly MediaController _controller;
        private readonly Mock<ILogger<MediaController>> _mockLogger;
        string validImagePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("bin\\Debug\\net8.0", string.Empty)) + @"\Screenshot 2024-07-27 151918.jpg";

        public MediaControllerTests()
        {
            _mockLogger = new Mock<ILogger<MediaController>>();
            _controller = new MediaController();
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenFileIsNull()
        {
            // Arrange
            IFormFile? file = null;
            string imageType = "DEADMILE";

            // Act
            var result = await _controller.UploadImage(file, imageType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.EMPTY_FILE, response.statusMessage);
        }

        //[Fact]
        //public async Task UploadImage_ShouldReturnSuccess_WhenValidImageIsUploaded()
        //{
        //    // Arrange            
        //    var validImageContent = System.IO.File.ReadAllBytes(validImagePath);  // Read actual image content

        //    var fileMock = new Mock<IFormFile>();
        //    var fileStream = new System.IO.MemoryStream(validImageContent);

        //    fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        //    fileMock.Setup(f => f.Length).Returns(validImageContent.Length);
        //    fileMock.Setup(f => f.FileName).Returns("Screenshot 2024-07-27 151918.jpg");
        //    fileMock.Setup(f => f.ContentType).Returns("image/jpeg");  // Setting a mock ContentType

        //    string imageType = "DEADMILE";

        //    // Act
        //    var result = await _controller.UploadImage(fileMock.Object, imageType);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(ResponseStatus.TRUE, response.status);
        //    Assert.Equal("Image uploaded successfully.", response.statusMessage);  // Assuming this is the success message
        //}

        //[Fact]
        //public async Task UploadImage_ShouldReturnSuccess_WhenValidImageIsUploadedFRONTENDTRIP()
        //{
        //    // Arrange

        //    //var sourceImagePath = @"\JNJServices.Tests\Screenshot 2024-07-27 151918.jpg";

        //    var validImageContent = System.IO.File.ReadAllBytes(validImagePath);  // Read actual image content

        //    var fileMock = new Mock<IFormFile>();
        //    var fileStream = new System.IO.MemoryStream(validImageContent);

        //    fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        //    fileMock.Setup(f => f.Length).Returns(validImageContent.Length);
        //    fileMock.Setup(f => f.FileName).Returns("Screenshot 2024-07-27 151918.jpg");
        //    fileMock.Setup(f => f.ContentType).Returns("image/jpeg");  // Setting a mock ContentType

        //    string imageType = "FRONTENDTRIP";

        //    // Act
        //    var result = await _controller.UploadImage(fileMock.Object, imageType);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(ResponseStatus.TRUE, response.status);
        //    Assert.Equal("Image uploaded successfully.", response.statusMessage);  // Assuming this is the success message
        //}

        //[Fact]
        //public async Task UploadImage_ShouldReturnSuccess_WhenValidImageIsUploaded_BACKENDTRIP()
        //{
        //    // Arrange
        //    //var validImagePath = @"\JNJServices.Tests\Screenshot 2024-07-27 151918.jpg";
        //    var validImageContent = System.IO.File.ReadAllBytes(validImagePath);  // Read actual image content

        //    var fileMock = new Mock<IFormFile>();
        //    var fileStream = new System.IO.MemoryStream(validImageContent);

        //    fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        //    fileMock.Setup(f => f.Length).Returns(validImageContent.Length);
        //    fileMock.Setup(f => f.FileName).Returns("Screenshot 2024-07-27 151918.jpg");
        //    fileMock.Setup(f => f.ContentType).Returns("image/jpeg");  // Setting a mock ContentType

        //    string imageType = "BACKENDTRIP";

        //    // Act
        //    var result = await _controller.UploadImage(fileMock.Object, imageType);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(ResponseStatus.TRUE, response.status);
        //    Assert.Equal("Image uploaded successfully.", response.statusMessage);  // Assuming this is the success message
        //}

        //[Fact]
        //public async Task UploadImage_ShouldReturnSuccess_WhenValidImageIsUploaded_imageTypeNull()
        //{
        //    // Arrange
        //    //var validImagePath = @"\JNJServices.Tests\Screenshot 2024-07-27 151918.jpg";
        //    var validImageContent = System.IO.File.ReadAllBytes(validImagePath);  // Read actual image content

        //    var fileMock = new Mock<IFormFile>();
        //    var fileStream = new System.IO.MemoryStream(validImageContent);

        //    fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        //    fileMock.Setup(f => f.Length).Returns(validImageContent.Length);
        //    fileMock.Setup(f => f.FileName).Returns("Screenshot 2024-07-27 151918.jpg");
        //    fileMock.Setup(f => f.ContentType).Returns("image/jpeg");  // Setting a mock ContentType

        //    string imageType = "abc";

        //    // Act
        //    var result = await _controller.UploadImage(fileMock.Object, imageType);

        //    // Assert
        //    var okResult = Assert.IsType<BadRequestObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(ResponseStatus.FALSE, response.status);
        //    Assert.Equal("Invalid image type.", response.statusMessage);  // Assuming this is the success message
        //}

        //[Fact]
        //public async Task UploadImage_ShouldReturnSuccess_WhenValidImageIsUploaded_contenttypeinvalid()
        //{
        //    // Arrange
        //    //var validImagePath = @"\JNJServices.Tests\200w.gif";
        //    var validImageContent = System.IO.File.ReadAllBytes(validImagePath);  // Read actual image content

        //    var fileMock = new Mock<IFormFile>();
        //    var fileStream = new System.IO.MemoryStream(validImageContent);

        //    fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        //    fileMock.Setup(f => f.Length).Returns(validImageContent.Length);
        //    fileMock.Setup(f => f.FileName).Returns("200w.gif");
        //    fileMock.Setup(f => f.ContentType).Returns("image/gif");  // Setting a mock ContentType

        //    string imageType = "abc";

        //    // Act
        //    var result = await _controller.UploadImage(fileMock.Object, imageType);

        //    // Assert
        //    var okResult = Assert.IsType<BadRequestObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(ResponseStatus.FALSE, response.status);
        //    Assert.Equal("Invalid file type. Only JPEG, PNG images are allowed.", response.statusMessage);  // Assuming this is the success message
        //}

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenFileTypeIsInvalid()
        {
            // Arrange
            var file = new FormFile(new System.IO.MemoryStream(), 0, 0, "file", "image.txt");
            file.Headers = new HeaderDictionary
            {
                { "Content-Type", "text/plain" }  // Simulate the invalid content type here
            };
            string imageType = "DEADMILE";

            // Act
            var result = await _controller.UploadImage(file, imageType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var validImageContent = new byte[] { 1, 2, 3, 4 };  // Fake image content
            var validFileStream = new System.IO.MemoryStream(validImageContent);
            fileMock.Setup(f => f.OpenReadStream()).Returns(validFileStream);
            fileMock.Setup(f => f.Length).Returns(validImageContent.Length);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.FileName).Returns("validimage.jpg");

            string imageType = "DEADMILE";

            // Simulate an exception in the method CompressAndSaveImage by throwing an exception in the controller
            _controller.ControllerContext = new ControllerContext();
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var mockLogger = new Mock<ILogger<MediaController>>();
            // _controller.Logger = mockLogger.Object;

            // Act
            var result = await _controller.UploadImage(fileMock.Object, imageType);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<ResponseModel>(statusCodeResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Internal server error", response.statusMessage);
        }

        [Fact]
        public async Task WebUploadImage_ShouldReturnBadRequest_WhenFileIsNull()
        {
            // Arrange
            IFormFile? file = null;

            // Act
            var result = await _controller.WebUploadImage(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("No file uploaded.", response.statusMessage);
        }

        [Fact]
        public async Task WebUploadImage_ShouldReturnBadRequest_WhenFileTypeIsInvalid()
        {
            // Arrange
            var file = new FormFile(new System.IO.MemoryStream(), 0, 0, "file", "image.txt")
            {
                // FormFile does not expose ContentType, so simulate the ContentType via headers
                Headers = new HeaderDictionary
                {
                    { "Content-Type", "text/plain" }  // Simulate an invalid content type for the file
                }
            };

            // Act
            var result = await _controller.WebUploadImage(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenFileIsNotMatched()
        {
            // Arrange
            IFormFile file = new FormFile(new System.IO.MemoryStream(), 0, 0, "file", "image.png");
            string imageType = "DEADMILE";

            // Act
            var result = await _controller.UploadImage(file, imageType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.EMPTY_FILE, response.statusMessage);
        }

        [Fact]
        public async Task WebUploadImage_NullFile_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.WebUploadImage(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("No file uploaded.", response.statusMessage);
        }

        [Fact]
        public async Task WebUploadImage_ShouldReturnBadRequest_WhenFileIsNullOrEmpty()
        {
            // Arrange
            IFormFile? file = null;

            // Act
            var result = await _controller.WebUploadImage(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("No file uploaded.", response.statusMessage);
        }

        [Fact]
        public async Task WebUploadImage_ShouldReturnBadRequest_WhenInvalidFileType()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var invalidFileStream = new System.IO.MemoryStream(new byte[10]);  // Fake file with 10 bytes
            fileMock.Setup(f => f.OpenReadStream()).Returns(invalidFileStream);
            fileMock.Setup(f => f.Length).Returns(10);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");  // Invalid file type
            fileMock.Setup(f => f.FileName).Returns("test.pdf");

            // Act
            var result = await _controller.WebUploadImage(fileMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Invalid file type. Only JPEG, PNG, and GIF images are allowed.", response.statusMessage);
        }

        //[Fact]
        //public async Task WebUploadImage_ShouldReturnSuccess_WhenValidImageIsUploaded()
        //{
        //    // Arrange
        //    //var validImagePath = @"\JNJServices.Tests\Screenshot 2024-07-27 151918.jpg";
        //    var validImageContent = System.IO.File.ReadAllBytes(validImagePath);  // Read actual image content
        //    var fileStream = new System.IO.MemoryStream(validImageContent);

        //    var fileMock = new Mock<IFormFile>();
        //    fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        //    fileMock.Setup(f => f.Length).Returns(validImageContent.Length);
        //    fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        //    fileMock.Setup(f => f.FileName).Returns("validimage.jpg");

        //    // Act
        //    var result = await _controller.WebUploadImage(fileMock.Object);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(ResponseStatus.TRUE, response.status);
        //    Assert.Equal("Image uploaded successfully.", response.statusMessage);
        //    Assert.NotNull(response.data);
        //    Assert.Contains("ImageUrl", response.data.ToString());  // Check if ImageUrl is included in response
        //}

        //[Fact]
        //public async Task WebUploadImage_ShouldReturnSuccess_WhenValidGifImageIsUploaded()
        //{
        //    // Arrange
        //    var validGifPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("bin\\Debug\\net8.0", string.Empty)) + @"\200w.gif";
        //    var validGifContent = System.IO.File.ReadAllBytes(validGifPath);  // Read actual GIF image content
        //    var fileStream = new System.IO.MemoryStream(validGifContent);

        //    var fileMock = new Mock<IFormFile>();
        //    fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        //    fileMock.Setup(f => f.Length).Returns(validGifContent.Length);
        //    fileMock.Setup(f => f.ContentType).Returns("image/gif");
        //    fileMock.Setup(f => f.FileName).Returns("200w.gif");

        //    // Act
        //    var result = await _controller.WebUploadImage(fileMock.Object);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(ResponseStatus.TRUE, response.status);
        //    Assert.Equal("Image uploaded successfully.", response.statusMessage);
        //    Assert.NotNull(response.data);
        //    Assert.Contains("ImageUrl", response.data.ToString());  // Check if ImageUrl is included in response
        //}

    }
}
