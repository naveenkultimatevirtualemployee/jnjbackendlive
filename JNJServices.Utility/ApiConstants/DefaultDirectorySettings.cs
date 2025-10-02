namespace JNJServices.Utility.ApiConstants
{
    public static class DefaultDirectorySettings
    {
        public static string Root => Path.Combine(Directory.GetCurrentDirectory(), "Content");
        public static string MediaFrontWayTripImage => "Content/Images/FrontEndTrip";
        public static string MediaBackWayTripImage => "Content/Images/BackEndTrip";
        public static string MediaDeadMileImages => "Content/Images/DeadMile";
        public static string MediaContractorProfileImages => "Content/Images/ContractorMedia";
        public static string MediaAssignmentMetricsDocuments => "Content/AssignmentsMetricsDocument";
        public static string SettingImages => "Content/Images/Setting";
    }
}
