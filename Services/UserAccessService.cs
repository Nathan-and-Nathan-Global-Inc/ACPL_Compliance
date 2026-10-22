
using CommonClassLibrary;
using MasterClass.Models;
using SupermarketRepository;
using System.Collections.Generic;
using System.Security.Claims;

namespace acpl_startup.Services
{
    public class UserAccessService
    {
        public readonly DBClass dbc;
        public readonly ILogger<UserAccessService> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MasterCrudClass _masterCrudClass;

        public UserAccessService(ILogger<UserAccessService> _logger, DBClass _db, 
            ILogger<MasterRepository> _masterlogger, IHttpContextAccessor httpContextAccessor,
            MasterCrudClass masterClass)
        {
            logger = _logger;
            masterlogger = _masterlogger;
            dbc = _db;
            dbc.LogLastCommand = true;
            dbc.GetCommand += Dbc_GetCommand;
            dbc.MailSendingRequested += Dbc_MailSendingRequested;
            dbc.UseExperimentalFeature = true;
            _httpContextAccessor = httpContextAccessor;
            _masterCrudClass = masterClass;
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

        public async Task<List<Class_UserScreenAccess>> GetUserModuleAccessAsync(int userId)
        {
            var context = _httpContextAccessor.HttpContext;

            try
            {
                string sql = "SELECT TOP 1 * FROM settings";
                var settingsList = _masterCrudClass.SelectBySQL<Class_settings>(sql).ToList();

                if (settingsList.Count > 0 && settingsList[0] != null)
                {
                    var settings = settingsList[0];

                    // Set session values with null checks
                    if (!string.IsNullOrEmpty(settings.base_url))
                    {
                        context.Session.SetString("MainDashboard", settings.base_url + "dqms/Dashboard?sl=" + userId);
                        context.Session.SetString("MainHome", settings.base_url + "dqms/Home?sl=" + userId);
                        context.Session.SetString("LogoutUrl", settings.base_url + "dqms/Logout");
                        context.Session.SetString("ChangePasswordUrl", settings.base_url + "dqms/ChangePwd?sl=" + userId);
                    }

                    // Set timeout with validation
                    int timeoutMinutes = 20; // Default value
                    if (settings.sessionTimeOut_mins > 0 && settings.sessionTimeOut_mins <= 1440)
                    {
                        timeoutMinutes = settings.sessionTimeOut_mins;
                    }
                    context.Session.SetString("TimeOutMins", timeoutMinutes.ToString());
                }
                else
                {
                    // Set default values if no settings found
                    context.Session.SetString("TimeOutMins", "20");
                    context.Session.SetString("LogoutUrl", "/Home/Index");
                    context.Session.SetString("ChangePasswordUrl", "/Home/Index");
                }
            }
            catch (Exception ex)
            {
                // Log error and set default values
                logger.LogError(ex, "Error fetching settings from database");
                context.Session.SetString("TimeOutMins", "20");
                context.Session.SetString("LogoutUrl", "/Home/Index");
                context.Session.SetString("ChangePasswordUrl", "/Home/Index");
            }

            using (MasterRepository mr = new MasterRepository(_masterCrudClass, masterlogger))
            {
                List<Class_UserScreenAccess> lst = mr.UserAccessList(userId).ToList();
                return lst;
            }
        }
    }
}
