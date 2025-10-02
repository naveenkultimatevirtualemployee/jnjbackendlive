using JNJServices.Models.ApiResponseModels;
using Microsoft.AspNetCore.Diagnostics;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Middleware
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;
        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ResponseModel response = new ResponseModel();
            List<string> Columns = new List<string>();
            switch (exception)
            {
                case BadHttpRequestException:
                    response.status = ResponseStatus.FALSE;
                    response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
                    response.data = Columns;
                    break;
                default:
                    _logger.LogError(exception, "An error occurred: {Message}. This is an Error log.", exception.Message);
                    response.status = ResponseStatus.FALSE;
                    response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
                    response.data = Columns;
                    break;
            }
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
