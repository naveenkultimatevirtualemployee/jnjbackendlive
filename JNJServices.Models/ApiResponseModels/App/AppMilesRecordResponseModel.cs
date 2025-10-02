using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JNJServices.Models.ApiResponseModels.App
{
    public class AppMilesRecordResponseModel
    {
        public string TotalDeadMiles { get; set; } = string.Empty;
        public string TotalTravellingMiles { get; set; } = string.Empty;
        public string TotalMiles { get; set; } = string.Empty;
    }
}
