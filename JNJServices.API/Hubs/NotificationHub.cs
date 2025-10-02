using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.App;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static JNJServices.Utility.ApiConstants.NotificationConstants;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Hubs
{
    [EnableCors("AllowSpecificOrigins")]
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IReservationAssignmentsService _reservationAssignmentService;
        public readonly IConnectionMapping _hubConnection;
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly ILogger<NotificationHub> _logger;
        private readonly IChatService _chatService;
        private readonly INotificationQueue _notificationQueue;
        public readonly IRoomConnectionMapping _roomConnection;

        public NotificationHub(IReservationAssignmentsService reservationAssignmentService, ITimeZoneConverter timeZoneConverter, ILogger<NotificationHub> logger, IChatService chatService, INotificationQueue notificationQueue, IRoomConnectionMapping roomConnection, IConnectionMapping connectionMapping)
        {
            _reservationAssignmentService = reservationAssignmentService;
            _timeZoneConverter = timeZoneConverter;
            _logger = logger;
            _chatService = chatService;
            _notificationQueue = notificationQueue;
            _roomConnection = roomConnection;
            _hubConnection = connectionMapping;
        }

        public async Task SendCurrentCoordinates(string who, string message)
        {
            foreach (var connectionId in _hubConnection.GetConnections(who))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveCurrentCoordinates", $"{who}:{message}");
            }
        }

        public override Task OnConnectedAsync()
        {
            if (Context?.User?.Identity != null)
            {
                var socketUserId = Context.User.FindFirst(DbParams.SocketUserID)?.Value;
                string connectionId = Context.ConnectionId;

                if (!string.IsNullOrEmpty(socketUserId))
                {
                    _hubConnection.Add(socketUserId, connectionId);
                }
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context?.User?.Identity != null)
            {
                var socketUserId = Context.User.FindFirst(DbParams.SocketUserID)?.Value;
                string connectionId = Context.ConnectionId;

                if (!string.IsNullOrEmpty(socketUserId))
                {
                    _roomConnection.RemoveUserFromAllRooms(socketUserId, _logger);
                    _hubConnection.Remove(socketUserId, connectionId);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendLiveCoordinates(string roomName, int ResAssignId, int Trackid, string LatLong, int IsDeadMile, int CurrentButtonID)
        {
            SaveLiveTrackingCoordinatesViewModel liveTraking = new SaveLiveTrackingCoordinatesViewModel();
            liveTraking.ReservationsAssignmentsID = Convert.ToInt32(ResAssignId);
            liveTraking.AssignmentTrackingID = Convert.ToInt32(Trackid);
            liveTraking.LatitudeLongitude = LatLong;
            liveTraking.TrackingDateTime = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();
            liveTraking.isDeadMile = IsDeadMile;
            await _reservationAssignmentService.LiveTraking(liveTraking);

            await Clients.Group(roomName).SendAsync("ReceiveLiveCoordinates", ResAssignId, LatLong, IsDeadMile, CurrentButtonID);
        }

        public async Task SendGoogleDirectionPath(string roomName, int ResAssignId, List<Coordinate> LatLong, string? EstimateTime)
        {
            if (LatLong != null && LatLong.Count != 0)
            {
                string serializedPath = JsonConvert.SerializeObject(LatLong);

                string assignmentPath = await _reservationAssignmentService.SearchAssignmentPath(ResAssignId);

                await _reservationAssignmentService.SaveAssignmentPath(
                    ResAssignId,
                    serializedPath,
                    !string.IsNullOrEmpty(assignmentPath)
                );
            }
            else
            {
                string serializedPath = await _reservationAssignmentService.SearchAssignmentPath(ResAssignId);

                if (serializedPath != null)
                {
                    var data = JsonConvert.DeserializeObject<List<Coordinate>>(serializedPath);
                    LatLong = data ?? new List<Coordinate>();
                }
                else
                {
                    LatLong = new List<Coordinate>();
                }
            }

            await Clients.Group(roomName).SendAsync("ReceiveGoogleDirectionPath", ResAssignId, LatLong, EstimateTime);
        }

        public async Task SendMessage(ChatMessageModel chatMessage)
        {
            if (string.IsNullOrEmpty(chatMessage.RoomId))
            {
                return;
            }

            try
            {
                var messageData = new ChatMessageViewModel
                {
                    RoomId = chatMessage.RoomId,
                    Type = chatMessage.Type,
                    SenderId = chatMessage.SenderId,
                    MessageContent = chatMessage.MessageContent,
                    SendAt = _timeZoneConverter.ConvertUtcToConfiguredTimeZone()
                };

                var result = await _chatService.UpsertChatRoomMessageAsync(messageData);

                if (result.ResponseCode == 0 || result.ChatRoomId == 0)
                {
                    _logger.LogError("UpsertChatRoomMessageAsync failed for RoomID: {RoomId}", chatMessage.RoomId);
                    return;
                }
                if (chatMessage.ChatRoomDetails != null)
                {
                    chatMessage.ChatRoomDetails.WebReadStatus = result.WebReadStatus;
                }
                var responseData = new ChatMessageResponseModel
                {
                    RoomId = chatMessage.RoomId,
                    Type = chatMessage.Type,
                    SenderId = chatMessage.SenderId,
                    SenderInitials = chatMessage.SenderInitials,
                    MessageContent = chatMessage.MessageContent,
                    SentAt = _timeZoneConverter.ConvertUtcToConfiguredTimeZone(),
                    ChatRoomId = result.ChatRoomId,
                    ChatRoomDetails = chatMessage.ChatRoomDetails,
                    SenderFullName = chatMessage.SenderFullName

                };


                //Send Push Notification(with try-catch)
                try
                {
                    await SendPushNotificationAsync(responseData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Push notification failed for RoomID: {RoomId}", chatMessage.RoomId);
                }


                await Clients.OthersInGroup(chatMessage.RoomId).SendAsync("NewMessage", responseData);
                await Clients.Caller.SendAsync("NewMessage", responseData);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SendMessage for RoomID: {RoomId}", chatMessage.RoomId);
            }
        }

        public async Task SendPushNotificationAsync(ChatMessageResponseModel chatMessage)
        {

            var fcmTokens = await _chatService.GetAllConnectionsForRoomFcmTokenAsync(
                chatMessage.RoomId ?? "0", chatMessage.Type, chatMessage.SenderId.ToString());

            if (fcmTokens == null || fcmTokens.Count == 0)
            {
                return;
            }

            var localSentAt = _timeZoneConverter.ConvertUtcToConfiguredTimeZone().ToString("o");
            var universalSentAt = chatMessage.SentAt.ToString("o");
            var title = chatMessage.Type == 3
            ? "Message from JNJ Team"
            : $"Message from Contractor #{chatMessage.SenderId} for Assignment #{chatMessage.RoomId}";

            var data = new Dictionary<string, string>
            {
                { NotificationData.TITLE, title },
                { NotificationData.BODY, chatMessage.MessageContent ?? "" },
                { NotificationData.NOTIFICATION_DATE, universalSentAt },
                { NotificationData.RESERVATIONSASSIGNMENTSID, chatMessage.RoomId ?? "0" },
                { NotificationData.NOTIFICATION_TYPE, NotificationType.CHAT },
                { "roomId", chatMessage.RoomId ?? "0" },
                { "type", chatMessage.Type.ToString() },
                { "senderId", chatMessage.SenderId.ToString() },
                { "messageContent", chatMessage.MessageContent ?? "" },
                { "sentAt", localSentAt },
                { "chatRoomId", chatMessage.ChatRoomId.ToString() },
                { "chatRoomDetails",  JsonConvert.SerializeObject(chatMessage.ChatRoomDetails) },
                { "senderFullName", chatMessage.SenderFullName ?? "" }
            };

            var fcmTokenListType3 = fcmTokens
                .Where(x => x.Type == 3)
                .Select(x => x.FcmToken)
                .ToList();

            var fcmTokenType1 = fcmTokens.ToList().Find(x => x.Type == 1)?.FcmToken;


            var notificationModel = new WebNotificationModel
            {
                FcmToken = fcmTokenListType3,
                data = data,
            };

            var appNotification = new AppNotificationModel
            {
                FcmToken = fcmTokenType1 ?? string.Empty,
                Title = $"Message from JNJ Team",
                Body = chatMessage.MessageContent ?? string.Empty,
                data = data,
            };

            await _notificationQueue.EnqueueAsync(notificationModel, appNotification);
        }

        public async Task JoinRoom(string roomName)
        {
            var socketUserId = Context.User?.FindFirst(DbParams.SocketUserID)?.Value ?? string.Empty;

            _roomConnection.AddToRoom(roomName, socketUserId);


            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task<ResponseModel> ChatJoinRoom(string roomName)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            var socketUserId = Context.User?.FindFirst(DbParams.SocketUserID)?.Value ?? string.Empty;
            _roomConnection.AddToRoom(roomName, socketUserId);

            ResponseModel response = new ResponseModel();
            ChatRoomViewModel model = new ChatRoomViewModel();

            if (!int.TryParse(roomName, out int roomId))
            {

                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.INVALID_INPUT_PARAMS;
                return response;
            }

            model.RoomId = roomId;
            model.CreateAt = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

            var (chatRoomId, responseCode, message, roomStatus) = await _chatService.InsertOrUpdateChatRoom(model);

            response.status = responseCode == 1 ? ResponseStatus.TRUE : ResponseStatus.FALSE;
            response.statusMessage = message;
            response.data = new { ChatRoomId = chatRoomId };

            if (roomStatus == "NEW")
            {

                await SendPrivateMessageToRoom(roomName);
            }


            return response;
        }

        public async Task SendPrivateMessageToRoom(string roomName)
        {
            ChatListViewModel model = new ChatListViewModel { RoomId = roomName, Page = 1, Limit = 1 };
            var result = await _chatService.GetChatListAsync(model);

            var connectionKeys = await _chatService.GetConnectionKeysForRoom(roomName);
            var socketUserId = Context.User?.FindFirst("SocketUserID")?.Value ?? string.Empty;
            var filteredConnectionKeys = connectionKeys.Where(key => key != socketUserId).ToList();
            foreach (var key in filteredConnectionKeys)
            {

                await Clients.Users(key).SendAsync("NewRoom", result.Item1);
            }


        }

        public async Task SendTypingNotification(string roomId, string userName)
        {

            try
            {
                await Clients.OthersInGroup(roomId).SendAsync("ReceiveTypingNotification", roomId, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send typing notification. RoomId: {RoomId}, UserName: {UserName}", roomId, userName);
            }
        }

        public async Task LeaveRoom(string roomName)
        {

            _roomConnection.RemoveFromRoom(roomName, Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

    }
}
