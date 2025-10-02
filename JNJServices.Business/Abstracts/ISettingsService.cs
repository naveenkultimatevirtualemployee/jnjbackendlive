using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
	public interface ISettingsService
	{
		Task<IEnumerable<Settings>> SettingsAsync();
		Task<(int responseCode, string message)> UpdateSettingsAsync(List<SettingWebViewModel> settings);
		Task<SettingValueResponseModel> GetSettingByKeyAsync(SettingKeyViewModel model);
	}
}
