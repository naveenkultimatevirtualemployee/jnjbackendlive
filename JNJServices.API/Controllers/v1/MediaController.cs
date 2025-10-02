using JNJServices.Models.ApiResponseModels;
using JNJServices.Utility.ApiConstants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1
{
    [Route("api")]
    [ApiController]
    public class MediaController : BaseController
    {
        [HttpPost]
        [Route("~/api-M/UploadImage")]
        //[Route("app/media/UploadImage")]
        public async Task<IActionResult> UploadImage([Required] IFormFile? file, [Required] string imageType)
        {
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };

            if (file == null || file.Length == 0)
            {
                var response = new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = ResponseMessage.EMPTY_FILE,
                };
                return BadRequest(response);
            }

            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                var response = new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "Invalid file type. Only JPEG, PNG images are allowed.",
                };
                return BadRequest(response);
            }

            try
            {
                string mediaDirectory;
                switch (imageType.ToUpper())
                {
                    case "DEADMILE":
                        mediaDirectory = DefaultDirectorySettings.MediaDeadMileImages;
                        break;
                    case "FRONTENDTRIP":
                        mediaDirectory = DefaultDirectorySettings.MediaFrontWayTripImage;
                        break;
                    case "BACKENDTRIP":
                        mediaDirectory = DefaultDirectorySettings.MediaBackWayTripImage;
                        break;
                    default:
                        var response = new ResponseModel
                        {
                            status = ResponseStatus.FALSE,
                            statusMessage = "Invalid image type.",
                        };
                        return BadRequest(response);
                }

                // Compress and save the file
                string relativePath = await CompressAndSaveImage(file, mediaDirectory);
                string imageUrl = $"/{relativePath}".Replace("\\", "/");

                var successResponse = new ResponseModel
                {
                    status = ResponseStatus.TRUE,
                    statusMessage = "Image uploaded successfully.",
                    data = new { ImageUrl = imageUrl }
                };

                return Ok(successResponse);
            }
            catch (Exception)
            {
                var errorResponse = new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "Internal server error",
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Route("~/api/webUploadImage")]
        //[Route("web/media/UploadImage")]
        public async Task<IActionResult> WebUploadImage([Required] IFormFile? file)
        {
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "No file uploaded.",
                });
            }

            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "Invalid file type. Only JPEG, PNG, and GIF images are allowed.",
                });
            }

            string imageUrl = await SaveImageAsync(file, DefaultDirectorySettings.SettingImages);

            return Ok(new ResponseModel
            {
                status = ResponseStatus.TRUE,
                statusMessage = "Image uploaded successfully.",
                data = new { ImageUrl = imageUrl }
            });
        }

    }
}
