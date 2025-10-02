using JNJServices.Models.Entities;

namespace JNJServices.Models.ApiResponseModels.App
{
    public class LiveTrackingMapResponseModel
    {
        public LiveTrackingMapResponseModel()
        {
            GooglePath = new List<Coordinate>();
            LatitudeLongitude = new List<Coordinate>();
        }

        public int ReservationsAssignmentsID { get; set; }
        public List<Coordinate> GooglePath { get; set; }
        public List<Coordinate> LatitudeLongitude { get; set; }
    }
}
