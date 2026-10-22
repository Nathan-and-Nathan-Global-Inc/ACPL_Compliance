using ams.Models;
using CommonClassLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPoco;
using System.Diagnostics;
using SupermarketRepository;
using System.Text;
using MasterClass.Models;

namespace ams.Controllers
{
    public class HomeController : Controller
    {
        public readonly DBClass _crudRepository;
        public readonly ILogger<HomeController> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        private readonly MasterCrudClass _masterCrudClass;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> _logger, 
            DBClass _db, 
            ILogger<MasterRepository> _masterlogger, 
            MasterCrudClass masterClass,
            IConfiguration configuration)
        {
            logger = _logger;
            masterlogger = _masterlogger;
            _crudRepository = _db;
            _crudRepository.LogLastCommand = true;
            _crudRepository.GetCommand += Dbc_GetCommand;
            _crudRepository.MailSendingRequested += Dbc_MailSendingRequested;
            _crudRepository.UseExperimentalFeature = true;
            _masterCrudClass = masterClass;
            _configuration = configuration;
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

        public IActionResult Index(int? sl)
        {
            if (sl == null)
            {
                sl = int.Parse(_configuration["AppSettings:EmpSl"] ?? "1");
            }

            int? plantslno = 1;
            using (MasterRepository mr = new MasterRepository(_masterCrudClass, masterlogger))
            {
                Class_employees clsemp = mr.EmployeeDetails((int)sl);
                if (clsemp != null)
                {
                    plantslno = clsemp.plant_slno;

                    // add the user details to session
                    HttpContext.Session.SetString("User", JsonConvert.SerializeObject(clsemp));
                    HttpContext.Session.SetString("_Name", clsemp.empl_name);
                    HttpContext.Session.SetString("UserSlno", clsemp.empl_slno.ToString());
                    HttpContext.Session.SetString("plantslno", clsemp.plant_slno.ToString());

                    // redirect to doclist/index
                    return RedirectToAction("Index", "Dashboard");
                }
                // redirect to doclist/index
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ShowUi()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
