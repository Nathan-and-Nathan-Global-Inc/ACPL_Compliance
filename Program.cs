using ElmahCore;
using ElmahCore.Mvc;
using NLog.Extensions.Logging;
using NPoco;
using SupermarketRepository;
using acpl_startup.Services;
using System.Data.SqlClient;
using MasterClass.Models;
using NLog.Extensions.Logging;
using ElmahCore.Sql;

var builder = WebApplication.CreateBuilder(args);

var errconn = builder.Configuration.GetConnectionString("errConnString");
var conn = builder.Configuration.GetConnectionString("Default");
var connMaster = builder.Configuration.GetConnectionString("ConnMaster");

// Add services to the container.
builder.Services.AddElmah(config =>
{
    config.ApplicationName = "process_valdn";
    config.ConnectionString = errconn;
});
builder.Services.AddElmah<SqlErrorLog>(options =>
{
    options.ConnectionString = errconn;
});

builder.Services.AddDBClassWithOptions(config =>
{
    config.ConnectionString = conn;
    config.DbProviderFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;
    config.DatabaseType = NPoco.DatabaseType.SqlServer2012;
    config.LogSQLCommands = true;
});

var myoptionsmaster = new MyDbOptions { ConnectionString = connMaster, DbProviderFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance, DatabaseType = DatabaseType.SqlServer2012 };
builder.Services.AddTransient<MasterCrudClass>(x => new MasterCrudClass(myoptionsmaster));

// Register UserAccessService
builder.Services.AddScoped<UserAccessService>();


// Register UserSessionService and IUserSessionService
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

// Register other necessary services
builder.Services.AddHttpContextAccessor(); // For IHttpContextAccessor

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddLogging(opt =>
{
    opt.AddNLog("nlog.config");
});
// Fetch session timeout from the database
int sessionTimeoutInMinutes = GetSessionTimeout(conn);

// Add session configuration using the fetched timeout value
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutInMinutes);  // Apply the timeout value from the DB
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddLogging(opt =>
{
    opt.AddNLog("nlog.config");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();
app.UseElmah();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{sl?}");

app.Run();

// Function to get session timeout from the database
int GetSessionTimeout(string connectionString)
{
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        var query = "SELECT TOP 1 sessionTimeOut_mins FROM acpl_common.dbo.settings";
        using (var command = new SqlCommand(query, connection))
        {
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 20; // Default to 20 minutes if no value found
        }
    }
}