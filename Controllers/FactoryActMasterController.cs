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
    public class FactoryActMasterController : Controller
    {
        private readonly DBClass _db;
        private readonly ILogger<FactoryActMasterController> _logger;

        public FactoryActMasterController(ILogger<FactoryActMasterController> logger, DBClass db)
        {
            _logger = logger;
            _db = db;
        }

        // GET: /Factoryactmaster
        public IActionResult Index()
        {
            try
            {
                var Factoryacts = _db.SelectAll<Class_factory_act_master>()
                    .Where(t => t.del_status == false)
                    .OrderBy(t => t.effective_dt)
                    .ToList();
                return View(Factoryacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Factory Acts");
                TempData["msg"] = "Error loading Factory Acts";
                return View(Enumerable.Empty<Class_factory_act_master>());
            }
        }

        // GET: /Factory Acts/Details/5
        public IActionResult Details(int sl)
        {
            var Factoryact= _db.SelectById<Class_factory_act_master>(sl);
            if (Factoryact== null || Factoryact.del_status)
            {
                return NotFound();
            }
            return View(Factoryact);
        }

        // GET: /Factory Acts/Create
        public IActionResult Create()
        {
            ViewBag.group_desc = _db.SelectAll<Class_group_master>()
                .Where(t => t.del_status == false)
                .ToList();
            return View(new Class_factory_act_master { del_status = false });
        }

        // POST: /Factory Acts/Create
        [HttpPost]
        public IActionResult Create(Class_factory_act_master model)
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees clsemp = JsonConvert.DeserializeObject<Class_employees>(st);
            if (IsDuplicate(model.legal_requirement))
                ModelState.AddModelError(nameof(model.legal_requirement),
                    "This Legal Requirement already exists");

            if (!ModelState.IsValid)
            {
                LoadGroups();
                return View(model);
            }

            try
            {
                model.created_by= clsemp.empl_slno;
                model.created_dt = DateTime.Now;
                model.effective_dt = DateTime.Now;
                model.del_status = false;
                _db.AddNew<Class_factory_act_master>(model);
                TempData["msg"] = "Data Submitted";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                ModelState.AddModelError(string.Empty, "Error creating group");
                return View(model);
            }
        }

        // GET: /Factory Acts/Edit/5
        public IActionResult Edit(int sl)
        {
            ViewBag.group_desc = _db.SelectAll<Class_group_master>()
                .Where(t => t.del_status == false)
                .ToList();
            var Factoryact= _db.SelectById<Class_factory_act_master>(sl);
            if (Factoryact== null || Factoryact.del_status)
            {
                return NotFound();
            }
            return View(Factoryact);
        }

        // POST: /Factory Acts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int sl, Class_factory_act_master model)
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees clsemp = JsonConvert.DeserializeObject<Class_employees>(st);
            if (sl != model.factory_act_slno)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //if (IsUsedInTransaction(sl))
            //{
            //    ModelState.AddModelError(string.Empty,
            //        "You cannot modify this Act as data is present in transactions.");
            //    LoadGroups();
            //    return View(model);
            //}

            if (IsDuplicate(model.legal_requirement, sl))
                ModelState.AddModelError(nameof(model.legal_requirement),
                    "This Legal Requirement already exists");

            if (!ModelState.IsValid)
            {
                LoadGroups();
                return View(model);
            }

            try
            {

                var existing = _db.SelectById<Class_factory_act_master>(sl);
                if (existing == null)
                {
                    return NotFound();
                }
                model.modified_by = clsemp.empl_slno;
                model.modified_dt = DateTime.Now;
                model.created_by = existing.created_by;
                model.created_dt = existing.created_dt;
                _db.Update<Class_factory_act_master>(model);
                TempData["msg"] = "Data Saved";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Factoryact{groupId}", sl);
                ModelState.AddModelError(string.Empty, "Error updating group");
                return View(model);
            }
        }

        // GET: /Factory Acts/Delete/5
        public IActionResult Delete(int sl)
        {
            var Factoryact= _db.SelectById<Class_factory_act_master>(sl);
            if (Factoryact== null || Factoryact.del_status)
            {
                return NotFound();
            }
            return View(Factoryact);
        }

        // POST: /Factory Acts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int sl)
        {
            try
            {
                var Factoryact= _db.SelectById<Class_factory_act_master>(sl);
                if (Factoryact== null)
                {
                    return NotFound();
                }

                Factoryact.del_status = true;
                _db.Update<Class_factory_act_master>(Factoryact);
                TempData["msg"] = "Data Deleted";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Factoryact{groupId}", sl);
                TempData["msg"] = "Error deleting group";
                return RedirectToAction(nameof(Index));
            }
        }

        private void LoadGroups()
        {
            ViewBag.group_desc = _db.SelectAll<Class_group_master>()
                .Where(t => t.del_status == false)
                .ToList();
        }

        // Same act name + same activity description = duplicate
        private bool IsDuplicate(string legalrequirement, int excludeId = 0)
        {
            var d = legalrequirement?.Trim();
            return _db.SelectAll<Class_factory_act_master>().Any(t =>
                t.del_status == false
                && t.factory_act_slno != excludeId
                && string.Equals(t.legal_requirement?.Trim(),d,StringComparison.OrdinalIgnoreCase));
        }

       // private bool IsUsedInTransaction(int legalActSlno)
       // {
       //     return _db.SelectAll<Class_legal_act_transaction>()
       //         .Any(t => t.factory_act_slno == legalActSlno && t.del_status == false);
       // }

       // //Called by the Edit page before submitting
       //[HttpGet]
       // public JsonResult CheckInUse(int sl)
       // {
       //     return Json(new { inUse = IsUsedInTransaction(sl) });
       // }
    }
}
