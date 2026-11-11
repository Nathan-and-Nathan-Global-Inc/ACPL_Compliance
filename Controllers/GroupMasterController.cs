using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasterClass.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SupermarketRepository;

namespace calibpro.Controllers
{
    public class GroupMasterController : Controller
    {
        private readonly DBClass _db;
        private readonly ILogger<GroupMasterController> _logger;

        public GroupMasterController(ILogger<GroupMasterController> logger, DBClass db)
        {
            _logger = logger;
            _db = db;
        }

        // GET: /groups
        public IActionResult Index()
        {
            try
            {
                var groups = _db.SelectAll<Class_group_master>()
                    .Where(t => t.del_status == false)
                    .OrderBy(t => t.group_desc)
                    .ToList();
                return View(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading groups");
                TempData["msg"] = "Error loading groups";
                return View(Enumerable.Empty<Class_group_master>());
            }
        }

        // GET: /groups/Details/5
        public IActionResult Details(int sl)
        {
            var group = _db.SelectById<Class_group_master>(sl);
            if (group == null || group.del_status)
            {
                return NotFound();
            }
            return View(group);
        }

        // GET: /groups/Create
        public IActionResult Create()
        {
            return View(new Class_group_master { del_status = false });
        }

        // POST: /groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Class_group_master model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for uniqueness
            var existinggroup = _db.SelectAll<Class_group_master>()
                .Any(t => t.del_status == false 
                    && t.group_desc != null 
                    && t.group_desc.Trim().Equals(model.group_desc?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (existinggroup)
            {
                ModelState.AddModelError(nameof(model.group_desc), "Group desc already exists");
                return View(model);
            }

            try
            {
                model.group_desc= model.group_desc?.Trim();
                model.del_status = false;
                _db.AddNew<Class_group_master>(model);
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

        // GET: /groups/Edit/5
        public IActionResult Edit(int sl)
        {
            var group = _db.SelectById<Class_group_master>(sl);
            if (group == null || group.del_status)
            {
                return NotFound();
            }
            return View(group);
        }

        // POST: /groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int sl, Class_group_master model)
        {
            if (sl != model.id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for uniqueness (excluding current record)
            var existinggroup = _db.SelectAll<Class_group_master>()
                .Any(t => t.del_status == false 
                    && t.group_desc != null 
                    && t.group_desc.Trim().Equals(model.group_desc?.Trim(), StringComparison.OrdinalIgnoreCase)
                    && t.id != sl);

            if (existinggroup)
            {
                ModelState.AddModelError(nameof(model.group_desc), "Group desc already exists");
                return View(model);
            }

            try
            {
                var existing = _db.SelectById<Class_group_master>(sl);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.group_desc = model.group_desc?.Trim();
                _db.Update<Class_group_master>(existing);
                TempData["msg"] = "Data Saved";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {groupId}", sl);
                ModelState.AddModelError(string.Empty, "Error updating group");
                return View(model);
            }
        }

        // GET: /groups/Delete/5
        public IActionResult Delete(int sl)
        {
            var group = _db.SelectById<Class_group_master>(sl);
            if (group == null || group.del_status)
            {
                return NotFound();
            }

            if (IsGroupInUse(sl))
            {
                TempData["msg"] = "GroupInUse";
                return RedirectToAction(nameof(Index));
            }

            return View(group);
        }

        // POST: /groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int sl)
        {
            try
            {
                var group = _db.SelectById<Class_group_master>(sl);
                if (group == null)
                {
                    return NotFound();
                }

                if (IsGroupInUse(sl))
                {
                    TempData["msg"] = "GroupInUse";
                    return RedirectToAction(nameof(Index));
                }

                group.del_status = true;
                _db.Update<Class_group_master>(group);
                TempData["msg"] = "Data Deleted";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {groupId}", sl);
                TempData["msg"] = "Error deleting group";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /groups/CheckNameUnique?name=XYZ&excludeId=1
        [HttpGet]
        public JsonResult CheckNameUnique(string name, int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { unique = false, message = "Group desc is required" });
            }

            var trimmed = name.Trim();
            var exists = _db.SelectAll<Class_group_master>()
                .Any(t => t.del_status == false 
                    && t.group_desc != null 
                    && t.group_desc.Trim().Equals(trimmed, StringComparison.OrdinalIgnoreCase)
                    && (!excludeId.HasValue || t.id != excludeId.Value));

            return Json(new { unique = !exists, message = exists ? "Group desc already exists" : "" });
        }

        private bool IsGroupInUse(int groupId)
        {
            return _db.SelectAll<Class_legal_act_master>()
                .Any(t => t.del_status == false && t.group_slno == groupId);
        }
    }
}
