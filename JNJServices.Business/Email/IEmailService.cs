using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;

namespace JNJServices.Business.Email
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(string toEmail, string subject, string message);
        Task<EmailWebResponseModel> RaiseTicketApp(RaiseTicketsViewModel email, AppTokenDetails appToken);
        Task<EmailWebResponseModel> SendPasswordResetEmailWeb(vwDBUsers model);
          Task<EmailWebResponseModel> PreferredContractorNotMatchedMail(int ReservationID, string AssgnNum);
        Task<EmailWebResponseModel> PreferredContractorMatchedNotAssigned( string AssgnNum);
    }
}
