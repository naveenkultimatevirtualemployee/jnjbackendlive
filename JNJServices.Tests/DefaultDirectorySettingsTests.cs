using JNJServices.Utility.ApiConstants;

namespace JNJServices.Tests
{
    public class DefaultDirectorySettingsTests
    {
        [Fact]
        public void Root_ShouldReturnCorrectDirectoryPath()
        {
            // Arrange
            var expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "Content");

            // Act
            var result = DefaultDirectorySettings.Root;

            // Assert
            Assert.Equal(expectedPath, result);
        }

        [Fact]
        public void MediaFrontWayTripImage_ShouldReturnCorrectDirectoryPath()
        {
            // Arrange
            var expectedPath = "Content/Images/FrontEndTrip";

            // Act
            var result = DefaultDirectorySettings.MediaFrontWayTripImage;

            // Assert
            Assert.Equal(expectedPath, result);
        }

        [Fact]
        public void MediaBackWayTripImage_ShouldReturnCorrectDirectoryPath()
        {
            // Arrange
            var expectedPath = "Content/Images/BackEndTrip";

            // Act
            var result = DefaultDirectorySettings.MediaBackWayTripImage;

            // Assert
            Assert.Equal(expectedPath, result);
        }

        [Fact]
        public void MediaDeadMileImages_ShouldReturnCorrectDirectoryPath()
        {
            // Arrange
            var expectedPath = "Content/Images/DeadMile";

            // Act
            var result = DefaultDirectorySettings.MediaDeadMileImages;

            // Assert
            Assert.Equal(expectedPath, result);
        }

        [Fact]
        public void SettingImages_ShouldReturnCorrectDirectoryPath()
        {
            // Arrange
            var expectedPath = "Content/Images/Setting";

            // Act
            var result = DefaultDirectorySettings.SettingImages;

            // Assert
            Assert.Equal(expectedPath, result);
        }

        [Fact]
        public void AllDirectoryPaths_ShouldReturnCorrectValues()
        {
            // Act & Assert
            Assert.Equal(Path.Combine(Directory.GetCurrentDirectory(), "Content"), DefaultDirectorySettings.Root);
            Assert.Equal("Content/Images/FrontEndTrip", DefaultDirectorySettings.MediaFrontWayTripImage);
            Assert.Equal("Content/Images/BackEndTrip", DefaultDirectorySettings.MediaBackWayTripImage);
            Assert.Equal("Content/Images/DeadMile", DefaultDirectorySettings.MediaDeadMileImages);
            Assert.Equal("Content/Images/Setting", DefaultDirectorySettings.SettingImages);
        }

        //[Fact]
        //public void RootDirectory_ShouldExist()
        //{
        //    // Act
        //    var rootPath = DefaultDirectorySettings.Root;

        //    // Assert
        //    Assert.False(Directory.Exists(rootPath), $"Directory '{rootPath}' does not exist.");
        //}

        [Fact]
        public void AllDirectoryPaths_ShouldNotBeNull()
        {
            // Act & Assert
            Assert.NotNull(DefaultDirectorySettings.Root);
            Assert.NotNull(DefaultDirectorySettings.MediaFrontWayTripImage);
            Assert.NotNull(DefaultDirectorySettings.MediaBackWayTripImage);
            Assert.NotNull(DefaultDirectorySettings.MediaDeadMileImages);
            Assert.NotNull(DefaultDirectorySettings.SettingImages);
        }

    }
}
