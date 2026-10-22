using System.Data.SqlClient;
using System.Threading.Tasks;
using acpl_startup.Services;
using CommonClassLibrary;
using Dapper;
using SupermarketRepository; // Assuming you're using Dapper for database access

public class UserSessionService : IUserSessionService
{
    public readonly DBClass dbc;
    public readonly ILogger<UserSessionService> logger;
    public readonly ILogger<MasterRepository> masterlogger;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public UserSessionService(ILogger<UserSessionService> _logger, DBClass _db, ILogger<MasterRepository> _masterlogger)
    {
        logger = _logger;
        masterlogger = _masterlogger;
        dbc = _db;
        dbc.LogLastCommand = true;
        dbc.GetCommand += Dbc_GetCommand;
        dbc.UseExperimentalFeature = true;
    }
    private void Dbc_MailSendingRequested(object? sender, bool e)
    {
        if (e)
        {
            logger.LogInformation("Mail Sending event triggered");
        }
    }

    private void Dbc_GetCommand(object? sender, DbCommandEventArgs e)
    {
        logger.LogInformation(e.CommandText);
    }

    public async Task SaveSessionAsync(string sessionId, string userId, string sessionData)
    {
        var query = $@"
                IF EXISTS (SELECT 1 FROM UserSessions WHERE SessionId = '{sessionId}')
                BEGIN
                    UPDATE UserSessions
                    SET UserId = '{userId}', SessionData = '{sessionData}', UpdatedAt = GETDATE()
                    WHERE SessionId = '{sessionId}'
                END
                ELSE
                BEGIN
                    INSERT INTO UserSessions (SessionId, UserId, SessionData)
                    VALUES ('{sessionId}', '{userId}', '{@sessionData}')
                END";
        dbc.Execute<Class_UserSessions>(query);
    }

    public async Task<string> GetSessionDataAsync(string sessionId)
    {
        var query = $"SELECT SessionData FROM UserSessions WHERE SessionId = '{sessionId}'";
        var sessiondata= dbc.ExecuteScalar<Class_UserSessions>(query);
        return sessiondata.ToString();
    }
}
