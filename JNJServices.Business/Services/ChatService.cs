using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.CommonModels;
using JNJServices.Utility.DbConstants;
using System.Data;

namespace JNJServices.Business.Services
{
    public class ChatService : IChatService
    {
        private readonly IDapperContext _context;

        public ChatService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<(int, int, string, string)> InsertOrUpdateChatRoom(ChatRoomViewModel model)
        {
            string procedureName = ProcEntities.spInsertOrUpdateChatRoom;
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.RoomID, model.RoomId, DbType.Int32);
            parameters.Add(DbParams.CreateAt, model.CreateAt, DbType.DateTime);
            parameters.Add(DbParams.ChatRoomId, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.RoomStatus, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);
            parameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(procedureName, parameters, CommandType.StoredProcedure);

            // Get output parameters
            var chatRoomId = parameters.Get<int>(DbParams.ChatRoomId);
            var responseCode = parameters.Get<int>(DbParams.ResponseCode);
            var message = parameters.Get<string>(DbParams.Msg);
            var roomStatus = parameters.Get<string>(DbParams.RoomStatus);

            return (chatRoomId, responseCode, message, roomStatus);
        }

        public async Task<(int ChatRoomId, int ResponseCode, string Message, bool WebReadStatus)> UpsertChatRoomMessageAsync(ChatMessageViewModel model)
        {

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.RoomID, model.RoomId, DbType.Int32);
            parameters.Add(DbParams.LastMessage, model.MessageContent, DbType.String);
            parameters.Add(DbParams.LastMessageAt, model.SendAt, DbType.DateTime);
            parameters.Add(DbParams.Type, model.Type, DbType.String);
            parameters.Add(DbParams.SenderID, model.SenderId, DbType.Int32);
            parameters.Add(DbParams.ChatRoomId, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);
            parameters.Add(DbParams.WebReadStatus, dbType: DbType.Boolean, size: 255, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(ProcEntities.spUpsertChatRoomMessage, parameters, CommandType.StoredProcedure);

            int chatRoomId = parameters.Get<int>(DbParams.ChatRoomId);
            int responseCode = parameters.Get<int>(DbParams.ResponseCode);
            string message = parameters.Get<string>(DbParams.Msg);
            bool WebReadStatus = parameters.Get<bool>(DbParams.WebReadStatus);

            return (chatRoomId, responseCode, message, WebReadStatus);
        }

        public async Task<(List<ChatRoom>, int TotalCount)> GetChatListAsync(ChatListViewModel model)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.SearchQuery, model.SearchQuery, DbType.String);
            parameters.Add(DbParams.RoomID, model.RoomId, DbType.Int32);
            parameters.Add(DbParams.Page, model.Page, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit, DbType.Int32);
            parameters.Add(DbParams.ContractorID, model.ContractorId, DbType.Int32);
            parameters.Add(DbParams.TotalCount, dbType: DbType.Int32, direction: ParameterDirection.Output);
            var chatList = await _context.ExecuteQueryAsync<ChatRoom>(
                ProcEntities.spGetChatList,
                parameters,
                commandType: CommandType.StoredProcedure
            );
            int totalCount = parameters.Get<int>(DbParams.TotalCount);
            return (chatList.ToList(), totalCount);
        }

        public async Task<List<ChatRoomsActiveResponse>> GetActiveChatRoomsAsync(int? contractorId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, contractorId, DbType.Int32); // Assuming ContractorID is VARCHAR(20)

            var chatRooms = await _context.ExecuteQueryAsync<ChatRoomsActiveResponse>(
                ProcEntities.GetActiveChatRooms,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return chatRooms.ToList();
        }

        public async Task<(List<ChatMessage>, int totalCount)> GetChatMessagesAsync(ChatMessageSearchViewModel model)
        {

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ChatRoomId, model.ChatRoomId, DbType.Int32);
            parameters.Add(DbParams.Page, model.Page, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit, DbType.Int32);
            parameters.Add(DbParams.TotalCount, dbType: DbType.Int32, direction: ParameterDirection.Output);
            var messages = await _context.ExecuteQueryAsync<ChatMessage>(
                ProcEntities.spGetChatMessages,
                parameters,
                commandType: CommandType.StoredProcedure
            );
            int totalCount = parameters.Get<int>(DbParams.TotalCount);
            return (messages.ToList(), totalCount);

        }

        public async Task<List<string>> GetConnectionKeysForRoom(string roomName)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.RoomID, roomName, DbType.String);

            var connectionKeys = await _context.ExecuteQueryAsync<string>(
                ProcEntities.spGetAllConnectionsForRoom,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return connectionKeys.ToList();
        }

        public async Task<List<ChatFcmTokenConnection>> GetAllConnectionsForRoomFcmTokenAsync(string roomId, int type, string senderId)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.RoomID, roomId, DbType.String);
            parameters.Add(DbParams.Type, type, DbType.Int32);
            parameters.Add(DbParams.SenderID, senderId, DbType.String);

            var result = await _context.ExecuteQueryAsync<ChatFcmTokenConnection>(
               ProcEntities.spGetAllConnectionsForRoomFcmToken,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result?.ToList() ?? new List<ChatFcmTokenConnection>(); // Ensuring a non-null list
        }
    }
}
