using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Models.CommonModels;
using JNJServices.Models.DbResponseModels;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using static JNJServices.Utility.ApiConstants.NotificationConstants;

namespace JNJServices.API.Helper
{
    public class NotificationHelper : INotificationHelper
    {
        private readonly TimeZoneConverter _timeZoneConverter;
        private readonly FirebaseSettings _settings;
        private readonly ILogger<NotificationHelper> _logger;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public static DateTime MinSqlDate => Convert.ToDateTime("1/1/1753");
        public NotificationHelper(TimeZoneConverter timeZoneConverter, IOptions<FirebaseSettings> settings, ILogger<NotificationHelper> logger, INotificationService notificationService, IEmailService emailService)
        {
            _timeZoneConverter = timeZoneConverter;
            _settings = settings.Value;
            _logger = logger;
            _notificationService = notificationService;
            _emailService = emailService;
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(_settings.FcmCredentialsFilePath)
                });
            }
        }

        //Send Claimant Accept and Cancel and contractor first time  cancel Notification Using This function
        public async Task SendNotificationClaimantContractorAccept(ReservationNotificationAppViewModel notificationResponse)
        {
            IEnumerable<ContractorFirstAcceptCancelResponseModel> claimantAccepts = Enumerable.Empty<ContractorFirstAcceptCancelResponseModel>();

            if (notificationResponse.ButtonStatus == ButtonStatus.ACCEPT)
            {
                if (notificationResponse.Type == 2)
                {
                    claimantAccepts = await _notificationService.ClaimantAcceptCancel(notificationResponse.ReservationID ?? 0);
                    if (!claimantAccepts.Any())
                    {
                        claimantAccepts = await _notificationService.ClaimantAcceptContractornotfoundLogic(notificationResponse.ReservationID ?? 0);
                    }
                }
                else if (notificationResponse.Type == 1)
                {
                    claimantAccepts = await _notificationService.ContractorAcceptLogic(notificationResponse.ReservationAssignmentID ?? 0);
                }
            }
            else if (notificationResponse.Type == 1 && notificationResponse.ButtonStatus == ButtonStatus.CANCEL)
            {
                claimantAccepts = await _notificationService.ContractorFirstCancelLogic(notificationResponse.ReservationAssignmentID ?? 0);
                if (!claimantAccepts.Any())
                {
                    claimantAccepts = await _notificationService.ContractorFirstCancelDataNotFound(notificationResponse.ReservationAssignmentID ?? 0);
                }
            }

            foreach (var item in claimantAccepts)
            {
                InsertNotificationLog notificationLog = new InsertNotificationLog
                {
                    UserID = item.ContractorID,
                    UserType = 1,
                    CreatedBy = (notificationResponse.Type == 2) ? item.ClaimantID : item.ContractorID,
                    NotificationDateTime = item.NotificationDateTime <= MinSqlDate
                        ? _timeZoneConverter.ConvertUtcToConfiguredTimeZone()
                        : item.NotificationDateTime,
                    ReservationsAssignmentsID = item.ReservationsAssignmentsID
                };

                List<string> tokens = new List<string>();
                if (notificationResponse.Type == 1 && notificationResponse.ButtonStatus == ButtonStatus.CANCEL)
                {
                    if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                    {
                        tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
                        notificationLog.webFcmToken = string.Join(",", tokens);
                    }
                }
                else
                {
                    tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
                    notificationLog.webFcmToken = string.Join(",", tokens);
                }

                notificationLog.webFcmToken = string.Join(",", tokens);

                // Prepare the Body and Title based on ASSGNCode
                if (item.ASSGNCode == AssignmentType.INTERPRET || item.ASSGNCode == AssignmentType.PHINTERPRET || item.ASSGNCode == AssignmentType.TRANSLATE)
                {
                    notificationLog.Body = $"{item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, from {item.PUAddress1} {item.PUAddress2} to {item.DOAddress1} {item.DOAddress2}";
                }
                else if (item.ASSGNCode == AssignmentType.TRANSINTERP)
                {
                    notificationLog.Body = $"Pick-up & drop-off ({item.ResTripType}) with {item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                }
                else if (item.ASSGNCode == AssignmentType.TRANSPORT)
                {
                    notificationLog.Body = $"Pick-up & drop-off ({item.ResTripType}) {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                }
                else
                {
                    notificationLog.Body = $"{item.ResAsgnCode} ({item.ResTripType}) {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                }

                // Set NotificationType and Title
                if (notificationResponse.Type == 2 && notificationResponse.ButtonStatus == ButtonStatus.ACCEPT)
                {
                    if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                    {
                        notificationLog.Title = NotificationTitle.ASSIGNMENT_NEED_ATTENTION;
                        notificationLog.Body = $"Please pay attention to the assignment ID:  {item.AssgnNum} dated {item.ReservationDate:dd MMMM}, contractor not found for this assignment!!!";
                        notificationLog.NotificationType = item.NotificationType;
                    }
                }
                else if (notificationResponse.Type == 1)
                {
                    notificationLog.Title = notificationResponse.NotificationTitle ?? string.Empty;
                    notificationLog.NotificationType = notificationResponse.ButtonStatus == ButtonStatus.ACCEPT
                        ? NotificationType.NEW_ASSIGNMENT_ASSIGNED
                        : NotificationType.NEW_ASSIGNMENT_REQUEST;
                }

                // Prepare data for notifications
                var data = new Dictionary<string, string>
                {
                    { NotificationData.TITLE, notificationLog.Title ?? "" },
                    { NotificationData.BODY, notificationLog.Body ?? "" },
                    { NotificationData.NOTIFICATION_DATE, notificationLog.NotificationDateTime.ToString("o") },
                    { NotificationData.RESERVATIONSASSIGNMENTSID, notificationLog.ReservationsAssignmentsID.ToString() },
                    { NotificationData.NOTIFICATION_TYPE, notificationLog.NotificationType.ToString() },
                    { NotificationData.RESERVATION_DATE, item.ReservationDate.ToString("o") },
                    { NotificationData.RESERVATION_TIME, item.ReservationTime.ToString("o") }
                };

                notificationLog.data = data;
                notificationLog.FcmToken = item.FcmToken;

                // Create and send notifications
                var notification = new AppNotificationModel
                {
                    FcmToken = notificationLog.FcmToken,
                    Title = notificationLog.Title ?? string.Empty,
                    Body = notificationLog.Body ?? string.Empty,
                    data = data
                };

                await SendAppPushNotifications(notification);

                var webNotification = new WebNotificationModel
                {
                    FcmToken = tokens,
                    data = data
                };

                await SendWebPushNotifications(webNotification);

                // Insert notification log
                if (notificationResponse.Type == 1 && notificationResponse.ButtonStatus == ButtonStatus.CANCEL && item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                {
                    await _notificationService.InsertNotificationLogWeb(notificationLog);
                }
                else
                {
                    if (item.NotificationType != NotificationType.ASSIGNMENT_NEED_ATTENTION)
                    {
                        await _notificationService.InsertNotificationLog(notificationLog);
                    }
                    else
                    {
                        await _notificationService.InsertNotificationLogWeb(notificationLog);
                    }
                }
            }
        }

        //Notification if Contractor Cancel it
        public async Task SendNotificationContractorCancel(ReservationNotificationAppViewModel model)
        {
            IEnumerable<ContractorFirstAcceptCancelResponseModel> claimantAccepts = new List<ContractorFirstAcceptCancelResponseModel>();

            claimantAccepts = await _notificationService.ContractorCancelLogic(model.ReservationAssignmentID.HasValue ? model.ReservationAssignmentID.Value : 0);

            foreach (var item in claimantAccepts)
            {
                InsertNotificationLog insertNotification = new InsertNotificationLog();

                var tokens = await _notificationService.GetDistinctUserFcmTokensAsync();

                insertNotification.Body = $"Assignment for pick-up and drop-off {item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt},  has been cancelled.";
                insertNotification.UserID = item.ContractorID;
                insertNotification.UserType = 1;
                insertNotification.CreatedBy = item.ContractorID;

                insertNotification.Title = model.NotificationTitle ?? NotificationTitle.ASSIGNMENT_CANCELLED;
                if (_timeZoneConverter.ConvertUtcToConfiguredTimeZone().ToString("dd/mm/yyyy") == (item.ReservationDate).AddDays(-1).ToString("dd/mm/yyyy"))
                {
                    insertNotification.Body = $"Assignment for pick-up & drop-off {item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt},  has been cancelled one day before reservation Date by contractor.";
                    insertNotification.NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION;
                }
                else
                {
                    insertNotification.NotificationType = NotificationType.ASSIGNMENT_CANCELLED;
                }


                insertNotification.NotificationDateTime = item.NotificationDateTime;
                if (insertNotification.NotificationDateTime <= MinSqlDate)
                {
                    insertNotification.NotificationDateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();
                }

                insertNotification.ReservationsAssignmentsID = item.ReservationsAssignmentsID;
                var data = new Dictionary<string, string>
                {
                    { NotificationData.TITLE,insertNotification.Title },
                    { NotificationData.BODY, insertNotification.Body },
                    { NotificationData.NOTIFICATION_DATE, item.NotificationDateTime.ToString("o") },
                    { NotificationData.RESERVATIONSASSIGNMENTSID, item.ReservationsAssignmentsID.ToString() },
                    { NotificationData.NOTIFICATION_TYPE, insertNotification.NotificationType }
                };
                insertNotification.data = data;

                AppNotificationModel notification = new AppNotificationModel();
                notification.FcmToken = item.FcmToken;
                notification.Title = insertNotification.Title;
                notification.Body = insertNotification.Body;
                notification.data = data;
                await SendAppPushNotifications(notification);

                WebNotificationModel webNotification = new WebNotificationModel();
                webNotification.FcmToken = tokens;
                webNotification.data = data;
                await SendWebPushNotifications(webNotification);

                await _notificationService.InsertNotificationLog(insertNotification);
                await _notificationService.InsertNotificationLogWeb(insertNotification);
            }
        }

        //Notification if Claimants Cancel
        public async Task SendNotificationClaimantCancel(ReservationNotificationAppViewModel notificationResponse)
        {
            var claimantCancel = await _notificationService.ClaimantCancelLogic(notificationResponse.ReservationID ?? 0);

            if (!claimantCancel.Any())
            {
                claimantCancel = await _notificationService.ClaimantCancelNodataFound(notificationResponse.ReservationID ?? 0);
            }
            var webFcmToken = string.Empty;
            var tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
            if (tokens.Count == 0)
                webFcmToken = string.Join(",", tokens);

            foreach (var item in claimantCancel)
            {
                var insertNotification = new InsertNotificationLog
                {
                    webFcmToken = webFcmToken,
                    UserID = 0,
                    CreatedBy = item.ClaimantID,
                    UserType = 1,
                    Title = notificationResponse.NotificationTitle ?? NotificationTitle.ASSIGNMENT_CANCELLED,
                    NotificationType = NotificationType.RESERVATION_CANCELLED,
                    NotificationDateTime = item.NotificationDateTime <= MinSqlDate
                        ? _timeZoneConverter.ConvertUtcToConfiguredTimeZone()
                        : item.NotificationDateTime,
                    ReservationsAssignmentsID = item.ReservationsAssignmentsID
                };

                // Set body for the web notification
                insertNotification.Body = $"Reservation visit to {item.DOAddress1} {item.DOAddress2} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, has been cancelled.";

                // Prepare data for notification
                var data = new Dictionary<string, string>
        {
            { NotificationData.TITLE, insertNotification.Title },
            { NotificationData.BODY, insertNotification.Body },
            { NotificationData.NOTIFICATION_DATE, insertNotification.NotificationDateTime.ToString("o") },
            { NotificationData.RESERVATIONSASSIGNMENTSID, insertNotification.ReservationsAssignmentsID.ToString() },
            { NotificationData.NOTIFICATION_TYPE, insertNotification.NotificationType.ToString() }
        };
                insertNotification.data = data;

                // Send web notification
                if (!string.IsNullOrEmpty(webFcmToken))
                {
                    var webNotification = new WebNotificationModel
                    {
                        FcmToken = tokens,
                        data = data
                    };
                    await SendWebPushNotifications(webNotification);
                    await _notificationService.InsertNotificationLogWeb(insertNotification);
                }

                // Send app notification to contractor if FcmToken is available
                if (!string.IsNullOrEmpty(item.FcmToken))
                {
                    insertNotification.UserID = item.ContractorID ?? 0; // Update UserID for contractor
                    insertNotification.Body = $"Assignment visit to {item.DOAddress1} {item.DOAddress2} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, has been cancelled by Claimant.";

                    // Prepare notification data for app notification
                    var appNotification = new AppNotificationModel
                    {
                        FcmToken = item.FcmToken,
                        Title = insertNotification.Title,
                        Body = insertNotification.Body,
                        data = data
                    };
                    await SendAppPushNotifications(appNotification);
                    await _notificationService.InsertNotificationLog(insertNotification);
                }
            }
        }

        //Notification Driver Traking to claimants
        public async Task SendNotificationDriverTraking(ClaimantTrakingAppViewModel notificationResponse)
        {
            if (string.IsNullOrEmpty(notificationResponse.ButtonStatus))
            {
                return;
            }

            var claimantAccepts = await _notificationService.ClaimantTraking(notificationResponse.ReservationAssignmentID ?? 0);

            foreach (var item in claimantAccepts)
            {
                var insertNotification = new InsertNotificationLog
                {
                    UserID = item.ClaimantID,
                    UserType = 2,
                    CreatedBy = item.ContractorID ?? 0,
                    NotificationDateTime = item.NotificationDateTime <= MinSqlDate
                        ? _timeZoneConverter.ConvertUtcToConfiguredTimeZone()
                        : item.NotificationDateTime,
                    ReservationsAssignmentsID = item.ReservationsAssignmentsID
                };

                string assignCode = item.ASSGNCode switch
                {
                    AssignmentType.INTERPRET => "Interpreter",
                    AssignmentType.PHINTERPRET => "Phone Interpreter",
                    AssignmentType.TRANSLATE => "Translator",
                    AssignmentType.HOMEHEALTH => "Attendant",
                    AssignmentType.DME => "Medical equipments with",
                    _ => string.Empty
                };

                // Convert ButtonStatus to uppercase once for efficiency
                string status = notificationResponse.ButtonStatus.ToUpper();

                if (item.ASSGNCode is AssignmentType.INTERPRET or AssignmentType.PHINTERPRET or AssignmentType.TRANSLATE or AssignmentType.HOMEHEALTH or AssignmentType.DME)
                {
                    if (string.Equals(status, ButtonStatus.START_SESSION, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(status, ButtonStatus.END_SESSION, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    insertNotification.Title = NotificationTitle.RESERVATION_UPDATE;
                    insertNotification.NotificationType = NotificationType.RESERVATION_INTERPRETATION_UPDATE;
                    if (item.ASSGNCode == "DME")
                    {
                        switch (item.RSVPRCode)
                        {
                            case "DMEDEL":
                                insertNotification.Body = status switch
                                {
                                    ButtonStatus.START => $"{assignCode} {item.Contractor} has started and would be reaching at your location shortly.",
                                    ButtonStatus.REACHED => $"{assignCode} {item.Contractor} reached your location.",
                                    _ => insertNotification.Body
                                };
                                break;

                            case "DMESETUP":
                                insertNotification.Body = status switch
                                {
                                    ButtonStatus.START => $"{assignCode} {item.Contractor} has started and would be reaching at your location shortly.",
                                    ButtonStatus.REACHED => $"Contractor {item.Contractor} reached your location will setup the equipment.",
                                    _ => insertNotification.Body
                                };
                                break;

                            case "DMEDELSETUP":
                                insertNotification.Body = status switch
                                {
                                    ButtonStatus.START => $"{assignCode} {item.Contractor} has started and would be reaching at your location shortly.",
                                    ButtonStatus.REACHED => $"{assignCode} {item.Contractor} reached your location and will set up the equipment.",
                                    _ => insertNotification.Body
                                };
                                break;
                        }
                    }
                    else
                    {

                        insertNotification.Body = status switch
                        {
                            ButtonStatus.START => $"{assignCode} {item.Contractor} has started and would be reaching at the location shortly.",
                            ButtonStatus.REACHED => $"{assignCode} {item.Contractor} reached the location and is waiting for you.",
                            _ => insertNotification.Body
                        };
                    }
                }
                else if (item.ASSGNCode == AssignmentType.TRANSINTERP)
                {
                    if (notificationResponse.CurrentButtonID == 4 && string.Equals(status, ButtonStatus.END, StringComparison.OrdinalIgnoreCase))
                    {
                        insertNotification.Title = NotificationTitle.RESERVATION_UPDATE;
                        insertNotification.NotificationType = NotificationType.RESERVATION_UPDATE;
                    }

                    insertNotification.Title = NotificationTitle.RESERVATION_UPDATE;
                    insertNotification.NotificationType = NotificationType.RESERVATION_UPDATE;

                    insertNotification.Body = status switch
                    {
                        ButtonStatus.START => $"Driver {item.Contractor} has started from their location to pick you up and should reach you by {item.PickupTime}.",
                        ButtonStatus.REACHED => $"Driver {item.Contractor} has reached your pickup location. Please meet at the pickup location.",
                        ButtonStatus.START_TRIP => $"Your trip to {item.DOAddress1}, {item.DOAddress2} has started with {item.ResAsgnCode}. Please sit back and relax.",
                        ButtonStatus.END => $"One way trip has ended. {item.Contractor} is available for the next service.",
                        ButtonStatus.S_ROUND_TRIP => $"Driver {item.Contractor} started to go back to {item.PUAddress1}, {item.PUAddress2}.",
                        ButtonStatus.E_ROUND_TRIP => $"Hope you had a good time, please do not forget to rate the {item.ResAsgnCode} for the service.",
                        ButtonStatus.START_SESSION => $"Your translation session has started.",
                        ButtonStatus.END_SESSION => $"Your translation session has ended.",
                        _ => insertNotification.Body
                    };
                }
                else if (item.ASSGNCode == AssignmentType.TRANSPORT)
                {
                    insertNotification.Title = NotificationTitle.RESERVATION_UPDATE;
                    insertNotification.NotificationType = NotificationType.RESERVATION_UPDATE;

                    insertNotification.Body = status switch
                    {
                        ButtonStatus.START => $"Driver {item.Contractor} has started from their location to pick you up and should reach you by {item.PickupTime}.",
                        ButtonStatus.REACHED => $"Driver {item.Contractor} has reached your pickup location. Please meet at the pickup location.",
                        ButtonStatus.START_TRIP => $"Your trip to {item.DOAddress1} {item.DOAddress2} has started. Please sit back and relax.",
                        ButtonStatus.END => $"Your trip has ended. Hope you had a pleasant time. Please do not forget to rate the driver for the service.",
                        ButtonStatus.HALT => $"Driver is at the facility and would be waiting for you till you finish your appointment.",
                        ButtonStatus.S_ROUND_TRIP => $"Your trip to {item.DOAddress1} {item.DOAddress2} has started. Please sit back and relax.",
                        ButtonStatus.E_ROUND_TRIP => $"Hope you had a good time. Please do not forget to rate the driver for the service.",
                        _ => insertNotification.Body
                    };
                }

                if (string.IsNullOrEmpty(insertNotification.Title))
                {
                    insertNotification.Title = string.Empty;
                }

                if (string.IsNullOrEmpty(insertNotification.Body))
                {
                    insertNotification.Body = string.Empty;
                }

                var data = new Dictionary<string, string>
        {
            { NotificationData.TITLE, insertNotification.Title },
            { NotificationData.BODY, insertNotification.Body },
            { NotificationData.NOTIFICATION_DATE, insertNotification.NotificationDateTime.ToString("o") },
            { NotificationData.RESERVATIONSASSIGNMENTSID, insertNotification.ReservationsAssignmentsID.ToString() },
            { NotificationData.RESERVATIONID, item.ReservationID.ToString() },
            { NotificationData.NOTIFICATION_TYPE, insertNotification.NotificationType.ToString() },
            { NotificationData.CURRENTBUTTONID, notificationResponse.CurrentButtonID?.ToString() ?? "0" },
            { NotificationData.BUTTONSTATUS, notificationResponse.ButtonStatus }
        };
                insertNotification.data = data;

                var notification = new AppNotificationModel
                {
                    FcmToken = item.FcmToken,
                    Title = insertNotification.Title,
                    Body = insertNotification.Body,
                    data = data
                };

                await SendAppPushNotifications(notification);
                await _notificationService.InsertNotificationLog(insertNotification);
            }
        }

        //Send PN to web for contractor not assigned
        public async Task SendWebNotificationContractorNotAssigned()
        {
            var results = await _notificationService.WebContractorNotAssignedLogic();
            var tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
            var tokenString = string.Join(",", tokens);
            var notificationDateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

            foreach (var item in results)
            {
                var insertNotification = new InsertNotificationLog
                {
                    webFcmToken = tokenString,
                    Body = $"Assignment for {item.AssgnNum} needs urgent attention for assigning the contractor.",
                    CreatedBy = 0,
                    Title = NotificationTitle.URGENT_ATTENTION_REQUIRED,
                    NotificationType = NotificationType.CONTRACTOR_NOT_ASSIGN_TRIGGER,
                    ReservationsAssignmentsID = Convert.ToInt32(item.ReservationsAssignmentsID),
                    NotificationDateTime = notificationDateTime <= MinSqlDate ? notificationDateTime : _timeZoneConverter.ConvertUtcToConfiguredTimeZone()
                };

                // Ensure Title and Body are not null or empty
                insertNotification.Title ??= string.Empty;
                insertNotification.Body ??= string.Empty;

                var data = new Dictionary<string, string>
                {
                    { NotificationData.TITLE, insertNotification.Title },
                    { NotificationData.BODY, insertNotification.Body },
                    { NotificationData.NOTIFICATION_DATE, insertNotification.NotificationDateTime.ToString("o") },
                    { NotificationData.RESERVATIONSASSIGNMENTSID, item.ReservationsAssignmentsID.ToString() },
                    { NotificationData.NOTIFICATION_TYPE, insertNotification.NotificationType.ToString() }
                };

                insertNotification.data = data;

                var webNotification = new WebNotificationModel
                {
                    FcmToken = tokens,
                    data = data
                };

                await SendWebPushNotifications(webNotification);
                await _notificationService.InsertNotificationTriggerLogWeb(insertNotification);
            }
        }

        //PN for Today and tommorow at night 12:30
        public async Task SendContractorNotification()
        {
            var results = await _notificationService.WebContractorAssignedLogic();
            var currentDate = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

            foreach (var item in results)
            {
                var insertNotification = new InsertNotificationLog
                {
                    UserID = item.ContractorID,
                    CreatedBy = 0,
                    Title = NotificationTitle.UPCOMING_ASSIGNMENT,
                    UserType = 1,
                    NotificationDateTime = currentDate,
                    ReservationsAssignmentsID = item.ReservationsAssignmentsID
                };

                var reservationDateString = item.ReservationDate.ToString("dd/MM/yyyy");
                var currentDateString = currentDate.ToString("dd/MM/yyyy");
                var day = reservationDateString == currentDateString ? "today" : "tomorrow";
                insertNotification.NotificationType = day == "today" ? NotificationType.CONTRACTOR_TODAY_ASSIGNMENT : NotificationType.CONTRACTOR_TOMMOROW_ASSIGNMENT;
                insertNotification.Body = $"You have an upcoming assignment {day} at {item.PickupTime:hh:mm tt}.";

                // Ensure Title and Body are not null or empty
                insertNotification.Title ??= string.Empty;
                insertNotification.Body ??= string.Empty;

                // Ensure NotificationDateTime is valid
                if (insertNotification.NotificationDateTime <= MinSqlDate)
                {
                    insertNotification.NotificationDateTime = currentDate;
                }

                var data = new Dictionary<string, string>
        {
            { NotificationData.TITLE, insertNotification.Title },
            { NotificationData.BODY, insertNotification.Body },
            { NotificationData.NOTIFICATION_DATE, insertNotification.NotificationDateTime.ToString("o") },
            { NotificationData.RESERVATIONSASSIGNMENTSID, item.ReservationsAssignmentsID.ToString() },
            { NotificationData.NOTIFICATION_TYPE, item.NotificationType.ToString() }
        };

                insertNotification.data = data;

                var notification = new AppNotificationModel
                {
                    FcmToken = item.FcmToken,
                    Title = insertNotification.Title,
                    Body = insertNotification.Body,
                    data = data
                };

                await SendAppPushNotifications(notification);
                await _notificationService.InsertNotificationLog(insertNotification);
            }
        }

        //Send Contractor Request after 4 hr cancel previous contractor  
        public async Task SendAssignmentjobRequestAndNotification()
        {
            var results = await _notificationService.AssignmentjobRequestaAndNotification();

            foreach (var item in results)
            {
                var insertNotification = new InsertNotificationLog
                {
                    CreatedBy = 0,
                    ReservationsAssignmentsID = item.ReservationsAssignmentsID,
                    UserID = item.ContractorID ?? 0,
                    UserType = 1,
                    NotificationDateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone()
                };

                var tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
                insertNotification.webFcmToken = string.Join(",", tokens);

                // Determine notification title and body based on the notification type
                switch (item.NotificationType)
                {
                    case NotificationType.NEW_ASSIGNMENT_REQUEST:
                        insertNotification.Title = NotificationTitle.NEW_ASSIGNMENT_REQUEST;
                        insertNotification.Body = GenerateAssignmentBody(item);
                        break;

                    case NotificationType.ASSIGNMENT_NEED_ATTENTION:
                        insertNotification.Title = NotificationTitle.ASSIGNMENT_NEED_ATTENTION;
                        insertNotification.Body = $"Please pay attention to the assignment ID: {item.AssgnNum} dated {item.ReservationDate:dd MMMM}, Contractor Not Found for this assignment!!!";
                        break;

                    case NotificationType.NEW_ASSIGNMENT_REQUEST_REMINDER:
                        insertNotification.Title = NotificationTitle.ASSIGNMENT_REQUEST_REMINDER;
                        insertNotification.Body = GenerateAssignmentBody(item);
                        break;

                    case NotificationType.NEW_ASSIGNMENT_REQUEST_WITHDRAWN:
                        insertNotification.Title = NotificationTitle.ASSIGNMENT_WITHDRAWN;
                        insertNotification.Body = $"The assignment request to {item.DOAddress1} {item.DOAddress2} dated {item.ReservationDate:dd MMMM} has been withdrawn.";
                        break;

                    case NotificationType.PREFERRED_CONTRACTOR_NOTFOUND:
                        insertNotification.Title = NotificationTitle.PREFERRED_CONTRACTOR_NOTFOUND;
                        insertNotification.Body = $"Please pay attention to the assignment ID: {item.AssgnNum} dated {item.ReservationDate:dd MMMM}, Preferred Contractor not found for this assignment!";
                        await Task.Run(async () =>
                        {
                            await _emailService.PreferredContractorNotMatchedMail(0,item.AssgnNum);
                        });
                        break;
                    case NotificationType.PREFERRED_CONTRACTOR_FOUND_NOTASSIGNED:
                        insertNotification.Title = NotificationTitle.PREFERRED_CONTRACTOR_FOUND_NOTASSIGNED;
                        insertNotification.Body = $"Please pay attention to the assignment ID: {item.AssgnNum} dated {item.ReservationDate:dd MMMM}, Preferred found but no one assigned!";
                         await Task.Run(async () =>
                        {
                            await _emailService.PreferredContractorMatchedNotAssigned( item.AssgnNum);
                        });
                        break;

                    default:
                        insertNotification.Title = string.Empty;
                        insertNotification.Body = string.Empty;
                        break;
                }

                // Ensure Title and Body are not null or empty
                insertNotification.Title ??= string.Empty;
                insertNotification.Body ??= string.Empty;

                var data = new Dictionary<string, string>
                {
                    { NotificationData.TITLE, insertNotification.Title },
                    { NotificationData.BODY, insertNotification.Body },
                    { NotificationData.NOTIFICATION_DATE, insertNotification.NotificationDateTime.ToString("o") },
                    { NotificationData.RESERVATIONSASSIGNMENTSID, item.ReservationsAssignmentsID.ToString() },
                    { NotificationData.NOTIFICATION_TYPE, item.NotificationType.ToString() },
                    { NotificationData.RESERVATION_DATE, item.ReservationDate.ToString("o") },
                    { NotificationData.RESERVATION_TIME, item.ReservationTime.ToString("o") }
                };

                insertNotification.data = data;

                // Create and send app notification
                var appNotification = new AppNotificationModel
                {
                    FcmToken = item.FcmToken,
                    Title = insertNotification.Title,
                    Body = insertNotification.Body,
                    data = data
                };
                await SendAppPushNotifications(appNotification);

                // Create and send web notification
                var webNotification = new WebNotificationModel
                {
                    FcmToken = tokens,
                    data = data
                };
                await SendWebPushNotifications(webNotification);

                if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION || item.NotificationType == NotificationType.PREFERRED_CONTRACTOR_NOTFOUND || item.NotificationType == NotificationType.PREFERRED_CONTRACTOR_FOUND_NOTASSIGNED)
                {
                    await _notificationService.InsertNotificationLogWeb(insertNotification);
                }
                else
                {
                    await _notificationService.InsertNotificationLog(insertNotification);
                }

                //// Insert notification log
                //await _notificationService.InsertNotificationLog(insertNotification);
            }
        }

        private static string GenerateAssignmentBody(AssignmentJobRequestResponseModel item)
        {
            string assignmentDetails;
            switch (item.ASSGNCode)
            {
                case AssignmentType.INTERPRET:
                case AssignmentType.PHINTERPRET:
                case AssignmentType.TRANSLATE:
                    assignmentDetails = $"{item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, from {item.PUAddress1} {item.PUAddress2} to {item.DOAddress1} {item.DOAddress2}";
                    break;

                case AssignmentType.TRANSINTERP:
                    assignmentDetails = $"Pick-up & Drop-off ({item.ResTripType}) with {item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                    break;

                case AssignmentType.TRANSPORT:
                    assignmentDetails = $"Pick-up & Drop-off ({item.ResTripType}) {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                    break;

                default:
                    assignmentDetails = $"{item.ResAsgnCode} ({item.ResTripType}) {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                    break;
            }
            return assignmentDetails;
        }

        //This Will Use on web When We click To Find The Contractor
        public async Task SendContractorWebJobSearch(int AssignmentID)
        {
            var result = await _notificationService.SendContractorWebJobSearchLogic(AssignmentID);

            foreach (var item in result)
            {
                InsertNotificationLog insertNotification = new InsertNotificationLog();

                List<string> tokens = new List<string>();


                if (item.NotificationType == NotificationType.NEW_ASSIGNMENT_REQUEST)
                {

                    insertNotification.Title = NotificationTitle.NEW_ASSIGNMENT_REQUEST;
                    if (item.ASSGNCode == AssignmentType.INTERPRET || item.ASSGNCode == AssignmentType.PHINTERPRET || item.ASSGNCode == AssignmentType.TRANSLATE)
                    {
                        insertNotification.Body = $"{item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, from {item.PUAddress1} {item.PUAddress2} to {item.DOAddress1} {item.DOAddress2}";
                    }
                    else if (item.ASSGNCode == AssignmentType.TRANSINTERP)
                    {
                        insertNotification.Body = $"Pick-up & Drop-off ({item.ResTripType}) on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                    }
                    else if (item.ASSGNCode == AssignmentType.TRANSPORT)
                    {
                        insertNotification.Body = $"Pick-up & Drop-off ({item.ResTripType}) {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                    }
                    else
                    {
                        insertNotification.Body = $"{item.ResAsgnCode} for ({item.ResTripType}) on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                    }
                    insertNotification.NotificationType = item.NotificationType;
                    insertNotification.UserID = item.ContractorID.HasValue ? item.ContractorID.Value : 0;
                    insertNotification.UserType = 1;
                }
                else if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                {
                    insertNotification.UserID = item.ContractorID.HasValue ? item.ContractorID.Value : 0;
                    tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
                    insertNotification.webFcmToken = string.Join(",", tokens);
                    insertNotification.Title = NotificationTitle.ASSIGNMENT_NEED_ATTENTION;
                    insertNotification.NotificationType = item.NotificationType;
                    insertNotification.Body = $"Please pay attention to the assignment ID:  {item.AssgnNum} Dated {item.ReservationDate:dd MMMM}, Contractor Not Found for this assignment!!!";
                }

                insertNotification.CreatedBy = 0;
                insertNotification.NotificationDateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();
                insertNotification.ReservationsAssignmentsID = item.ReservationsAssignmentsID;
                var data = new Dictionary<string, string>
                {
                    { NotificationData.TITLE, insertNotification.Title },
                    { NotificationData.BODY, insertNotification.Body },
                    { NotificationData.NOTIFICATION_DATE, insertNotification.NotificationDateTime.ToString("o") },
                    { NotificationData.RESERVATIONSASSIGNMENTSID, item.ReservationsAssignmentsID.ToString() },
                    { NotificationData.NOTIFICATION_TYPE, item.NotificationType.ToString() }
                };
                insertNotification.data = data;
                AppNotificationModel notification = new AppNotificationModel();
                notification.FcmToken = item.FcmToken;
                notification.Title = insertNotification.Title;
                notification.Body = insertNotification.Body;
                notification.data = data;
                await SendAppPushNotifications(notification);
                WebNotificationModel webNotification = new WebNotificationModel();
                webNotification.FcmToken = tokens;
                webNotification.data = data;
                await SendWebPushNotifications(webNotification);

                if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                {
                    await _notificationService.InsertNotificationLogWeb(insertNotification);
                }
                else if (item.NotificationType == NotificationType.NEW_ASSIGNMENT_REQUEST)
                {
                    await _notificationService.InsertNotificationLog(insertNotification);
                }
            }
        }

        //web Forcefully assigned/ Send Request
        public async Task SendNotificationWebForceRequest(ReservationNotificationWebViewModel model)
        {
            InsertNotificationLog insertNotification = new InsertNotificationLog();

            IEnumerable<ContractorFirstAcceptCancelResponseModel> claimantAccepts = new List<ContractorFirstAcceptCancelResponseModel>();

            if (model.Type == 2 && model.ButtonStatus == ButtonStatus.FORCED)
            {
                claimantAccepts = await _notificationService.ContractorAcceptLogic(model.ReservationAssignmentID.HasValue ? model.ReservationAssignmentID.Value : 0);
            }
            else if (model.Type == 1 && model.ButtonStatus == ButtonStatus.REQUEST)
            {
                claimantAccepts = await _notificationService.ContractorFirstCancelLogic(model.ReservationAssignmentID.HasValue ? model.ReservationAssignmentID.Value : 0);
            }

            foreach (var item in claimantAccepts)
            {
                List<string> strings = new List<string>();
                var tokens = strings;
                if (model.Type == 1 && model.ButtonStatus == ButtonStatus.REQUEST)
                {
                    if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                    {
                        tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
                        insertNotification.webFcmToken = String.Join(",", tokens);
                    }
                }
                else
                {
                    tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
                    insertNotification.webFcmToken = String.Join(",", tokens);
                }

                if (item.ASSGNCode == AssignmentType.INTERPRET || item.ASSGNCode == AssignmentType.PHINTERPRET || item.ASSGNCode == AssignmentType.TRANSLATE)
                {
                    insertNotification.Body = $"{item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, from {item.PUAddress1} {item.PUAddress2} to {item.DOAddress1} {item.DOAddress2}";
                }
                else if (item.ASSGNCode == AssignmentType.TRANSINTERP)
                {
                    insertNotification.Body = $"Pick-up & drop-off ({item.ResTripType}) with {item.ResAsgnCode} on {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                }
                else if (item.ASSGNCode == AssignmentType.TRANSPORT)
                {
                    insertNotification.Body = $"Pick-up & drop-off ({item.ResTripType}) {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                }
                else
                {
                    insertNotification.Body = $"{item.ResAsgnCode} ({item.ResTripType}) {item.ReservationDate:dd MMMM}, {item.ReservationTime:hh:mm tt}, From {item.PUAddress1} {item.PUAddress2} To {item.DOAddress1} {item.DOAddress2}";
                }
                insertNotification.CreatedBy = 0;
                insertNotification.UserID = item.ContractorID;
                insertNotification.UserType = 1;

                insertNotification.Title = string.IsNullOrEmpty(model.NotificationTitle)
                    ? NotificationTitle.ASSIGNMENT_NEED_ATTENTION
                    : model.NotificationTitle;


                if (model.Type == 1 && model.ButtonStatus == ButtonStatus.FORCED)
                {
                    insertNotification.Title = NotificationTitle.NEW_ASSIGNMENT_ASSIGNED;
                    insertNotification.NotificationType = NotificationType.NEW_ASSIGNMENT_ASSIGNED;

                }
                else if (model.Type == 1 && model.ButtonStatus == ButtonStatus.REQUEST)
                {
                    if (string.IsNullOrEmpty(item.NotificationType))
                    {
                        insertNotification.Title = NotificationTitle.NEW_ASSIGNMENT_REQUEST;
                        insertNotification.NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST;
                    }
                    if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                    {
                        insertNotification.Title = NotificationTitle.ASSIGNMENT_NEED_ATTENTION;
                        insertNotification.Body = $"Please pay attention to the assignment ID:  {item.AssgnNum} dated {item.ReservationDate:dd MMMM}, contractor not found for this assignment!!!";
                        insertNotification.UserID = 0;
                        insertNotification.NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION;
                    }
                }
                if (String.IsNullOrEmpty(insertNotification.Title))
                {
                    insertNotification.Title = "";
                }
                if (String.IsNullOrEmpty(insertNotification.Body))
                {
                    insertNotification.Body = "";
                }
                insertNotification.NotificationDateTime = item.NotificationDateTime;
                insertNotification.ReservationsAssignmentsID = item.ReservationsAssignmentsID;
                if (insertNotification.NotificationDateTime <= MinSqlDate)
                {
                    insertNotification.NotificationDateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();
                }
                var data = new Dictionary<string, string>
                {
                    { NotificationData.TITLE,insertNotification.Title },
                    { NotificationData.BODY, insertNotification.Body },
                    { NotificationData.NOTIFICATION_DATE, item.NotificationDateTime.ToString("o") },
                    { NotificationData.RESERVATIONSASSIGNMENTSID, item.ReservationsAssignmentsID.ToString() },
                    { NotificationData.NOTIFICATION_TYPE, item.NotificationType.ToString() },
                    { NotificationData.RESERVATION_DATE, item.ReservationDate.ToString("o") },
                    { NotificationData.RESERVATION_TIME, item.ReservationTime.ToString("o") }
                };
                insertNotification.data = data;

                AppNotificationModel notification = new AppNotificationModel();
                notification.FcmToken = insertNotification.FcmToken;
                notification.Title = insertNotification.Title;
                notification.Body = insertNotification.Body;
                notification.data = data;
                await SendAppPushNotifications(notification);

                WebNotificationModel webNotification = new WebNotificationModel();
                webNotification.FcmToken = tokens;
                webNotification.data = data;
                await SendWebPushNotifications(webNotification);

                if (model.ButtonStatus == ButtonStatus.REQUEST)
                {
                    if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                    {
                        await _notificationService.InsertNotificationLogWeb(insertNotification);
                    }
                    else
                    {
                        await _notificationService.InsertNotificationLog(insertNotification);
                    }
                }
                else if (item.NotificationType == NotificationType.ASSIGNMENT_NEED_ATTENTION)
                {
                    await _notificationService.InsertNotificationLogWeb(insertNotification);
                }
                else
                {
                    await _notificationService.InsertNotificationLog(insertNotification);
                    await _notificationService.InsertNotificationLogWeb(insertNotification);
                }
            }
        }

        public async Task SendNotificationAsync(NotificationMessageAppViewModel model)
        {
            if (!String.IsNullOrEmpty(model.FcmToken))
            {
                try
                {
                    var message = new Message
                    {
                        Token = model.FcmToken,
                        Notification = new Notification
                        {
                            Title = model.Title,
                            Body = model.Body
                        },
                        Data = new Dictionary<string, string>
                        {
                            { NotificationData.TITLE, model.Title },
                            { NotificationData.BODY, model.Body },
                            { NotificationData.NOTIFICATION_DATE, _timeZoneConverter.ConvertUtcToConfiguredTimeZone().ToString("o") },
                            { NotificationData.RESERVATIONSASSIGNMENTSID, model.ReservationsAssignmentsID.ToString() },
                            { NotificationData.NOTIFICATION_TYPE, model.NotificationType },
                            { NotificationData.RESERVATION_DATE, model.ReservationDate.ToString("o") },
                            { NotificationData.RESERVATION_TIME, model.ReservationTime.ToString("o") }
                        }
                    };

                    // Attempt to send the notification
                    await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }
                catch (FirebaseMessagingException ex)
                {
                    // Log Firebase-specific errors
                    _logger.LogError(ex, "Firebase Messaging error occurred: {Message}", ex.Message);

                }
                catch (Exception ex)
                {
                    // Log general errors
                    _logger.LogError(ex, "Unexpected error occurred: {Message}", ex.Message);
                    // Handle general errors appropriately
                }
            }

        }

        public async Task SendAppPushNotifications(AppNotificationModel inputValues)
        {
            if (!String.IsNullOrEmpty(inputValues.FcmToken))
            {
                try
                {
                    var message = new Message
                    {
                        Token = inputValues.FcmToken,
                        Notification = new Notification
                        {
                            Title = inputValues.Title,
                            Body = inputValues.Body,
                        },
                        Data = inputValues.data
                    };

                    // Attempt to send the notification
                    await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }
                catch (FirebaseMessagingException ex)
                {
                    // Log Firebase-specific errors
                    _logger.LogError(ex, "Firebase Messaging error occurred: {Message}", ex.Message);
                }
                catch (Exception ex)
                {
                    // Log general errors
                    _logger.LogError(ex, "Unexpected error occurred: {Message}", ex.Message);
                    // Handle general errors appropriately
                }
            }

        }

        public async Task SendWebPushNotifications(WebNotificationModel inputValues)
        {
            if (inputValues.FcmToken != null && inputValues.FcmToken.Count > 0)
            {
                try
                {
                    var message = new MulticastMessage
                    {
                        Tokens = inputValues.FcmToken,
                        Data = inputValues.data
                    };

                    // Attempt to send the notifications
                    await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                }
                catch (FirebaseMessagingException ex)
                {
                    // Log Firebase-specific errors
                    _logger.LogError(ex, "Firebase Messaging error occurred: {Message}", ex.Message);
                }
                catch (Exception ex)
                {
                    // Log general errors
                    _logger.LogError(ex, "Unexpected error occurred: {Message}", ex.Message);
                    // Handle general errors appropriately
                }
            }

        }

        public async Task DeleteInactiveChatRooms()
        {
            try
            {
                await _notificationService.DeleteInactiveChatRoomsAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while deleting old inactive chat rooms: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting old inactive chat rooms: {Message}", ex.Message);
            }
        }
        public async Task DeleteNotificationCrone()
        {
            try
            {
                await _notificationService.DeleteNotifications();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while deleting old inactive chat rooms: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting old inactive chat rooms: {Message}", ex.Message);
            }
        }
        public async Task DeleteOldLiveCoordinatesCrone()
        {
            try
            {
                await _notificationService.DeleteOldLiveCoordinates();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while deleting old inactive chat rooms: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting old inactive chat rooms: {Message}", ex.Message);
            }
        }
        public async Task SendPreferredContractorNotFound(int AssignmentID)
        {
            var result = await _notificationService.SendContractorWebJobSearchLogicNoDataFound(AssignmentID);
            var tokens = await _notificationService.GetDistinctUserFcmTokensAsync();
            string webFcmToken = string.Join(",", tokens);

            foreach (var item in result)
            {
                var notificationDateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

                var body = $"Please pay attention to the assignment ID: {item.AssgnNum} dated {item.ReservationDate:dd MMMM}, Preferred Contractor not matched for this assignment!";

                var data = new Dictionary<string, string>
                {
                    { NotificationData.TITLE, NotificationTitle.PREFERRED_CONTRACTOR_NOTFOUND },
                    { NotificationData.BODY, body },
                    { NotificationData.NOTIFICATION_DATE, notificationDateTime.ToString("o") },
                    { NotificationData.RESERVATIONSASSIGNMENTSID, item.ReservationsAssignmentsID.ToString() },
                    { NotificationData.NOTIFICATION_TYPE, NotificationType.ASSIGNMENT_NEED_ATTENTION.ToString() }
                };

                var insertNotification = new InsertNotificationLog
                {
                    Title = NotificationTitle.PREFERRED_CONTRACTOR_NOTFOUND,
                    Body = body,
                    NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION,
                    UserID = item.ContractorID ?? 0,
                    UserType = 1,
                    CreatedBy = 0,
                    NotificationDateTime = notificationDateTime,
                    ReservationsAssignmentsID = item.ReservationsAssignmentsID,
                    webFcmToken = webFcmToken,
                    data = data
                };

                var webNotification = new WebNotificationModel
                {
                    FcmToken = tokens,
                    data = data
                };
                await SendWebPushNotifications(webNotification);

                await _notificationService.InsertNotificationLogWeb(insertNotification);
            }
        }
    }
}