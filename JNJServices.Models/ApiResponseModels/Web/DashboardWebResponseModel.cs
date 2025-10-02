namespace JNJServices.Models.ApiResponseModels.Web
{
    public class DashboardWebResponseModel
    {
        public DashboardWebResponseModel()
        {
            reservationbyMonths = new DatabyMonths();
            reservationStatus = new DataByStatus();
            //ClaimantByMonths = new DatabyMonths();
            //ContractorStatus = new DataByStatus();
            //ContractorByMonths = new DatabyMonths();
            //ReservationbyService = new DataByStatus();
            //CustomerbyCategory = new DataByStatus();
            //ContractorbyService = new DataByStatus();
            counts = new List<Counts>();
        }
        public List<Counts> counts { get; set; }
        public DatabyMonths reservationbyMonths { get; set; }
        public DataByStatus reservationStatus { get; set; }
        //public DatabyMonths ClaimantByMonths { get; set; }
        //public DataByStatus ContractorStatus { get; set; }
        //public DatabyMonths ContractorByMonths { get; set; }
        //public DataByStatus ReservationbyService { get; set; }
        //public DataByStatus CustomerbyCategory { get; set; }
        //public DataByStatus ContractorbyService { get; set; }
    }

    public class DatabyMonths
    {
        public DatabyMonths()
        {
            name = string.Empty;
            data = new List<GraphByMonth>();
        }

        public string name { get; set; }
        public List<GraphByMonth> data { get; set; }
    }

    public class DataByStatus
    {
        public DataByStatus()
        {
            name = string.Empty;
            data = new List<GraphByStatus>();
        }

        public string name { get; set; }
        public List<GraphByStatus> data { get; set; }
    }


    public class Counts
    {
        public string name { get; set; } = string.Empty;
        public int value { get; set; }
        public string icon { get; set; } = string.Empty;
    }
    public class GraphByMonth
    {
        public string month { get; set; } = string.Empty;
        public int value { get; set; }
    }
    public class GraphByStatus
    {
        public string group { get; set; } = string.Empty;
        public int value { get; set; }

    }
}
