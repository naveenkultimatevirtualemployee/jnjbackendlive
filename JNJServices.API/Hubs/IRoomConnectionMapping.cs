namespace JNJServices.API.Hubs
{
    public interface IRoomConnectionMapping
    {
        void AddToRoom(string roomName, string userId);
        void RemoveFromRoom(string roomName, string userId);
        int GetConnectionCount(string roomName);
        List<string> GetUsers(string roomName);
        string GetAllUserIdsInRoomCommaSeparated(string roomName);
        Dictionary<string, List<string>> GetAllRoomsWithUsers();
        void RemoveUserFromAllRooms(string userId, ILogger logger);
    }
}
