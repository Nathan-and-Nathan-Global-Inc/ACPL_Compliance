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
    public class LegalActReportController : Controller
    {
        private readonly DBClass _db;
        private readonly ILogger<LegalActReportController> _logger;

        public LegalActReportController(ILogger<LegalActReportController> logger, DBClass db)
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
            List<Class_legal_act_report> lstMain = new List<Class_legal_act_report>().ToList();
            ViewBag.indexdata = lstMain;
            return View();
        }

    }
}
