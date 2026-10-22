namespace acpl_startup.Services
{
    public interface IUserSessionService
    {
        Task SaveSessionAsync(string sessionId, string userId, string sessionData);
        Task<string> GetSessionDataAsync(string sessionId);
    }

}
