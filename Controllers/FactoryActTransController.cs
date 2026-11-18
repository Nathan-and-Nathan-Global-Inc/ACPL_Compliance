using CommonClassLibrary;
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
    public class FactoryActTransController : Controller
    {
        private readonly DBClass _db;
        private readonly ILogger<FactoryActTransController> _logger;
        private readonly MasterCrudClass _masterCrudClass;
        public readonly ILogger<MasterRepository> masterlogger;
        public FactoryActTransController(ILogger<FactoryActTransController> logger, DBClass db, ILogger<MasterRepository> _masterlogger, MasterCrudClass masterClass)
        {
            _logger = logger;
            _db = db;
            _masterCrudClass = masterClass;
            masterlogger = _masterlogger;
        }

        // GET: /Factoryactmaster
        public IActionResult Index()
        {
            try
            {
                var Factoryacts = _db.SelectAll<Class_factory_act_hdr>()
                    .OrderBy(t => t.entry_dt)
                    .ToList();
                return View(Factoryacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Factory Acts");
                TempData["msg"] = "Error loading Factory Acts";
                return View(Enumerable.Empty<Class_factory_act_hdr>());
            }
        }

        // GET: /Factory Acts/Create
        public IActionResult Create()
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees clsemp = JsonConvert.DeserializeObject<Class_employees>(st);
            ViewBag.entry_by = clsemp.empl_name;
            return View(new Class_factory_act_hdr {});
        }

        // POST: /Factory Acts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveFactoryAct([FromBody] FactoryActTransSaveModel model)
        {
            try
            {
                // Get logged-in employee
                var st = HttpContext.Session.GetString("User");

                if (string.IsNullOrEmpty(st))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Session expired. Please login again."
                    });
                }

                Class_employees clsemp =
                    JsonConvert.DeserializeObject<Class_employees>(st);

                if (clsemp == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid user session."
                    });
                }

                // Validate request
                if (model == null || model.children == null || !model.children.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please add at least one Factory Act record."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.entry_dt))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Entry Date is required."
                    });
                }

                if (model.submit_status != "N" && model.submit_status != "Y")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid submission status."
                    });
                }

                // Validate child records
                foreach (var child in model.children)
                {
                    if (child.factory_act_slno <= 0)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Factory Act selected."
                        });
                    }

                    if (string.IsNullOrWhiteSpace(child.legal_requirement) ||
                        string.IsNullOrWhiteSpace(child.compliance_rule_act_reference) ||
                        string.IsNullOrWhiteSpace(child.compliance_approving_authority) ||
                        string.IsNullOrWhiteSpace(child.appointed_competent_person))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Factory Act master details are missing."
                        });
                    }
                }

                // Verify selected Factory Act master records
                var factoryActIds = model.children
                    .Select(x => x.factory_act_slno)
                    .Distinct()
                    .ToList();

                var validFactoryActIds = _db.SelectAll<Class_factory_act_master>()
                    .Where(x => !x.del_status && factoryActIds.Contains(x.factory_act_slno))
                    .Select(x => x.factory_act_slno)
                    .ToList();

                if (factoryActIds.Except(validFactoryActIds).Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "One or more selected Factory Act records are invalid or inactive."
                    });
                }

                // Prepare header
                var header = new Class_factory_act_hdr
                {
                    entry_by = clsemp.empl_name,
                    entry_dt = model.entry_dt,
                    created_by = clsemp.empl_slno,
                    created_dt = DateTime.Now,
                    fin_year=model.fin_year,
                    submit_status = model.submit_status,

                    submitted_by = model.submit_status == "Y"
                        ? clsemp.empl_name
                        : "",

                    submitted_dt = model.submit_status == "Y"
                        ? DateTime.Now.ToString("yyyy-MM-dd")
                        : ""
                };

                // Use one transaction for header and child records
                var database = _db.GetDatabase();

                using (var transaction = database.GetTransaction())
                {
                    // Insert header and retrieve generated identity
                    int headerId = Convert.ToInt32(database.Insert(header));

                    if (headerId <= 0)
                    {
                        throw new Exception("Unable to generate Factory Act Header ID.");
                    }

                    // Insert child records
                    foreach (var child in model.children)
                    {
                        child.fact_act_hdr_slno = headerId;
                        child.submit_status = model.submit_status;

                        child.fin_year = model.fin_year;

                        child.submitted_by = model.submit_status == "Y"
                            ? clsemp.empl_name
                            : "";

                        child.submitted_dt = model.submit_status == "Y"
                            ? DateTime.Now.ToString("yyyy-MM-dd")
                            : "";

                        database.Insert(child);
                    }

                    // Commit header and children together
                    transaction.Complete();
                }

                return Json(new
                {
                    success = true,
                    headerId = header.fact_act_hdr_slno,
                    message = model.submit_status == "Y"
                        ? "Factory Act data submitted successfully."
                        : "Factory Act data saved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Factory Act transaction");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error while saving Factory Act data."
                });
            }
        }

        // GET: /Factory Acts/Edit/5
        [HttpGet]
        public IActionResult Edit(int sl)
        {
            try
            {
                var factoryAct = _db.SelectById<Class_factory_act_hdr>(sl);

                if (factoryAct == null)
                {
                    return NotFound();
                }

                return View(factoryAct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Factory Act transaction for editing");

                TempData["msg"] = "Error loading Factory Act transaction.";

                return RedirectToAction(nameof(Index));
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateFactoryAct([FromBody] FactoryActTransSaveModel model)
        {
            try
            {
                // Get logged-in user
                var st = HttpContext.Session.GetString("User");

                if (string.IsNullOrEmpty(st))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Session expired. Please login again."
                    });
                }

                Class_employees clsemp =
                    JsonConvert.DeserializeObject<Class_employees>(st);

                if (clsemp == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid user session."
                    });
                }

                // Validate request
                if (model == null || model.fact_act_hdr_slno <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid Factory Act transaction."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.entry_dt))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Entry Date is required."
                    });
                }

                if (model.submit_status != "N" && model.submit_status != "Y")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid submission status."
                    });
                }

                if (model.children == null || !model.children.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please add at least one Factory Act record."
                    });
                }

                // Get existing header
                var header = _db.SelectById<Class_factory_act_hdr>(
                    model.fact_act_hdr_slno);

                if (header == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Factory Act transaction not found."
                    });
                }

                // Validate child records
                foreach (var child in model.children)
                {
                    if (child.factory_act_slno <= 0)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Factory Act master selection."
                        });
                    }

                    if (string.IsNullOrWhiteSpace(child.legal_requirement) ||
                        string.IsNullOrWhiteSpace(child.compliance_rule_act_reference) ||
                        string.IsNullOrWhiteSpace(child.compliance_approving_authority) ||
                        string.IsNullOrWhiteSpace(child.appointed_competent_person))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Required Factory Act details are missing."
                        });
                    }
                }

                // Validate active Factory Act master records
                var factoryActIds = model.children
                    .Select(x => x.factory_act_slno)
                    .Distinct()
                    .ToList();

                var validFactoryActIds = _db.SelectAll<Class_factory_act_master>()
                    .Where(x => !x.del_status &&
                                factoryActIds.Contains(x.factory_act_slno))
                    .Select(x => x.factory_act_slno)
                    .ToList();

                if (factoryActIds.Except(validFactoryActIds).Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "One or more Factory Act master records are inactive or invalid."
                    });
                }

                // Get existing child records for this header only
                var existingChildren = _db.SelectAll<Class_factory_act_child>()
                    .Where(x => x.fact_act_hdr_slno == model.fact_act_hdr_slno)
                    .ToList();

                // Update header details
                header.entry_dt = model.entry_dt;
                header.fin_year = model.fin_year;
                header.modified_by = clsemp.empl_slno;
                header.modified_dt = DateTime.Now;
                header.submit_status = model.submit_status;

                // Update submission details only when submitted
                if (model.submit_status == "Y")
                {
                    header.submitted_by = clsemp.empl_name;
                    header.submitted_dt = DateTime.Now.ToString("yyyy-MM-dd");
                }

                // Database transaction
                var database = _db.GetDatabase();

                using (var transaction = database.GetTransaction())
                {
                    // Update header
                    database.Update(header);

                    // Delete existing child records
                    foreach (var oldChild in existingChildren)
                    {
                        database.Delete(oldChild);
                    }

                    // Insert updated child records
                    foreach (var child in model.children)
                    {
                        child.fact_act_chd_slno = 0;
                        child.fact_act_hdr_slno = model.fact_act_hdr_slno;

                        child.submit_status = model.submit_status;
                        child.fin_year = model.fin_year;

                        child.submitted_by = model.submit_status == "Y"
                            ? clsemp.empl_name
                            : "";

                        child.submitted_dt = model.submit_status == "Y"
                            ? DateTime.Now.ToString("yyyy-MM-dd")
                            : "";

                        database.Insert(child);
                    }

                    transaction.Complete();
                }

                return Json(new
                {
                    success = true,
                    message = model.submit_status == "Y"
                        ? "Factory Act data submitted successfully."
                        : "Factory Act data updated successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Factory Act transaction");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error while updating Factory Act data."
                });
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

        public List<Class_employees> GetEmployeesList()
        {
            try
            {
                var st = HttpContext.Session.GetString("User");
                Class_employees clsemp = JsonConvert.DeserializeObject<Class_employees>(st);
                using (MasterRepository mr = new MasterRepository(_masterCrudClass, masterlogger))
                {
                    List<Class_employees> lst = mr.EmployeeList((int)clsemp.plant_slno).ToList();
                    return lst;
                }
            }
            catch (Exception ex)
            {
                // Handle error and log it
                ElmahCore.ElmahExtensions.RaiseError(ex);
                throw;
            }
        }

        [HttpGet]
        public IActionResult GetFactoryActMasterList()
        {
            try
            {
                var factoryActs = _db.SelectAll<Class_factory_act_master>()
                    .Where(x => !x.del_status)
                    .Select(x => new
                    {
                        factory_act_slno = x.factory_act_slno,
                        legal_requirement = x.legal_requirement,
                        compliance_rule_act_reference = x.compliance_rule_act_reference,
                        compliance_approving_authority = x.compliance_approving_authority,
                        appointed_competent_person = x.appointed_competent_person
                    })
                    .ToList();

                return Json(factoryActs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Factory Act master records");
                return StatusCode(500, "Error loading Factory Act master records");
            }
        }

        [HttpGet]
        public IActionResult GetFactoryActChildList(int sl)
        {
            try
            {
                var children = (
                    from master in _db.SelectAll<Class_factory_act_master>()
                        .Where(x => !x.del_status)

                    join child in _db.SelectAll<Class_factory_act_child>()
                        .Where(x => x.fact_act_hdr_slno == sl)
                        on master.factory_act_slno equals child.factory_act_slno
                        into childGroup

                    from child in childGroup.DefaultIfEmpty()

                    select new
                    {
                        fact_act_chd_slno = child != null
                            ? child.fact_act_chd_slno : 0,

                        fact_act_hdr_slno = sl,

                        // Always fetch latest master values
                        legal_requirement = master.legal_requirement,
                        compliance_rule_act_reference = master.compliance_rule_act_reference,
                        compliance_approving_authority = master.compliance_approving_authority,
                        appointed_competent_person = master.appointed_competent_person,

                        // Existing child values, otherwise empty
                        form_no = child != null ? child.form_no : null,
                        freq_renewal = child != null ? child.freq_renewal : null,
                        freq = child != null ? child.freq : null,
                        date_of_filing = child != null ? child.date_of_filing : null,
                        valid_from = child != null ? child.valid_from : null,
                        valid_upto = child != null ? child.valid_upto : null,
                        next_due_date = child != null ? child.next_due_date : null,

                        responsibility = child != null ? child.responsibility : null,
                        responsible_person = child != null ? child.responsible_person : null,
                        resp_Slno = child != null ? child.resp_Slno : (int?)null,

                        status = child != null ? child.status : null,
                        remarks = child != null ? child.remarks : null,

                        submit_status = child != null ? child.submit_status : null,

                        factory_act_slno = master.factory_act_slno
                    }
                ).ToList();

                return Json(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Factory Act child records");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error loading Factory Act records."
                });
            }
        }

        [HttpGet]
        public IActionResult AddNew()
        {
            try
            {
                DateTime today = DateTime.Today;

                int startYear = today.Month >= 4
                    ? today.Year
                    : today.Year - 1;

                string financialYear = $"{startYear}-{startYear + 1}";

                var database = _db.GetDatabase();

                var existingRecord = database.FirstOrDefault<Class_factory_act_hdr>(
                    "WHERE fin_year = @0",
                    financialYear
                );

                if (existingRecord != null)
                {
                    return Json(new
                    {
                        exists = true,
                        message = "Data already present for financial year " + financialYear,
                        id = existingRecord.fact_act_hdr_slno
                    });
                }

                return Json(new
                {
                    exists = false,
                    redirectUrl = Url.Action("Create", "FactoryActTrans")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Factory Act financial year.");

                return StatusCode(500, new
                {
                    message = "Unable to check the financial year."
                });
            }
        }
    }
}
