using MasterClass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SupermarketRepository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace calibpro.Controllers
{
    public class FactoryReportController : Controller
    {
        private readonly DBClass _db;
        private readonly ILogger<FactoryReportController> _logger;

        public FactoryReportController(ILogger<FactoryReportController> logger, DBClass db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult ViewReport()
        {
            ViewBag.Years = DateTime.Now.Year;
            ViewBag.groups = _db.SelectAll<Class_group_master>()
               .Where(t => t.del_status == false)
               .ToList();
            List<Class_factory_act_report> lstMain = new List<Class_factory_act_report>().ToList();
            ViewBag.indexdata = lstMain;
            return View();
        }

    }
}
