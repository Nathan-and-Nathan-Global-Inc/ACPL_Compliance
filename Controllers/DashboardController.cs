using CommonClassLibrary;
using MasterClass.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPoco;
using SupermarketRepository;

namespace ams.Controllers
{
    public class DashboardController : Controller
    {
        public readonly DBClass _crudRepository;
        public readonly ILogger<DashboardController> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        private readonly MasterCrudClass _masterCrudClass;
        public DashboardController(ILogger<DashboardController> _logger, DBClass _db, ILogger<MasterRepository> _masterlogger, MasterCrudClass masterClass)
        {
            logger = _logger;
            masterlogger = _masterlogger;
            _crudRepository = _db;
            _crudRepository.LogLastCommand = true;
            _crudRepository.GetCommand += Dbc_GetCommand;
            _crudRepository.MailSendingRequested += Dbc_MailSendingRequested;
            _crudRepository.UseExperimentalFeature = true;
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

        // GET: DashboardController
        public ActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                ElmahCore.ElmahExtensions.RaiseError(ex);
                throw;
            }
        }

        private int GetCount(string sql)
        {
            var row = _crudRepository.SelectBySQL<DashboardCountRow>(sql).FirstOrDefault();
            return row == null ? 0 : (int)Math.Round(row.cnt);
        }

        // GET: DashboardController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DashboardController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DashboardController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DashboardController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DashboardController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DashboardController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DashboardController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}