using JNJServices.API.Controllers.v1;

namespace JNJServices.Tests.Controllers.v1
{
    public class BaseControllerTests
    {
        private readonly BaseController _controller;

        public BaseControllerTests()
        {
            _controller = new BaseController();
        }

        //[Fact]
        //public async Task SaveImageAsync_ShouldSaveImage_WhenFileIsValid()
        //{
        //    // Arrange
        //    var mockFile = new Mock<IFormFile>();
        //    var fileName = "test_image.jpg";
        //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", fileName);
        //    var fileContent = new byte[5 * 1024 * 1024]; // 5 MB file content
        //    mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(fileContent));
        //    mockFile.Setup(f => f.FileName).Returns(fileName);

        //    // Act
        //    var result = await _controller.SaveImageAsync(mockFile.Object, "media");

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Contains(fileName, result); // Check if the returned path contains the file name
        //    Assert.True(File.Exists(result)); // Check if the file exists on disk
        //}

        //[Fact]
        //public async Task SaveImageAsync_ShouldCreateDirectory_WhenDirectoryDoesNotExist()
        //{
        //    // Arrange
        //    var mockFile = new Mock<IFormFile>();
        //    var fileName = "test_image.jpg";
        //    var fileContent = new byte[5 * 1024 * 1024]; // 5 MB file content
        //    mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(fileContent));
        //    mockFile.Setup(f => f.FileName).Returns(fileName);

        //    // Act
        //    var result = await _controller.SaveImageAsync(mockFile.Object, "media");

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "media"))); // Check if directory is created
        //}

        //[Fact]
        //public async Task SaveImageAsync_ShouldThrowArgumentNullException_WhenFileIsNull()
        //{
        //    // Arrange
        //    IFormFile mockFile = null;

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
        //        _controller.SaveImageAsync(mockFile, "media")
        //    );
        //}
    }
}
