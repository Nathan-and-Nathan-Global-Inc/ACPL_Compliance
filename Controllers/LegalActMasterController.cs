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
    public class LegalActMasterController : Controller
    {
        private readonly DBClass _db;
        private readonly ILogger<LegalActMasterController> _logger;

        public LegalActMasterController(ILogger<LegalActMasterController> logger, DBClass db)
        {
            _logger = logger;
            _db = db;
        }

        // GET: /legalactmaster
        public IActionResult Index()
        {
            try
            {
                var legalacts = _db.SelectAll<Class_legal_act_master>()
                    .Where(t => t.del_status == false)
                    .OrderBy(t => t.effective_dt)
                    .ToList();
                return View(legalacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Legal Acts");
                TempData["msg"] = "Error loading Legal Acts";
                return View(Enumerable.Empty<Class_legal_act_master>());
            }
        }

        // GET: /Legal Acts/Details/5
        public IActionResult Details(int sl)
        {
            var legalact= _db.SelectById<Class_legal_act_master>(sl);
            if (legalact== null || legalact.del_status)
            {
                return NotFound();
            }
            return View(legalact);
        }

        // GET: /Legal Acts/Create
        public IActionResult Create()
        {
            ViewBag.group_desc = _db.SelectAll<Class_group_master>()
                .Where(t => t.del_status == false)
                .ToList();
            return View(new Class_legal_act_master { del_status = false });
        }

        // POST: /Legal Acts/Create
        [HttpPost]
        public IActionResult Create(Class_legal_act_master model)
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees clsemp = JsonConvert.DeserializeObject<Class_employees>(st);
            if (IsDuplicate(model.act_law_rules_name, model.activity_description))
                ModelState.AddModelError(nameof(model.act_law_rules_name),
                    "This Act with the same Activity Description already exists");

            if (!ModelState.IsValid)
            {
                LoadGroups();
                return View(model);
            }

            try
            {
                Class_group_master cls=_db.SelectById<Class_group_master>(model.group_slno);
                model.group_desc = cls.group_desc;
                model.created_by= clsemp.empl_slno;
                model.created_dt = DateTime.Now;
                model.effective_dt = DateTime.Now;
                model.del_status = false;
                _db.AddNew<Class_legal_act_master>(model);
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

        // GET: /Legal Acts/Edit/5
        public IActionResult Edit(int sl)
        {
            ViewBag.group_desc = _db.SelectAll<Class_group_master>()
                .Where(t => t.del_status == false)
                .ToList();
            var legalact= _db.SelectById<Class_legal_act_master>(sl);
            if (legalact== null || legalact.del_status)
            {
                return NotFound();
            }
            return View(legalact);
        }

        // POST: /Legal Acts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int sl, Class_legal_act_master model)
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees clsemp = JsonConvert.DeserializeObject<Class_employees>(st);
            if (sl != model.legal_act_slno)
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

            if (IsDuplicate(model.act_law_rules_name, model.activity_description, sl))
                ModelState.AddModelError(nameof(model.act_law_rules_name),
                    "This Act with the same Activity Description already exists");

            if (!ModelState.IsValid)
            {
                LoadGroups();
                return View(model);
            }

            try
            {

                var existing = _db.SelectById<Class_legal_act_master>(sl);
                if (existing == null)
                {
                    return NotFound();
                }

                Class_group_master cls = _db.SelectById<Class_group_master>(model.group_slno);
                model.group_desc = cls.group_desc;
                model.modified_by = clsemp.empl_slno;
                model.modified_dt = DateTime.Now;
                model.created_by = existing.created_by;
                model.created_dt = existing.created_dt;
                _db.Update<Class_legal_act_master>(model);
                TempData["msg"] = "Data Saved";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legalact{groupId}", sl);
                ModelState.AddModelError(string.Empty, "Error updating group");
                return View(model);
            }
        }

        // GET: /Legal Acts/Delete/5
        public IActionResult Delete(int sl)
        {
            var legalact= _db.SelectById<Class_legal_act_master>(sl);
            if (legalact== null || legalact.del_status)
            {
                return NotFound();
            }
            return View(legalact);
        }

        // POST: /Legal Acts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int sl)
        {
            try
            {
                var legalact= _db.SelectById<Class_legal_act_master>(sl);
                if (legalact== null)
                {
                    return NotFound();
                }

                legalact.del_status = true;
                _db.Update<Class_legal_act_master>(legalact);
                TempData["msg"] = "Data Deleted";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting legalact{groupId}", sl);
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
        private bool IsDuplicate(string actName, string activityDesc, int excludeId = 0)
        {
            var n = actName?.Trim();
            var d = activityDesc?.Trim();
            return _db.SelectAll<Class_legal_act_master>().Any(t =>
                t.del_status == false
                && t.legal_act_slno != excludeId
                && string.Equals(t.act_law_rules_name?.Trim(), n, StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.activity_description?.Trim(), d, StringComparison.OrdinalIgnoreCase));
        }

       // private bool IsUsedInTransaction(int legalActSlno)
       // {
       //     return _db.SelectAll<Class_legal_act_transaction>()
       //         .Any(t => t.legal_act_slno == legalActSlno && t.del_status == false);
       // }

       // //Called by the Edit page before submitting
       //[HttpGet]
       // public JsonResult CheckInUse(int sl)
       // {
       //     return Json(new { inUse = IsUsedInTransaction(sl) });
       // }
    }
}
