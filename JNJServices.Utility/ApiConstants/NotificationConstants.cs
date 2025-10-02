namespace JNJServices.Utility.ApiConstants
{
    public static class NotificationConstants
    {
        public static class ButtonStatus
        {
            public const string ACCEPT = "accept";
            public const string CANCEL = "cancel";
            public const string START = "START";
            public const string REACHED = "REACHED";
            public const string START_TRIP = "START_TRIP";
            public const string S_ROUND_TRIP = "S_ROUND_TRIP";
            public const string E_ROUND_TRIP = "E_ROUND_TRIP";
            public const string HALT = "HALT";
            public const string START_SESSION = "START_SESSION";
            public const string END_SESSION = "END_SESSION";
            public const string END = "END";
            public const string FORCED = "Forced";
            public const string REQUEST = "Request";
            public const string DEFAULT = "Default";
        }

        public static class NotificationType
        {
            public const string ASSIGNMENT_NEED_ATTENTION = "ASSIGNMENT_NEED_ATTENTION";
            public const string NEW_ASSIGNMENT_ASSIGNED = "NEW_ASSIGNMENT_ASSIGNED";
            public const string NEW_ASSIGNMENT_REQUEST = "NEW_ASSIGNMENT_REQUEST";
            public const string ASSIGNMENT_CANCELLED = "ASSIGNMENT_CANCELLED";
            public const string RESERVATION_CANCELLED = "RESERVATION_CANCELLED";
            public const string RESERVATION_INTERPRETATION_UPDATE = "RESERVATION_INTERPRETATION_UPDATE";
            public const string RESERVATION_UPDATE = "RESERVATION_UPDATE";
            public const string DRIVER_IS_WAITING = "DRIVER_IS_WAITING";
            public const string RESERVATION_COMPLETED = "RESERVATION_COMPLETED";
            public const string TI_INTREPRETATION = "TI_INTREPRETATION";
            public const string CONTRACTOR_NOT_ASSIGN_TRIGGER = "CONTRACTOR_NOT_ASSIGN_TRIGGER";
            public const string NEW_ASSIGNMENT_REQUEST_REMINDER = "NEW_ASSIGNMENT_REQUEST_REMINDER";
            public const string NEW_ASSIGNMENT_REQUEST_WITHDRAWN = "NEW_ASSIGNMENT_REQUEST_WITHDRAWN";
            public const string CONTRACTOR_TODAY_ASSIGNMENT = "CONTRACTOR_TODAY_ASSIGNMENT";
            public const string CONTRACTOR_TOMMOROW_ASSIGNMENT = "CONTRACTOR_TOMMOROW_ASSIGNMENT";
            public const string CHAT = "CHAT";
            public const string PREFERRED_CONTRACTOR_NOTFOUND = "PREFERRED_CONTRACTOR_NOTFOUND";
            public const string PREFERRED_CONTRACTOR_FOUND_NOTASSIGNED = "PREFERRED_CONTRACTOR_FOUND_NOTASSIGNED";
        }

        public static class AssignmentType
        {
            public const string PHINTERPRET = "PHINTERPRET";
            public const string INTERPRET = "INTERPRET";
            public const string TRANSLATE = "TRANSLATE";
            public const string HOMEHEALTH = "HOMEHEALTH";
            public const string TRANSINTERP = "TRANSINTERP";
            public const string TRANSPORT = "TRANSPORT";
            public const string DME = "DME";
        }

        public static class NotificationTitle
        {
            public const string ASSIGNMENT_NEED_ATTENTION = "Assignment need attention";
            public const string ASSIGNMENT_CANCELLED = "Assignment Cancelled";
            public const string RESERVATION_UPDATE = "Reservation Update";
            public const string RESERVATION_COMPLETED = "Reservation Completed";
            public const string DRIVER_IS_WAITING = "Driver is waiting";
            public const string URGENT_ATTENTION_REQUIRED = "Urgent attention required";
            public const string NEW_ASSIGNMENT_REQUEST = "New Assignment Request";
            public const string ASSIGNMENT_REQUEST_REMINDER = "Assignment Request Reminder";
            public const string ASSIGNMENT_WITHDRAWN = "Assignment Withdrawn";
            public const string NEW_ASSIGNMENT_ASSIGNED = "New Assignment Assigned";
            public const string UPCOMING_ASSIGNMENT = "Upcoming Assignment";
            public const string PREFERRED_CONTRACTOR_NOTFOUND = "Preferred Contractor Not Matched";
            public const string PREFERRED_CONTRACTOR_FOUND_NOTASSIGNED = "Preferred Contractor Found but not Assigned";
        }
        public static class NotificationData
        {
            public const string TITLE = "title";
            public const string BODY = "body";
            public const string NOTIFICATION_DATE = "notificationDate";
            public const string RESERVATIONSASSIGNMENTSID = "reservationsAssignmentsID";
            public const string NOTIFICATION_TYPE = "notificationType";
            public const string RESERVATION_DATE = "reservationDate";
            public const string RESERVATION_TIME = "reservationTime";
            public const string RESERVATIONID = "reservationID";
            public const string CURRENTBUTTONID = "currentButtonID";
            public const string BUTTONSTATUS = "buttonStatus";
        }


    }
}
