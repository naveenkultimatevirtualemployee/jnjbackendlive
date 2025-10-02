namespace JNJServices.Utility
{
    public static class UserResponses
    {
        public static class ResponseStatus
        {
            public static readonly bool TRUE = true;
            public static readonly bool FALSE = false;

        }

        public static class ResponseMessage
        {
            public static readonly string SUCCESS = "Success";
            public static readonly string ERROR = "Error";
            public static readonly string NOTOKEN = "No Token";
            public static readonly string USER_NOT_FOUND = "User Not Found";
            public static readonly string COMPANY_NOT_FOUND = "Company Not Found";
            public static readonly string INVALID_USER_CREDENTIALS = "Invalid Credentials";
            public static readonly string DATA_FETCHED_SUCCESSFULLY = "Data Fetched Successfully";
            public static readonly string DATA_NOT_FOUND = "Data Not Found";
            public static readonly string CONTRACTOR_NOT_FOUND = "Contractor Not Found";
            public static readonly string PLEASE_PROVIDE_PROPER_INPUT = "Please Provide Proper Input";
            public static readonly string FORGOT_PASSWORD_MESSAGES_ARE_SENT = "If there exists an account for the supplied email address, you will be receiving a password reset link shortly.";
            public static readonly string RESETTING_PASSWORD_USER_NOT_FOUND = "Resetting password user not found";
            public static readonly string Security_Code_Expired = "Invalid security code or expired !!";
            public static readonly string RESETTING_PASSWORD_ERRORS = "Password not updated";
            public static readonly string RESETTING_PASSWORD_SUCCEEDED = "Resetting password: succeeded";
            public static readonly string EMPTY_FILE = "File cannot be empty";
            public static readonly string EMPTY_FILENAME = "FileName cannot be empty.";
            public static readonly string INVALID_INPUT_PARAMS = "Invalid input parameters";
            public static readonly string USER_LOGGED_OFF = "User Logged off";
            public static readonly string PROMPT_NOT_FOUND = "prompt not found";
            public static readonly string INTERNALSERVERERROR = "Internal Server Error";
            public static readonly string UNAUTHORIZE = "Unauthorize";
            public static readonly string EMAILSEND = "Email Send";
            public static readonly string EMAILNOTFOUND = "No emailID found";
            public static readonly string EMAILNOTSEND = "Email Not Send";
            public static readonly string NCNOTMATCHED = "New Password and Confirm Password not Match";
            public static readonly string TLREACHED = "Link is Expired Please Resend Email Again";
            public static readonly string USER_NOT_UPDATED = "User Password Not Updated";
            public static readonly string INVALID_USER_PHONE = "Invalid User Phone no.";
            public static readonly string VALID_USER_PHONE = "Valid User Phone no";
            public static readonly string INVALIDTOKEN = "Invalid Token";
            public static readonly string DATA_NOT_INSERTED = "Data Not Inserted";
            public static readonly string DATA_NOT_DELETED = "Data Not Deleted";
            public static readonly string NOTIFICATION_DELETED = "Notification Deleted";
            public static readonly string CONTRACTOR_NOT_AVAILABLE = "No contractor found for this assignment service";
            public static readonly string PASSWORD_SAME_AS_OLD = "The new password cannot be the same as the old password.";
            public static readonly string LOGOUT_SUCCESSFUL = "Logout Successfull";
            public static readonly string INSERT_ASSIGNMENT_METRICS_SUCCESS = "Insert Successfull";
            public static readonly string NOTUPDATED = "Data Not Updated";
            public static readonly string PASSWORDNOTFOUND = "Password cannot be empty";
            public static readonly string USER_ROLE_NOT_DEFINED = "Your role is not authorized to access the system.";
            public static readonly string USER_INVALID_CREDENTIALS = "Invalid credentials, please try again.";
            public static readonly string USER_INACTIVE = "Your account is inactive.";
        }
        public static class UserRole
        {
            public const string ADMIN = "Administrators";
            public const string BILLING = "Billing";
        }
    }
}
